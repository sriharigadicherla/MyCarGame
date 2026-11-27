using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Automated Highway Overtake Game Scene Builder.
/// Generates:
/// 1. Multi-lane infinite highway system with endless chunk recycling.
/// 2. Overhead highway signboards, gantries, steel crash barriers, streetlights, and colorful roadside trees.
/// 3. High-speed cruising traffic stream (20 vehicles across 4 lanes with varying speeds).
/// 4. Overtake & Near-Miss combo scoring detector with floating HUD rewards.
/// 5. Digital speedometer, gear indicator, touch pedals, pause menu, and procedural audio.
/// 6. Configures player Mercedes sports sedan with smooth wheel physics and chase camera.
/// 7. AAA Main Menu Lobby with procedural glassmorphic cards, pills, interactive buttons, modal dialogs, and tactile click feedback.
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
        htm.headlightMat = mats["Mat_Headlight_LED"];
        htm.glassMat = mats["Mat_Glass_Tinted"];
        htm.tireMat = mats["Mat_Tire_Rubber"];
        htm.darkTrimMat = mats["Mat_Dark_Trim"];

        // 7. Setup Highway Overtake Detector
        GameObject overtakeObj = new GameObject("HighwayOvertakeDetector");
        var hod = overtakeObj.AddComponent<HighwayOvertakeDetector>();
        hod.trafficManager = htm;

        // 8. Setup Highway HUD UI Canvas
        SetupHighwayUI(hod, htm, hm);

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
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }

    private static Dictionary<string, Material> CreateOrUpdateMaterials()
    {
        var mats = new Dictionary<string, Material>();
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");

        mats["Mat_Road_Asphalt"] = GetOrCreateMaterial("Mat_Road_Asphalt", litShader, new Color(0.18f, 0.20f, 0.22f), 0.05f, 0.25f);
        mats["Mat_Road_Marking_Yellow"] = GetOrCreateMaterial("Mat_Road_Marking_Yellow", litShader, new Color(1.0f, 0.85f, 0.05f), 0.1f, 0.15f);
        mats["Mat_Road_Marking_White"] = GetOrCreateMaterial("Mat_Road_Marking_White", litShader, new Color(0.96f, 0.96f, 0.98f), 0.1f, 0.2f);
        mats["Mat_Barrier_Metal"] = GetOrCreateMaterial("Mat_Barrier_Metal", litShader, new Color(0.80f, 0.82f, 0.86f), 0.7f, 0.55f);
        mats["Mat_StreetLight_Pole"] = GetOrCreateMaterial("Mat_StreetLight_Pole", litShader, new Color(0.25f, 0.27f, 0.30f), 0.6f, 0.45f);
        mats["Mat_Grass_Park"] = GetOrCreateMaterial("Mat_Grass_Park", litShader, new Color(0.20f, 0.55f, 0.18f), 0.0f, 0.05f);
        mats["Mat_Tree_Trunk"] = GetOrCreateMaterial("Mat_Tree_Trunk", litShader, new Color(0.38f, 0.24f, 0.14f), 0.0f, 0.05f);
        mats["Mat_Tree_Leaves_Autumn_Red"] = GetOrCreateMaterial("Mat_Tree_Leaves_Autumn_Red", litShader, new Color(0.85f, 0.20f, 0.12f), 0.0f, 0.05f);
        mats["Mat_Tree_Leaves_Autumn_Orange"] = GetOrCreateMaterial("Mat_Tree_Leaves_Autumn_Orange", litShader, new Color(0.95f, 0.52f, 0.08f), 0.0f, 0.05f);
        mats["Mat_Tree_Leaves_Green"] = GetOrCreateMaterial("Mat_Tree_Leaves_Green", litShader, new Color(0.14f, 0.58f, 0.22f), 0.0f, 0.05f);
        mats["Mat_Highway_Sign"] = GetOrCreateMaterial("Mat_Highway_Sign", litShader, new Color(0.05f, 0.45f, 0.25f), 0.2f, 0.4f);

        // Traffic Car Materials
        mats["Mat_TrafficCar_Body1"] = GetOrCreateMaterial("Mat_TrafficCar_Body1", litShader, new Color(0.85f, 0.12f, 0.12f), 0.85f, 0.85f); // Crimson
        mats["Mat_TrafficCar_Body2"] = GetOrCreateMaterial("Mat_TrafficCar_Body2", litShader, new Color(0.10f, 0.35f, 0.85f), 0.85f, 0.85f); // Cobalt Blue
        mats["Mat_TrafficCar_Body3"] = GetOrCreateMaterial("Mat_TrafficCar_Body3", litShader, new Color(0.92f, 0.92f, 0.95f), 0.90f, 0.90f); // Pearl White
        mats["Mat_TrafficCar_Body4"] = GetOrCreateMaterial("Mat_TrafficCar_Body4", litShader, new Color(0.12f, 0.12f, 0.14f), 0.80f, 0.85f); // Midnight Black
        mats["Mat_TrafficCar_Body5"] = GetOrCreateMaterial("Mat_TrafficCar_Body5", litShader, new Color(0.95f, 0.75f, 0.05f), 0.85f, 0.80f); // Racing Yellow

        mats["Mat_Glass_Tinted"] = GetOrCreateMaterial("Mat_Glass_Tinted", litShader, new Color(0.08f, 0.12f, 0.18f), 0.95f, 0.95f);
        mats["Mat_Tire_Rubber"] = GetOrCreateMaterial("Mat_Tire_Rubber", litShader, new Color(0.12f, 0.12f, 0.13f), 0.05f, 0.15f);
        mats["Mat_Dark_Trim"] = GetOrCreateMaterial("Mat_Dark_Trim", litShader, new Color(0.10f, 0.10f, 0.11f), 0.30f, 0.40f);

        // Emission Materials
        mats["Mat_StreetLight_Glow"] = GetOrCreateEmissionMaterial("Mat_StreetLight_Glow", litShader, new Color(1f, 0.95f, 0.80f), new Color(1.8f, 1.7f, 1.3f));
        mats["Mat_TrafficLight_Off"] = GetOrCreateMaterial("Mat_TrafficLight_Off", litShader, new Color(0.35f, 0.05f, 0.05f), 0.1f, 0.3f);
        mats["Mat_TrafficLight_Red"] = GetOrCreateEmissionMaterial("Mat_TrafficLight_Red", litShader, new Color(1f, 0.05f, 0.05f), new Color(2.5f, 0.1f, 0.1f));
        mats["Mat_Headlight_LED"] = GetOrCreateEmissionMaterial("Mat_Headlight_LED", litShader, new Color(0.95f, 0.98f, 1.0f), new Color(2.2f, 2.3f, 2.5f));

        return mats;
    }

    private static Material GetOrCreateMaterial(string name, Shader shader, Color color, float metallic = 0f, float smoothness = 0.5f)
    {
        string path = $"{MAT_DIR}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Material GetOrCreateEmissionMaterial(string name, Shader shader, Color color, Color emission)
    {
        string path = $"{MAT_DIR}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        mat.EnableKeyword("_EMISSION");
        if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void CleanUpOldObjects()
    {
        string[] deleteTargets = new string[] {
            "HighwayManager", "HighwayTrafficManager", "HighwayOvertakeDetector",
            "HighwayHUDCanvas", "UI_Canvas", "Canvas", "AudioManager", "TrafficCarPool", "EnvironmentChunks",
            "ProceduralCity", "CityGrid", "MinimapCamera", "MinimapUI", "EventSystem"
        };

        foreach (string targetName in deleteTargets)
        {
            GameObject obj = GameObject.Find(targetName);
            while (obj != null)
            {
                Object.DestroyImmediate(obj);
                obj = GameObject.Find(targetName);
            }
        }

        // Clean up any lingering canvases or event systems so there are never duplicates
        var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in allCanvases)
        {
            Object.DestroyImmediate(c.gameObject);
        }
        var allEventSystems = Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
        foreach (var es in allEventSystems)
        {
            Object.DestroyImmediate(es.gameObject);
        }
    }

    private static void SetupLighting()
    {
        Light sun = RenderSettings.sun;
        if (sun == null)
        {
            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional) { sun = l; break; }
            }
        }

        if (sun == null)
        {
            GameObject sunObj = new GameObject("SunLight");
            sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
        }

        sun.transform.rotation = Quaternion.Euler(38f, -40f, 0f);
        sun.color = new Color(1.0f, 0.96f, 0.88f);
        sun.intensity = 1.35f;
        sun.shadows = LightShadows.Soft;
        RenderSettings.sun = sun;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.60f, 0.72f, 0.90f);
        RenderSettings.ambientEquatorColor = new Color(0.45f, 0.52f, 0.60f);
        RenderSettings.ambientGroundColor = new Color(0.25f, 0.30f, 0.22f);

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.0018f;
        RenderSettings.fogColor = new Color(0.70f, 0.82f, 0.92f);
    }

    private static void SetupHighwayUI(HighwayOvertakeDetector detector, HighwayTrafficManager trafficMgr, HighwayManager highwayMgr)
    {
        GameObject canvasObj = new GameObject("HighwayHUDCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject es = GameObject.Find("EventSystem");
        if (es == null) es = new GameObject("EventSystem");
        if (es.GetComponent<UnityEngine.EventSystems.EventSystem>() == null)
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();

        var oldMod = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        if (oldMod != null) Object.DestroyImmediate(oldMod);

        var newMod = es.GetComponent<InputSystemUIInputModule>();
        if (newMod == null) es.AddComponent<InputSystemUIInputModule>();

        HighwayUIManager uiMgr = canvasObj.AddComponent<HighwayUIManager>();
        uiMgr.overtakeDetector = detector;
        uiMgr.trafficManager = trafficMgr;
        uiMgr.highwayManager = highwayMgr;
        var carObjRef = GameObject.Find("SportsSedan");
        if (carObjRef != null)
        {
            uiMgr.carController = carObjRef.GetComponent<CarController>();
            uiMgr.carVisuals = carObjRef.GetComponent<CarVisuals>();
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Load procedural UI sprites with 9-slice borders
        Sprite panelCardSp = LoadProceduralSprite("panel_card", new Vector4(20, 20, 20, 20));
        Sprite panelCardActiveSp = LoadProceduralSprite("panel_card_active", new Vector4(20, 20, 20, 20));
        Sprite pillCapsuleSp = LoadProceduralSprite("pill_capsule", new Vector4(28, 28, 28, 28));
        Sprite squareBtnSp = LoadProceduralSprite("btn_icon_square", new Vector4(18, 18, 18, 18));
        Sprite playShapeSp = LoadProceduralSprite("btn_play_shape");
        Sprite modalGlassSp = LoadProceduralSprite("panel_modal_glass", new Vector4(24, 24, 24, 24));
        Sprite redBadgeSp = LoadProceduralSprite("badge_red_dot");

        // Clean vector icons
        Sprite iconProfile = LoadProceduralSprite("icon_profile");
        Sprite iconCoin = LoadProceduralSprite("icon_coin");
        Sprite iconGem = LoadProceduralSprite("icon_gem");
        Sprite iconMail = LoadProceduralSprite("icon_mail");
        Sprite iconSettings = LoadProceduralSprite("icon_settings");
        Sprite iconEvents = LoadProceduralSprite("icon_events");
        Sprite iconGarage = LoadProceduralSprite("icon_garage");
        Sprite iconCustom = LoadProceduralSprite("icon_custom");
        Sprite iconStore = LoadProceduralSprite("icon_store");
        Sprite iconCars = LoadProceduralSprite("icon_cars");
        Sprite iconUpgrades = LoadProceduralSprite("icon_upgrades");
        Sprite iconMissions = LoadProceduralSprite("icon_missions");
        Sprite iconLeaderboard = LoadProceduralSprite("icon_leaderboard");
        Sprite iconClose = LoadProceduralSprite("icon_close");

        // Thumbnails
        Sprite thumbAlpine = LoadProceduralSprite("thumb_alpine");
        Sprite thumbDesert = LoadProceduralSprite("thumb_desert");
        Sprite thumbCity = LoadProceduralSprite("thumb_city");
        Sprite thumbHighway = LoadProceduralSprite("thumb_highway");

        // -------------------------------------------------------------
        // A. MAIN MENU / LOBBY PANEL (Clean Backdrop + Procedural UI)
        // -------------------------------------------------------------
        GameObject menuPanel = CreateUIPanel(canvasObj, "MainMenuPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white, true);
        uiMgr.mainMenuPanel = menuPanel;

        // Clean Backdrop Image (Car on frozen lake)
        string bgPath = "Assets/Textures/UI/MainMenu_Backdrop.jpg";
        TextureImporter bgImporter = AssetImporter.GetAtPath(bgPath) as TextureImporter;
        if (bgImporter != null && bgImporter.textureType != TextureImporterType.Sprite)
        {
            bgImporter.textureType = TextureImporterType.Sprite;
            bgImporter.spriteImportMode = SpriteImportMode.Single;
            bgImporter.SaveAndReimport();
        }
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
        if (bgSprite != null)
        {
            Image bgImg = menuPanel.GetComponent<Image>();
            bgImg.sprite = bgSprite;
            bgImg.type = Image.Type.Simple;
            bgImg.preserveAspect = false;
        }

        // 1. TOP BAR
        // Profile Card (Top-Left)
        uiMgr.profileBtn = CreateProceduralProfileCard(menuPanel, font, panelCardSp, iconProfile, () => uiMgr.OnProfileClicked());

        // Coins Capsule (Top-Right)
        uiMgr.coinsBtn = CreateProceduralPillButton(menuPanel, "Btn_Coins", new Vector2(1f, 1f), new Vector2(-430f, -24f), new Vector2(150f, 48f),
            pillCapsuleSp, iconCoin, "15,000", new Color(1f, 0.85f, 0.1f), () => uiMgr.OnCoinsClicked(), font);

        // Gems Capsule
        uiMgr.gemsBtn = CreateProceduralPillButton(menuPanel, "Btn_Gems", new Vector2(1f, 1f), new Vector2(-265f, -24f), new Vector2(145f, 48f),
            pillCapsuleSp, iconGem, "320", new Color(0.1f, 0.85f, 1f), () => uiMgr.OnGemsClicked(), font);

        // Mail Square Button
        uiMgr.mailBtn = CreateProceduralSquareButton(menuPanel, "Btn_Mail", new Vector2(1f, 1f), new Vector2(-195f, -24f), new Vector2(48f, 48f),
            squareBtnSp, iconMail, null, () => uiMgr.OnMailClicked());

        // Settings Square Button
        uiMgr.settingsBtn = CreateProceduralSquareButton(menuPanel, "Btn_Settings", new Vector2(1f, 1f), new Vector2(-135f, -24f), new Vector2(48f, 48f),
            squareBtnSp, iconSettings, null, () => uiMgr.OnSettingsClicked());

        // Desktop Close ✕ Button (Top-Right)
        CreateProceduralSquareButton(menuPanel, "Btn_DesktopExit", new Vector2(1f, 1f), new Vector2(-75f, -24f), new Vector2(48f, 48f),
            squareBtnSp, iconClose, new Color(0.85f, 0.18f, 0.18f, 0.95f), () => uiMgr.OnExitGameClicked());

        // 2. RIGHT SIDE MENU (Vertical Action Cards)
        float startY = 140f;
        float spacingY = 70f;
        uiMgr.eventsBtn = CreateProceduralActionCard(menuPanel, "Btn_Events", new Vector2(-30f, startY), panelCardSp, iconEvents, "EVENTS", font, redBadgeSp, () => uiMgr.OnEventsClicked());
        uiMgr.garageBtn = CreateProceduralActionCard(menuPanel, "Btn_Garage", new Vector2(-30f, startY - spacingY), panelCardSp, iconGarage, "GARAGE", font, null, () => uiMgr.OnGarageClicked());
        uiMgr.customizationBtn = CreateProceduralActionCard(menuPanel, "Btn_Customization", new Vector2(-30f, startY - spacingY * 2), panelCardSp, iconCustom, "CUSTOMIZATION", font, null, () => uiMgr.OnCustomizationClicked(), 15);
        uiMgr.storeBtn = CreateProceduralActionCard(menuPanel, "Btn_Store", new Vector2(-30f, startY - spacingY * 3), panelCardSp, iconStore, "STORE", font, null, () => uiMgr.OnStoreClicked());

        // 3. BOTTOM BAR (Horizontal Navigation Dock)
        GameObject bottomDock = CreateUIPanel(menuPanel, "BottomNavDock", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 32f), new Vector2(740f, 56f), new Color(0.06f, 0.10f, 0.18f, 0.88f), false);
        Image dockImg = bottomDock.GetComponent<Image>();
        dockImg.sprite = panelCardSp;
        dockImg.type = Image.Type.Sliced;

        uiMgr.carsBtn = CreateProceduralDockButton(bottomDock, "Btn_Cars", -270f, iconCars, "Cars", font, () => uiMgr.OnCarsClicked());
        uiMgr.upgradesBtn = CreateProceduralDockButton(bottomDock, "Btn_Upgrades", -90f, iconUpgrades, "Upgrades", font, () => uiMgr.OnUpgradesClicked());
        uiMgr.missionsBtn = CreateProceduralDockButton(bottomDock, "Btn_Missions", 90f, iconMissions, "Missions", font, () => uiMgr.OnMissionsClicked());
        uiMgr.leaderboardBtn = CreateProceduralDockButton(bottomDock, "Btn_Leaderboard", 270f, iconLeaderboard, "Leaderboard", font, () => uiMgr.OnLeaderboardClicked());

        // 4. BOTTOM-LEFT: LOCATION SELECTOR
        GameObject locContainer = CreateUIPanel(menuPanel, "LocationSelector", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(30f, 32f), new Vector2(280f, 275f), Color.clear, false);

        // Header
        GameObject locHeader = CreateUIPanel(locContainer, "HeaderCard", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(280f, 40f), Color.white, false);
        Image lhImg = locHeader.GetComponent<Image>();
        lhImg.sprite = panelCardSp;
        lhImg.type = Image.Type.Sliced;
        Text headerTxt = CreateUIText(locHeader, "Title", "📍 SELECT LOCATION ▲", font, 13, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.15f), new Vector2(0.5f, 0.5f), new Vector2(260f, 36f));
        headerTxt.fontStyle = FontStyle.Bold;

        // 4 Map Cards Stack
        uiMgr.mapAlpineBtn = CreateProceduralMapCard(locContainer, "MapBtn_Alpine", -46f, panelCardSp, thumbAlpine, "Alpine Circuit", "🔒 LOCKED", new Color(0.7f, 0.7f, 0.7f), () => uiMgr.OnMapSelected("Alpine Circuit", false), font);
        uiMgr.mapDesertBtn = CreateProceduralMapCard(locContainer, "MapBtn_Desert", -102f, panelCardSp, thumbDesert, "Desert Rush", "🔒 LOCKED", new Color(0.7f, 0.7f, 0.7f), () => uiMgr.OnMapSelected("Desert Rush", false), font);
        uiMgr.mapCityBtn = CreateProceduralMapCard(locContainer, "MapBtn_City", -158f, panelCardSp, thumbCity, "City Night", "🔒 LOCKED", new Color(0.7f, 0.7f, 0.7f), () => uiMgr.OnMapSelected("City Night", false), font);
        uiMgr.mapHighwayBtn = CreateProceduralMapCard(locContainer, "MapBtn_Highway", -214f, panelCardActiveSp, thumbHighway, "Coastal Highway", "● READY TO PLAY", new Color(1f, 0.85f, 0.1f), () => uiMgr.OnMapSelected("Coastal Highway", true), font, true);

        // 5. BOTTOM-RIGHT: LARGE PLAY BUTTON
        uiMgr.playButton = CreateProceduralPlayButton(menuPanel, font, playShapeSp, () => uiMgr.StartHighwayGame());

        // 6. INTERACTIVE MODAL DIALOG POPUP
        SetupProceduralModalSystem(canvasObj, uiMgr, font, modalGlassSp, panelCardActiveSp, iconClose);

        // 7. FLOATING TOAST NOTIFICATION
        GameObject toast = CreateUIPanel(canvasObj, "ToastPanel", new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 52f), new Color(0.06f, 0.09f, 0.14f, 0.96f), false);
        Image tImg = toast.GetComponent<Image>();
        tImg.sprite = panelCardSp;
        tImg.type = Image.Type.Sliced;
        CanvasGroup toastCg = toast.AddComponent<CanvasGroup>();
        uiMgr.toastCanvasGroup = toastCg;
        uiMgr.toastPanel = toast;
        Text tText = CreateUIText(toast, "ToastText", "Welcome, RacerSrihari!", font, 17, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.15f), new Vector2(0.5f, 0.5f), new Vector2(600f, 44f));
        tText.fontStyle = FontStyle.Bold;
        uiMgr.toastText = tText;
        toast.SetActive(false);

        // -------------------------------------------------------------
        // B. IN-GAME GAMEPLAY HUD PANEL (Speedometer, Pedals, Overtakes)
        // -------------------------------------------------------------
        GameObject gameplayHUD = new GameObject("GameplayHUDPanel");
        gameplayHUD.transform.SetParent(canvasObj.transform, false);
        RectTransform ghRt = gameplayHUD.AddComponent<RectTransform>();
        ghRt.anchorMin = Vector2.zero;
        ghRt.anchorMax = Vector2.one;
        ghRt.sizeDelta = Vector2.zero;
        uiMgr.gameplayHUDPanel = gameplayHUD;

        // In-game Exit Button (Top-Right)
        Button topExitBtn = CreateMiniButton(gameplayHUD, "Btn_TopExit", "✕ EXIT", font, new Vector2(1f, 1f), new Vector2(-60f, -25f), new Vector2(95f, 36f), () => uiMgr.OnExitGameClicked(), new Color(0.85f, 0.18f, 0.18f, 0.95f), 14);
        uiMgr.topExitButton = topExitBtn;

        // 1. Top Overtake Stats Bar
        GameObject topBar = CreateUIPanel(gameplayHUD, "TopStatsBar", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(620f, 75f), new Color(0.08f, 0.10f, 0.14f, 0.88f), false);
        topBar.GetComponent<Image>().sprite = panelCardSp;
        topBar.GetComponent<Image>().type = Image.Type.Sliced;

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
        GameObject popupObj = CreateUIPanel(gameplayHUD, "OvertakePopup", new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 65f), new Color(0.10f, 0.12f, 0.18f, 0.92f), false);
        popupObj.GetComponent<Image>().sprite = panelCardSp;
        popupObj.GetComponent<Image>().type = Image.Type.Sliced;
        CanvasGroup cg = popupObj.AddComponent<CanvasGroup>();
        uiMgr.popupCanvasGroup = cg;

        Text pTxt = CreateUIText(popupObj, "PopupText", "⚡ CLOSE OVERTAKE! +$250 ⚡", font, 26, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.15f), new Vector2(0.5f, 0.5f), new Vector2(480f, 50f));
        pTxt.fontStyle = FontStyle.Bold;
        uiMgr.popupText = pTxt;

        // 3. Top-Right Speedometer
        GameObject speedo = CreateUIPanel(gameplayHUD, "Speedometer", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -70f), new Vector2(220f, 125f), new Color(0.08f, 0.10f, 0.14f, 0.88f), false);
        speedo.GetComponent<Image>().sprite = panelCardSp;
        speedo.GetComponent<Image>().type = Image.Type.Sliced;

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
        GameObject gaugeBg = CreateUIPanel(speedo, "GaugeBg", new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180f, 8f), new Color(0.2f, 0.25f, 0.3f, 1f), false);
        GameObject gaugeFill = CreateUIPanel(gaugeBg, "GaugeFill", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(180f, 8f), new Color(0.1f, 0.85f, 1.0f, 1f), false);
        Image fillImg = gaugeFill.GetComponent<Image>();
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0f;
        uiMgr.speedGaugeBar = fillImg;

        // Quick action buttons under speedometer (LIGHTS, CAM, PAUSE, RESET)
        Button btnLights = CreateMiniButton(gameplayHUD, "Btn_Lights", "LIGHTS (H)", font, new Vector2(1f, 1f), new Vector2(-195f, -205f), new Vector2(85f, 30f), () => uiMgr.OnLightsButtonClicked());
        Button btnCam = CreateMiniButton(gameplayHUD, "Btn_Cam", "CAM (C)", font, new Vector2(1f, 1f), new Vector2(-105f, -205f), new Vector2(85f, 30f), () => uiMgr.OnCamButtonClicked());
        Button btnPause = CreateMiniButton(gameplayHUD, "Btn_Pause", "PAUSE (ESC)", font, new Vector2(1f, 1f), new Vector2(-195f, -242f), new Vector2(85f, 30f), () => uiMgr.TogglePause());
        Button btnRestart = CreateMiniButton(gameplayHUD, "Btn_Restart", "RESET (R)", font, new Vector2(1f, 1f), new Vector2(-105f, -242f), new Vector2(85f, 30f), () => uiMgr.OnRestartClicked());

        uiMgr.lightsButton = btnLights;
        uiMgr.camButton = btnCam;
        uiMgr.pauseButton = btnPause;
        uiMgr.restartButton = btnRestart;

        // 4. On-Screen Virtual Driving Controls for Mobile & Touch
        // LEFT SIDE: FRONT (Gas) & BACK (Reverse)
        GameObject frontGas = CreateTouchPedal(gameplayHUD, "Btn_FrontGas", "▲\nFRONT\n(GAS)", font,
            new Vector2(0f, 0f), new Vector2(100f, 195f), new Vector2(125f, 120f), new Color(0.10f, 0.72f, 0.35f, 0.92f),
            () => uiMgr.OnGasDown(), () => uiMgr.OnGasUp(), panelCardSp);

        GameObject backReverse = CreateTouchPedal(gameplayHUD, "Btn_BackReverse", "▼\nBACK\n(REV)", font,
            new Vector2(0f, 0f), new Vector2(100f, 65f), new Vector2(125f, 105f), new Color(0.92f, 0.55f, 0.12f, 0.92f),
            () => uiMgr.OnReverseDown(), () => uiMgr.OnReverseUp(), panelCardSp);

        // RIGHT SIDE: LEFT TURN, RIGHT TURN & BRAKE
        GameObject steerLeft = CreateTouchPedal(gameplayHUD, "Btn_SteerLeft", "◀\nLEFT", font,
            new Vector2(1f, 0f), new Vector2(-280f, 85f), new Vector2(105f, 105f), new Color(0.18f, 0.40f, 0.75f, 0.92f),
            () => uiMgr.OnSteerLeftDown(), () => uiMgr.OnSteerLeftUp(), panelCardSp);

        GameObject steerRight = CreateTouchPedal(gameplayHUD, "Btn_SteerRight", "▶\nRIGHT", font,
            new Vector2(1f, 0f), new Vector2(-165f, 85f), new Vector2(105f, 105f), new Color(0.18f, 0.40f, 0.75f, 0.92f),
            () => uiMgr.OnSteerRightDown(), () => uiMgr.OnSteerRightUp(), panelCardSp);

        GameObject brake = CreateTouchPedal(gameplayHUD, "Btn_Brake", "🛑\nBRAKE", font,
            new Vector2(1f, 0f), new Vector2(-55f, 95f), new Vector2(95f, 125f), new Color(0.88f, 0.18f, 0.18f, 0.95f),
            () => uiMgr.OnBrakePedalDown(), () => uiMgr.OnBrakePedalUp(), panelCardSp);

        uiMgr.gasPedal = frontGas;
        uiMgr.reversePedal = backReverse;
        uiMgr.steerLeftPedal = steerLeft;
        uiMgr.steerRightPedal = steerRight;
        uiMgr.brakePedal = brake;

        gameplayHUD.SetActive(false); // Initially hidden while in Main Menu!

        // -------------------------------------------------------------
        // C. PAUSE MENU PANEL
        // -------------------------------------------------------------
        GameObject pause = CreateUIPanel(canvasObj, "PausePanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 350f), new Color(0.08f, 0.10f, 0.14f, 0.97f), true);
        pause.GetComponent<Image>().sprite = modalGlassSp;
        pause.GetComponent<Image>().type = Image.Type.Sliced;
        CreateUIText(pause, "PauseTitle", "GAME PAUSED", font, 28, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.86f), new Vector2(320f, 40f));

        Button resumeBtn = CreateMiniButton(pause, "ResumeBtn", "▶ RESUME GAME", font, new Vector2(0.5f, 0.5f), new Vector2(0f, 50f), new Vector2(280f, 42f), () => uiMgr.OnResumeClicked(), new Color(0.15f, 0.65f, 0.32f, 0.95f), 15);
        Button restartBtn = CreateMiniButton(pause, "RestartBtn", "↺ RESTART GAME", font, new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(280f, 42f), () => uiMgr.OnRestartClicked(), new Color(0.20f, 0.38f, 0.58f, 0.95f), 15);
        Button menuBtn = CreateMiniButton(pause, "MainMenuBtn", "🏠 MAIN MENU", font, new Vector2(0.5f, 0.5f), new Vector2(0f, -50f), new Vector2(280f, 42f), () => uiMgr.ShowMainMenu(), new Color(0.85f, 0.65f, 0.15f, 0.95f), 15);
        Button exitBtn = CreateMiniButton(pause, "ExitBtn", "✕ EXIT TO DESKTOP", font, new Vector2(0.5f, 0.5f), new Vector2(0f, -100f), new Vector2(280f, 42f), () => uiMgr.OnExitGameClicked(), new Color(0.85f, 0.18f, 0.18f, 0.95f), 15);

        uiMgr.resumeButton = resumeBtn;
        uiMgr.pauseRestartButton = restartBtn;
        uiMgr.pauseMainMenuButton = menuBtn;
        uiMgr.pauseExitButton = exitBtn;

        pause.SetActive(false);
        uiMgr.pausePanel = pause;

        // -------------------------------------------------------------
        // D. COUNTDOWN SEQUENCE PANEL ("3, 2, 1, GO!")
        // -------------------------------------------------------------
        GameObject countdown = CreateUIPanel(canvasObj, "CountdownPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(500f, 300f), new Color(0f, 0f, 0f, 0f), false);
        CanvasGroup cdCg = countdown.AddComponent<CanvasGroup>();
        uiMgr.countdownCanvasGroup = cdCg;

        Text countTxt = CreateUIText(countdown, "CountdownText", "3", font, 115, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.15f), new Vector2(0.5f, 0.6f), new Vector2(450f, 160f));
        countTxt.fontStyle = FontStyle.Bold;
        var outline = countTxt.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        outline.effectDistance = new Vector2(5f, -5f);
        uiMgr.countdownText = countTxt;

        Text countSub = CreateUIText(countdown, "CountdownSubText", "GET READY!", font, 28, TextAnchor.MiddleCenter, new Color(0.9f, 0.95f, 1.0f, 0.9f), new Vector2(0.5f, 0.18f), new Vector2(400f, 45f));
        countSub.fontStyle = FontStyle.Bold;
        var subOutline = countSub.gameObject.AddComponent<Outline>();
        subOutline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        subOutline.effectDistance = new Vector2(2f, -2f);
        uiMgr.countdownSubText = countSub;

        countdown.SetActive(false);
        uiMgr.countdownPanel = countdown;
    }

    #region Procedural UI Component Builders
    private static Button CreateProceduralProfileCard(GameObject parent, Font font, Sprite cardSprite, Sprite icon, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject("Btn_Profile");
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(30f, -24f);
        rt.sizeDelta = new Vector2(280f, 70f);

        Image img = go.AddComponent<Image>();
        img.sprite = cardSprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(action);

        var feedback = go.AddComponent<UIButtonFeedback>();
        feedback.normalScale = 1.0f;
        feedback.hoverScale = 1.03f;
        feedback.pressedScale = 0.95f;

        // Avatar Icon
        GameObject iconObj = new GameObject("AvatarIcon");
        iconObj.transform.SetParent(go.transform, false);
        RectTransform iRt = iconObj.AddComponent<RectTransform>();
        iRt.anchorMin = new Vector2(0f, 0.5f);
        iRt.anchorMax = new Vector2(0f, 0.5f);
        iRt.pivot = new Vector2(0f, 0.5f);
        iRt.anchoredPosition = new Vector2(10f, 0f);
        iRt.sizeDelta = new Vector2(50f, 50f);
        Image iImg = iconObj.AddComponent<Image>();
        iImg.sprite = icon;

        // Name
        Text nameTxt = CreateUIText(go, "Name", "RacerSrihari", font, 17, TextAnchor.MiddleLeft, Color.white, new Vector2(0f, 0.5f), new Vector2(190f, 24f));
        nameTxt.fontStyle = FontStyle.Bold;
        nameTxt.rectTransform.anchoredPosition = new Vector2(170f, 12f);

        // Level Badge
        Text lvTxt = CreateUIText(go, "Level", "Lv. 28  PRO", font, 12, TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.15f), new Vector2(0f, 0.5f), new Vector2(190f, 20f));
        lvTxt.fontStyle = FontStyle.Bold;
        lvTxt.rectTransform.anchoredPosition = new Vector2(170f, -12f);

        return btn;
    }

    private static Button CreateProceduralPillButton(GameObject parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Sprite pillSprite, Sprite icon, string text, Color textColor, UnityEngine.Events.UnityAction action, Font font)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.sprite = pillSprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(action);

        var feedback = go.AddComponent<UIButtonFeedback>();
        feedback.normalScale = 1.0f;
        feedback.hoverScale = 1.04f;
        feedback.pressedScale = 0.94f;

        // Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(go.transform, false);
        RectTransform iRt = iconObj.AddComponent<RectTransform>();
        iRt.anchorMin = new Vector2(0f, 0.5f);
        iRt.anchorMax = new Vector2(0f, 0.5f);
        iRt.pivot = new Vector2(0f, 0.5f);
        iRt.anchoredPosition = new Vector2(10f, 0f);
        iRt.sizeDelta = new Vector2(32f, 32f);
        Image iImg = iconObj.AddComponent<Image>();
        iImg.sprite = icon;

        // Text
        Text valTxt = CreateUIText(go, "Value", text, font, 16, TextAnchor.MiddleLeft, Color.white, new Vector2(0f, 0.5f), new Vector2(size.x - 65f, 30f));
        valTxt.fontStyle = FontStyle.Bold;
        valTxt.rectTransform.anchoredPosition = new Vector2(46f + (size.x - 65f) * 0.5f, 0f);

        // Plus
        Text plusTxt = CreateUIText(go, "Plus", "+", font, 18, TextAnchor.MiddleRight, textColor, new Vector2(1f, 0.5f), new Vector2(30f, 30f));
        plusTxt.fontStyle = FontStyle.Bold;
        plusTxt.rectTransform.anchoredPosition = new Vector2(-12f, 0f);

        return btn;
    }

    private static Button CreateProceduralSquareButton(GameObject parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Sprite squareSprite, Sprite icon, Color? tint, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.sprite = squareSprite;
        img.type = Image.Type.Sliced;
        img.color = tint ?? Color.white;

        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(action);

        var feedback = go.AddComponent<UIButtonFeedback>();
        feedback.normalScale = 1.0f;
        feedback.hoverScale = 1.06f;
        feedback.pressedScale = 0.92f;

        if (icon != null)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(go.transform, false);
            RectTransform iRt = iconObj.AddComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0.5f, 0.5f);
            iRt.anchorMax = new Vector2(0.5f, 0.5f);
            iRt.pivot = new Vector2(0.5f, 0.5f);
            iRt.anchoredPosition = Vector2.zero;
            iRt.sizeDelta = new Vector2(size.x * 0.55f, size.y * 0.55f);
            Image iImg = iconObj.AddComponent<Image>();
            iImg.sprite = icon;
        }

        return btn;
    }

    private static Button CreateProceduralActionCard(GameObject parent, string name, Vector2 pos, Sprite cardSprite, Sprite icon, string title, Font font, Sprite badge, UnityEngine.Events.UnityAction action, int fontSize = 16)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(230f, 56f);

        Image img = go.AddComponent<Image>();
        img.sprite = cardSprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(action);

        var feedback = go.AddComponent<UIButtonFeedback>();
        feedback.normalScale = 1.0f;
        feedback.hoverScale = 1.04f;
        feedback.pressedScale = 0.94f;

        // Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(go.transform, false);
        RectTransform iRt = iconObj.AddComponent<RectTransform>();
        iRt.anchorMin = new Vector2(0f, 0.5f);
        iRt.anchorMax = new Vector2(0f, 0.5f);
        iRt.pivot = new Vector2(0f, 0.5f);
        iRt.anchoredPosition = new Vector2(14f, 0f);
        iRt.sizeDelta = new Vector2(36f, 36f);
        Image iImg = iconObj.AddComponent<Image>();
        iImg.sprite = icon;

        // Text
        Text t = CreateUIText(go, "Title", title, font, fontSize, TextAnchor.MiddleLeft, Color.white, new Vector2(0f, 0.5f), new Vector2(150f, 30f));
        t.fontStyle = FontStyle.Bold;
        t.rectTransform.anchoredPosition = new Vector2(58f + 75f, 0f);

        // Chevron ›
        Text chev = CreateUIText(go, "Chev", "›", font, 22, TextAnchor.MiddleRight, new Color(0.7f, 0.8f, 0.95f, 0.8f), new Vector2(1f, 0.5f), new Vector2(30f, 30f));
        chev.fontStyle = FontStyle.Bold;
        chev.rectTransform.anchoredPosition = new Vector2(-12f, 0f);

        // Optional Notification Badge
        if (badge != null)
        {
            GameObject bObj = new GameObject("NotificationBadge");
            bObj.transform.SetParent(go.transform, false);
            RectTransform bRt = bObj.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(1f, 1f);
            bRt.anchorMax = new Vector2(1f, 1f);
            bRt.pivot = new Vector2(0.5f, 0.5f);
            bRt.anchoredPosition = new Vector2(-12f, -12f);
            bRt.sizeDelta = new Vector2(20f, 20f);
            Image bImg = bObj.AddComponent<Image>();
            bImg.sprite = badge;
        }

        return btn;
    }

    private static Button CreateProceduralDockButton(GameObject parent, string name, float posX, Sprite icon, string title, Font font, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(posX, 0f);
        rt.sizeDelta = new Vector2(170f, 44f);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.12f, 0.18f, 0.28f, 0.70f);

        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(action);

        var feedback = go.AddComponent<UIButtonFeedback>();
        feedback.normalScale = 1.0f;
        feedback.hoverScale = 1.05f;
        feedback.pressedScale = 0.93f;

        // Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(go.transform, false);
        RectTransform iRt = iconObj.AddComponent<RectTransform>();
        iRt.anchorMin = new Vector2(0f, 0.5f);
        iRt.anchorMax = new Vector2(0f, 0.5f);
        iRt.pivot = new Vector2(0f, 0.5f);
        iRt.anchoredPosition = new Vector2(14f, 0f);
        iRt.sizeDelta = new Vector2(28f, 28f);
        Image iImg = iconObj.AddComponent<Image>();
        iImg.sprite = icon;

        // Title
        Text t = CreateUIText(go, "Title", title, font, 15, TextAnchor.MiddleLeft, Color.white, new Vector2(0f, 0.5f), new Vector2(115f, 30f));
        t.fontStyle = FontStyle.Bold;
        t.rectTransform.anchoredPosition = new Vector2(48f + 57f, 0f);

        return btn;
    }

    private static Button CreateProceduralMapCard(GameObject parent, string name, float posY, Sprite cardSprite, Sprite thumb, string title, string badge, Color badgeColor, UnityEngine.Events.UnityAction action, Font font, bool isSelected = false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, posY);
        rt.sizeDelta = new Vector2(280f, 52f);

        Image img = go.AddComponent<Image>();
        img.sprite = cardSprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(action);

        var feedback = go.AddComponent<UIButtonFeedback>();
        feedback.normalScale = 1.0f;
        feedback.hoverScale = 1.03f;
        feedback.pressedScale = 0.95f;
        feedback.isSelected = isSelected;

        // Thumbnail
        GameObject thObj = new GameObject("Thumb");
        thObj.transform.SetParent(go.transform, false);
        RectTransform tRt = thObj.AddComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 0.5f);
        tRt.anchorMax = new Vector2(0f, 0.5f);
        tRt.pivot = new Vector2(0f, 0.5f);
        tRt.anchoredPosition = new Vector2(8f, 0f);
        tRt.sizeDelta = new Vector2(58f, 38f);
        Image tImg = thObj.AddComponent<Image>();
        tImg.sprite = thumb;

        // Title
        Text t = CreateUIText(go, "Title", title, font, 14, TextAnchor.MiddleLeft, Color.white, new Vector2(0f, 0.5f), new Vector2(140f, 22f));
        t.fontStyle = FontStyle.Bold;
        t.rectTransform.anchoredPosition = new Vector2(74f + 70f, 8f);

        // Badge
        Text bTxt = CreateUIText(go, "Badge", badge, font, 11, TextAnchor.MiddleLeft, badgeColor, new Vector2(0f, 0.5f), new Vector2(140f, 18f));
        bTxt.fontStyle = FontStyle.Bold;
        bTxt.rectTransform.anchoredPosition = new Vector2(74f + 70f, -10f);

        return btn;
    }

    private static Button CreateProceduralPlayButton(GameObject parent, Font font, Sprite playShape, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject("Btn_Play");
        go.transform.SetParent(parent.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-30f, 32f);
        rt.sizeDelta = new Vector2(320f, 82f);

        Image img = go.AddComponent<Image>();
        img.sprite = playShape;
        img.type = Image.Type.Simple;
        img.color = Color.white;

        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(action);

        var feedback = go.AddComponent<UIButtonFeedback>();
        feedback.normalScale = 1.0f;
        feedback.hoverScale = 1.07f;
        feedback.pressedScale = 0.92f;

        // Title: PLAY ▶
        Text playTxt = CreateUIText(go, "PlayText", "PLAY ▶", font, 38, TextAnchor.MiddleCenter, new Color(0.06f, 0.09f, 0.16f), new Vector2(0.5f, 0.58f), new Vector2(280f, 48f));
        playTxt.fontStyle = FontStyle.Bold;

        // Subtitle: COASTAL HIGHWAY
        Text subTxt = CreateUIText(go, "SubText", "COASTAL HIGHWAY • 4 LANES", font, 11, TextAnchor.MiddleCenter, new Color(0.24f, 0.18f, 0.02f), new Vector2(0.5f, 0.22f), new Vector2(280f, 20f));
        subTxt.fontStyle = FontStyle.Bold;

        return btn;
    }

    private static void SetupProceduralModalSystem(GameObject canvasObj, HighwayUIManager uiMgr, Font font, Sprite modalBg, Sprite actionBtnBg, Sprite closeIcon)
    {
        // Fullscreen Modal Panel (Dark Dimmed Scrim)
        GameObject modalObj = CreateUIPanel(canvasObj, "MenuModalPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.75f), true);
        uiMgr.menuModalPanel = modalObj;

        // Centered Modal Window
        GameObject win = CreateUIPanel(modalObj, "ModalWindow", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 440f), Color.white, true);
        Image winImg = win.GetComponent<Image>();
        winImg.sprite = modalBg;
        winImg.type = Image.Type.Sliced;

        // Title
        Text titleTxt = CreateUIText(win, "ModalTitle", "MODAL TITLE", font, 22, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 1f), new Vector2(460f, 50f));
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.rectTransform.anchoredPosition = new Vector2(0f, -32f);
        uiMgr.modalTitleText = titleTxt;

        // Close ✕ Button
        Button closeBtn = CreateProceduralSquareButton(win, "Btn_ModalClose", new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(40f, 40f),
            actionBtnBg, closeIcon, null, () => uiMgr.CloseModal());
        uiMgr.modalCloseBtn = closeBtn;

        // Body Text
        Text bodyTxt = CreateUIText(win, "ModalBody", "Content goes here...", font, 16, TextAnchor.UpperLeft, new Color(0.85f, 0.92f, 1f), new Vector2(0.5f, 0.5f), new Vector2(520f, 240f));
        bodyTxt.rectTransform.anchoredPosition = new Vector2(0f, 8f);
        uiMgr.modalBodyText = bodyTxt;

        // Action Button
        GameObject actGo = new GameObject("Btn_ModalAction");
        actGo.transform.SetParent(win.transform, false);
        RectTransform aRt = actGo.AddComponent<RectTransform>();
        aRt.anchorMin = new Vector2(0.5f, 0f);
        aRt.anchorMax = new Vector2(0.5f, 0f);
        aRt.pivot = new Vector2(0.5f, 0f);
        aRt.anchoredPosition = new Vector2(0f, 26f);
        aRt.sizeDelta = new Vector2(260f, 50f);

        Image aImg = actGo.AddComponent<Image>();
        aImg.sprite = actionBtnBg;
        aImg.type = Image.Type.Sliced;

        Button actBtn = actGo.AddComponent<Button>();
        var feedback = actGo.AddComponent<UIButtonFeedback>();
        feedback.normalScale = 1.0f;
        feedback.hoverScale = 1.04f;
        feedback.pressedScale = 0.94f;

        Text actTxt = CreateUIText(actGo, "ActionText", "CONFIRM", font, 16, TextAnchor.MiddleCenter, new Color(1f, 0.90f, 0.20f), new Vector2(0.5f, 0.5f), new Vector2(240f, 40f));
        actTxt.fontStyle = FontStyle.Bold;

        uiMgr.modalActionBtn = actBtn;
        uiMgr.modalActionBtnText = actTxt;

        modalObj.SetActive(false);
    }
    #endregion

    private static void SetupAudio()
    {
        GameObject audioObj = new GameObject("AudioManager");
        audioObj.AddComponent<CarAudioManager>();
        audioObj.AddComponent<ProceduralUISound>();
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
                    frictionCombine = PhysicsMaterialCombine.Minimum,
                    bounceCombine = PhysicsMaterialCombine.Minimum
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
                SphereCollider sc = wColGo.AddComponent<SphereCollider>();
                sc.radius = 0.34f;
                sc.material = tireMat;
            }

            var cc = car.GetComponent<CarController>();
            if (cc != null)
            {
                cc.maxSpeedKmh = 240f;
                cc.accelerationRate = 32f;
                cc.brakePower = 45f;
                cc.reverseAccelerationRate = 18f;
                cc.steerSpeed = 8.5f;
                cc.maxSteerAngle = 30f;
                cc.driftLateralGrip = 2.2f;
                cc.enabled = false; // Disabled while in main menu
            }
            if (rb != null)
            {
                rb.isKinematic = true; // Frozen while in main menu
            }
        }

        // Camera Follower Setup
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            var camObj = GameObject.Find("Main Camera");
            if (camObj != null) mainCam = camObj.GetComponent<Camera>();
        }

        if (mainCam != null && car != null)
        {
            var camFollower = mainCam.GetComponent<CarCameraController>();
            if (camFollower == null) camFollower = mainCam.gameObject.AddComponent<CarCameraController>();
            camFollower.target = car.transform;
            camFollower.currentMode = CarCameraController.CameraViewMode.ChaseMedium;
            camFollower.mediumOffset = new Vector3(0f, 2.3f, -6.5f);
            camFollower.mediumLookHeight = 1.15f;
        }
    }

    #region UI Helper Methods
    private static Sprite LoadProceduralSprite(string name, Vector4? border = null)
    {
        string path = $"Assets/Textures/UI/Procedural/{name}.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            bool needReimport = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                needReimport = true;
            }
            if (border.HasValue && importer.spriteBorder != border.Value)
            {
                importer.spriteBorder = border.Value;
                needReimport = true;
            }
            if (needReimport)
            {
                importer.SaveAndReimport();
            }
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject CreateUIPanel(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, Color color, bool raycast = false)
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
        img.raycastTarget = raycast;
        return panel;
    }

    private static Text CreateUIText(GameObject parent, string name, string content, Font font, int size, TextAnchor align, Color color, Vector2 anchorPos, Vector2 dimension, bool raycast = false)
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
        t.raycastTarget = raycast;
        return t;
    }

    private static Button CreateMiniButton(GameObject parent, string name, string text, Font font, Vector2 anchor, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action, Color? bgColor = null, int fontSize = 12)
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
        img.color = bgColor ?? new Color(0.18f, 0.22f, 0.28f, 0.90f);
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(action);

        var feedback = btnObj.AddComponent<UIButtonFeedback>();
        feedback.normalScale = 1.0f;
        feedback.hoverScale = 1.06f;
        feedback.pressedScale = 0.92f;

        Text txt = CreateUIText(btnObj, "Text", text, font, fontSize, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), size, false);
        txt.fontStyle = FontStyle.Bold;
        return btn;
    }

    private static GameObject CreateTouchPedal(GameObject parent, string name, string label, Font font, Vector2 anchor, Vector2 pos, Vector2 size, Color color,
        System.Action onDown, System.Action onUp, Sprite sprite = null)
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
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
        }
        img.color = color;
        img.raycastTarget = true;

        Text lbl = CreateUIText(pedal, "Label", label, font, 19, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), size, false);
        lbl.fontStyle = FontStyle.Bold;
        lbl.lineSpacing = 0.95f;
        var outline = lbl.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.90f);
        outline.effectDistance = new Vector2(2f, -2f);

        // Tactile button feedback
        var feedback = pedal.AddComponent<UIButtonFeedback>();
        feedback.normalScale = 1.0f;
        feedback.hoverScale = 1.05f;
        feedback.pressedScale = 0.92f;

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
        return pedal;
    }
    #endregion
}
