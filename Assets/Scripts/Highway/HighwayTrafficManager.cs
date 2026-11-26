using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns and manages dynamic highway traffic cars across the 4 highway lanes.
/// Ensures an endless, continuous stream of vehicles cruising ahead of the player to overtake.
/// Recycles vehicles that fall behind into open spots ahead.
/// </summary>
public class HighwayTrafficManager : MonoBehaviour
{
    [Header("Traffic Pool Settings")]
    public int totalTrafficCars = 20;
    public float minSpawnDistanceAhead = 90f;
    public float maxSpawnDistanceAhead = 260f;
    public float despawnDistanceBehind = 45f;
    public bool isTrafficActive = false;

    [Header("Lane Setup")]
    // 4 Lanes: Lanes 0 & 1 are Left Forward, Lanes 2 & 3 are Right Oncoming
    public float[] laneXPositions = new float[] { -5.7f, -1.9f, 1.9f, 5.7f };
    public bool[] laneIsOncoming = new bool[] { false, false, true, true };
    // Speeds in KM/H per lane
    public float[] laneSpeedsMin = new float[] { 55f, 75f, 70f, 55f };
    public float[] laneSpeedsMax = new float[] { 75f, 95f, 90f, 75f };

    [Header("References")]
    public Transform playerCar;
    public Material[] carPaintMaterials;
    public Material taillightNormalMat;
    public Material taillightBrakeMat;
    public Material headlightMat;
    public Material glassMat;
    public Material tireMat;
    public Material darkTrimMat;

    private List<HighwayTrafficCar> activeCars = new List<HighwayTrafficCar>();

    void Awake()
    {
        // Enforce 2-way highway configuration: Left lanes = Forward, Right lanes = Oncoming
        laneXPositions = new float[] { -5.7f, -1.9f, 1.9f, 5.7f };
        laneIsOncoming = new bool[] { false, false, true, true };
        laneSpeedsMin = new float[] { 55f, 75f, 70f, 55f };
        laneSpeedsMax = new float[] { 75f, 95f, 90f, 75f };
    }

    void Start()
    {
        FindPlayerIfNull();
        ClearOldCars();
        SpawnInitialTrafficStream();
        isTrafficActive = false; // Stay frozen until user clicks PLAY
    }

