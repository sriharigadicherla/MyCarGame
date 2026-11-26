using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns and manages active AI traffic vehicles across the city road network.
/// Recycles vehicles ahead of the player to maintain dense, bustling city traffic.
/// </summary>
public class TrafficManager : MonoBehaviour
{
    [Header("Traffic Settings")]
    public int maxTrafficCars = 14;
    public float respawnDistance = 220f;
    public Transform playerCar;

    [Header("Car Body Materials")]
    public Material[] carPaintMaterials;

    private List<AITrafficCar> activeCars = new List<AITrafficCar>();
    private List<TrafficWaypoint> allWaypoints = new List<TrafficWaypoint>();

    void Start()
    {
        FindPlayerIfNull();
        GatherWaypoints();
        SpawnInitialTraffic();
    }

    private void FindPlayerIfNull()
    {
        if (playerCar == null)
        {
            var p = GameObject.Find("SportsSedan");
            if (p != null) playerCar = p.transform;
        }
    }

    private void GatherWaypoints()
    {
        allWaypoints.Clear();
        allWaypoints.AddRange(Object.FindObjectsByType<TrafficWaypoint>(FindObjectsSortMode.None));
    }

    private void SpawnInitialTraffic()
    {
        if (allWaypoints.Count == 0) return;

        // Shuffle waypoints to distribute cars evenly
        List<TrafficWaypoint> spawnPoints = new List<TrafficWaypoint>(allWaypoints);
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            int r = Random.Range(i, spawnPoints.Count);
            var temp = spawnPoints[i];
            spawnPoints[i] = spawnPoints[r];
            spawnPoints[r] = temp;
        }

