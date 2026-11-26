using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Endless 4-Lane Highway Road & Environment Chunk Manager.
/// Seamlessly recycles highway chunks as the player drives forward at high speeds,
/// providing an infinite, smooth expressway with overhead gantries, streetlights, barriers, and scenery.
/// </summary>
public class HighwayManager : MonoBehaviour
{
    [Header("Highway Dimensions")]
    public int totalChunks = 8;
    public float chunkLength = 100f;
    public float roadWidth = 18f; // 4 lanes (3.8m each) + shoulders

    [Header("Player Tracking")]
    public Transform playerCar;

    [Header("Materials")]
    public Material asphaltMat;
    public Material yellowLineMat;
    public Material whiteLineMat;
    public Material barrierMat;
    public Material poleMat;
    public Material lampGlowMat;
    public Material grassMat;
    public Material treeTrunkMat;
    public Material treeLeafRedMat;
    public Material treeLeafOrangeMat;
    public Material treeLeafGreenMat;
    public Material gantrySignMat;

    private List<GameObject> activeChunks = new List<GameObject>();
    private float highestChunkEnd = 0f;

    void Start()
    {
        FindPlayerIfNull();
        BuildInitialChunks();
    }

    private void FindPlayerIfNull()
    {
        if (playerCar == null)
        {
            var p = GameObject.Find("SportsSedan");
            if (p != null) playerCar = p.transform;
        }
    }

    private void BuildInitialChunks()
    {
        float startZ = -chunkLength;
        for (int i = 0; i < totalChunks; i++)
        {
            float chunkStartZ = startZ + i * chunkLength;
            GameObject chunk = GenerateHighwayChunk(i, chunkStartZ);
            activeChunks.Add(chunk);
            highestChunkEnd = chunkStartZ + chunkLength;
        }
    }

    void Update()
    {
        if (playerCar == null)
        {
            FindPlayerIfNull();
            return;
        }

        float playerZ = playerCar.position.z;

        // Check if the rear-most chunk has fallen behind the player
        if (activeChunks.Count > 0)
        {
            GameObject oldestChunk = activeChunks[0];
            float chunkPosZ = oldestChunk.transform.position.z;

            // When player passes chunk by more than chunkLength
            if (playerZ - chunkPosZ > chunkLength * 1.5f)
            {
                // Move oldest chunk to the front
                oldestChunk.transform.position = new Vector3(0f, 0f, highestChunkEnd);
                highestChunkEnd += chunkLength;

                // Rotate in list
                activeChunks.RemoveAt(0);
                activeChunks.Add(oldestChunk);
            }
        }
    }

    private GameObject GenerateHighwayChunk(int id, float zStart)
    {
        GameObject chunk = new GameObject($"HighwayChunk_{id}");
        chunk.transform.SetParent(transform, false);
        chunk.transform.position = new Vector3(0f, 0f, zStart);

        float midZ = chunkLength * 0.5f;

        // 1. Lush Green Landscape Terrain Shoulder (60m wide on each side)
        GameObject groundLeft = GameObject.CreatePrimitive(PrimitiveType.Plane);
        groundLeft.name = "Ground_Left";
        groundLeft.transform.SetParent(chunk.transform, false);
        groundLeft.transform.localPosition = new Vector3(-35f, -0.05f, midZ);
        groundLeft.transform.localScale = new Vector3(6f, 1f, chunkLength * 0.1f);
        if (grassMat != null) groundLeft.GetComponent<MeshRenderer>().sharedMaterial = grassMat;

        GameObject groundRight = GameObject.CreatePrimitive(PrimitiveType.Plane);
        groundRight.name = "Ground_Right";
        groundRight.transform.SetParent(chunk.transform, false);
        groundRight.transform.localPosition = new Vector3(35f, -0.05f, midZ);
        groundRight.transform.localScale = new Vector3(6f, 1f, chunkLength * 0.1f);
        if (grassMat != null) groundRight.GetComponent<MeshRenderer>().sharedMaterial = grassMat;

        // 2. Main 4-Lane Highway Asphalt Road Slab
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "RoadSlab";
        road.transform.SetParent(chunk.transform, false);
        road.transform.localPosition = new Vector3(0f, 0.01f, midZ);
        road.transform.localScale = new Vector3(roadWidth, 0.1f, chunkLength);
        if (asphaltMat != null) road.GetComponent<MeshRenderer>().sharedMaterial = asphaltMat;

        // 3. Yellow Shoulder Lines (Edges)
        CreateRoadLine(chunk, "ShoulderLine_Left", new Vector3(-7.9f, 0.035f, midZ), new Vector3(0.24f, 0.01f, chunkLength), yellowLineMat);
        CreateRoadLine(chunk, "ShoulderLine_Right", new Vector3(7.9f, 0.035f, midZ), new Vector3(0.24f, 0.01f, chunkLength), yellowLineMat);

        // 4. White Dashed Lane Dividers (Separating the 4 lanes)
        float[] dashX = new float[] { -3.8f, 0f, 3.8f };
        foreach (float dx in dashX)
        {
            CreateDashedLine(chunk, $"LaneLine_{dx}", dx, chunkLength, whiteLineMat);
        }

        // 5. Steel Crash Guardrails on Highway Edges
        CreateGuardrail(chunk, "Guardrail_Left", -9.1f, chunkLength);
        CreateGuardrail(chunk, "Guardrail_Right", 9.1f, chunkLength);

        // 6. Highway Streetlights (every 35m)
        for (float z = 15f; z < chunkLength; z += 35f)
        {
            CreateHighwayLight(chunk, new Vector3(-9.6f, 0f, z), false);
            CreateHighwayLight(chunk, new Vector3(9.6f, 0f, z), true);
        }

        // 7. Roadside Trees along outer shoulders
        for (float z = 10f; z < chunkLength; z += 20f)
        {
            Material leafL = (id + z) % 3 == 0 ? treeLeafRedMat : ((id + z) % 3 == 1 ? treeLeafOrangeMat : treeLeafGreenMat);
            CreateRoadsideTree(chunk, new Vector3(-12.5f, 0f, z), leafL);

            Material leafR = (id + z) % 3 == 1 ? treeLeafRedMat : ((id + z) % 3 == 2 ? treeLeafOrangeMat : treeLeafGreenMat);
            CreateRoadsideTree(chunk, new Vector3(12.5f, 0f, z + 10f), leafR);
        }

        // 8. Overhead Highway Sign Gantry (every alternate chunk)
        if (id % 2 == 1)
        {
            CreateHighwayGantry(chunk, new Vector3(0f, 0f, midZ));
        }

        // Strip any residual colliders from road markings
        foreach (var col in chunk.GetComponentsInChildren<Collider>())
        {
            if (col.gameObject.name.Contains("Line") || col.gameObject.name.Contains("Dash") ||
                col.gameObject.name.Contains("Stripe") || col.gameObject.name.Contains("Sign"))
            {
                DestroyImmediate(col);
            }
        }

        return chunk;
    }