    private void ClearOldCars()
    {
        activeCars.Clear();
        // Remove any residual cars from previous play sessions
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child != null && child.name.StartsWith("TrafficCar"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void FindPlayerIfNull()
    {
        if (playerCar == null)
        {
            var p = GameObject.Find("SportsSedan");
            if (p != null) playerCar = p.transform;
        }
    }

    private void SpawnInitialTrafficStream()
    {
        for (int i = 0; i < totalTrafficCars; i++)
        {
            int lane = i % laneXPositions.Length;
            float laneX = laneXPositions[lane];
            bool oncoming = laneIsOncoming[lane];
            float speed = Random.Range(laneSpeedsMin[lane], laneSpeedsMax[lane]);

            float zPos;
            int laneIndexInGroup = i / laneXPositions.Length;

            if (oncoming)
            {
                // Oncoming traffic spawns ahead and drives towards the player
                zPos = 45f + laneIndexInGroup * 50f + Random.Range(-6f, 6f);
            }
            else
            {
                // Forward traffic spawns ahead and drives forward
                zPos = 25f + laneIndexInGroup * 42f + Random.Range(-4f, 6f);
            }

            var car = CreateHighwayTrafficCar(i, lane, laneX, zPos, speed, oncoming);
            activeCars.Add(car);
        }
    }

    public HighwayTrafficCar CreateHighwayTrafficCar(int id, int lane, float laneX, float zPos, float speed, bool oncoming = false)
    {
        GameObject carObj = new GameObject($"TrafficCar_{id + 1}");
        carObj.transform.SetParent(transform, false);
        carObj.transform.position = new Vector3(laneX, 0.15f, zPos);
        carObj.transform.rotation = Quaternion.Euler(0f, oncoming ? 180f : 0f, 0f);

        // Rigidbody
        Rigidbody rb = carObj.AddComponent<Rigidbody>();
        rb.mass = 1350f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 1.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Smooth SphereColliders for wheels & BoxCollider for body
        BoxCollider bodyCol = carObj.AddComponent<BoxCollider>();
        bodyCol.size = new Vector3(1.85f, 0.70f, 4.4f);
        bodyCol.center = new Vector3(0f, 0.75f, 0f);

        // Visual model
        Material paintMat = null;
        if (carPaintMaterials != null && carPaintMaterials.Length > 0)
        {
            paintMat = carPaintMaterials[id % carPaintMaterials.Length];
        }

        BuildTrafficCarModel(carObj, paintMat, out MeshRenderer taillight);
        BuildTrafficWheels(carObj, out Transform fl, out Transform fr, out Transform rl, out Transform rr);

        // Traffic Car Script
        HighwayTrafficCar tc = carObj.AddComponent<HighwayTrafficCar>();
        tc.trafficManager = this;
        tc.frontLeftWheel = fl;
        tc.frontRightWheel = fr;
        tc.rearLeftWheel = rl;
        tc.rearRightWheel = rr;
        tc.taillightRenderer = taillight;
        tc.taillightNormalMat = taillightNormalMat;
        tc.taillightBrakeMat = taillightBrakeMat;
        tc.Setup(lane, laneX, speed, oncoming);

        return tc;
    }

    void Update()
    {
        if (!isTrafficActive) return;

        if (playerCar == null)
        {
            FindPlayerIfNull();
            return;
        }

        float playerZ = playerCar.position.z;

        // Recycle cars that passed out of view
        for (int i = 0; i < activeCars.Count; i++)
        {
            var car = activeCars[i];
            if (car == null) continue;

            float zDiff = car.transform.position.z - playerZ;

            if (car.isOncoming)
            {
                // Oncoming car travels -Z towards player.
                // Once it has passed the player and is behind by > 35m:
                if (zDiff < -35f || zDiff > 420f)
                {
                    RecycleCarAhead(car, playerZ);
                }
            }
            else
            {
                // Forward car travels +Z.
                // Once player overtakes it and it falls behind by > despawnDistanceBehind:
                if (zDiff < -despawnDistanceBehind || zDiff > 360f)
                {
                    RecycleCarAhead(car, playerZ);
                }
            }
        }
    }

    private void RecycleCarAhead(HighwayTrafficCar car, float playerZ)
    {
        // Pick a random lane
        int lane = Random.Range(0, laneXPositions.Length);
        float laneX = laneXPositions[lane];
        bool oncoming = laneIsOncoming[lane];
        float targetSpeed = Random.Range(laneSpeedsMin[lane], laneSpeedsMax[lane]);

        float candidateZ;
        if (oncoming)
        {
            // Oncoming cars spawn ahead (130m to 320m) and drive towards player
            candidateZ = playerZ + Random.Range(130f, 320f);
        }
        else
        {
            // Forward cars spawn ahead (85m to 240m) and cruise forward
            candidateZ = playerZ + Random.Range(minSpawnDistanceAhead, maxSpawnDistanceAhead);
        }

        // Verify no car is too close in that lane at that Z
        for (int attempt = 0; attempt < 8; attempt++)
        {
            bool occupied = false;
            foreach (var other in activeCars)
            {
                if (other == car || other == null) continue;
                if (other.laneIndex == lane && Mathf.Abs(other.transform.position.z - candidateZ) < 22f)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied) break;
            candidateZ += 25f;
        }

        // Teleport car to new spot ahead
        car.transform.position = new Vector3(laneX, 0.15f, candidateZ);
        car.Setup(lane, laneX, targetSpeed, oncoming);
    }

    #region Procedural 3D Visual Mesh for Traffic Car
    private void BuildTrafficCarModel(GameObject parent, Material paintMat, out MeshRenderer taillight)
    {
        GameObject bodyObj = new GameObject("Body");
        bodyObj.transform.SetParent(parent.transform, false);

        // 1. Lower chassis
        GameObject lower = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lower.name = "Chassis";
        lower.transform.SetParent(bodyObj.transform, false);
        lower.transform.localPosition = new Vector3(0f, 0.48f, 0f);
        lower.transform.localScale = new Vector3(1.82f, 0.55f, 4.3f);
        DestroyImmediate(lower.GetComponent<BoxCollider>());
        if (paintMat != null) lower.GetComponent<MeshRenderer>().sharedMaterial = paintMat;

        // 2. Cabin / Roof (Sleek sports sedan cabin with tinted glass)
        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.name = "Cabin";
        cabin.transform.SetParent(bodyObj.transform, false);
        cabin.transform.localPosition = new Vector3(0f, 0.96f, -0.2f);
        cabin.transform.localScale = new Vector3(1.50f, 0.50f, 2.3f);
        DestroyImmediate(cabin.GetComponent<BoxCollider>());
        if (glassMat != null) cabin.GetComponent<MeshRenderer>().sharedMaterial = glassMat;

        // 3. Bright Glowing LED Headlights (Distinct Front Profile)
        // Left Headlight
        GameObject hlLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hlLeft.name = "Headlight_L";
        hlLeft.transform.SetParent(bodyObj.transform, false);
        hlLeft.transform.localPosition = new Vector3(-0.62f, 0.52f, 2.16f);
        hlLeft.transform.localScale = new Vector3(0.42f, 0.16f, 0.08f);
        DestroyImmediate(hlLeft.GetComponent<BoxCollider>());
        if (headlightMat != null) hlLeft.GetComponent<MeshRenderer>().sharedMaterial = headlightMat;

        // Right Headlight
        GameObject hlRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hlRight.name = "Headlight_R";
        hlRight.transform.SetParent(bodyObj.transform, false);
        hlRight.transform.localPosition = new Vector3(0.62f, 0.52f, 2.16f);
        hlRight.transform.localScale = new Vector3(0.42f, 0.16f, 0.08f);
        DestroyImmediate(hlRight.GetComponent<BoxCollider>());
        if (headlightMat != null) hlRight.GetComponent<MeshRenderer>().sharedMaterial = headlightMat;

        // Front Grille (Dark Matte Black)
        GameObject grille = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grille.name = "Grille";
        grille.transform.SetParent(bodyObj.transform, false);
        grille.transform.localPosition = new Vector3(0f, 0.44f, 2.165f);
        grille.transform.localScale = new Vector3(0.70f, 0.22f, 0.06f);
        DestroyImmediate(grille.GetComponent<BoxCollider>());
        if (darkTrimMat != null) grille.GetComponent<MeshRenderer>().sharedMaterial = darkTrimMat;

        // 4. Taillights (Dynamic red emissive)
        GameObject tl = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tl.name = "Taillights";
        tl.transform.SetParent(bodyObj.transform, false);
        tl.transform.localPosition = new Vector3(0f, 0.58f, -2.16f);
        tl.transform.localScale = new Vector3(1.55f, 0.15f, 0.08f);
        DestroyImmediate(tl.GetComponent<BoxCollider>());
        taillight = tl.GetComponent<MeshRenderer>();
        if (taillightNormalMat != null) taillight.sharedMaterial = taillightNormalMat;
    }

    private void BuildTrafficWheels(GameObject parent, out Transform fl, out Transform fr, out Transform rl, out Transform rr)
    {
        GameObject wheelsObj = new GameObject("Wheels");
        wheelsObj.transform.SetParent(parent.transform, false);

        fl = CreateWheel(wheelsObj, "Wheel_FL", new Vector3(-0.88f, 0.34f, 1.35f), tireMat);
        fr = CreateWheel(wheelsObj, "Wheel_FR", new Vector3(0.88f, 0.34f, 1.35f), tireMat);
        rl = CreateWheel(wheelsObj, "Wheel_RL", new Vector3(-0.88f, 0.34f, -1.35f), tireMat);
        rr = CreateWheel(wheelsObj, "Wheel_RR", new Vector3(0.88f, 0.34f, -1.35f), tireMat);
    }

    private Transform CreateWheel(GameObject parent, string name, Vector3 pos, Material mat)
    {
        GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        w.name = name;
        w.transform.SetParent(parent.transform, false);
        w.transform.localPosition = pos;
        w.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        w.transform.localScale = new Vector3(0.68f, 0.28f, 0.68f);
        DestroyImmediate(w.GetComponent<CapsuleCollider>());
        if (mat != null) w.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return w.transform;
    }
    #endregion

    public List<HighwayTrafficCar> GetAllActiveCars()
    {
        return activeCars;
    }

    public void StartTraffic()
    {
        isTrafficActive = true;
    }

    public void ResetTrafficStream()
    {
        ClearOldCars();
        SpawnInitialTrafficStream();
        isTrafficActive = false;
    }
}
