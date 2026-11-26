using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Modern Highway Driving & Overtake HUD + AAA Main Menu System.
/// Coordinates Main Menu Lobby (Profile, Map Selector, Play Button),
/// Speedometer, Gear Display, RPM, Overtake Counter, Combo Multiplier,
/// Floating Reward Pop-up, On-screen Pedals, and Pause Menu.
/// </summary>
public class HighwayUIManager : MonoBehaviour
{
    [Header("References")]
    public CarController carController;
    public CarVisuals carVisuals;
    public CarCameraController cameraController;
    public HighwayOvertakeDetector overtakeDetector;
    public HighwayTrafficManager trafficManager;
    public HighwayManager highwayManager;

    [Header("Main Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameplayHUDPanel;
    public GameObject pausePanel;

    [Header("Main Menu UI - Top Bar")]
    public Button profileBtn;
    public Button coinsBtn;
    public Button gemsBtn;
    public Button mailBtn;
    public Button settingsBtn;

    [Header("Main Menu UI - Right Side Menu")]
    public Button eventsBtn;
    public Button garageBtn;
    public Button customizationBtn;
    public Button storeBtn;

    [Header("Main Menu UI - Bottom Bar")]
    public Button carsBtn;
    public Button upgradesBtn;
    public Button missionsBtn;
    public Button leaderboardBtn;

    [Header("Main Menu UI - Location Selector")]
    public Button mapHighwayBtn;
    public Button mapAlpineBtn;
    public Button mapDesertBtn;
    public Button mapCityBtn;
    public Text selectedMapTitleText;
    public Text selectedMapBadgeText;
    public string selectedLocation = "Coastal Highway";

    [Header("Main Menu UI - Play Button")]
    public Button playButton;

    [Header("Toast Notification")]
    public GameObject toastPanel;
    public Text toastText;
    public CanvasGroup toastCanvasGroup;

    [Header("Speedometer & Dash")]
    public Text speedText;
    public Text gearText;
    public Text rpmText;
    public Image speedGaugeBar;

    [Header("Overtake & Score Stats")]
    public Text overtakeCountText;
    public Text scoreText;
    public Text cashText;
    public Text comboText;

    [Header("Overtake Pop-up Notification")]
    public Text popupText;
    public CanvasGroup popupCanvasGroup;

    [Header("HUD Buttons")]
    public Button topExitButton;
    public Button lightsButton;
    public Button camButton;
    public Button pauseButton;
    public Button restartButton;

    [Header("Pause Panel Buttons")]
    public Button resumeButton;
    public Button pauseRestartButton;
    public Button pauseMainMenuButton;
    public Button pauseExitButton;

    [Header("Touch Pedals")]
    public GameObject steerLeftPedal;
    public GameObject steerRightPedal;
    public GameObject gasPedal;
    public GameObject brakePedal;
    public GameObject driftPedal;

    // Runtime state
    public bool IsGameplayRunning { get; private set; } = false;
    private float popupTimer = 0f;
    private float toastTimer = 0f;
    private bool isPaused = false;

    // Virtual Inputs
    private float virtualMove = 0f;
    private float virtualSteer = 0f;
    private bool virtualHandbrake = false;

    void Start()
    {
        FindReferencesIfNull();
        WireButtonEvents();

        // Initialize UI panels: Show Main Menu first!
        ShowMainMenu();

        if (popupCanvasGroup != null) popupCanvasGroup.alpha = 0f;
        if (toastCanvasGroup != null) toastCanvasGroup.alpha = 0f;

        if (overtakeDetector != null)
        {
            overtakeDetector.OnOvertakeEvent += ShowOvertakePopup;
        }
    }

    public void ShowMainMenu()
    {
        IsGameplayRunning = false;
        isPaused = false;
        Time.timeScale = 1f;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (gameplayHUDPanel != null) gameplayHUDPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        if (carController != null) carController.ResetToStart();
        if (trafficManager != null) trafficManager.ResetTrafficStream();
    }

    public void StartHighwayGame()
    {
        IsGameplayRunning = true;
        isPaused = false;
        Time.timeScale = 1f;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameplayHUDPanel != null) gameplayHUDPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);

        if (carController != null) carController.isGameplayActive = true;
        if (trafficManager != null) trafficManager.StartTraffic();

        ShowToast("🏁 GO! Highway Overtake Mode Activated!", 2.0f);
    }

