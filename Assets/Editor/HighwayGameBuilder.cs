using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Automated Highway Overtake Game Scene Builder.
/// Generates:
/// 1. Multi-lane infinite highway system with endless chunk recycling.
/// 2. Overhead highway signboards, gantries, steel crash barriers, streetlights, and colorful roadside trees.
/// 3. High-speed cruising traffic stream (20 vehicles across 4 lanes with varying speeds).
/// 4. Overtake & Near-Miss combo scoring detector with floating HUD rewards.
/// 5. Digital speedometer, gear indicator, touch pedals, pause menu, and procedural audio.
/// 6. Configures player Mercedes sports sedan with smooth wheel physics and chase camera.
/// </summary>
public class HighwayGameBuilder
{
    private const string MAT_DIR = "Assets/Materials/Environment";

    [MenuItem("Tools/Build Highway Overtake Game")]
    public static void BuildHighwayGame()
    {
        Debug.Log("<color=cyan>[HighwayGameBuilder]</color> Building High-Speed Highway Overtake Game...");

        EnsureDirectory(MAT_DIR);

        // 1. Open MainScene
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Single);

        // 2. Setup Materials
        var mats = CreateOrUpdateMaterials();

        // 3. Clean up previous environment / systems (preserves SportsSedan)
        CleanUpOldObjects();

        // 4. Setup Lighting
        SetupLighting();

        // 5. Setup Endless Highway Manager
        GameObject highwayObj = new GameObject("HighwayManager");
        var hm = highwayObj.AddComponent<HighwayManager>();
        hm.asphaltMat = mats["Mat_Road_Asphalt"];
        hm.yellowLineMat = mats["Mat_Road_Marking_Yellow"];
        hm.whiteLineMat = mats["Mat_Road_Marking_White"];
        hm.barrierMat = mats["Mat_Barrier_Metal"];
        hm.poleMat = mats["Mat_StreetLight_Pole"];
        hm.lampGlowMat = mats["Mat_StreetLight_Glow"];
        hm.grassMat = mats["Mat_Grass_Park"];
        hm.treeTrunkMat = mats["Mat_Tree_Trunk"];
        hm.treeLeafRedMat = mats["Mat_Tree_Leaves_Autumn_Red"];
        hm.treeLeafOrangeMat = mats["Mat_Tree_Leaves_Autumn_Orange"];
        hm.treeLeafGreenMat = mats["Mat_Tree_Leaves_Green"];
        hm.gantrySignMat = mats["Mat_Highway_Sign"];

        // 6. Setup Highway Traffic Manager
        GameObject trafficObj = new GameObject("HighwayTrafficManager");
        var htm = trafficObj.AddComponent<HighwayTrafficManager>();
        htm.totalTrafficCars = 20;
        htm.carPaintMaterials = new Material[] {
            mats["Mat_TrafficCar_Body1"],
            mats["Mat_TrafficCar_Body2"],
            mats["Mat_TrafficCar_Body3"],
            mats["Mat_TrafficCar_Body4"],
            mats["Mat_TrafficCar_Body5"]
        };
        htm.taillightNormalMat = mats["Mat_TrafficLight_Off"];
        htm.taillightBrakeMat = mats["Mat_TrafficLight_Red"];

        // 7. Setup Highway Overtake Detector
        GameObject overtakeObj = new GameObject("HighwayOvertakeDetector");
        var hod = overtakeObj.AddComponent<HighwayOvertakeDetector>();
        hod.trafficManager = htm;

        // 8. Setup Highway HUD UI Canvas
        SetupHighwayUI(hod);

        // 9. Setup Procedural Audio Manager
        SetupAudio();

        // 10. Setup Player Car & Camera
        SetupPlayerCarAndCamera();

        // 11. Save Scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=green>[HighwayGameBuilder]</color> Highway Overtake Game successfully built! Ready to play!");
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

    private static void CleanUpOldObjects()
    {
        string[] names = new string[] {
            "Environment", "TrafficSystem", "GameplayManager", "UI_Canvas", "AudioManager",
            "HighwayManager", "HighwayTrafficManager", "HighwayOvertakeDetector", "Plane"
        };
        foreach (var n in names)
        {
            var obj = GameObject.Find(n);
            if (obj != null) Object.DestroyImmediate(obj);
        }
    }

