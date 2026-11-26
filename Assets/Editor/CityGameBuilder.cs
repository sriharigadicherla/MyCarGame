using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Automated City Environment & Driving Game Scene Builder.
/// Generates:
/// 1. High-clarity multi-lane dark asphalt road network with crisp yellow & white markings.
/// 2. Vibrant, colorful 3D architecture (sapphire glass skyscrapers, red brick apartments, ivory commercial towers, storefronts).
/// 3. Colorful trees (fiery crimson red, golden autumn orange, emerald summer green).
/// 4. Concrete sidewalks with raised curbs, streetlights, traffic lights, and barriers.
/// 5. AI traffic waypoints, checkpoints, modern HUD, and procedural audio.
/// 6. Fixed camera chase angle and distance.
/// </summary>
public class CityGameBuilder
{
    private const string ENV_MAT_DIR = "Assets/Materials/Environment";

    [MenuItem("Tools/Build Complete 3D Driving Game")]
    public static void BuildCompleteGame()
    {
        Debug.Log("<color=cyan>[CityGameBuilder]</color> Starting Vibrant 3D Driving Game Generation...");

        // 1. Ensure Directory
        EnsureDirectory(ENV_MAT_DIR);

        // 2. Open MainScene
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Single);

        // 3. Create or Update All Materials with Vibrant Colors via Unity Editor API
        var envMats = CreateOrUpdateAllMaterials();

        // 4. Clean up previous environment / UI roots (leaves SportsSedan untouched!)
        CleanUpPreviousBuilds();

        // 5. Setup Lighting & Sky Atmosphere
        SetupEnvironmentLighting();

        // 6. Generate High-Clarity Road Network & Environment
        GameObject envRoot = new GameObject("Environment");
        BuildHighClarityRoadNetwork(envRoot, envMats, out List<TrafficWaypoint> waypoints, out List<TrafficLightController> trafficLights);
        BuildColorfulCityBlocks(envRoot, envMats);
        BuildPerimeterBarriers(envRoot, envMats);

        // 7. Setup AI Traffic Manager
        SetupTrafficManager(waypoints, envMats);

        // 8. Setup Checkpoints & Gameplay Manager
        SetupGameplayManager(envMats);

        // 9. Setup Modern HUD UI & Minimap
        SetupGameUI();

        // 10. Setup Audio Manager
        SetupAudioManager();

        // 11. Connect Player Car & Camera
        SetupPlayerCarAndCamera();

        // 12. Ensure MainScene is in Build Settings
        EnsureSceneInBuildSettings();