    private void CreateRoadLine(GameObject parent, string name, Vector3 pos, Vector3 size, Material mat)
    {
        GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.name = name;
        line.transform.SetParent(parent.transform, false);
        line.transform.localPosition = pos;
        line.transform.localScale = size;
        DestroyImmediate(line.GetComponent<BoxCollider>());
        if (mat != null) line.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private void CreateDashedLine(GameObject parent, string name, float x, float length, Material mat)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent.transform, false);

        float dashLen = 4.0f;
        float gapLen = 4.5f;
        float curZ = 0f;
        int idx = 0;

        while (curZ < length)
        {
            GameObject dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dash.name = $"Dash_{idx++}";
            dash.transform.SetParent(container.transform, false);
            dash.transform.localPosition = new Vector3(x, 0.035f, curZ + dashLen * 0.5f);
            dash.transform.localScale = new Vector3(0.20f, 0.01f, dashLen);
            DestroyImmediate(dash.GetComponent<BoxCollider>());
            if (mat != null) dash.GetComponent<MeshRenderer>().sharedMaterial = mat;

            curZ += dashLen + gapLen;
        }
    }

    private void CreateGuardrail(GameObject parent, string name, float x, float length)
    {
        GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rail.name = name;
        rail.transform.SetParent(parent.transform, false);
        rail.transform.localPosition = new Vector3(x, 0.45f, length * 0.5f);
        rail.transform.localScale = new Vector3(0.35f, 0.70f, length);
        if (barrierMat != null) rail.GetComponent<MeshRenderer>().sharedMaterial = barrierMat;
    }

    private void CreateHighwayLight(GameObject parent, Vector3 pos, bool faceWest)
    {
        GameObject lightObj = new GameObject("HighwayLight");
        lightObj.transform.SetParent(parent.transform, false);
        lightObj.transform.localPosition = pos;

        // Vertical Pole
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "Pole";
        pole.transform.SetParent(lightObj.transform, false);
        pole.transform.localPosition = new Vector3(0f, 3.2f, 0f);
        pole.transform.localScale = new Vector3(0.20f, 3.2f, 0.20f);
        DestroyImmediate(pole.GetComponent<CapsuleCollider>());
        if (poleMat != null) pole.GetComponent<MeshRenderer>().sharedMaterial = poleMat;

        // Horizontal Arm extending towards road
        float dir = faceWest ? -1f : 1f;
        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arm.name = "Arm";
        arm.transform.SetParent(lightObj.transform, false);
        arm.transform.localPosition = new Vector3(dir * 0.9f, 6.4f, 0f);
        arm.transform.localScale = new Vector3(1.8f, 0.15f, 0.15f);
        DestroyImmediate(arm.GetComponent<BoxCollider>());
        if (poleMat != null) arm.GetComponent<MeshRenderer>().sharedMaterial = poleMat;

        // Emissive Lantern
        GameObject lantern = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lantern.name = "Lantern";
        lantern.transform.SetParent(lightObj.transform, false);
        lantern.transform.localPosition = new Vector3(dir * 1.6f, 6.25f, 0f);
        lantern.transform.localScale = new Vector3(0.5f, 0.18f, 0.35f);
        DestroyImmediate(lantern.GetComponent<BoxCollider>());
        if (lampGlowMat != null) lantern.GetComponent<MeshRenderer>().sharedMaterial = lampGlowMat;
    }

    private void CreateRoadsideTree(GameObject parent, Vector3 pos, Material leafMat)
    {
        GameObject tree = new GameObject("Tree");
        tree.transform.SetParent(parent.transform, false);
        tree.transform.localPosition = pos;

        // Trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform, false);
        trunk.transform.localPosition = new Vector3(0f, 2.0f, 0f);
        trunk.transform.localScale = new Vector3(0.5f, 2.0f, 0.5f);
        DestroyImmediate(trunk.GetComponent<CapsuleCollider>());
        if (treeTrunkMat != null) trunk.GetComponent<MeshRenderer>().sharedMaterial = treeTrunkMat;

        // 3-Tier Foliage Crown
        GameObject c1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        c1.name = "Crown_Low";
        c1.transform.SetParent(tree.transform, false);
        c1.transform.localPosition = new Vector3(0f, 4.2f, 0f);
        c1.transform.localScale = new Vector3(3.8f, 2.8f, 3.8f);
        DestroyImmediate(c1.GetComponent<SphereCollider>());
        if (leafMat != null) c1.GetComponent<MeshRenderer>().sharedMaterial = leafMat;

        GameObject c2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        c2.name = "Crown_High";
        c2.transform.SetParent(tree.transform, false);
        c2.transform.localPosition = new Vector3(0f, 5.8f, 0f);
        c2.transform.localScale = new Vector3(2.6f, 2.2f, 2.6f);
        DestroyImmediate(c2.GetComponent<SphereCollider>());
        if (leafMat != null) c2.GetComponent<MeshRenderer>().sharedMaterial = leafMat;
    }

    private void CreateHighwayGantry(GameObject parent, Vector3 pos)
    {
        GameObject gantry = new GameObject("HighwayGantry");
        gantry.transform.SetParent(parent.transform, false);
        gantry.transform.localPosition = pos;

        // Left Pillar
        GameObject leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leftPillar.name = "Pillar_Left";
        leftPillar.transform.SetParent(gantry.transform, false);
        leftPillar.transform.localPosition = new Vector3(-9.4f, 3.8f, 0f);
        leftPillar.transform.localScale = new Vector3(0.40f, 3.8f, 0.40f);
        DestroyImmediate(leftPillar.GetComponent<CapsuleCollider>());
        if (barrierMat != null) leftPillar.GetComponent<MeshRenderer>().sharedMaterial = barrierMat;

        // Right Pillar
        GameObject rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rightPillar.name = "Pillar_Right";
        rightPillar.transform.SetParent(gantry.transform, false);
        rightPillar.transform.localPosition = new Vector3(9.4f, 3.8f, 0f);
        rightPillar.transform.localScale = new Vector3(0.40f, 3.8f, 0.40f);
        DestroyImmediate(rightPillar.GetComponent<CapsuleCollider>());
        if (barrierMat != null) rightPillar.GetComponent<MeshRenderer>().sharedMaterial = barrierMat;

        // Cross Beam Overhead (7.2m high clearance)
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "CrossBeam";
        beam.transform.SetParent(gantry.transform, false);
        beam.transform.localPosition = new Vector3(0f, 7.4f, 0f);
        beam.transform.localScale = new Vector3(19.2f, 0.50f, 0.60f);
        DestroyImmediate(beam.GetComponent<BoxCollider>());
        if (barrierMat != null) beam.GetComponent<MeshRenderer>().sharedMaterial = barrierMat;

        // Green Highway Directional Signs
        GameObject sign1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign1.name = "Sign_Lanes";
        sign1.transform.SetParent(gantry.transform, false);
        sign1.transform.localPosition = new Vector3(-3.2f, 6.4f, 0f);
        sign1.transform.localScale = new Vector3(5.5f, 1.6f, 0.12f);
        DestroyImmediate(sign1.GetComponent<BoxCollider>());
        if (gantrySignMat != null) sign1.GetComponent<MeshRenderer>().sharedMaterial = gantrySignMat;

        GameObject sign2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign2.name = "Sign_Express";
        sign2.transform.SetParent(gantry.transform, false);
        sign2.transform.localPosition = new Vector3(3.2f, 6.4f, 0f);
        sign2.transform.localScale = new Vector3(5.5f, 1.6f, 0.12f);
        DestroyImmediate(sign2.GetComponent<BoxCollider>());
        if (gantrySignMat != null) sign2.GetComponent<MeshRenderer>().sharedMaterial = gantrySignMat;
    }
}