    private static Dictionary<string, Material> CreateOrUpdateMaterials()
    {
        var dict = new Dictionary<string, Material>();
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");

        dict["Mat_Road_Asphalt"] = GetOrCreateMat("Mat_Road_Asphalt", litShader,
            new Color(0.12f, 0.13f, 0.15f, 1f), 0.15f, 0.35f);

        dict["Mat_Road_Marking_Yellow"] = GetOrCreateMat("Mat_Road_Marking_Yellow", litShader,
            new Color(0.98f, 0.78f, 0.05f, 1f), 0.0f, 0.25f);

        dict["Mat_Road_Marking_White"] = GetOrCreateMat("Mat_Road_Marking_White", litShader,
            new Color(0.98f, 0.98f, 1.0f, 1f), 0.0f, 0.25f);

        dict["Mat_Barrier_Metal"] = GetOrCreateMat("Mat_Barrier_Metal", litShader,
            new Color(0.78f, 0.80f, 0.82f, 1f), 0.85f, 0.65f);

        dict["Mat_StreetLight_Pole"] = GetOrCreateMat("Mat_StreetLight_Pole", litShader,
            new Color(0.20f, 0.22f, 0.25f, 1f), 0.70f, 0.50f);

        dict["Mat_StreetLight_Glow"] = GetOrCreateMat("Mat_StreetLight_Glow", litShader,
            new Color(1f, 0.95f, 0.8f, 1f), 0f, 0.9f, new Color(2.5f, 2.2f, 1.5f, 1f));

        dict["Mat_Grass_Park"] = GetOrCreateMat("Mat_Grass_Park", litShader,
            new Color(0.18f, 0.44f, 0.15f, 1f), 0f, 0.15f);

        dict["Mat_Tree_Trunk"] = GetOrCreateMat("Mat_Tree_Trunk", litShader,
            new Color(0.28f, 0.18f, 0.10f, 1f), 0f, 0.10f);

        dict["Mat_Tree_Leaves_Autumn_Red"] = GetOrCreateMat("Mat_Tree_Leaves_Autumn_Red", litShader,
            new Color(0.85f, 0.18f, 0.10f, 1f), 0.05f, 0.25f);

        dict["Mat_Tree_Leaves_Autumn_Orange"] = GetOrCreateMat("Mat_Tree_Leaves_Autumn_Orange", litShader,
            new Color(0.94f, 0.48f, 0.08f, 1f), 0.05f, 0.25f);

        dict["Mat_Tree_Leaves_Green"] = GetOrCreateMat("Mat_Tree_Leaves_Green", litShader,
            new Color(0.16f, 0.52f, 0.14f, 1f), 0.05f, 0.20f);

        dict["Mat_Highway_Sign"] = GetOrCreateMat("Mat_Highway_Sign", litShader,
            new Color(0.08f, 0.55f, 0.28f, 1f), 0.1f, 0.60f, new Color(0.1f, 0.9f, 0.4f, 1f));

        // 5 Traffic Car Colors
        dict["Mat_TrafficCar_Body1"] = GetOrCreateMat("Mat_TrafficCar_Body1", litShader,
            new Color(0.82f, 0.12f, 0.12f, 1f), 0.85f, 0.90f); // Crimson Red
        dict["Mat_TrafficCar_Body2"] = GetOrCreateMat("Mat_TrafficCar_Body2", litShader,
            new Color(0.95f, 0.96f, 0.98f, 1f), 0.70f, 0.92f); // Pearl White
        dict["Mat_TrafficCar_Body3"] = GetOrCreateMat("Mat_TrafficCar_Body3", litShader,
            new Color(0.12f, 0.32f, 0.78f, 1f), 0.85f, 0.92f); // Electric Blue
        dict["Mat_TrafficCar_Body4"] = GetOrCreateMat("Mat_TrafficCar_Body4", litShader,
            new Color(0.50f, 0.52f, 0.55f, 1f), 0.88f, 0.88f); // Metallic Silver
        dict["Mat_TrafficCar_Body5"] = GetOrCreateMat("Mat_TrafficCar_Body5", litShader,
            new Color(0.95f, 0.75f, 0.08f, 1f), 0.85f, 0.90f); // Racing Yellow

        dict["Mat_TrafficLight_Off"] = GetOrCreateMat("Mat_TrafficLight_Off", litShader,
            new Color(0.15f, 0.15f, 0.15f, 1f), 0.2f, 0.5f);
        dict["Mat_TrafficLight_Red"] = GetOrCreateMat("Mat_TrafficLight_Red", litShader,
            new Color(0.95f, 0.10f, 0.10f, 1f), 0.1f, 0.9f, new Color(4.5f, 0.2f, 0.2f, 1f));

        return dict;
    }