        int spawnCount = Mathf.Min(maxTrafficCars, spawnPoints.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            var wp = spawnPoints[i];
            // Don't spawn directly on top of player
            if (playerCar != null && Vector3.Distance(wp.transform.position, playerCar.position) < 18f) continue;

            AITrafficCar car = CreateTrafficCar(i, wp);
            activeCars.Add(car);
        }
    }

    public AITrafficCar CreateTrafficCar(int index, TrafficWaypoint startWp)
    {
        GameObject carObj = new GameObject($"TrafficCar_{index + 1}");
        carObj.transform.SetParent(transform, false);

        // Position & Rotate
        Vector3 pos = startWp.transform.position + Vector3.up * 0.15f;
        Quaternion rot = Quaternion.identity;
        if (startWp.nextWaypoints.Count > 0 && startWp.nextWaypoints[0] != null)
        {
            rot = Quaternion.LookRotation(startWp.nextWaypoints[0].transform.position - pos);
        }
        carObj.transform.position = pos;
        carObj.transform.rotation = rot;

        // Rigidbody
        Rigidbody rb = carObj.AddComponent<Rigidbody>();
        rb.mass = 1350f;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 1.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Main BoxCollider
        BoxCollider col = carObj.AddComponent<BoxCollider>();
        col.size = new Vector3(1.85f, 1.25f, 4.4f);
        col.center = new Vector3(0f, 0.65f, 0f);

        // Select paint material
        Material paintMat = null;
        if (carPaintMaterials != null && carPaintMaterials.Length > 0)
        {
            paintMat = carPaintMaterials[index % carPaintMaterials.Length];
        }

        // Procedural Visual Mesh for Traffic Car
        Transform bodyTr = BuildTrafficCarModel(carObj, paintMat, out MeshRenderer taillight);

        // Wheels
        BuildTrafficWheels(carObj, out Transform fl, out Transform fr, out Transform rl, out Transform rr);

        // AI Script
        AITrafficCar ai = carObj.AddComponent<AITrafficCar>();
        ai.currentWaypoint = startWp;
        ai.frontLeftWheel = fl;
        ai.frontRightWheel = fr;
        ai.rearLeftWheel = rl;
        ai.rearRightWheel = rr;
        ai.taillightRenderer = taillight;
        ai.taillightNormalMat = Resources.Load<Material>("Mat_TrafficLight_Off");
        ai.taillightBrakeMat = Resources.Load<Material>("Mat_TrafficLight_Red");

        return ai;
    }

    private Transform BuildTrafficCarModel(GameObject parent, Material paintMat, out MeshRenderer taillight)
    {
        GameObject bodyObj = new GameObject("Body");
        bodyObj.transform.SetParent(parent.transform, false);

        // 1. Lower chassis
        GameObject lower = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lower.name = "Chassis";
        lower.transform.SetParent(bodyObj.transform, false);
        lower.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        lower.transform.localScale = new Vector3(1.8f, 0.65f, 4.3f);
        DestroyImmediate(lower.GetComponent<BoxCollider>());
        if (paintMat != null) lower.GetComponent<MeshRenderer>().sharedMaterial = paintMat;

        // 2. Cabin / Roof
        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.name = "Cabin";
        cabin.transform.SetParent(bodyObj.transform, false);
        cabin.transform.localPosition = new Vector3(0f, 1.05f, -0.2f);
        cabin.transform.localScale = new Vector3(1.5f, 0.55f, 2.3f);
        DestroyImmediate(cabin.GetComponent<BoxCollider>());
        Material glassMat = Resources.Load<Material>("Mat_Glass_Tinted");
        if (glassMat != null) cabin.GetComponent<MeshRenderer>().sharedMaterial = glassMat;

        // 3. Headlights
        GameObject hl = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hl.name = "Headlights";
        hl.transform.SetParent(bodyObj.transform, false);
        hl.transform.localPosition = new Vector3(0f, 0.55f, 2.16f);
        hl.transform.localScale = new Vector3(1.6f, 0.18f, 0.05f);
        DestroyImmediate(hl.GetComponent<BoxCollider>());
        Material hlMat = Resources.Load<Material>("Mat_Headlight_LED");
        if (hlMat != null) hl.GetComponent<MeshRenderer>().sharedMaterial = hlMat;

        // 4. Taillights
        GameObject tl = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tl.name = "Taillights";
        tl.transform.SetParent(bodyObj.transform, false);
        tl.transform.localPosition = new Vector3(0f, 0.55f, -2.16f);
        tl.transform.localScale = new Vector3(1.6f, 0.18f, 0.05f);
        DestroyImmediate(tl.GetComponent<BoxCollider>());
        taillight = tl.GetComponent<MeshRenderer>();
        Material tlMat = Resources.Load<Material>("Mat_TrafficLight_Off");
        if (tlMat != null) taillight.sharedMaterial = tlMat;

        return bodyObj.transform;
    }

    private void BuildTrafficWheels(GameObject parent, out Transform fl, out Transform fr, out Transform rl, out Transform rr)
    {
        Material tireMat = Resources.Load<Material>("Mat_Tire_Rubber");
        Material rimMat = Resources.Load<Material>("Mat_Rim_Alloy");

        fl = CreateWheel(parent, "Wheel_FL", new Vector3(-0.85f, 0.32f, 1.3f), tireMat, rimMat);
        fr = CreateWheel(parent, "Wheel_FR", new Vector3(0.85f, 0.32f, 1.3f), tireMat, rimMat);
        rl = CreateWheel(parent, "Wheel_RL", new Vector3(-0.85f, 0.32f, -1.3f), tireMat, rimMat);
        rr = CreateWheel(parent, "Wheel_RR", new Vector3(0.85f, 0.32f, -1.3f), tireMat, rimMat);
    }

    private Transform CreateWheel(GameObject parent, string name, Vector3 pos, Material tireMat, Material rimMat)
    {
        GameObject wObj = new GameObject(name);
        wObj.transform.SetParent(parent.transform, false);
        wObj.transform.localPosition = pos;

        // Tire cylinder
        GameObject tire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tire.name = "Tire";
        tire.transform.SetParent(wObj.transform, false);
        tire.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        tire.transform.localScale = new Vector3(0.65f, 0.22f, 0.65f);
        DestroyImmediate(tire.GetComponent<CapsuleCollider>());
        if (tireMat != null) tire.GetComponent<MeshRenderer>().sharedMaterial = tireMat;

        return wObj.transform;
    }

    void Update()
    {
        if (playerCar == null)
        {
            FindPlayerIfNull();
            return;
        }

        // Recycle cars that get too far behind or ahead
        foreach (var car in activeCars)
        {
            if (car == null) continue;
            float dist = Vector3.Distance(car.transform.position, playerCar.position);
            if (dist > respawnDistance)
            {
                RelocateCarNearPlayer(car);
            }
        }
    }

    private void RelocateCarNearPlayer(AITrafficCar car)
    {
        if (allWaypoints.Count == 0 || playerCar == null) return;

        // Find a waypoint 70m - 150m ahead of player in driving direction
        Vector3 playerForward = playerCar.forward;
        TrafficWaypoint candidate = null;
        float bestScore = float.MinValue;

        for (int i = 0; i < 20; i++)
        {
            var wp = allWaypoints[Random.Range(0, allWaypoints.Count)];
            Vector3 diff = wp.transform.position - playerCar.position;
            float d = diff.magnitude;

            if (d >= 50f && d <= 180f)
            {
                // Score based on being ahead of player
                float dot = Vector3.Dot(diff.normalized, playerForward);
                float score = dot * 100f - Mathf.Abs(d - 90f);
                if (score > bestScore)
                {
                    bestScore = score;
                    candidate = wp;
                }
            }
        }

        if (candidate != null)
        {
            car.Initialize(candidate);
        }
    }
}
