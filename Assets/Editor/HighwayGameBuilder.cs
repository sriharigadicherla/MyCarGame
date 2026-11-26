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
            "HighwayManager", "HighwayTrafficManager", "HighwayOvertakeDetector", "Plane", "EventSystem"
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
            new Color(0.98f, 0.82f, 0.08f, 1f), 0.0f, 0.35f, new Color(0.45f, 0.38f, 0.04f, 1f));

        dict["Mat_Road_Marking_White"] = GetOrCreateMat("Mat_Road_Marking_White", litShader,
            new Color(1f, 1f, 1f, 1f), 0.0f, 0.35f, new Color(0.6f, 0.6f, 0.65f, 1f));

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

        dict["Mat_Headlight_LED"] = GetOrCreateMat("Mat_Headlight_LED", litShader,
            new Color(1f, 1f, 1f, 1f), 0.1f, 0.95f, new Color(3.5f, 3.5f, 3.6f, 1f));
        dict["Mat_Glass_Tinted"] = GetOrCreateMat("Mat_Glass_Tinted", litShader,
            new Color(0.10f, 0.12f, 0.16f, 0.95f), 0.8f, 0.95f);
        dict["Mat_Tire_Rubber"] = GetOrCreateMat("Mat_Tire_Rubber", litShader,
            new Color(0.12f, 0.12f, 0.12f, 1f), 0.0f, 0.2f);
        dict["Mat_Dark_Trim"] = GetOrCreateMat("Mat_Dark_Trim", litShader,
            new Color(0.08f, 0.08f, 0.08f, 1f), 0.2f, 0.4f);

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

    private static void SetupHighwayUI(HighwayOvertakeDetector detector, HighwayTrafficManager trafficMgr, HighwayManager highwayMgr)
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

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // -------------------------------------------------------------
        // A. MAIN MENU / LOBBY PANEL (AAA Racing UI Professional System)
        // -------------------------------------------------------------
        GameObject menuPanel = CreateUIPanel(canvasObj, "MainMenuPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white, true);
        uiMgr.mainMenuPanel = menuPanel;

        // Backdrop Image (Clean first image with hypercar on frozen lake)
        string texPath = "Assets/Textures/UI/MainMenu_Backdrop.jpg";
        TextureImporter bgImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (bgImporter != null && bgImporter.textureType != TextureImporterType.Sprite)
        {
            bgImporter.textureType = TextureImporterType.Sprite;
            bgImporter.spriteImportMode = SpriteImportMode.Single;
            bgImporter.SaveAndReimport();
        }
        Sprite menuSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
        if (menuSprite != null)
        {
            Image bgImg = menuPanel.GetComponent<Image>();
            bgImg.sprite = menuSprite;
            bgImg.type = Image.Type.Simple;
            bgImg.preserveAspect = false;
        }

        // 1. TOP BAR
        Button profBtn = CreateSpriteButton(menuPanel, "Btn_Profile", LoadSprite("UI_Profile"), new Vector2(0.0107f, 0.8979f), new Vector2(0.1650f, 0.9787f), () => uiMgr.OnProfileClicked());
        uiMgr.profileBtn = profBtn;

        Button coinsBtn = CreateSpriteButton(menuPanel, "Btn_Coins", LoadSprite("UI_Coins"), new Vector2(0.7207f, 0.9106f), new Vector2(0.7930f, 0.9787f), () => uiMgr.OnCoinsClicked());
        uiMgr.coinsBtn = coinsBtn;

        Button gemsBtn = CreateSpriteButton(menuPanel, "Btn_Gems", LoadSprite("UI_Gems"), new Vector2(0.7998f, 0.9106f), new Vector2(0.8770f, 0.9787f), () => uiMgr.OnGemsClicked());
        uiMgr.gemsBtn = gemsBtn;

        Button mailBtn = CreateSpriteButton(menuPanel, "Btn_Mail", LoadSprite("UI_Mail"), new Vector2(0.8838f, 0.9106f), new Vector2(0.9258f, 0.9787f), () => uiMgr.OnMailClicked());
        uiMgr.mailBtn = mailBtn;

        Button settingsBtn = CreateSpriteButton(menuPanel, "Btn_Settings", LoadSprite("UI_Settings"), new Vector2(0.9307f, 0.9106f), new Vector2(0.9727f, 0.9787f), () => uiMgr.OnSettingsClicked());
        uiMgr.settingsBtn = settingsBtn;

        // Windows Exit Button (Top right tiny red close button)
        CreateMiniButton(menuPanel, "Btn_MenuExit", "✕", font, new Vector2(1f, 1f), new Vector2(-15f, -15f), new Vector2(28f, 28f), () => uiMgr.OnExitGameClicked(), new Color(0.85f, 0.18f, 0.18f, 0.95f), 14);

        // 2. RIGHT SIDE MENU (Vertical Column)
        Button eventsBtn = CreateSpriteButton(menuPanel, "Btn_Events", LoadSprite("UI_Btn_Events"), new Vector2(0.8438f, 0.7809f), new Vector2(0.9824f, 0.8745f), () => uiMgr.OnEventsClicked());
        uiMgr.eventsBtn = eventsBtn;

        Button garageBtn = CreateSpriteButton(menuPanel, "Btn_Garage", LoadSprite("UI_Btn_Garage"), new Vector2(0.8438f, 0.6745f), new Vector2(0.9824f, 0.7681f), () => uiMgr.OnGarageClicked());
        uiMgr.garageBtn = garageBtn;

        Button customBtn = CreateSpriteButton(menuPanel, "Btn_Customization", LoadSprite("UI_Btn_Custom"), new Vector2(0.8438f, 0.5681f), new Vector2(0.9824f, 0.6617f), () => uiMgr.OnCustomizationClicked());
        uiMgr.customizationBtn = customBtn;

        Button storeBtn = CreateSpriteButton(menuPanel, "Btn_Store", LoadSprite("UI_Btn_Store"), new Vector2(0.8438f, 0.4617f), new Vector2(0.9824f, 0.5553f), () => uiMgr.OnStoreClicked());
        uiMgr.storeBtn = storeBtn;

        // 3. BOTTOM BAR (Horizontal Navigation Row)
        Button carsBtn = CreateSpriteButton(menuPanel, "Btn_Cars", LoadSprite("UI_Btn_Cars"), new Vector2(0.2773f, 0.0362f), new Vector2(0.3867f, 0.1191f), () => uiMgr.OnCarsClicked());
        uiMgr.carsBtn = carsBtn;

        Button upgradesBtn = CreateSpriteButton(menuPanel, "Btn_Upgrades", LoadSprite("UI_Btn_Upgrades"), new Vector2(0.3945f, 0.0362f), new Vector2(0.5088f, 0.1191f), () => uiMgr.OnUpgradesClicked());
        uiMgr.upgradesBtn = upgradesBtn;

        Button missionsBtn = CreateSpriteButton(menuPanel, "Btn_Missions", LoadSprite("UI_Btn_Missions"), new Vector2(0.5166f, 0.0362f), new Vector2(0.6309f, 0.1191f), () => uiMgr.OnMissionsClicked());
        uiMgr.missionsBtn = missionsBtn;

        Button leaderboardBtn = CreateSpriteButton(menuPanel, "Btn_Leaderboard", LoadSprite("UI_Btn_Leaderboard"), new Vector2(0.6387f, 0.0362f), new Vector2(0.7676f, 0.1191f), () => uiMgr.OnLeaderboardClicked());
        uiMgr.leaderboardBtn = leaderboardBtn;

        // 4. BOTTOM-LEFT: LOCATION SELECTOR
        CreateSpriteButton(menuPanel, "UI_Loc_Header", LoadSprite("UI_Loc_Header"), new Vector2(0.0186f, 0.3064f), new Vector2(0.1865f, 0.3702f), null);

        Button alpineMapBtn = CreateSpriteButton(menuPanel, "MapBtn_Alpine", LoadSprite("UI_Loc_Alpine"), new Vector2(0.0186f, 0.2383f), new Vector2(0.1865f, 0.3064f), () => uiMgr.OnMapSelected("Alpine Circuit", false));
        uiMgr.mapAlpineBtn = alpineMapBtn;

        Button desertMapBtn = CreateSpriteButton(menuPanel, "MapBtn_Desert", LoadSprite("UI_Loc_Desert"), new Vector2(0.0186f, 0.1702f), new Vector2(0.1865f, 0.2383f), () => uiMgr.OnMapSelected("Desert Rush", false));
        uiMgr.mapDesertBtn = desertMapBtn;

        Button cityMapBtn = CreateSpriteButton(menuPanel, "MapBtn_City", LoadSprite("UI_Loc_City"), new Vector2(0.0186f, 0.1021f), new Vector2(0.1865f, 0.1702f), () => uiMgr.OnMapSelected("City Night", false));
        uiMgr.mapCityBtn = cityMapBtn;

        Button highwayMapBtn = CreateSpriteButton(menuPanel, "MapBtn_Highway", LoadSprite("UI_Loc_Highway"), new Vector2(0.0186f, 0.0340f), new Vector2(0.1865f, 0.1021f), () => uiMgr.OnMapSelected("Coastal Highway", true));
        uiMgr.mapHighwayBtn = highwayMapBtn;

        // 5. BOTTOM-RIGHT: LARGE PLAY BUTTON
        Button playBtn = CreateSpriteButton(menuPanel, "Btn_Play", LoadSprite("UI_Btn_Play"), new Vector2(0.7793f, 0.0277f), new Vector2(0.9863f, 0.1745f), () => uiMgr.StartHighwayGame());
        uiMgr.playButton = playBtn;

        // 6. FLOATING TOAST NOTIFICATIONS (Top Center)
        GameObject toast = CreateUIPanel(canvasObj, "ToastPanel", new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 52f), new Color(0.06f, 0.09f, 0.14f, 0.96f), false);
        CanvasGroup toastCg = toast.AddComponent<CanvasGroup>();
        uiMgr.toastCanvasGroup = toastCg;
        uiMgr.toastPanel = toast;
        Text tText = CreateUIText(toast, "ToastText", "Welcome, RacerSrihari!", font, 18, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.1f), new Vector2(0.5f, 0.5f), new Vector2(600f, 44f));
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
        CanvasGroup cg = popupObj.AddComponent<CanvasGroup>();
        uiMgr.popupCanvasGroup = cg;

        Text pTxt = CreateUIText(popupObj, "PopupText", "⚡ CLOSE OVERTAKE! +$250 ⚡", font, 26, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.15f), new Vector2(0.5f, 0.5f), new Vector2(480f, 50f));
        pTxt.fontStyle = FontStyle.Bold;
        uiMgr.popupText = pTxt;

        // 3. Top-Right Speedometer
        GameObject speedo = CreateUIPanel(gameplayHUD, "Speedometer", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -70f), new Vector2(220f, 125f), new Color(0.08f, 0.10f, 0.14f, 0.88f), false);

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

        // 4. On-Screen Virtual Driving Controls (Bottom)
        GameObject steerLeft = CreateTouchPedal(gameplayHUD, "Btn_SteerLeft", "◀\n(A)", font, new Vector2(0f, 0f), new Vector2(80f, 90f), new Vector2(90f, 90f), new Color(0.18f, 0.22f, 0.30f, 0.85f),
            () => uiMgr.OnSteerLeftDown(), () => uiMgr.OnSteerLeftUp());

        GameObject steerRight = CreateTouchPedal(gameplayHUD, "Btn_SteerRight", "▶\n(D)", font, new Vector2(0f, 0f), new Vector2(185f, 90f), new Vector2(90f, 90f), new Color(0.18f, 0.22f, 0.30f, 0.85f),
            () => uiMgr.OnSteerRightDown(), () => uiMgr.OnSteerRightUp());

        GameObject gas = CreateTouchPedal(gameplayHUD, "Btn_Gas", "GAS\n(W)", font, new Vector2(1f, 0f), new Vector2(-75f, 110f), new Vector2(100f, 130f), new Color(0.12f, 0.65f, 0.32f, 0.88f),
            () => uiMgr.OnGasDown(), () => uiMgr.OnGasUp());

        GameObject brake = CreateTouchPedal(gameplayHUD, "Btn_Brake", "BRAKE\n(S)", font, new Vector2(1f, 0f), new Vector2(-185f, 95f), new Vector2(90f, 105f), new Color(0.85f, 0.22f, 0.22f, 0.88f),
            () => uiMgr.OnBrakeDown(), () => uiMgr.OnBrakeUp());

        GameObject drift = CreateTouchPedal(gameplayHUD, "Btn_Drift", "DRIFT\n(Space)", font, new Vector2(1f, 0f), new Vector2(-285f, 85f), new Vector2(85f, 85f), new Color(0.88f, 0.55f, 0.12f, 0.88f),
            () => uiMgr.OnDriftDown(), () => uiMgr.OnDriftUp());

        uiMgr.steerLeftPedal = steerLeft;
        uiMgr.steerRightPedal = steerRight;
        uiMgr.gasPedal = gas;
        uiMgr.brakePedal = brake;
        uiMgr.driftPedal = drift;

        gameplayHUD.SetActive(false); // Initially hidden while in Main Menu!

        // -------------------------------------------------------------
        // C. PAUSE MENU PANEL
        // -------------------------------------------------------------
        GameObject pause = CreateUIPanel(canvasObj, "PausePanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 350f), new Color(0.08f, 0.10f, 0.14f, 0.97f), true);
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
            cc.isGameplayActive = false;

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
    private static Sprite LoadSprite(string name)
    {
        string path = $"Assets/Textures/UI/Sprites/{name}.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Button CreateSpriteButton(GameObject parent, string name, Sprite sprite, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent.transform, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = btnObj.AddComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.18f, 1.18f, 1.18f, 1f);
        cb.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        cb.selectedColor = Color.white;
        btn.colors = cb;

        if (action != null) btn.onClick.AddListener(action);
        return btn;
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

        Text txt = CreateUIText(btnObj, "Text", text, font, fontSize, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), size, false);
        txt.fontStyle = FontStyle.Bold;
        return btn;
    }

    private static GameObject CreateTouchPedal(GameObject parent, string name, string label, Font font, Vector2 anchor, Vector2 pos, Vector2 size, Color color,
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
        img.raycastTarget = true;

        CreateUIText(pedal, "Label", label, font, 16, TextAnchor.MiddleCenter, Color.white, new Vector2(0.5f, 0.5f), size, false);

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