    private static Material GetOrCreateMat(string matName, Shader shader, Color baseColor, float metallic, float smoothness, Color? emission = null)
    {
        string path = $"{MAT_DIR}/{matName}.mat";
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

    private static void SetupLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.72f, 0.85f, 0.98f);
        RenderSettings.ambientEquatorColor = new Color(0.55f, 0.62f, 0.70f);
        RenderSettings.ambientGroundColor = new Color(0.25f, 0.30f, 0.25f);

        GameObject lightObj = GameObject.Find("Directional Light");
        if (lightObj == null)
        {
            lightObj = new GameObject("Directional Light");
            lightObj.AddComponent<Light>();
        }

        Light dirLight = lightObj.GetComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.color = new Color(1.0f, 0.98f, 0.94f);
        dirLight.intensity = 1.35f;
        dirLight.shadows = LightShadows.Soft;
        lightObj.transform.rotation = Quaternion.Euler(48f, -25f, 0f);
    }

    private static void SetupHighwayUI(HighwayOvertakeDetector detector)
    {
        GameObject canvasObj = new GameObject("UI_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject es = GameObject.Find("EventSystem");
        if (es == null)
        {
            es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        HighwayUIManager uiMgr = canvasObj.AddComponent<HighwayUIManager>();
        uiMgr.overtakeDetector = detector;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 1. Top Overtake Stats Bar
        GameObject topBar = CreateUIPanel(canvasObj, "TopStatsBar", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(620f, 75f), new Color(0.08f, 0.10f, 0.14f, 0.88f));

        Text overtakeTxt = CreateUIText(topBar, "OvertakeCountText", "OVERTAKES: 0", font, 24, TextAnchor.MiddleCenter, Color.white, new Vector2(0.28f, 0.7f), new Vector2(260f, 35f));
        overtakeTxt.fontStyle = FontStyle.Bold;
        uiMgr.overtakeCountText = overtakeTxt;

        Text scoreTxt = CreateUIText(topBar, "ScoreText", "SCORE: 0", font, 18, TextAnchor.MiddleCenter, new Color(0.7f, 0.9f, 1f), new Vector2(0.28f, 0.3f), new Vector2(260f, 28f));
        uiMgr.scoreText = scoreTxt;

        Text cashTxt = CreateUIText(topBar, "CashText", "$1,500", font, 26, TextAnchor.MiddleCenter, new Color(0.2f, 1.0f, 0.4f), new Vector2(0.72f, 0.5f), new Vector2(200f, 40f));
        cashTxt.fontStyle = FontStyle.Bold;
        uiMgr.cashText = cashTxt;

        Text comboTxt = CreateUIText(topBar, "ComboText", "COMBO x2 (4.5s)", font, 18, TextAnchor.MiddleCenter, new Color(1.0f, 0.85f, 0.2f), new Vector2(0.5f, -22f), new Vector2(240f, 26f));
        comboTxt.fontStyle = FontStyle.Bold;
        uiMgr.comboText = comboTxt;

        // 2. Center Animated Overtake Pop-up
        GameObject popupObj = CreateUIPanel(canvasObj, "OvertakePopup", new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 65f), new Color(0.10f, 0.12f, 0.18f, 0.92f));
        CanvasGroup cg = popupObj.AddComponent<CanvasGroup>();
        uiMgr.popupCanvasGroup = cg;

        Text pTxt = CreateUIText(popupObj, "PopupText", "⚡ CLOSE OVERTAKE! +$250 ⚡", font, 26, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.15f), new Vector2(0.5f, 0.5f), new Vector2(480f, 50f));
        pTxt.fontStyle = FontStyle.Bold;
        uiMgr.popupText = pTxt;

        // 3. Top-Right Speedometer
        GameObject speedo = CreateUIPanel(canvasObj, "Speedometer", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-25f, -25f), new Vector2(220f, 130f), new Color(0.08f, 0.10f, 0.14f, 0.88f));

        Text spdTxt = CreateUIText(speedo, "SpeedNumber", "0", font, 52, TextAnchor.UpperCenter, Color.white, new Vector2(0.42f, 0.65f), new Vector2(120f, 60f));
        spdTxt.fontStyle = FontStyle.Bold;
        uiMgr.speedText = spdTxt;

        CreateUIText(speedo, "KmhUnit", "KM/H", font, 14, TextAnchor.LowerLeft, new Color(0.7f, 0.75f, 0.85f), new Vector2(0.78f, 0.48f), new Vector2(60f, 25f));

        Text gTxt = CreateUIText(speedo, "GearText", "N", font, 24, TextAnchor.MiddleCenter, new Color(1f, 0.8f, 0.1f), new Vector2(0.78f, 0.75f), new Vector2(50f, 35f));
        gTxt.fontStyle = FontStyle.Bold;
        uiMgr.gearText = gTxt;

        Text rTxt = CreateUIText(speedo, "RpmText", "850 RPM", font, 13, TextAnchor.MiddleCenter, new Color(0.6f, 0.65f, 0.75f), new Vector2(0.5f, 0.16f), new Vector2(160f, 20f));
        uiMgr.rpmText = rTxt;

        // Speed Gauge Bar
        GameObject gaugeBg = CreateUIPanel(speedo, "GaugeBg", new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180f, 8f), new Color(0.2f, 0.25f, 0.3f, 1f));
        GameObject gaugeFill = CreateUIPanel(gaugeBg, "GaugeFill", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(180f, 8f), new Color(0.1f, 0.85f, 1.0f, 1f));
        Image fillImg = gaugeFill.GetComponent<Image>();
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0f;
        uiMgr.speedGaugeBar = fillImg;

        // Quick action buttons under speedometer (LIGHTS, CAM, PAUSE)
        CreateMiniButton(canvasObj, "Btn_Lights", "LIGHTS (H)", font, new Vector2(1f, 1f), new Vector2(-195f, -170f), new Vector2(85f, 30f), () => uiMgr.OnLightsButtonClicked());
        CreateMiniButton(canvasObj, "Btn_Cam", "CAM (C)", font, new Vector2(1f, 1f), new Vector2(-105f, -170f), new Vector2(85f, 30f), () => uiMgr.OnCamButtonClicked());
        CreateMiniButton(canvasObj, "Btn_Pause", "PAUSE", font, new Vector2(1f, 1f), new Vector2(-195f, -206f), new Vector2(85f, 30f), () => uiMgr.TogglePause());
        CreateMiniButton(canvasObj, "Btn_Restart", "RESET (R)", font, new Vector2(1f, 1f), new Vector2(-105f, -206f), new Vector2(85f, 30f), () => uiMgr.OnRestartClicked());

        // 4. On-Screen Virtual Driving Controls (Bottom)
        // Steer Left / Right Buttons
        CreateTouchPedal(canvasObj, "Btn_SteerLeft", "◀\n(A)", font, new Vector2(0f, 0f), new Vector2(80f, 90f), new Vector2(90f, 90f), new Color(0.18f, 0.22f, 0.30f, 0.85f),
            () => uiMgr.OnSteerLeftDown(), () => uiMgr.OnSteerLeftUp());

        CreateTouchPedal(canvasObj, "Btn_SteerRight", "▶\n(D)", font, new Vector2(0f, 0f), new Vector2(185f, 90f), new Vector2(90f, 90f), new Color(0.18f, 0.22f, 0.30f, 0.85f),
            () => uiMgr.OnSteerRightDown(), () => uiMgr.OnSteerRightUp());

        // Gas (W), Brake (S), Drift (Space) Pedals
        CreateTouchPedal(canvasObj, "Btn_Gas", "GAS\n(W)", font, new Vector2(1f, 0f), new Vector2(-75f, 110f), new Vector2(100f, 130f), new Color(0.12f, 0.65f, 0.32f, 0.88f),
            () => uiMgr.OnGasDown(), () => uiMgr.OnGasUp());

        CreateTouchPedal(canvasObj, "Btn_Brake", "BRAKE\n(S)", font, new Vector2(1f, 0f), new Vector2(-185f, 95f), new Vector2(90f, 105f), new Color(0.85f, 0.22f, 0.22f, 0.88f),
            () => uiMgr.OnBrakeDown(), () => uiMgr.OnBrakeUp());

        CreateTouchPedal(canvasObj, "Btn_Drift", "DRIFT\n(Space)", font, new Vector2(1f, 0f), new Vector2(-285f, 85f), new Vector2(85f, 85f), new Color(0.88f, 0.55f, 0.12f, 0.88f),
            () => uiMgr.OnDriftDown(), () => uiMgr.OnDriftUp());

        // 5. Pause Menu Panel
        GameObject pause = CreateUIPanel(canvasObj, "PausePanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 260f), new Color(0.08f, 0.10f, 0.14f, 0.96f));
        CreateUIText(pause, "PauseTitle", "GAME PAUSED", font, 28, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.80f), new Vector2(300f, 40f));
        CreateMiniButton(pause, "ResumeBtn", "RESUME GAME", font, new Vector2(0.5f, 0.5f), new Vector2(0f, 15f), new Vector2(220f, 45f), () => uiMgr.TogglePause());
        CreateMiniButton(pause, "RestartBtn", "RESTART GAME", font, new Vector2(0.5f, 0.5f), new Vector2(0f, -45f), new Vector2(220f, 45f), () => uiMgr.OnRestartClicked());
        pause.SetActive(false);
        uiMgr.pausePanel = pause;
    }

    private static void SetupAudio()
    {
        GameObject audioObj = new GameObject("AudioManager");
        audioObj.AddComponent<CarAudioManager>();
    }

    private static void SetupPlayerCarAndCamera()
    {
        GameObject car = GameObject.Find("SportsSedan");
        if (car != null)
        {
            // Position in Lane 1 of the Highway facing forward (+Z)
            car.transform.position = new Vector3(-1.9f, 0.15f, 0f);
            car.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            var rb = car.GetComponent<Rigidbody>();
            if (rb == null) rb = car.AddComponent<Rigidbody>();
            rb.mass = 1450f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Smooth low-friction physics material for wheels
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

            // Body BoxCollider: raised off ground
            BoxCollider boxCol = car.GetComponent<BoxCollider>();
            if (boxCol == null) boxCol = car.AddComponent<BoxCollider>();
            boxCol.center = new Vector3(0f, 0.82f, 0.02f);
            boxCol.size = new Vector3(1.88f, 0.65f, 4.80f);
            boxCol.material = tireMat;

            // 4 Smooth Wheel SphereColliders
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
            cc.accelerationRate = 28f;
            cc.maxSpeedKmh = 195f;
            cc.steerSpeed = 8.5f;

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

    #region UI Helper Methods
    private static GameObject CreateUIPanel(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent.transform, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = panel.AddComponent<Image>();
        img.color = color;
        return panel;
    }

    private static Text CreateUIText(GameObject parent, string name, string content, Font font, int size, TextAnchor align, Color color, Vector2 anchorPos, Vector2 dimension)
    {
        GameObject txtObj = new GameObject(name);
        txtObj.transform.SetParent(parent.transform, false);

        RectTransform rt = txtObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorPos;
        rt.anchorMax = anchorPos;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = dimension;

        Text t = txtObj.AddComponent<Text>();
        t.text = content;
        t.font = font;
        t.fontSize = size;
        t.alignment = align;
        t.color = color;
        return t;
    }

    private static Button CreateMiniButton(GameObject parent, string name, string text, Font font, Vector2 anchor, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent.transform, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.18f, 0.22f, 0.28f, 0.90f);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(action);

        CreateUIText(btnObj, "Text", text, font, 12, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), size);
        return btn;
    }

    private static void CreateTouchPedal(GameObject parent, string name, string label, Font font, Vector2 anchor, Vector2 pos, Vector2 size, Color color,
        System.Action onDown, System.Action onUp)
    {
        GameObject pedal = new GameObject(name);
        pedal.transform.SetParent(parent.transform, false);

        RectTransform rt = pedal.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = pedal.AddComponent<Image>();
        img.color = color;

        CreateUIText(pedal, "Label", label, font, 16, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), size);

        // EventTrigger for pointer down and up
        var trigger = pedal.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var entryDown = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
        entryDown.callback.AddListener((data) => onDown());
        trigger.triggers.Add(entryDown);

        var entryUp = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
        entryUp.callback.AddListener((data) => onUp());
        trigger.triggers.Add(entryUp);
    }
    #endregion
}