        // 13. Save Scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=green>[CityGameBuilder]</color> Vibrant 3D Driving Game successfully built with colorful buildings, trees, and high-clarity roads!");
    }

    private static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static void CleanUpPreviousBuilds()
    {
        string[] cleanNames = new string[] {
            "Environment", "TrafficSystem", "GameplayManager", "UI_Canvas", "AudioManager", "Plane"
        };

        foreach (string name in cleanNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null) Object.DestroyImmediate(obj);
        }
    }

    #region Materials Creation
    public static Dictionary<string, Material> CreateOrUpdateAllMaterials()
    {
        var dict = new Dictionary<string, Material>();

        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");

        // 1. Roads & Markings (High clarity and contrast)
        dict["Mat_Road_Asphalt"] = GetOrCreateMat("Mat_Road_Asphalt", litShader,
            new Color(0.12f, 0.13f, 0.15f, 1f), metallic: 0.15f, smoothness: 0.35f);

        dict["Mat_Road_Marking_Yellow"] = GetOrCreateMat("Mat_Road_Marking_Yellow", litShader,
            new Color(0.98f, 0.78f, 0.05f, 1f), metallic: 0.0f, smoothness: 0.25f);

        dict["Mat_Road_Marking_White"] = GetOrCreateMat("Mat_Road_Marking_White", litShader,
            new Color(0.98f, 0.98f, 1.0f, 1f), metallic: 0.0f, smoothness: 0.25f);

        dict["Mat_Sidewalk_Concrete"] = GetOrCreateMat("Mat_Sidewalk_Concrete", litShader,
            new Color(0.72f, 0.74f, 0.76f, 1f), metallic: 0.05f, smoothness: 0.25f);

        dict["Mat_Grass_Park"] = GetOrCreateMat("Mat_Grass_Park", litShader,
            new Color(0.18f, 0.44f, 0.15f, 1f), metallic: 0.0f, smoothness: 0.15f);

        // 2. Colorful 3D Buildings
        dict["Mat_Building_ModernGlass"] = GetOrCreateMat("Mat_Building_ModernGlass", litShader,
            new Color(0.12f, 0.26f, 0.44f, 1f), metallic: 0.94f, smoothness: 0.96f); // Sapphire Blue Glass

        dict["Mat_Building_EmeraldGlass"] = GetOrCreateMat("Mat_Building_EmeraldGlass", litShader,
            new Color(0.10f, 0.32f, 0.28f, 1f), metallic: 0.94f, smoothness: 0.96f); // Emerald Teal Glass

        dict["Mat_Building_Concrete_1"] = GetOrCreateMat("Mat_Building_Concrete_1", litShader,
            new Color(0.85f, 0.82f, 0.76f, 1f), metallic: 0.08f, smoothness: 0.30f); // Warm Ivory Limestone

        dict["Mat_Building_Concrete_2"] = GetOrCreateMat("Mat_Building_Concrete_2", litShader,
            new Color(0.25f, 0.28f, 0.32f, 1f), metallic: 0.15f, smoothness: 0.35f); // Modern Urban Slate

        dict["Mat_Building_Brick"] = GetOrCreateMat("Mat_Building_Brick", litShader,
            new Color(0.68f, 0.26f, 0.18f, 1f), metallic: 0.0f, smoothness: 0.15f); // Warm Terracotta Brick

        dict["Mat_Building_Storefront"] = GetOrCreateMat("Mat_Building_Storefront", litShader,
            new Color(0.95f, 0.85f, 0.55f, 1f), metallic: 0.2f, smoothness: 0.85f,
            emission: new Color(1.8f, 1.4f, 0.8f, 1f)); // Warm Glowing Display Windows

        dict["Mat_Building_WhiteTrim"] = GetOrCreateMat("Mat_Building_WhiteTrim", litShader,
            new Color(0.92f, 0.93f, 0.95f, 1f), metallic: 0.1f, smoothness: 0.45f);

        // 3. Colorful Trees
        dict["Mat_Tree_Trunk"] = GetOrCreateMat("Mat_Tree_Trunk", litShader,
            new Color(0.28f, 0.18f, 0.10f, 1f), metallic: 0.0f, smoothness: 0.12f); // Deep Walnut Bark

        dict["Mat_Tree_Leaves_Autumn_Red"] = GetOrCreateMat("Mat_Tree_Leaves_Autumn_Red", litShader,
            new Color(0.85f, 0.18f, 0.10f, 1f), metallic: 0.0f, smoothness: 0.20f); // Fiery Maple Red

        dict["Mat_Tree_Leaves_Autumn_Orange"] = GetOrCreateMat("Mat_Tree_Leaves_Autumn_Orange", litShader,
            new Color(0.94f, 0.48f, 0.08f, 1f), metallic: 0.0f, smoothness: 0.20f); // Golden Autumn Orange

        dict["Mat_Tree_Leaves_Green"] = GetOrCreateMat("Mat_Tree_Leaves_Green", litShader,
            new Color(0.16f, 0.52f, 0.14f, 1f), metallic: 0.0f, smoothness: 0.20f); // Summer Forest Green

        // 4. Streetlights & Props
        dict["Mat_StreetLight_Pole"] = GetOrCreateMat("Mat_StreetLight_Pole", litShader,
            new Color(0.22f, 0.24f, 0.26f, 1f), metallic: 0.85f, smoothness: 0.60f);

        dict["Mat_StreetLight_Glow"] = GetOrCreateMat("Mat_StreetLight_Glow", litShader,
            new Color(1.0f, 0.95f, 0.80f, 1f), metallic: 0.0f, smoothness: 0.90f,
            emission: new Color(3.5f, 3.2f, 2.2f, 1f));

        dict["Mat_TrafficLight_Housing"] = GetOrCreateMat("Mat_TrafficLight_Housing", litShader,
            new Color(0.10f, 0.10f, 0.10f, 1f), metallic: 0.2f, smoothness: 0.30f);

        dict["Mat_TrafficLight_Red"] = GetOrCreateMat("Mat_TrafficLight_Red", litShader,
            new Color(0.95f, 0.10f, 0.10f, 1f), metallic: 0.1f, smoothness: 0.85f,
            emission: new Color(4.5f, 0.2f, 0.2f, 1f));

        dict["Mat_TrafficLight_Yellow"] = GetOrCreateMat("Mat_TrafficLight_Yellow", litShader,
            new Color(0.95f, 0.85f, 0.10f, 1f), metallic: 0.1f, smoothness: 0.85f,
            emission: new Color(3.8f, 3.0f, 0.2f, 1f));

        dict["Mat_TrafficLight_Green"] = GetOrCreateMat("Mat_TrafficLight_Green", litShader,
            new Color(0.10f, 0.95f, 0.25f, 1f), metallic: 0.1f, smoothness: 0.85f,
            emission: new Color(0.2f, 4.2f, 0.6f, 1f));

        dict["Mat_TrafficLight_Off"] = GetOrCreateMat("Mat_TrafficLight_Off", litShader,
            new Color(0.15f, 0.15f, 0.15f, 1f), metallic: 0.1f, smoothness: 0.50f);

        dict["Mat_Barrier_Metal"] = GetOrCreateMat("Mat_Barrier_Metal", litShader,
            new Color(0.78f, 0.80f, 0.82f, 1f), metallic: 0.90f, smoothness: 0.65f);

        dict["Mat_Checkpoint_Beacon"] = GetOrCreateMat("Mat_Checkpoint_Beacon", litShader,
            new Color(0.10f, 0.85f, 1.0f, 0.85f), metallic: 0.2f, smoothness: 0.95f,
            emission: new Color(1.5f, 5.0f, 6.5f, 1f));

        // 5. Traffic Cars Assorted Paints
        dict["Mat_TrafficCar_Body1"] = GetOrCreateMat("Mat_TrafficCar_Body1", litShader,
            new Color(0.92f, 0.93f, 0.95f, 1f), metallic: 0.85f, smoothness: 0.92f); // White

        dict["Mat_TrafficCar_Body2"] = GetOrCreateMat("Mat_TrafficCar_Body2", litShader,
            new Color(0.85f, 0.15f, 0.12f, 1f), metallic: 0.88f, smoothness: 0.90f); // Red

        dict["Mat_TrafficCar_Body3"] = GetOrCreateMat("Mat_TrafficCar_Body3", litShader,
            new Color(0.12f, 0.32f, 0.78f, 1f), metallic: 0.88f, smoothness: 0.90f); // Blue

        dict["Mat_TrafficCar_Body4"] = GetOrCreateMat("Mat_TrafficCar_Body4", litShader,
            new Color(0.50f, 0.52f, 0.55f, 1f), metallic: 0.88f, smoothness: 0.88f); // Silver

        dict["Mat_TrafficCar_Body5"] = GetOrCreateMat("Mat_TrafficCar_Body5", litShader,
            new Color(0.95f, 0.75f, 0.08f, 1f), metallic: 0.85f, smoothness: 0.90f); // Yellow

        return dict;
    }

    private static Material GetOrCreateMat(string matName, Shader shader, Color baseColor, float metallic, float smoothness, Color? emission = null)
    {
        string path = $"{ENV_MAT_DIR}/{matName}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = shader;
        }

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", baseColor);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

        if (emission.HasValue)
        {
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission.Value);
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", Color.black);
        }

        EditorUtility.SetDirty(mat);
        return mat;
    }
    #endregion

    #region Environment Lighting
    private static void SetupEnvironmentLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.70f, 0.82f, 0.96f);
        RenderSettings.ambientEquatorColor = new Color(0.52f, 0.60f, 0.68f);
        RenderSettings.ambientGroundColor = new Color(0.22f, 0.28f, 0.22f);

        GameObject lightObj = GameObject.Find("Directional Light");
        if (lightObj == null)
        {
            lightObj = new GameObject("Directional Light");
            lightObj.AddComponent<Light>();
        }

        Light dirLight = lightObj.GetComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.color = new Color(1.0f, 0.97f, 0.92f);
        dirLight.intensity = 1.30f;
        dirLight.shadows = LightShadows.Soft;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }
    #endregion

    #region Road Network & Markings
    private static void BuildHighClarityRoadNetwork(GameObject parent, Dictionary<string, Material> mats,
        out List<TrafficWaypoint> allWaypoints, out List<TrafficLightController> allTrafficLights)
    {
        allWaypoints = new List<TrafficWaypoint>();
        allTrafficLights = new List<TrafficLightController>();

        GameObject roadsRoot = new GameObject("Roads");
        roadsRoot.transform.SetParent(parent.transform, false);

        Material asphaltMat = mats["Mat_Road_Asphalt"];
        Material yellowMat = mats["Mat_Road_Marking_Yellow"];
        Material whiteMat = mats["Mat_Road_Marking_White"];
        Material sidewalkMat = mats["Mat_Sidewalk_Concrete"];
        Material grassMat = mats["Mat_Grass_Park"];

        // 1. Lush Green Landscape Terrain Base (640m x 640m)
        GameObject groundBase = GameObject.CreatePrimitive(PrimitiveType.Plane);
        groundBase.name = "Landscape_TerrainBase";
        groundBase.transform.SetParent(roadsRoot.transform, false);
        groundBase.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        groundBase.transform.localScale = new Vector3(64f, 1f, 64f);
        groundBase.GetComponent<MeshRenderer>().sharedMaterial = grassMat;

        // 2. Central 4-Lane Boulevard (Dark Asphalt, 18m wide, 460m long)
        CreateRoadSegment(roadsRoot, "Boulevard_NS", new Vector3(0f, 0.01f, 0f), new Vector3(18f, 0.1f, 460f), asphaltMat);

        // Center double yellow line (raised to Y = 0.035m to prevent z-fighting)
        CreateRoadMarking(roadsRoot, "CenterLine_Yellow_L", new Vector3(-0.20f, 0.035f, 0f), new Vector3(0.22f, 0.01f, 456f), yellowMat);
        CreateRoadMarking(roadsRoot, "CenterLine_Yellow_R", new Vector3(0.20f, 0.035f, 0f), new Vector3(0.22f, 0.01f, 456f), yellowMat);

        // White dashed lane lines (raised to Y = 0.035m)
        CreateDashedLaneLine(roadsRoot, "LaneLine_West", -4.5f, -220f, 220f, 4.0f, 4.5f, whiteMat);
        CreateDashedLaneLine(roadsRoot, "LaneLine_East", 4.5f, -220f, 220f, 4.0f, 4.5f, whiteMat);

        // Sidewalks along Boulevard (Segmented between the 4 cross avenues - ZERO blocking at intersections!)
        float[][] bldSwIntervals = new float[][] {
            new float[] { -220f, -161f },
            new float[] { -139f, -61f },
            new float[] { -39f, 39f },
            new float[] { 61f, 139f },
            new float[] { 161f, 220f }
        };
        for (int s = 0; s < bldSwIntervals.Length; s++)
        {
            float zStart = bldSwIntervals[s][0];
            float zEnd = bldSwIntervals[s][1];
            float segLen = zEnd - zStart;
            float segMid = (zStart + zEnd) * 0.5f;
            CreateSidewalk(roadsRoot, $"Sidewalk_West_{s}", new Vector3(-10.5f, 0.10f, segMid), new Vector3(3.0f, 0.20f, segLen), sidewalkMat);
            CreateSidewalk(roadsRoot, $"Sidewalk_East_{s}", new Vector3(10.5f, 0.10f, segMid), new Vector3(3.0f, 0.20f, segLen), sidewalkMat);
        }

        // 3. 4 Cross Avenues (East-West at Z = -150, -50, +50, +150, width 14m)
        float[] crossZ = new float[] { -150f, -50f, 50f, 150f };
        for (int i = 0; i < crossZ.Length; i++)
        {
            float z = crossZ[i];
            // West and East Avenue road branches (Boulevard runs continuously through the center!)
            CreateRoadSegment(roadsRoot, $"Avenue_EW_W_{i + 1}", new Vector3(-114.5f, 0.01f, z), new Vector3(211f, 0.1f, 14f), asphaltMat);
            CreateRoadSegment(roadsRoot, $"Avenue_EW_E_{i + 1}", new Vector3(114.5f, 0.01f, z), new Vector3(211f, 0.1f, 14f), asphaltMat);

            // Center double yellow line (branches out from Boulevard edge)
            CreateRoadMarking(roadsRoot, $"AveCenter_Yellow_L_W_{i}", new Vector3(-114.5f, 0.035f, z - 0.18f), new Vector3(211f, 0.01f, 0.22f), yellowMat);
            CreateRoadMarking(roadsRoot, $"AveCenter_Yellow_R_W_{i}", new Vector3(-114.5f, 0.035f, z + 0.18f), new Vector3(211f, 0.01f, 0.22f), yellowMat);
            CreateRoadMarking(roadsRoot, $"AveCenter_Yellow_L_E_{i}", new Vector3(114.5f, 0.035f, z - 0.18f), new Vector3(211f, 0.01f, 0.22f), yellowMat);
            CreateRoadMarking(roadsRoot, $"AveCenter_Yellow_R_E_{i}", new Vector3(114.5f, 0.035f, z + 0.18f), new Vector3(211f, 0.01f, 0.22f), yellowMat);

            // Dashed lines along Avenue (skips Boulevard intersection)
            CreateDashedLaneLineEW(roadsRoot, $"AveLaneLine_N_{i}", z + 3.5f, -220f, 220f, 4.0f, 4.5f, whiteMat);
            CreateDashedLaneLineEW(roadsRoot, $"AveLaneLine_S_{i}", z - 3.5f, -220f, 220f, 4.0f, 4.5f, whiteMat);

            // Sidewalks along Avenue (WEST and EAST branches only - ZERO crossing Boulevard!)
            CreateSidewalk(roadsRoot, $"AveSidewalk_North_W_{i}", new Vector3(-116.25f, 0.10f, z + 8.5f), new Vector3(207.5f, 0.20f, 3.0f), sidewalkMat);
            CreateSidewalk(roadsRoot, $"AveSidewalk_South_W_{i}", new Vector3(-116.25f, 0.10f, z - 8.5f), new Vector3(207.5f, 0.20f, 3.0f), sidewalkMat);
            CreateSidewalk(roadsRoot, $"AveSidewalk_North_E_{i}", new Vector3(116.25f, 0.10f, z + 8.5f), new Vector3(207.5f, 0.20f, 3.0f), sidewalkMat);
            CreateSidewalk(roadsRoot, $"AveSidewalk_South_E_{i}", new Vector3(116.25f, 0.10f, z - 8.5f), new Vector3(207.5f, 0.20f, 3.0f), sidewalkMat);

            // Pedestrian Zebra Crossings (NO colliders, raised 0.04m)
            CreateZebraCrossing(roadsRoot, $"Zebra_North_{i}", new Vector3(0f, 0.04f, z + 9.5f), whiteMat);
            CreateZebraCrossing(roadsRoot, $"Zebra_South_{i}", new Vector3(0f, 0.04f, z - 9.5f), whiteMat);

            // Stop Lines (NO colliders, raised 0.04m)
            CreateStopLine(roadsRoot, $"StopLine_North_{i}", new Vector3(4.5f, 0.04f, z - 11.2f), new Vector3(8.5f, 0.01f, 0.65f), whiteMat);
            CreateStopLine(roadsRoot, $"StopLine_South_{i}", new Vector3(-4.5f, 0.04f, z + 11.2f), new Vector3(8.5f, 0.01f, 0.65f), whiteMat);

            // Traffic Light Gantry
            TrafficLightController tlc = CreateIntersectionTrafficLights(roadsRoot, mats, new Vector3(0f, 0f, z));
            if (tlc != null) allTrafficLights.Add(tlc);
        }

        // Extra guarantee: ensure no road markings, dashes, or stripes ever have any colliders
        foreach (var col in roadsRoot.GetComponentsInChildren<Collider>())
        {
            string colName = col.gameObject.name;
            if (colName.StartsWith("Dash_") || colName.StartsWith("Stripe_") || colName.StartsWith("StopLine_") ||
                colName.StartsWith("CenterLine_") || colName.StartsWith("AveCenter_") || colName.Contains("Marking"))
            {
                Object.DestroyImmediate(col);
            }
        }

        // 4. Outer Highway Perimeter Loop (Connecting X = +/-210, Z = +/-210)
        CreateRoadSegment(roadsRoot, "Perimeter_North", new Vector3(0f, 0.01f, 210f), new Vector3(440f, 0.1f, 14f), asphaltMat);
        CreateRoadSegment(roadsRoot, "Perimeter_South", new Vector3(0f, 0.01f, -210f), new Vector3(440f, 0.1f, 14f), asphaltMat);
        CreateRoadSegment(roadsRoot, "Perimeter_West", new Vector3(-210f, 0.01f, 0f), new Vector3(14f, 0.1f, 440f), asphaltMat);
        CreateRoadSegment(roadsRoot, "Perimeter_East", new Vector3(210f, 0.01f, 0f), new Vector3(14f, 0.1f, 440f), asphaltMat);

        // 5. Generate AI Traffic Waypoint Network
        allWaypoints = GenerateTrafficWaypointGraph(roadsRoot, crossZ);
    }

    private static void CreateRoadSegment(GameObject parent, string name, Vector3 pos, Vector3 size, Material mat)
    {
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = name;
        road.transform.SetParent(parent.transform, false);
        road.transform.localPosition = pos;
        road.transform.localScale = size;
        if (mat != null) road.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private static void CreateRoadMarking(GameObject parent, string name, Vector3 pos, Vector3 size, Material mat)
    {
        GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mark.name = name;
        mark.transform.SetParent(parent.transform, false);
        mark.transform.localPosition = pos;
        mark.transform.localScale = size;
        Object.DestroyImmediate(mark.GetComponent<BoxCollider>());
        if (mat != null) mark.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private static void CreateStopLine(GameObject parent, string name, Vector3 pos, Vector3 size, Material mat)
    {
        GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.name = name;
        line.transform.SetParent(parent.transform, false);
        line.transform.localPosition = pos;
        line.transform.localScale = size;
        Object.DestroyImmediate(line.GetComponent<BoxCollider>());
        if (mat != null) line.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private static void CreateDashedLaneLine(GameObject parent, string name, float x, float zStart, float zEnd, float dashLen, float gapLen, Material mat)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent.transform, false);

        float curZ = zStart;
        int idx = 0;
        while (curZ < zEnd)
        {
            bool inIntersection = false;
            float[] crossZ = new float[] { -150f, -50f, 50f, 150f };
            foreach (var cz in crossZ)
            {
                if (Mathf.Abs(curZ - cz) < 12f) { inIntersection = true; break; }
            }

            if (!inIntersection)
            {
                var dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dash.name = $"Dash_{idx++}";
                dash.transform.SetParent(container.transform, false);
                dash.transform.localPosition = new Vector3(x, 0.035f, curZ + dashLen * 0.5f);
                dash.transform.localScale = new Vector3(0.22f, 0.01f, dashLen);
                Object.DestroyImmediate(dash.GetComponent<BoxCollider>());
                if (mat != null) dash.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }

            curZ += dashLen + gapLen;
        }
    }

    private static void CreateDashedLaneLineEW(GameObject parent, string name, float z, float xStart, float xEnd, float dashLen, float gapLen, Material mat)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent.transform, false);

        float curX = xStart;
        int idx = 0;
        while (curX < xEnd)
        {
            if (Mathf.Abs(curX) > 13f)
            {
                var dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dash.name = $"Dash_{idx++}";
                dash.transform.SetParent(container.transform, false);
                dash.transform.localPosition = new Vector3(curX + dashLen * 0.5f, 0.035f, z);
                dash.transform.localScale = new Vector3(dashLen, 0.01f, 0.22f);
                Object.DestroyImmediate(dash.GetComponent<BoxCollider>());
                if (mat != null) dash.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }

            curX += dashLen + gapLen;
        }
    }

    private static void CreateSidewalk(GameObject parent, string name, Vector3 pos, Vector3 size, Material mat)
    {
        GameObject sw = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sw.name = name;
        sw.transform.SetParent(parent.transform, false);
        sw.transform.localPosition = pos;
        sw.transform.localScale = size;
        if (mat != null) sw.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private static void CreateZebraCrossing(GameObject parent, string name, Vector3 center, Material mat)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent.transform, false);

        int stripeCount = 10;
        float stripeWidth = 0.60f;
        float spacing = 1.15f;
        float startX = -((stripeCount - 1) * spacing) * 0.5f;

        for (int i = 0; i < stripeCount; i++)
        {
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = $"Stripe_{i}";
            stripe.transform.SetParent(container.transform, false);
            stripe.transform.localPosition = new Vector3(startX + i * spacing, 0.04f, center.z);
            stripe.transform.localScale = new Vector3(stripeWidth, 0.01f, 3.4f);
            Object.DestroyImmediate(stripe.GetComponent<BoxCollider>());
            if (mat != null) stripe.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }
    }
    #endregion

    #region Colorful 3D Buildings & City Blocks
    private static void BuildColorfulCityBlocks(GameObject parent, Dictionary<string, Material> mats)
    {
        GameObject blocksRoot = new GameObject("CityBlocks");
        blocksRoot.transform.SetParent(parent.transform, false);

        Material sapphireGlass = mats["Mat_Building_ModernGlass"];
        Material emeraldGlass = mats["Mat_Building_EmeraldGlass"];
        Material ivoryConc = mats["Mat_Building_Concrete_1"];
        Material slateConc = mats["Mat_Building_Concrete_2"];
        Material redBrick = mats["Mat_Building_Brick"];
        Material storefront = mats["Mat_Building_Storefront"];
        Material whiteTrim = mats["Mat_Building_WhiteTrim"];
        Material grassMat = mats["Mat_Grass_Park"];
        Material trunkMat = mats["Mat_Tree_Trunk"];
        Material leafAutumnRed = mats["Mat_Tree_Leaves_Autumn_Red"];
        Material leafAutumnOrange = mats["Mat_Tree_Leaves_Autumn_Orange"];
        Material leafGreen = mats["Mat_Tree_Leaves_Green"];
        Material lampPole = mats["Mat_StreetLight_Pole"];
        Material lampGlow = mats["Mat_StreetLight_Glow"];

        // 12 Urban Blocks (3 columns X x 4 rows Z)
        float[] blockXs = new float[] { -125f, -55f, 55f, 125f };
        float[] blockZs = new float[] { -100f, 0f, 100f };

        int blockId = 0;
        foreach (float bx in blockXs)
        {
            foreach (float bz in blockZs)
            {
                blockId++;
                Vector3 bCenter = new Vector3(bx, 0f, bz);

                // Blocks 5 and 8 are lush Central Parks with vibrant autumn and green trees
                if (blockId == 5 || blockId == 8)
                {
                    BuildParkBlock(blocksRoot, bCenter, grassMat, trunkMat, leafAutumnRed, leafAutumnOrange, leafGreen);
                }
                else
                {
                    BuildColorfulUrbanBlock(blocksRoot, blockId, bCenter, sapphireGlass, emeraldGlass, ivoryConc, slateConc, redBrick, storefront, whiteTrim);
                }
            }
        }

        // Add Streetlights and Colorful Autumn Trees along Boulevard
        for (float z = -200f; z <= 200f; z += 28f)
        {
            if (Mathf.Abs(z - (-150f)) < 14f || Mathf.Abs(z - (-50f)) < 14f ||
                Mathf.Abs(z - 50f) < 14f || Mathf.Abs(z - 150f) < 14f) continue;

            // West side
            CreateStreetlight(blocksRoot, new Vector3(-10.2f, 0f, z), lampPole, lampGlow, false);
            Material westFoliage = (z > 0) ? leafAutumnRed : leafAutumnOrange;
            CreateColorfulTree(blocksRoot, new Vector3(-11.8f, 0f, z + 14f), trunkMat, westFoliage);

            // East side
            CreateStreetlight(blocksRoot, new Vector3(10.2f, 0f, z), lampPole, lampGlow, true);
            Material eastFoliage = (z > 0) ? leafAutumnOrange : leafGreen;
            CreateColorfulTree(blocksRoot, new Vector3(11.8f, 0f, z + 14f), trunkMat, eastFoliage);
        }
    }

    private static void BuildColorfulUrbanBlock(GameObject parent, int id, Vector3 center,
        Material sapphire, Material emerald, Material ivory, Material slate, Material brick, Material store, Material white)
    {
        GameObject block = new GameObject($"Block_{id}");
        block.transform.SetParent(parent.transform, false);
        block.transform.position = center;

        // Variety of distinct architectural buildings in each block
        // 1. Sapphire Glass Skyscraper
        float h1 = Random.Range(55f, 85f);
        CreateArchitecturalBuilding(block, "Sapphire_Skyscraper", new Vector3(-12f, 0f, -14f), new Vector3(26f, h1, 30f), sapphire, white, store);

        // 2. Terracotta Red Brick Apartment Tower
        float h2 = Random.Range(24f, 40f);
        CreateArchitecturalBuilding(block, "Brick_Apartments", new Vector3(14f, 0f, -14f), new Vector3(24f, h2, 28f), brick, ivory, store);

        // 3. Ivory Limestone Commercial Office
        float h3 = Random.Range(32f, 52f);
        CreateArchitecturalBuilding(block, "Ivory_Commercial", new Vector3(-12f, 0f, 18f), new Vector3(26f, h3, 26f), ivory, sapphire, store);

        // 4. Emerald Glass & Modern Slate Plaza
        float h4 = Random.Range(20f, 32f);
        CreateArchitecturalBuilding(block, "Emerald_Plaza", new Vector3(14f, 0f, 18f), new Vector3(24f, h4, 26f), emerald, slate, store);
    }

    private static void CreateArchitecturalBuilding(GameObject parent, string name, Vector3 localPos, Vector3 size, Material facadeMat, Material trimMat, Material storeMat)
    {
        GameObject bld = new GameObject(name);
        bld.transform.SetParent(parent.transform, false);
        bld.transform.localPosition = localPos;

        // Main Tower Body
        GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tower.name = "Tower";
        tower.transform.SetParent(bld.transform, false);
        tower.transform.localPosition = new Vector3(0f, size.y * 0.5f, 0f);
        tower.transform.localScale = size;
        if (facadeMat != null) tower.GetComponent<MeshRenderer>().sharedMaterial = facadeMat;

        // Ground Floor Warm Lit Storefront Windows
        GameObject store = GameObject.CreatePrimitive(PrimitiveType.Cube);
        store.name = "Storefront";
        store.transform.SetParent(bld.transform, false);
        store.transform.localPosition = new Vector3(0f, 2.2f, size.z * 0.5f + 0.1f);
        store.transform.localScale = new Vector3(size.x * 0.88f, 3.8f, 0.35f);
        Object.DestroyImmediate(store.GetComponent<BoxCollider>());
        if (storeMat != null) store.GetComponent<MeshRenderer>().sharedMaterial = storeMat;

        // Decorative Architectural Roof Trim
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "RoofStructure";
        roof.transform.SetParent(bld.transform, false);
        roof.transform.localPosition = new Vector3(0f, size.y + 1.2f, 0f);
        roof.transform.localScale = new Vector3(size.x * 0.92f, 2.4f, size.z * 0.92f);
        Object.DestroyImmediate(roof.GetComponent<BoxCollider>());
        if (trimMat != null) roof.GetComponent<MeshRenderer>().sharedMaterial = trimMat;

        // Helipad / Antenna Accent
        GameObject antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        antenna.name = "Antenna";
        antenna.transform.SetParent(bld.transform, false);
        antenna.transform.localPosition = new Vector3(0f, size.y + 6.0f, 0f);
        antenna.transform.localScale = new Vector3(0.35f, 4.5f, 0.35f);
        Object.DestroyImmediate(antenna.GetComponent<CapsuleCollider>());
        if (trimMat != null) antenna.GetComponent<MeshRenderer>().sharedMaterial = trimMat;
    }

    private static void BuildParkBlock(GameObject parent, Vector3 center, Material grassMat, Material trunkMat,
        Material autumnRed, Material autumnOrange, Material green)
    {
        GameObject park = new GameObject("CentralPark");
        park.transform.SetParent(parent.transform, false);
        park.transform.position = center;

        // Green Lawn
        GameObject lawn = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lawn.name = "ParkLawn";
        lawn.transform.SetParent(park.transform, false);
        lawn.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        lawn.transform.localScale = new Vector3(58f, 0.16f, 72f);
        if (grassMat != null) lawn.GetComponent<MeshRenderer>().sharedMaterial = grassMat;

        // Tree Cluster in Park
        Vector3[] treeOffsets = new Vector3[] {
            new Vector3(-16f, 0.16f, -22f), new Vector3(0f, 0.16f, -20f), new Vector3(16f, 0.16f, -22f),
            new Vector3(-18f, 0.16f, 0f), new Vector3(0f, 0.16f, 0f), new Vector3(18f, 0.16f, 0f),
            new Vector3(-16f, 0.16f, 22f), new Vector3(0f, 0.16f, 20f), new Vector3(16f, 0.16f, 22f)
        };

        for (int i = 0; i < treeOffsets.Length; i++)
        {
            Material leaf = (i % 3 == 0) ? autumnRed : ((i % 3 == 1) ? autumnOrange : green);
            CreateColorfulTree(park, treeOffsets[i], trunkMat, leaf);
        }
    }

    private static void CreateColorfulTree(GameObject parent, Vector3 pos, Material trunkMat, Material leafMat)
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
        if (trunkMat != null) trunk.GetComponent<MeshRenderer>().sharedMaterial = trunkMat;

        // 3-Tier Layered Foliage Crown
        GameObject l1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        l1.name = "Crown_Lower";
        l1.transform.SetParent(tree.transform, false);
        l1.transform.localPosition = new Vector3(0f, 4.4f, 0f);
        l1.transform.localScale = new Vector3(4.2f, 3.0f, 4.2f);
        Object.DestroyImmediate(l1.GetComponent<SphereCollider>());
        if (leafMat != null) l1.GetComponent<MeshRenderer>().sharedMaterial = leafMat;

        GameObject l2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        l2.name = "Crown_Mid";
        l2.transform.SetParent(tree.transform, false);
        l2.transform.localPosition = new Vector3(0.3f, 5.8f, -0.2f);
        l2.transform.localScale = new Vector3(3.2f, 2.5f, 3.2f);
        Object.DestroyImmediate(l2.GetComponent<SphereCollider>());
        if (leafMat != null) l2.GetComponent<MeshRenderer>().sharedMaterial = leafMat;

        GameObject l3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        l3.name = "Crown_Top";
        l3.transform.SetParent(tree.transform, false);
        l3.transform.localPosition = new Vector3(-0.2f, 7.0f, 0.2f);
        l3.transform.localScale = new Vector3(2.2f, 1.8f, 2.2f);
        Object.DestroyImmediate(l3.GetComponent<SphereCollider>());
        if (leafMat != null) l3.GetComponent<MeshRenderer>().sharedMaterial = leafMat;
    }

    private static void CreateStreetlight(GameObject parent, Vector3 pos, Material poleMat, Material glowMat, bool faceWest)
    {
        GameObject lightObj = new GameObject("StreetLight");
        lightObj.transform.SetParent(parent.transform, false);
        lightObj.transform.localPosition = pos;

        // Pole
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "Pole";
        pole.transform.SetParent(lightObj.transform, false);
        pole.transform.localPosition = new Vector3(0f, 3.6f, 0f);
        pole.transform.localScale = new Vector3(0.22f, 3.6f, 0.22f);
        if (poleMat != null) pole.GetComponent<MeshRenderer>().sharedMaterial = poleMat;

        // Overhang Arm
        float dir = faceWest ? -1f : 1f;
        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arm.name = "Arm";
        arm.transform.SetParent(lightObj.transform, false);
        arm.transform.localPosition = new Vector3(dir * 1.0f, 7.1f, 0f);
        arm.transform.localScale = new Vector3(2.0f, 0.16f, 0.22f);
        Object.DestroyImmediate(arm.GetComponent<BoxCollider>());
        if (poleMat != null) arm.GetComponent<MeshRenderer>().sharedMaterial = poleMat;

        // Glowing Lamp Head
        GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lamp.name = "LampHead";
        lamp.transform.SetParent(lightObj.transform, false);
        lamp.transform.localPosition = new Vector3(dir * 1.8f, 6.95f, 0f);
        lamp.transform.localScale = new Vector3(0.6f, 0.16f, 0.38f);
        Object.DestroyImmediate(lamp.GetComponent<BoxCollider>());
        if (glowMat != null) lamp.GetComponent<MeshRenderer>().sharedMaterial = glowMat;

        // Night Point Light
        GameObject pLight = new GameObject("LampLight");
        pLight.transform.SetParent(lightObj.transform, false);
        pLight.transform.localPosition = new Vector3(dir * 1.8f, 6.6f, 0f);
        Light l = pLight.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = 16f;
        l.intensity = 1.4f;
        l.color = new Color(1.0f, 0.95f, 0.82f);
        l.shadows = LightShadows.None;
    }

    private static void BuildPerimeterBarriers(GameObject parent, Dictionary<string, Material> mats)
    {
        Material barrierMat = mats["Mat_Barrier_Metal"];
        GameObject barriersRoot = new GameObject("Barriers");
        barriersRoot.transform.SetParent(parent.transform, false);

        CreateBarrierSection(barriersRoot, "Barrier_North", new Vector3(0f, 0.5f, 218f), new Vector3(440f, 1.0f, 0.4f), barrierMat);
        CreateBarrierSection(barriersRoot, "Barrier_South", new Vector3(0f, 0.5f, -218f), new Vector3(440f, 1.0f, 0.4f), barrierMat);
        CreateBarrierSection(barriersRoot, "Barrier_West", new Vector3(-218f, 0.5f, 0f), new Vector3(0.4f, 1.0f, 440f), barrierMat);
        CreateBarrierSection(barriersRoot, "Barrier_East", new Vector3(218f, 0.5f, 0f), new Vector3(0.4f, 1.0f, 440f), barrierMat);
    }

    private static void CreateBarrierSection(GameObject parent, string name, Vector3 pos, Vector3 size, Material mat)
    {
        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
        b.name = name;
        b.transform.SetParent(parent.transform, false);
        b.transform.localPosition = pos;
        b.transform.localScale = size;
        if (mat != null) b.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }
    #endregion

    #region Traffic Lights
    private static TrafficLightController CreateIntersectionTrafficLights(GameObject parent, Dictionary<string, Material> mats, Vector3 center)
    {
        GameObject tlcObj = new GameObject($"TrafficLight_Junction_{Mathf.RoundToInt(center.z)}");
        tlcObj.transform.SetParent(parent.transform, false);
        tlcObj.transform.localPosition = center;

        var tlc = tlcObj.AddComponent<TrafficLightController>();
        tlc.matRed = mats["Mat_TrafficLight_Red"];
        tlc.matYellow = mats["Mat_TrafficLight_Yellow"];
        tlc.matGreen = mats["Mat_TrafficLight_Green"];
        tlc.matOff = mats["Mat_TrafficLight_Off"];

        Material housingMat = mats["Mat_TrafficLight_Housing"];
        Material poleMat = mats["Mat_StreetLight_Pole"];

        Vector3[] corners = new Vector3[] {
            new Vector3(-10.2f, 0f, 8.2f),
            new Vector3(10.2f, 0f, 8.2f),
            new Vector3(-10.2f, 0f, -8.2f),
            new Vector3(10.2f, 0f, -8.2f)
        };

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 cPos = center + corners[i];
            CreateTrafficLightPole(tlcObj, i, cPos, poleMat, housingMat, tlc, i < 2);
        }

        CreateStopZone(tlcObj, "StopZone_North", center + new Vector3(4.5f, 0.5f, -11.5f), new Vector3(8f, 2f, 2f), tlc.northSouthStopZones);
        CreateStopZone(tlcObj, "StopZone_South", center + new Vector3(-4.5f, 0.5f, 11.5f), new Vector3(8f, 2f, 2f), tlc.northSouthStopZones);
        CreateStopZone(tlcObj, "StopZone_East", center + new Vector3(-11.5f, 0.5f, -3.5f), new Vector3(2f, 2f, 6f), tlc.eastWestStopZones);
        CreateStopZone(tlcObj, "StopZone_West", center + new Vector3(11.5f, 0.5f, 3.5f), new Vector3(2f, 2f, 6f), tlc.eastWestStopZones);

        return tlc;
    }

    private static void CreateTrafficLightPole(GameObject parent, int id, Vector3 pos, Material poleMat, Material housingMat, TrafficLightController tlc, bool isNorth)
    {
        GameObject pole = new GameObject($"Pole_{id}");
        pole.transform.SetParent(parent.transform, false);
        pole.transform.position = pos;

        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = "Post";
        post.transform.SetParent(pole.transform, false);
        post.transform.localPosition = new Vector3(0f, 3.0f, 0f);
        post.transform.localScale = new Vector3(0.24f, 3.0f, 0.24f);
        if (poleMat != null) post.GetComponent<MeshRenderer>().sharedMaterial = poleMat;

        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "Housing";
        box.transform.SetParent(pole.transform, false);
        box.transform.localPosition = new Vector3(0f, 5.2f, 0f);
        box.transform.localScale = new Vector3(0.45f, 1.35f, 0.45f);
        Object.DestroyImmediate(box.GetComponent<BoxCollider>());
        if (housingMat != null) box.GetComponent<MeshRenderer>().sharedMaterial = housingMat;

        MeshRenderer r = CreateLamp(box.transform, "Lamp_Red", new Vector3(0f, 0.35f, 0.24f), tlc.matRed);
        MeshRenderer y = CreateLamp(box.transform, "Lamp_Yellow", new Vector3(0f, 0.0f, 0.24f), tlc.matYellow);
        MeshRenderer g = CreateLamp(box.transform, "Lamp_Green", new Vector3(0f, -0.35f, 0.24f), tlc.matGreen);

        if (isNorth)
        {
            tlc.nsRedLamps.Add(r);
            tlc.nsYellowLamps.Add(y);
            tlc.nsGreenLamps.Add(g);
        }
        else
        {
            tlc.ewRedLamps.Add(r);
            tlc.ewYellowLamps.Add(y);
            tlc.ewGreenLamps.Add(g);
        }
    }

    private static MeshRenderer CreateLamp(Transform parent, string name, Vector3 localPos, Material mat)
    {
        GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lamp.name = name;
        lamp.transform.SetParent(parent, false);
        lamp.transform.localPosition = localPos;
        lamp.transform.localScale = new Vector3(0.26f, 0.26f, 0.26f);
        Object.DestroyImmediate(lamp.GetComponent<SphereCollider>());
        var mr = lamp.GetComponent<MeshRenderer>();
        if (mat != null) mr.sharedMaterial = mat;
        return mr;
    }

    private static void CreateStopZone(GameObject parent, string name, Vector3 pos, Vector3 size, List<TrafficStopZone> list)
    {
        GameObject zoneObj = new GameObject(name);
        zoneObj.transform.SetParent(parent.transform, false);
        zoneObj.transform.position = pos;

        BoxCollider bc = zoneObj.AddComponent<BoxCollider>();
        bc.size = size;
        bc.isTrigger = true;

        TrafficStopZone tsz = zoneObj.AddComponent<TrafficStopZone>();
        list.Add(tsz);
    }
    #endregion

    #region AI Traffic Waypoints
    private static List<TrafficWaypoint> GenerateTrafficWaypointGraph(GameObject parent, float[] crossZ)
    {
        List<TrafficWaypoint> waypoints = new List<TrafficWaypoint>();
        GameObject wpRoot = new GameObject("TrafficWaypoints");
        wpRoot.transform.SetParent(parent.transform, false);

        TrafficWaypoint prevNB = null;
        for (float z = -200f; z <= 200f; z += 40f)
        {
            var wp = CreateWaypoint(wpRoot, $"WP_NB_{Mathf.RoundToInt(z)}", new Vector3(4.5f, 0.3f, z), 50f);
            waypoints.Add(wp);
            if (prevNB != null) prevNB.nextWaypoints.Add(wp);
            prevNB = wp;
        }

        TrafficWaypoint prevSB = null;
        for (float z = 200f; z >= -200f; z -= 40f)
        {
            var wp = CreateWaypoint(wpRoot, $"WP_SB_{Mathf.RoundToInt(z)}", new Vector3(-4.5f, 0.3f, z), 50f);
            waypoints.Add(wp);
            if (prevSB != null) prevSB.nextWaypoints.Add(wp);
            prevSB = wp;
        }

        if (prevNB != null && waypoints.Count > 0) prevNB.nextWaypoints.Add(waypoints[waypoints.Count - 1]);
        if (prevSB != null && waypoints.Count > 0) prevSB.nextWaypoints.Add(waypoints[0]);

        foreach (float cz in crossZ)
        {
            TrafficWaypoint prevEB = null;
            for (float x = -180f; x <= 180f; x += 45f)
            {
                var wp = CreateWaypoint(wpRoot, $"WP_EB_{Mathf.RoundToInt(x)}_{Mathf.RoundToInt(cz)}", new Vector3(x, 0.3f, cz - 3.5f), 45f);
                waypoints.Add(wp);
                if (prevEB != null) prevEB.nextWaypoints.Add(wp);
                prevEB = wp;
            }

            TrafficWaypoint prevWB = null;
            for (float x = 180f; x >= -180f; x -= 45f)
            {
                var wp = CreateWaypoint(wpRoot, $"WP_WB_{Mathf.RoundToInt(x)}_{Mathf.RoundToInt(cz)}", new Vector3(x, 0.3f, cz + 3.5f), 45f);
                waypoints.Add(wp);
                if (prevWB != null) prevWB.nextWaypoints.Add(wp);
                prevWB = wp;
            }

            if (prevEB != null && prevWB != null)
            {
                prevEB.nextWaypoints.Add(prevWB);
            }
        }

        return waypoints;
    }

    private static TrafficWaypoint CreateWaypoint(GameObject parent, string name, Vector3 pos, float speed)
    {
        GameObject wpObj = new GameObject(name);
        wpObj.transform.SetParent(parent.transform, false);
        wpObj.transform.position = pos;

        TrafficWaypoint tw = wpObj.AddComponent<TrafficWaypoint>();
        tw.targetSpeedKmh = speed;
        return tw;
    }

    private static void SetupTrafficManager(List<TrafficWaypoint> waypoints, Dictionary<string, Material> mats)
    {
        GameObject tmObj = new GameObject("TrafficSystem");
        var tm = tmObj.AddComponent<TrafficManager>();
        tm.maxTrafficCars = 14;

        tm.carPaintMaterials = new Material[] {
            mats["Mat_TrafficCar_Body1"],
            mats["Mat_TrafficCar_Body2"],
            mats["Mat_TrafficCar_Body3"],
            mats["Mat_TrafficCar_Body4"],
            mats["Mat_TrafficCar_Body5"]
        };
    }
    #endregion

    #region Checkpoints & Missions
    private static void SetupGameplayManager(Dictionary<string, Material> mats)
    {
        GameObject gmObj = new GameObject("GameplayManager");
        var gm = gmObj.AddComponent<GameplayManager>();
        gm.currentMode = GameplayManager.GameMode.CheckpointSprint;

        Material beaconMat = mats["Mat_Checkpoint_Beacon"];

        Vector3[] cpPositions = new Vector3[] {
            new Vector3(4.5f, 0f, 40f),    // CP 1: Start straight boulevard
            new Vector3(4.5f, 0f, 135f),   // CP 2: North intersection approach
            new Vector3(65f, 0f, 146.5f),  // CP 3: East avenue sweep
            new Vector3(190f, 0f, 146.5f), // CP 4: Outer highway entrance
            new Vector3(210f, 0f, 0f),     // CP 5: Highway top-speed straight
            new Vector3(75f, 0f, -53.5f),  // CP 6: Downtown turn
            new Vector3(-65f, 0f, -53.5f), // CP 7: West commercial district
            new Vector3(-4.5f, 0f, -135f)  // CP 8: Grand Avenue finish line!
        };

        for (int i = 0; i < cpPositions.Length; i++)
        {
            var cp = CreateCheckpoint(gmObj, i, cpPositions[i], beaconMat);
            gm.sprintCheckpoints.Add(cp);
        }
    }

    private static Checkpoint CreateCheckpoint(GameObject parent, int index, Vector3 pos, Material beaconMat)
    {
        GameObject cpObj = new GameObject($"Checkpoint_{index + 1}");
        cpObj.transform.SetParent(parent.transform, false);
        cpObj.transform.position = pos;

        BoxCollider bc = cpObj.AddComponent<BoxCollider>();
        bc.size = new Vector3(9.5f, 5.5f, 5.0f);
        bc.isTrigger = true;

        Checkpoint cp = cpObj.AddComponent<Checkpoint>();
        cp.checkpointIndex = index;
        cp.bonusTime = 16f;
        cp.bonusCash = 300;

        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Ring";
        ring.transform.SetParent(cpObj.transform, false);
        ring.transform.localPosition = new Vector3(0f, 0.15f, 0f);
        ring.transform.localScale = new Vector3(8.5f, 0.15f, 8.5f);
        Object.DestroyImmediate(ring.GetComponent<CapsuleCollider>());
        if (beaconMat != null) ring.GetComponent<MeshRenderer>().sharedMaterial = beaconMat;

        GameObject chevron = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chevron.name = "Chevron";
        chevron.transform.SetParent(cpObj.transform, false);
        chevron.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        chevron.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        chevron.transform.localRotation = Quaternion.Euler(45f, 45f, 0f);
        Object.DestroyImmediate(chevron.GetComponent<BoxCollider>());
        if (beaconMat != null) chevron.GetComponent<MeshRenderer>().sharedMaterial = beaconMat;
        cp.rotatingChevron = chevron.transform;

        GameObject pLight = new GameObject("BeaconLight");
        pLight.transform.SetParent(cpObj.transform, false);
        pLight.transform.localPosition = new Vector3(0f, 2.0f, 0f);
        Light l = pLight.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = 16f;
        l.color = new Color(0.1f, 0.85f, 1.0f);
        l.intensity = 2.5f;
        cp.beaconLight = l;

        cpObj.SetActive(false);
        return cp;
    }
    #endregion

    #region UI & Minimap
    private static void SetupGameUI()
    {
        GameObject canvasObj = new GameObject("UI_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        GameUIManager uiManager = canvasObj.AddComponent<GameUIManager>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        BuildSpeedometerWidget(canvasObj.transform, font, uiManager);
        BuildMissionHUDWidget(canvasObj.transform, font, uiManager);
        BuildMinimapWidget(canvasObj.transform, font);
        BuildControlButtons(canvasObj.transform, font, uiManager);
        BuildMenusAndPopups(canvasObj.transform, font, uiManager);
    }

    private static void BuildSpeedometerWidget(Transform canvasTr, Font font, GameUIManager ui)
    {
        GameObject speedPanel = new GameObject("SpeedometerPanel");
        speedPanel.transform.SetParent(canvasTr, false);
        RectTransform rt = speedPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-30f, -25f);
        rt.sizeDelta = new Vector2(260f, 160f);

        Image bg = speedPanel.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.08f, 0.12f, 0.85f);

        GameObject stObj = new GameObject("SpeedValue");
        stObj.transform.SetParent(speedPanel.transform, false);
        RectTransform srt = stObj.AddComponent<RectTransform>();
        srt.anchorMin = new Vector2(0f, 0.35f);
        srt.anchorMax = new Vector2(0.75f, 1.0f);
        srt.sizeDelta = Vector2.zero;

        Text st = stObj.AddComponent<Text>();
        st.font = font;
        st.fontSize = 68;
        st.fontStyle = FontStyle.Bold;
        st.alignment = TextAnchor.MiddleCenter;
        st.color = Color.white;
        st.text = "0";
        ui.speedText = st;

        GameObject kmhObj = new GameObject("KMH_Label");
        kmhObj.transform.SetParent(speedPanel.transform, false);
        RectTransform krt = kmhObj.AddComponent<RectTransform>();
        krt.anchorMin = new Vector2(0.72f, 0.45f);
        krt.anchorMax = new Vector2(1.0f, 0.9f);
        krt.sizeDelta = Vector2.zero;

        Text kt = kmhObj.AddComponent<Text>();
        kt.font = font;
        kt.fontSize = 20;
        kt.fontStyle = FontStyle.Bold;
        kt.alignment = TextAnchor.MiddleLeft;
        kt.color = new Color(0.6f, 0.85f, 1.0f);
        kt.text = "KM/H";

        GameObject gtObj = new GameObject("GearValue");
        gtObj.transform.SetParent(speedPanel.transform, false);
        RectTransform grt = gtObj.AddComponent<RectTransform>();
        grt.anchorMin = new Vector2(0.72f, 0.15f);
        grt.anchorMax = new Vector2(1.0f, 0.45f);
        grt.sizeDelta = Vector2.zero;

        Text gt = gtObj.AddComponent<Text>();
        gt.font = font;
        gt.fontSize = 28;
        gt.fontStyle = FontStyle.Bold;
        gt.alignment = TextAnchor.MiddleLeft;
        gt.color = new Color(0.2f, 0.9f, 1f);
        gt.text = "N";
        ui.gearText = gt;

        GameObject rtObj = new GameObject("RPM_Value");
        rtObj.transform.SetParent(speedPanel.transform, false);
        RectTransform rrt = rtObj.AddComponent<RectTransform>();
        rrt.anchorMin = new Vector2(0.08f, 0.18f);
        rrt.anchorMax = new Vector2(0.65f, 0.38f);
        rrt.sizeDelta = Vector2.zero;

        Text rmt = rtObj.AddComponent<Text>();
        rmt.font = font;
        rmt.fontSize = 18;
        rmt.alignment = TextAnchor.MiddleLeft;
        rmt.color = new Color(0.7f, 0.75f, 0.8f);
        rmt.text = "850 RPM";
        ui.rpmText = rmt;

        GameObject barBg = new GameObject("SpeedBarBg");
        barBg.transform.SetParent(speedPanel.transform, false);
        RectTransform bgrt = barBg.AddComponent<RectTransform>();
        bgrt.anchorMin = new Vector2(0.05f, 0.05f);
        bgrt.anchorMax = new Vector2(0.95f, 0.14f);
        bgrt.sizeDelta = Vector2.zero;

        Image bgi = barBg.AddComponent<Image>();
        bgi.color = new Color(0.2f, 0.25f, 0.3f);

        GameObject barFill = new GameObject("SpeedBarFill");
        barFill.transform.SetParent(barBg.transform, false);
        RectTransform frt = barFill.AddComponent<RectTransform>();
        frt.anchorMin = Vector2.zero;
        frt.anchorMax = Vector2.one;
        frt.sizeDelta = Vector2.zero;

        Image fi = barFill.AddComponent<Image>();
        fi.color = new Color(0.1f, 0.85f, 1.0f);
        fi.type = Image.Type.Filled;
        fi.fillMethod = Image.FillMethod.Horizontal;
        fi.fillAmount = 0f;
        ui.speedGaugeBar = fi;
    }

    private static void BuildMissionHUDWidget(Transform canvasTr, Font font, GameUIManager ui)
    {
        GameObject mPanel = new GameObject("MissionHUDPanel");
        mPanel.transform.SetParent(canvasTr, false);
        RectTransform rt = mPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -20f);
        rt.sizeDelta = new Vector2(440f, 95f);

        Image bg = mPanel.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.08f, 0.12f, 0.82f);

        GameObject titObj = new GameObject("MissionTitle");
        titObj.transform.SetParent(mPanel.transform, false);
        RectTransform trt = titObj.AddComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.05f, 0.65f);
        trt.anchorMax = new Vector2(0.65f, 0.95f);
        trt.sizeDelta = Vector2.zero;

        Text tit = titObj.AddComponent<Text>();
        tit.font = font;
        tit.fontSize = 20;
        tit.fontStyle = FontStyle.Bold;
        tit.alignment = TextAnchor.MiddleLeft;
        tit.color = new Color(0.95f, 0.85f, 0.15f);
        tit.text = "CHECKPOINT SPRINT";
        ui.missionTitleText = tit;

        GameObject objObj = new GameObject("MissionObjective");
        objObj.transform.SetParent(mPanel.transform, false);
        RectTransform ort = objObj.AddComponent<RectTransform>();
        ort.anchorMin = new Vector2(0.05f, 0.15f);
        ort.anchorMax = new Vector2(0.65f, 0.60f);
        ort.sizeDelta = Vector2.zero;

        Text ot = objObj.AddComponent<Text>();
        ot.font = font;
        ot.fontSize = 17;
        ot.alignment = TextAnchor.MiddleLeft;
        ot.color = Color.white;
        ot.text = "Reach Checkpoint 1 / 8";
        ui.missionObjectiveText = ot;

        GameObject timObj = new GameObject("TimerValue");
        timObj.transform.SetParent(mPanel.transform, false);
        RectTransform timrt = timObj.AddComponent<RectTransform>();
        timrt.anchorMin = new Vector2(0.68f, 0.50f);
        timrt.anchorMax = new Vector2(0.95f, 0.95f);
        timrt.sizeDelta = Vector2.zero;

        Text tt = timObj.AddComponent<Text>();
        tt.font = font;
        tt.fontSize = 26;
        tt.fontStyle = FontStyle.Bold;
        tt.alignment = TextAnchor.MiddleRight;
        tt.color = new Color(0.2f, 1f, 0.4f);
        tt.text = "01:15.00";
        ui.timerText = tt;

        GameObject cashObj = new GameObject("CashValue");
        cashObj.transform.SetParent(mPanel.transform, false);
        RectTransform crt = cashObj.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.68f, 0.12f);
        crt.anchorMax = new Vector2(0.95f, 0.48f);
        crt.sizeDelta = Vector2.zero;

        Text ct = cashObj.AddComponent<Text>();
        ct.font = font;
        ct.fontSize = 18;
        ct.fontStyle = FontStyle.Bold;
        ct.alignment = TextAnchor.MiddleRight;
        ct.color = new Color(1f, 0.85f, 0.2f);
        ct.text = "$1,500";
        ui.scoreCashText = ct;
    }

    private static void BuildMinimapWidget(Transform canvasTr, Font font)
    {
        GameObject mmPanel = new GameObject("MinimapRadar");
        mmPanel.transform.SetParent(canvasTr, false);
        RectTransform rt = mmPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(25f, -25f);
        rt.sizeDelta = new Vector2(220f, 220f);

        Image bg = mmPanel.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.08f, 0.12f, 0.75f);

        GameObject arrowObj = new GameObject("PlayerArrow");
        arrowObj.transform.SetParent(mmPanel.transform, false);
        RectTransform art = arrowObj.AddComponent<RectTransform>();
        art.anchorMin = new Vector2(0.5f, 0.5f);
        art.anchorMax = new Vector2(0.5f, 0.5f);
        art.sizeDelta = new Vector2(24f, 24f);

        Text at = arrowObj.AddComponent<Text>();
        at.font = font;
        at.fontSize = 24;
        at.fontStyle = FontStyle.Bold;
        at.alignment = TextAnchor.MiddleCenter;
        at.color = new Color(0.1f, 0.9f, 1.0f);
        at.text = "▲";

        var mmCtrl = mmPanel.AddComponent<MinimapController>();
        mmCtrl.playerIcon = art;
    }

    private static void BuildControlButtons(Transform canvasTr, Font font, GameUIManager ui)
    {
        CreatePedalButton(canvasTr, font, "GasPedal", "GAS\n(W)", new Vector2(1f, 0f), new Vector2(-60f, 50f), new Vector2(110f, 150f),
            new Color(0.1f, 0.75f, 0.35f, 0.85f), () => ui.OnGasDown(), () => ui.OnGasUp());

        CreatePedalButton(canvasTr, font, "BrakePedal", "BRAKE\n(S)", new Vector2(1f, 0f), new Vector2(-190f, 50f), new Vector2(100f, 120f),
            new Color(0.85f, 0.18f, 0.18f, 0.85f), () => ui.OnBrakeDown(), () => ui.OnBrakeUp());

        CreatePedalButton(canvasTr, font, "HandbrakeBtn", "DRIFT\n(Space)", new Vector2(1f, 0f), new Vector2(-305f, 50f), new Vector2(95f, 75f),
            new Color(0.9f, 0.55f, 0.1f, 0.85f), () => ui.OnHandbrakeDown(), () => ui.OnHandbrakeUp());

        CreatePedalButton(canvasTr, font, "SteerLeftBtn", "◀\n(A)", new Vector2(0f, 0f), new Vector2(50f, 50f), new Vector2(105f, 105f),
            new Color(0.15f, 0.22f, 0.32f, 0.85f), () => ui.OnSteerLeftDown(), () => ui.OnSteerLeftUp());

        CreatePedalButton(canvasTr, font, "SteerRightBtn", "▶\n(D)", new Vector2(0f, 0f), new Vector2(175f, 50f), new Vector2(105f, 105f),
            new Color(0.15f, 0.22f, 0.32f, 0.85f), () => ui.OnSteerRightDown(), () => ui.OnSteerRightUp());

        CreateSimpleButton(canvasTr, font, "CamBtn", "CAM (C)", new Vector2(1f, 1f), new Vector2(-30f, -200f), new Vector2(120f, 40f),
            () => ui.OnCameraSwitchClick());

        CreateSimpleButton(canvasTr, font, "LightsBtn", "LIGHTS (H)", new Vector2(1f, 1f), new Vector2(-160f, -200f), new Vector2(120f, 40f),
            () => ui.OnHeadlightsToggleClick());

        CreateSimpleButton(canvasTr, font, "ResetBtn", "RESET (R)", new Vector2(1f, 1f), new Vector2(-30f, -250f), new Vector2(120f, 40f),
            () => ui.OnResetCarClick());

        CreateSimpleButton(canvasTr, font, "PauseBtn", "❚❚ PAUSE", new Vector2(1f, 1f), new Vector2(-160f, -250f), new Vector2(120f, 40f),
            () => ui.TogglePause());
    }

    private static void CreatePedalButton(Transform parent, Font font, string name, string label,
        Vector2 anchor, Vector2 pos, Vector2 size, Color col, System.Action onDown, System.Action onUp)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = col;

        Button btn = btnObj.AddComponent<Button>();

        var trigger = btnObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var entryDown = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
        entryDown.callback.AddListener((data) => onDown());
        trigger.triggers.Add(entryDown);

        var entryUp = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
        entryUp.callback.AddListener((data) => onUp());
        trigger.triggers.Add(entryUp);

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform trt = txtObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        Text t = txtObj.AddComponent<Text>();
        t.font = font;
        t.fontSize = 20;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.text = label;
    }

    private static void CreateSimpleButton(Transform parent, Font font, string name, string label,
        Vector2 anchor, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.12f, 0.16f, 0.24f, 0.85f);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform trt = txtObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        Text t = txtObj.AddComponent<Text>();
        t.font = font;
        t.fontSize = 17;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.text = label;
    }

    private static void BuildMenusAndPopups(Transform canvasTr, Font font, GameUIManager ui)
    {
        GameObject pauseObj = CreateModalPanel(canvasTr, "PauseMenuPanel", 500f, 440f);
        ui.pauseMenuPanel = pauseObj;

        CreateMenuHeader(pauseObj.transform, font, "GAME PAUSED");
        CreateMenuButton(pauseObj.transform, font, "ResumeBtn", "RESUME", 30f, () => ui.ResumeGame());
        CreateMenuButton(pauseObj.transform, font, "RestartBtn", "RESTART MISSION", -40f, () => ui.RestartMission());
        CreateMenuButton(pauseObj.transform, font, "FreeRoamBtn", "FREE ROAM MODE", -110f, () => ui.SetFreeRoamMode());
        CreateMenuButton(pauseObj.transform, font, "AudioBtn", "AUDIO: ON", -180f, () => ui.ToggleAudio());

        GameObject compObj = CreateModalPanel(canvasTr, "MissionCompletePanel", 560f, 380f);
        ui.missionCompletePanel = compObj;

        CreateMenuHeader(compObj.transform, font, "MISSION COMPLETED!");
        ui.completeStatsText = CreateStatsText(compObj.transform, font, "Stats here");
        CreateMenuButton(compObj.transform, font, "NextBtn", "NEXT MISSION", -50f, () => ui.SetNextMission());
        CreateMenuButton(compObj.transform, font, "FreeBtn", "FREE ROAM", -125f, () => ui.SetFreeRoamMode());

        GameObject failObj = CreateModalPanel(canvasTr, "MissionFailedPanel", 560f, 340f);
        ui.missionFailedPanel = failObj;

        CreateMenuHeader(failObj.transform, font, "MISSION FAILED");
        ui.failedStatsText = CreateStatsText(failObj.transform, font, "Reason here");
        CreateMenuButton(failObj.transform, font, "RetryBtn", "TRY AGAIN", -45f, () => ui.RestartMission());
        CreateMenuButton(failObj.transform, font, "FreeBtn2", "FREE ROAM", -120f, () => ui.SetFreeRoamMode());
    }

    private static GameObject CreateModalPanel(Transform parent, string name, float width, float height)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, height);

        Image img = panel.AddComponent<Image>();
        img.color = new Color(0.05f, 0.07f, 0.11f, 0.95f);
        panel.SetActive(false);
        return panel;
    }

    private static void CreateMenuHeader(Transform parent, Font font, string title)
    {
        GameObject tObj = new GameObject("Header");
        tObj.transform.SetParent(parent, false);
        RectTransform rt = tObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.78f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.sizeDelta = Vector2.zero;

        Text t = tObj.AddComponent<Text>();
        t.font = font;
        t.fontSize = 32;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(1f, 0.85f, 0.15f);
        t.text = title;
    }

    private static Text CreateStatsText(Transform parent, Font font, string text)
    {
        GameObject tObj = new GameObject("Stats");
        tObj.transform.SetParent(parent, false);
        RectTransform rt = tObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.08f, 0.42f);
        rt.anchorMax = new Vector2(0.92f, 0.75f);
        rt.sizeDelta = Vector2.zero;

        Text t = tObj.AddComponent<Text>();
        t.font = font;
        t.fontSize = 20;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.text = text;
        return t;
    }

    private static void CreateMenuButton(Transform parent, Font font, string name, string label, float yPos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, yPos);
        rt.sizeDelta = new Vector2(340f, 50f);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.22f, 0.35f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform trt = txtObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        Text t = txtObj.AddComponent<Text>();
        t.font = font;
        t.fontSize = 20;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.text = label;
    }
    #endregion

    #region Audio & Player Setup
    private static void SetupAudioManager()
    {
        GameObject audioObj = new GameObject("AudioManager");
        audioObj.AddComponent<CarAudioManager>();
    }

    private static void SetupPlayerCarAndCamera()
    {
        GameObject car = GameObject.Find("SportsSedan");
        if (car != null)
        {
            // Position on right lane of central boulevard facing forward (+Z), perfectly resting on road
            car.transform.position = new Vector3(4.5f, 0.15f, 15f);
            car.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            var rb = car.GetComponent<Rigidbody>();
            if (rb == null) rb = car.AddComponent<Rigidbody>();
            rb.mass = 1450f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // 1. Setup smooth low-friction physics material for wheels
            #if UNITY_6000_0_OR_NEWER
            PhysicsMaterial tireMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/Materials/Car/Mat_Car_TireFriction.physicMaterial");
            if (tireMat == null)
            {
                tireMat = new PhysicsMaterial("Mat_Car_TireFriction")
                {
                    dynamicFriction = 0.02f,
                    staticFriction = 0.02f,
                    bounciness = 0.0f,
                    frictionCombine = PhysicsMaterialCombine.Minimum,
                    bounceCombine = PhysicsMaterialCombine.Minimum
                };
                AssetDatabase.CreateAsset(tireMat, "Assets/Materials/Car/Mat_Car_TireFriction.physicMaterial");
            }
            #else
            PhysicMaterial tireMat = AssetDatabase.LoadAssetAtPath<PhysicMaterial>("Assets/Materials/Car/Mat_Car_TireFriction.physicMaterial");
            if (tireMat == null)
            {
                tireMat = new PhysicMaterial("Mat_Car_TireFriction")
                {
                    dynamicFriction = 0.02f,
                    staticFriction = 0.02f,
                    bounciness = 0.0f,
                    frictionCombine = PhysicMaterialCombine.Minimum,
                    bounceCombine = PhysicMaterialCombine.Minimum
                };
                AssetDatabase.CreateAsset(tireMat, "Assets/Materials/Car/Mat_Car_TireFriction.physicMaterial");
            }
            #endif

            // 2. Body BoxCollider: raised off ground so it NEVER scrapes on roads, only detects car/building crashes
            BoxCollider boxCol = car.GetComponent<BoxCollider>();
            if (boxCol == null) boxCol = car.AddComponent<BoxCollider>();
            boxCol.center = new Vector3(0f, 0.82f, 0.02f);
            boxCol.size = new Vector3(1.88f, 0.65f, 4.80f);
            boxCol.material = tireMat;

            // 3. 4 Smooth Wheel SphereColliders: guarantees the car rolls effortlessly over any road seam
            Transform oldWc = car.transform.Find("WheelColliders");
            if (oldWc != null) Object.DestroyImmediate(oldWc.gameObject);

            GameObject wcObj = new GameObject("WheelColliders");
            wcObj.transform.SetParent(car.transform, false);

            Vector3[] wheelOffsets = new Vector3[] {
                new Vector3(-0.88f, 0.34f, 1.43f),  // FL
                new Vector3(0.88f, 0.34f, 1.43f),   // FR
                new Vector3(-0.88f, 0.34f, -1.43f), // RL
                new Vector3(0.88f, 0.34f, -1.43f)   // RR
            };
            string[] wheelNames = new string[] { "Col_FL", "Col_FR", "Col_RL", "Col_RR" };

            for (int w = 0; w < wheelOffsets.Length; w++)
            {
                GameObject wColGo = new GameObject(wheelNames[w]);
                wColGo.transform.SetParent(wcObj.transform, false);
                wColGo.transform.localPosition = wheelOffsets[w];
                var sc = wColGo.AddComponent<SphereCollider>();
                sc.radius = 0.34f;
                sc.material = tireMat;
            }

            var cc = car.GetComponent<CarController>();
            if (cc == null) cc = car.AddComponent<CarController>();

            var cv = car.GetComponent<CarVisuals>();
            if (cv == null) cv = car.AddComponent<CarVisuals>();
        }

        GameObject camObj = GameObject.FindWithTag("MainCamera");
        if (camObj == null)
        {
            var cam = Object.FindFirstObjectByType<Camera>();
            if (cam != null) camObj = cam.gameObject;
        }

        if (camObj != null && car != null)
        {
            var camFollower = camObj.GetComponent<CarCameraController>();
            if (camFollower == null) camFollower = camObj.AddComponent<CarCameraController>();
            camFollower.target = car.transform;
            camFollower.currentMode = CarCameraController.CameraViewMode.ChaseMedium;
            camFollower.mediumOffset = new Vector3(0f, 2.3f, -6.5f);
            camFollower.mediumLookHeight = 1.15f;
        }
    }

    private static void EnsureSceneInBuildSettings()
    {
        string scenePath = "Assets/Scenes/MainScene.unity";
        var currentScenes = EditorBuildSettings.scenes;
        bool exists = false;
        foreach (var s in currentScenes)
        {
            if (s.path == scenePath) { exists = true; break; }
        }

        if (!exists)
        {
            var newScenes = new EditorBuildSettingsScene[currentScenes.Length + 1];
            newScenes[0] = new EditorBuildSettingsScene(scenePath, true);
            for (int i = 0; i < currentScenes.Length; i++)
            {
                newScenes[i + 1] = currentScenes[i];
            }
            EditorBuildSettings.scenes = newScenes;
        }
    }
    #endregion
}