    private void WireButtonEvents()
    {
        // Auto-find buttons if not hooked in inspector
        if (playButton == null) playButton = FindButtonByName("Btn_Play");
        if (topExitButton == null) topExitButton = FindButtonByName("Btn_TopExit");
        if (lightsButton == null) lightsButton = FindButtonByName("Btn_Lights");
        if (camButton == null) camButton = FindButtonByName("Btn_Cam");
        if (pauseButton == null) pauseButton = FindButtonByName("Btn_Pause");
        if (restartButton == null) restartButton = FindButtonByName("Btn_Restart");

        if (resumeButton == null) resumeButton = FindButtonByName("ResumeBtn");
        if (pauseRestartButton == null) pauseRestartButton = FindButtonByName("RestartBtn");
        if (pauseMainMenuButton == null) pauseMainMenuButton = FindButtonByName("MainMenuBtn");
        if (pauseExitButton == null) pauseExitButton = FindButtonByName("ExitBtn");

        // Top Bar Buttons
        if (profileBtn == null) profileBtn = FindButtonByName("Btn_Profile");
        if (coinsBtn == null) coinsBtn = FindButtonByName("Btn_Coins");
        if (gemsBtn == null) gemsBtn = FindButtonByName("Btn_Gems");
        if (mailBtn == null) mailBtn = FindButtonByName("Btn_Mail");
        if (settingsBtn == null) settingsBtn = FindButtonByName("Btn_Settings");

        // Right Side Menu Buttons
        if (eventsBtn == null) eventsBtn = FindButtonByName("Btn_Events");
        if (garageBtn == null) garageBtn = FindButtonByName("Btn_Garage");
        if (customizationBtn == null) customizationBtn = FindButtonByName("Btn_Customization");
        if (storeBtn == null) storeBtn = FindButtonByName("Btn_Store");

        // Bottom Bar Buttons
        if (carsBtn == null) carsBtn = FindButtonByName("Btn_Cars");
        if (upgradesBtn == null) upgradesBtn = FindButtonByName("Btn_Upgrades");
        if (missionsBtn == null) missionsBtn = FindButtonByName("Btn_Missions");
        if (leaderboardBtn == null) leaderboardBtn = FindButtonByName("Btn_Leaderboard");

        // Wire Main Menu Callbacks
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(StartHighwayGame);
        }

        if (profileBtn != null) { profileBtn.onClick.RemoveAllListeners(); profileBtn.onClick.AddListener(OnProfileClicked); }
        if (coinsBtn != null) { coinsBtn.onClick.RemoveAllListeners(); coinsBtn.onClick.AddListener(OnCoinsClicked); }
        if (gemsBtn != null) { gemsBtn.onClick.RemoveAllListeners(); gemsBtn.onClick.AddListener(OnGemsClicked); }
        if (mailBtn != null) { mailBtn.onClick.RemoveAllListeners(); mailBtn.onClick.AddListener(OnMailClicked); }
        if (settingsBtn != null) { settingsBtn.onClick.RemoveAllListeners(); settingsBtn.onClick.AddListener(OnSettingsClicked); }

        if (eventsBtn != null) { eventsBtn.onClick.RemoveAllListeners(); eventsBtn.onClick.AddListener(OnEventsClicked); }
        if (garageBtn != null) { garageBtn.onClick.RemoveAllListeners(); garageBtn.onClick.AddListener(OnGarageClicked); }
        if (customizationBtn != null) { customizationBtn.onClick.RemoveAllListeners(); customizationBtn.onClick.AddListener(OnCustomizationClicked); }
        if (storeBtn != null) { storeBtn.onClick.RemoveAllListeners(); storeBtn.onClick.AddListener(OnStoreClicked); }

        if (carsBtn != null) { carsBtn.onClick.RemoveAllListeners(); carsBtn.onClick.AddListener(OnCarsClicked); }
        if (upgradesBtn != null) { upgradesBtn.onClick.RemoveAllListeners(); upgradesBtn.onClick.AddListener(OnUpgradesClicked); }
        if (missionsBtn != null) { missionsBtn.onClick.RemoveAllListeners(); missionsBtn.onClick.AddListener(OnMissionsClicked); }
        if (leaderboardBtn != null) { leaderboardBtn.onClick.RemoveAllListeners(); leaderboardBtn.onClick.AddListener(OnLeaderboardClicked); }

        if (pauseMainMenuButton != null)
        {
            pauseMainMenuButton.onClick.RemoveAllListeners();
            pauseMainMenuButton.onClick.AddListener(ShowMainMenu);
        }

        // Map buttons
        WireMapButtons();

