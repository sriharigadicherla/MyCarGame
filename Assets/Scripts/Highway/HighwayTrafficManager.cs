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

    [Header("Lane Setup")]
    // 4 Forward Lanes (Width = 3.8m each, road width = 18m)
    public float[] laneXPositions = new float[] { -5.7f, -1.9f, 1.9f, 5.7f };
    // Speeds in KM/H per lane (Lane 0 = slow trucks/cars, Lane 3 = fast express)
    public float[] laneSpeedsMin = new float[] { 50f, 65f, 80f, 95f };
    public float[] laneSpeedsMax = new float[] { 65f, 80f, 95f, 110f };

    [Header("References")]
    public Transform playerCar;
    public Material[] carPaintMaterials;
    public Material taillightNormalMat;
    public Material taillightBrakeMat;

    private List<HighwayTrafficCar> activeCars = new List<HighwayTrafficCar>();

    void Start()
    {
        FindPlayerIfNull();
        SpawnInitialTrafficStream();
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
        float startZ = 40f;
        float spacing = (maxSpawnDistanceAhead - startZ) / (totalTrafficCars / 4f);

        int spawned = 0;
        float curZ = startZ;

        while (spawned < totalTrafficCars)
        {
            // Pick a lane
            int lane = spawned % laneXPositions.Length;
            float laneX = laneXPositions[lane];
            float speed = Random.Range(laneSpeedsMin[lane], laneSpeedsMax[lane]);

            // Add slight Z offset to vary traffic layout
            float zPos = curZ + Random.Range(-4f, 6f);

            var car = CreateHighwayTrafficCar(spawned, lane, laneX, zPos, speed);
            activeCars.Add(car);

            spawned++;
            if (spawned % laneXPositions.Length == 0)
            {
                curZ += spacing;
            }
        }
    }

    public HighwayTrafficCar CreateHighwayTrafficCar(int id, int lane, float laneX, float zPos, float speed)
    {
        GameObject carObj = new GameObject($"TrafficCar_{id + 1}");
        carObj.transform.SetParent(transform, false);
        carObj.transform.position = new Vector3(laneX, 0.15f, zPos);
        carObj.transform.rotation = Quaternion.identity;

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
        tc.frontLeftWheel = fl;
        tc.frontRightWheel = fr;
        tc.rearLeftWheel = rl;
        tc.rearRightWheel = rr;
        tc.taillightRenderer = taillight;
        tc.taillightNormalMat = taillightNormalMat;
        tc.taillightBrakeMat = taillightBrakeMat;
        tc.Setup(lane, laneX, speed);

        return tc;
    }

    void Update()
    {
        if (playerCar == null)
        {
            FindPlayerIfNull();
            return;
        }

        float playerZ = playerCar.position.z;

        // Check if any car has fallen behind and recycle it ahead
        for (int i = 0; i < activeCars.Count; i++)
        {
            var car = activeCars[i];
            if (car == null) continue;

            float zDiff = car.transform.position.z - playerZ;

            // Car fell behind player by > despawnDistanceBehind
            if (zDiff < -despawnDistanceBehind)
            {
                RecycleCarAhead(car, playerZ);
            }
        }
    }

    private void RecycleCarAhead(HighwayTrafficCar car, float playerZ)
    {
        // Pick a random lane
        int lane = Random.Range(0, laneXPositions.Length);
        float laneX = laneXPositions[lane];
        float targetSpeed = Random.Range(laneSpeedsMin[lane], laneSpeedsMax[lane]);

        // Find a clear spot ahead of player
        float candidateZ = playerZ + Random.Range(minSpawnDistanceAhead, maxSpawnDistanceAhead);

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
            candidateZ += 24f;
        }

        // Teleport car to new spot ahead
        car.transform.position = new Vector3(laneX, 0.15f, candidateZ);
        car.Setup(lane, laneX, targetSpeed);
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

        // 2. Cabin / Roof (Sleek sports sedan cabin)
        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.name = "Cabin";
        cabin.transform.SetParent(bodyObj.transform, false);
        cabin.transform.localPosition = new Vector3(0f, 0.96f, -0.2f);
        cabin.transform.localScale = new Vector3(1.50f, 0.50f, 2.3f);
        DestroyImmediate(cabin.GetComponent<BoxCollider>());
        Material glassMat = Resources.Load<Material>("Mat_Glass_Tinted");
        if (glassMat != null) cabin.GetComponent<MeshRenderer>().sharedMaterial = glassMat;

        // 3. Headlights (Cool white LEDs)
        GameObject hl = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hl.name = "Headlights";
        hl.transform.SetParent(bodyObj.transform, false);
        hl.transform.localPosition = new Vector3(0f, 0.52f, 2.16f);
        hl.transform.localScale = new Vector3(1.55f, 0.15f, 0.08f);
        DestroyImmediate(hl.GetComponent<BoxCollider>());
        Material hlMat = Resources.Load<Material>("Mat_Headlight_LED");
        if (hlMat != null) hl.GetComponent<MeshRenderer>().sharedMaterial = hlMat;

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

        Material tireMat = Resources.Load<Material>("Mat_Tire_Rubber");

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
}