        // Runtime click listeners
        if (topExitButton != null)
        {
            topExitButton.onClick.RemoveAllListeners();
            topExitButton.onClick.AddListener(OnExitGameClicked);
        }
        if (lightsButton != null)
        {
            lightsButton.onClick.RemoveAllListeners();
            lightsButton.onClick.AddListener(OnLightsButtonClicked);
        }
        if (camButton != null)
        {
            camButton.onClick.RemoveAllListeners();
            camButton.onClick.AddListener(OnCamButtonClicked);
        }
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(TogglePause);
        }
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(OnResumeClicked);
        }
        if (pauseRestartButton != null)
        {
            pauseRestartButton.onClick.RemoveAllListeners();
            pauseRestartButton.onClick.AddListener(OnRestartClicked);
        }
        if (pauseExitButton != null)
        {
            pauseExitButton.onClick.RemoveAllListeners();
            pauseExitButton.onClick.AddListener(OnExitGameClicked);
        }

        // Wire touch pedals
        WirePedal(steerLeftPedal, OnSteerLeftDown, OnSteerLeftUp);
        WirePedal(steerRightPedal, OnSteerRightDown, OnSteerRightUp);
        WirePedal(gasPedal, OnGasDown, OnGasUp);
        WirePedal(brakePedal, OnBrakeDown, OnBrakeUp);
        WirePedal(driftPedal, OnDriftDown, OnDriftUp);
    }

    private void WireMapButtons()
    {
        if (mapHighwayBtn == null) mapHighwayBtn = FindButtonByName("MapBtn_Highway");
        if (mapAlpineBtn == null) mapAlpineBtn = FindButtonByName("MapBtn_Alpine");
        if (mapDesertBtn == null) mapDesertBtn = FindButtonByName("MapBtn_Desert");
        if (mapCityBtn == null) mapCityBtn = FindButtonByName("MapBtn_City");

        if (mapHighwayBtn != null)
        {
            mapHighwayBtn.onClick.RemoveAllListeners();
            mapHighwayBtn.onClick.AddListener(() => OnMapSelected("Coastal Highway", true));
        }
        if (mapAlpineBtn != null)
        {
            mapAlpineBtn.onClick.RemoveAllListeners();
            mapAlpineBtn.onClick.AddListener(() => OnMapSelected("Alpine Circuit", false));
        }
        if (mapDesertBtn != null)
        {
            mapDesertBtn.onClick.RemoveAllListeners();
            mapDesertBtn.onClick.AddListener(() => OnMapSelected("Desert Rush", false));
        }
        if (mapCityBtn != null)
        {
            mapCityBtn.onClick.RemoveAllListeners();
            mapCityBtn.onClick.AddListener(() => OnMapSelected("City Night", false));
        }
    }

    #region Custom Main Menu Hooks (Ready for Custom Logic!)
    public void OnProfileClicked()
    {
        Debug.Log("[MainMenu] Profile clicked!");
        ShowToast("👤 Profile: RacerSrihari • Level 28 • Pro Driver");
        // TODO: Add your profile screen / stats popup here!
    }

    public void OnCoinsClicked()
    {
        Debug.Log("[MainMenu] Coins clicked!");
        ShowToast("💰 Balance: 15,000 Coins");
        // TODO: Add your currency shop / coin packages here!
    }

    public void OnGemsClicked()
    {
        Debug.Log("[MainMenu] Gems clicked!");
        ShowToast("💎 Balance: 320 Gems • Click to add more!");
        // TODO: Add your gem shop logic here!
    }

    public void OnMailClicked()
    {
        Debug.Log("[MainMenu] Mail clicked!");
        ShowToast("✉️ Mailbox: 0 unread messages");
        // TODO: Add your mailbox / reward claim logic here!
    }

    public void OnSettingsClicked()
    {
        Debug.Log("[MainMenu] Settings clicked!");
        ShowToast("⚙️ Settings: Quality HIGH • Audio 100%");
        // TODO: Add your settings dialog here!
    }

    public void OnEventsClicked()
    {
        Debug.Log("[MainMenu] Events clicked!");
        ShowToast("🏆 Events: Daily Highway Rush Tournament active!");
        // TODO: Add your events / tournament popup here!
    }

    public void OnGarageClicked()
    {
        Debug.Log("[MainMenu] Garage clicked!");
        ShowToast("🏎️ Garage: Sports Sedan V8 (200 KM/H) Ready to Drive!");
        // TODO: Add your 3D car garage / selector here!
    }

    public void OnCustomizationClicked()
    {
        Debug.Log("[MainMenu] Customization clicked!");
        ShowToast("🔧 Customization: Paint, Rims, Bodykits coming soon!");
        // TODO: Add your vehicle tuning / visual styling logic here!
    }

    public void OnStoreClicked()
    {
        Debug.Log("[MainMenu] Store clicked!");
        ShowToast("🛒 Store: Bundles, Supercars, and Crates coming soon!");
        // TODO: Add your marketplace / store popup here!
    }

    public void OnCarsClicked()
    {
        Debug.Log("[MainMenu] Cars clicked!");
        ShowToast("🚗 Cars: 1/5 Vehicles Unlocked (Sports Sedan V8)");
        // TODO: Add your car showroom / fleet list here!
    }

    public void OnUpgradesClicked()
    {
        Debug.Log("[MainMenu] Upgrades clicked!");
        ShowToast("🛠️ Upgrades: Engine Stage 3 • Twin-Turbo Active");
        // TODO: Add your performance upgrade logic here!
    }

    public void OnMissionsClicked()
    {
        Debug.Log("[MainMenu] Missions clicked!");
        ShowToast("🎯 Missions: Perform 10 Close Passes (Reward: $2,500)");
        // TODO: Add your daily / weekly missions system here!
    }

    public void OnLeaderboardClicked()
    {
        Debug.Log("[MainMenu] Leaderboard clicked!");
        ShowToast("📊 Leaderboard: #1 Srihari - 15 Overtakes - Score 5,890");
        // TODO: Add your global / friends leaderboard here!
    }

    public void OnMapSelected(string mapName, bool isUnlocked)
    {
        selectedLocation = mapName;
        if (isUnlocked)
        {
            if (selectedMapTitleText != null) selectedMapTitleText.text = mapName;
            if (selectedMapBadgeText != null) selectedMapBadgeText.text = "● READY TO PLAY";
            ShowToast($"📍 Selected: {mapName} (4 Lanes Two-Way Traffic)", 2.5f);
        }
        else
        {
            ShowToast($"🔒 {mapName} is Locked! Unlocks in future update. Enjoy Coastal Highway now!", 3.0f);
        }
    }
    #endregion

    public void ShowToast(string message, float duration = 2.5f)
    {
        if (toastText != null) toastText.text = message;
        if (toastCanvasGroup != null)
        {
            toastCanvasGroup.alpha = 1f;
            toastTimer = duration;
        }
        if (toastPanel != null) toastPanel.SetActive(true);
    }

    private Button FindButtonByName(string name)
    {
        var allButtons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (var b in allButtons)
        {
            if (b != null && b.gameObject.name == name) return b;
        }
        return null;
    }

    private void WirePedal(GameObject pedal, System.Action onDown, System.Action onUp)
    {
        if (pedal == null) return;
        var trigger = pedal.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null) trigger = pedal.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        trigger.triggers.Clear();

        var entryDown = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
        entryDown.callback.AddListener((data) => onDown());
        trigger.triggers.Add(entryDown);

        var entryUp = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
        entryUp.callback.AddListener((data) => onUp());
        trigger.triggers.Add(entryUp);
    }

    private void FindReferencesIfNull()
    {
        if (carController == null)
        {
            var car = GameObject.Find("SportsSedan");
            if (car != null)
            {
                carController = car.GetComponent<CarController>();
                carVisuals = car.GetComponent<CarVisuals>();
            }
        }
        if (cameraController == null)
        {
            cameraController = Object.FindFirstObjectByType<CarCameraController>();
        }
        if (overtakeDetector == null)
        {
            overtakeDetector = Object.FindFirstObjectByType<HighwayOvertakeDetector>();
        }
        if (trafficManager == null)
        {
            trafficManager = Object.FindFirstObjectByType<HighwayTrafficManager>();
        }
        if (highwayManager == null)
        {
            highwayManager = Object.FindFirstObjectByType<HighwayManager>();
        }
    }

    void Update()
    {
        HandleKeyboardShortcuts();

        if (IsGameplayRunning)
        {
            UpdateDashboard();
            UpdateOvertakeHUD();
            UpdatePopupAnimation();
            SendInputsToCar();
        }

        UpdateToastAnimation();
    }

    private void UpdateToastAnimation()
    {
        if (toastTimer > 0f)
        {
            toastTimer -= Time.unscaledDeltaTime;
            if (toastTimer < 0.5f && toastCanvasGroup != null)
            {
                toastCanvasGroup.alpha = toastTimer / 0.5f;
            }
            if (toastTimer <= 0f && toastPanel != null)
            {
                toastPanel.SetActive(false);
            }
        }
    }

    private void HandleKeyboardShortcuts()
    {
        bool escPressed = false;
        bool cPressed = false;
        bool hPressed = false;
        bool rPressed = false;

        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame) escPressed = true;
            if (Keyboard.current.cKey.wasPressedThisFrame) cPressed = true;
            if (Keyboard.current.hKey.wasPressedThisFrame) hPressed = true;
            if (Keyboard.current.rKey.wasPressedThisFrame) rPressed = true;
        }
        #endif
        if (!escPressed && !cPressed && !hPressed && !rPressed)
        {
            try { if (Input.GetKeyDown(KeyCode.Escape)) escPressed = true; } catch { }
            try { if (Input.GetKeyDown(KeyCode.C)) cPressed = true; } catch { }
            try { if (Input.GetKeyDown(KeyCode.H)) hPressed = true; } catch { }
            try { if (Input.GetKeyDown(KeyCode.R)) rPressed = true; } catch { }
        }

        if (escPressed && IsGameplayRunning) TogglePause();
        if (cPressed && IsGameplayRunning) OnCamButtonClicked();
        if (hPressed && IsGameplayRunning) OnLightsButtonClicked();
        if (rPressed && IsGameplayRunning) OnRestartClicked();
    }

    private void UpdateDashboard()
    {
        if (carController == null) return;

        int speed = Mathf.RoundToInt(carController.CurrentSpeedKmh);
        if (speedText != null) speedText.text = speed.ToString();
        if (gearText != null) gearText.text = carController.CurrentGearName;
        if (rpmText != null) rpmText.text = $"{Mathf.RoundToInt(carController.CurrentRpm)} RPM";
        if (speedGaugeBar != null) speedGaugeBar.fillAmount = carController.NormalizedSpeed;
    }

    private void UpdateOvertakeHUD()
    {
        if (overtakeDetector == null) return;

        if (overtakeCountText != null) overtakeCountText.text = $"OVERTAKES: {overtakeDetector.TotalOvertakes}";
        if (scoreText != null) scoreText.text = $"SCORE: {overtakeDetector.CurrentScore:N0}";
        if (cashText != null) cashText.text = $"${overtakeDetector.CurrentCash:N0}";

        if (comboText != null)
        {
            if (overtakeDetector.CurrentCombo > 1)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"COMBO x{overtakeDetector.CurrentCombo} ({overtakeDetector.ComboTimer:F1}s)";
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    private void ShowOvertakePopup(string msg, int cash, int combo)
    {
        if (popupText != null)
        {
            popupText.text = msg;
        }
        popupTimer = 1.6f;
        if (popupCanvasGroup != null) popupCanvasGroup.alpha = 1f;
    }

    private void UpdatePopupAnimation()
    {
        if (popupTimer > 0f)
        {
            popupTimer -= Time.deltaTime;
            if (popupTimer < 0.4f && popupCanvasGroup != null)
            {
                popupCanvasGroup.alpha = popupTimer / 0.4f;
            }
        }
        else if (popupCanvasGroup != null && popupCanvasGroup.alpha > 0f)
        {
            popupCanvasGroup.alpha = 0f;
        }
    }

    private void SendInputsToCar()
    {
        if (carController == null) return;
        carController.SetVirtualInputs(virtualMove, virtualSteer, virtualHandbrake);
    }

    #region On-Screen Button Callbacks
    public void OnGasDown() { virtualMove = 1f; }
    public void OnGasUp() { if (virtualMove > 0f) virtualMove = 0f; }

    public void OnBrakeDown() { virtualMove = -1f; }
    public void OnBrakeUp() { if (virtualMove < 0f) virtualMove = 0f; }

    public void OnSteerLeftDown() { virtualSteer = -1f; }
    public void OnSteerLeftUp() { if (virtualSteer < 0f) virtualSteer = 0f; }

    public void OnSteerRightDown() { virtualSteer = 1f; }
    public void OnSteerRightUp() { if (virtualSteer > 0f) virtualSteer = 0f; }

    public void OnDriftDown() { virtualHandbrake = true; }
    public void OnDriftUp() { virtualHandbrake = false; }

    public void OnLightsButtonClicked()
    {
        if (carVisuals != null) carVisuals.ToggleHeadlights();
    }

    public void OnCamButtonClicked()
    {
        if (cameraController != null) cameraController.CycleCameraMode();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (pausePanel != null) pausePanel.SetActive(isPaused);
    }

    public void OnResumeClicked()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void OnExitGameClicked()
    {
        Time.timeScale = 1f;
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    #endregion
}
