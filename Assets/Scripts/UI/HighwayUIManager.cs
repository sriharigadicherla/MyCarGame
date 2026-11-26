using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Modern Highway Driving & Overtake HUD + AAA Main Menu System.
/// Coordinates Main Menu Lobby (Profile, Map Selector, Play Button),
/// Speedometer, Gear Display, RPM, Overtake Counter, Combo Multiplier,
/// Floating Reward Pop-up, On-screen Pedals, Pause Menu, and Interactive Modals.
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

    [Header("Interactive Modal System")]
    public GameObject menuModalPanel;
    public Text modalTitleText;
    public Image modalIconImage;
    public Text modalBodyText;
    public GameObject modalCustomContainer;
    public Button modalCloseBtn;
    public Button modalActionBtn;
    public Text modalActionBtnText;
    public CanvasGroup modalCanvasGroup;

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
    private bool isModalOpen = false;

    // Virtual Inputs
    private float virtualMove = 0f;
    private float virtualSteer = 0f;
    private bool virtualHandbrake = false;

    // Player Stats
    public int playerCoins = 15000;
    public int playerGems = 320;
    public int playerLevel = 28;

    void Start()
    {
        FindReferencesIfNull();
        WireButtonEvents();

        // Initialize UI panels: Show Main Menu first!
        ShowMainMenu();

        if (popupCanvasGroup != null) popupCanvasGroup.alpha = 0f;
        if (toastCanvasGroup != null) toastCanvasGroup.alpha = 0f;
        if (menuModalPanel != null) menuModalPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        IsGameplayRunning = false;
        Time.timeScale = 1f;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (gameplayHUDPanel != null) gameplayHUDPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (menuModalPanel != null) menuModalPanel.SetActive(false);

        var legacyCanvas = GameObject.Find("UI_Canvas");
        if (legacyCanvas != null) legacyCanvas.SetActive(false);

        // Immobilize car and disable driving controls while in Main Menu!
        if (carController != null)
        {
            carController.isGameplayActive = false;
            carController.enabled = false;
            carController.transform.position = new Vector3(-1.9f, 0.15f, 0f);
            carController.transform.rotation = Quaternion.identity;
            var rb = carController.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

        if (trafficManager != null)
        {
            trafficManager.isTrafficActive = false;
            trafficManager.ResetTrafficStream();
        }
        if (overtakeDetector != null)
        {
            overtakeDetector.enabled = false;
        }

        // Ensure default location selection
        UpdateMapSelectionVisuals("Coastal Highway");
    }

    public void StartHighwayGame()
    {
        IsGameplayRunning = true;
        Time.timeScale = 1f;

        ProceduralUISound.Instance.PlayStartRace();

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameplayHUDPanel != null) gameplayHUDPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (menuModalPanel != null) menuModalPanel.SetActive(false);

        var legacyCanvas = GameObject.Find("UI_Canvas");
        if (legacyCanvas != null) legacyCanvas.SetActive(false);

        // Reposition Player in Lane 1 facing forward and enable driving!
        if (carController != null)
        {
            var rb = carController.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.forward * 12f; // Initial rolling speed
                rb.angularVelocity = Vector3.zero;
            }
            carController.transform.position = new Vector3(-1.9f, 0.15f, 0f);
            carController.transform.rotation = Quaternion.identity;
            carController.isGameplayActive = true;
            carController.enabled = true;
        }

        // Activate Traffic & Overtake scoring
        if (trafficManager != null)
        {
            trafficManager.isTrafficActive = true;
            trafficManager.enabled = true;
        }

        if (overtakeDetector != null)
        {
            overtakeDetector.enabled = true;
            overtakeDetector.ResetScore();
            overtakeDetector.OnOvertakeEvent -= HandleOvertake;
            overtakeDetector.OnOvertakeEvent += HandleOvertake;
        }

        ShowToast($"🏁 RACE STARTED on {selectedLocation}! Overtake traffic for bonus cash!", 3.0f);
    }

    private void HandleOvertake(string msg, int cash, int combo)
    {
        ShowOvertakePopup(msg, cash, combo);
        ProceduralUISound.Instance.PlayClick();
    }

    private void WireButtonEvents()
    {
        // 1. Play Button
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(StartHighwayGame);
        }

        // 2. Top Bar
        if (profileBtn != null) { profileBtn.onClick.RemoveAllListeners(); profileBtn.onClick.AddListener(OnProfileClicked); }
        if (coinsBtn != null) { coinsBtn.onClick.RemoveAllListeners(); coinsBtn.onClick.AddListener(OnCoinsClicked); }
        if (gemsBtn != null) { gemsBtn.onClick.RemoveAllListeners(); gemsBtn.onClick.AddListener(OnGemsClicked); }
        if (mailBtn != null) { mailBtn.onClick.RemoveAllListeners(); mailBtn.onClick.AddListener(OnMailClicked); }
        if (settingsBtn != null) { settingsBtn.onClick.RemoveAllListeners(); settingsBtn.onClick.AddListener(OnSettingsClicked); }

        // 3. Right Menu
        if (eventsBtn != null) { eventsBtn.onClick.RemoveAllListeners(); eventsBtn.onClick.AddListener(OnEventsClicked); }
        if (garageBtn != null) { garageBtn.onClick.RemoveAllListeners(); garageBtn.onClick.AddListener(OnGarageClicked); }
        if (customizationBtn != null) { customizationBtn.onClick.RemoveAllListeners(); customizationBtn.onClick.AddListener(OnCustomizationClicked); }
        if (storeBtn != null) { storeBtn.onClick.RemoveAllListeners(); storeBtn.onClick.AddListener(OnStoreClicked); }

        // 4. Bottom Menu
        if (carsBtn != null) { carsBtn.onClick.RemoveAllListeners(); carsBtn.onClick.AddListener(OnCarsClicked); }
        if (upgradesBtn != null) { upgradesBtn.onClick.RemoveAllListeners(); upgradesBtn.onClick.AddListener(OnUpgradesClicked); }
        if (missionsBtn != null) { missionsBtn.onClick.RemoveAllListeners(); missionsBtn.onClick.AddListener(OnMissionsClicked); }
        if (leaderboardBtn != null) { leaderboardBtn.onClick.RemoveAllListeners(); leaderboardBtn.onClick.AddListener(OnLeaderboardClicked); }

        // 5. Locations
        WireMapButtons();

        // 6. HUD Controls
        if (topExitButton != null) { topExitButton.onClick.RemoveAllListeners(); topExitButton.onClick.AddListener(OnExitGameClicked); }
        if (lightsButton != null) { lightsButton.onClick.RemoveAllListeners(); lightsButton.onClick.AddListener(OnLightsButtonClicked); }
        if (camButton != null) { camButton.onClick.RemoveAllListeners(); camButton.onClick.AddListener(OnCamButtonClicked); }
        if (pauseButton != null) { pauseButton.onClick.RemoveAllListeners(); pauseButton.onClick.AddListener(TogglePause); }
        if (restartButton != null) { restartButton.onClick.RemoveAllListeners(); restartButton.onClick.AddListener(OnRestartClicked); }

        // 7. Pause Panel
        if (resumeButton != null) { resumeButton.onClick.RemoveAllListeners(); resumeButton.onClick.AddListener(OnResumeClicked); }
        if (pauseRestartButton != null) { pauseRestartButton.onClick.RemoveAllListeners(); pauseRestartButton.onClick.AddListener(OnRestartClicked); }
        if (pauseMainMenuButton != null) { pauseMainMenuButton.onClick.RemoveAllListeners(); pauseMainMenuButton.onClick.AddListener(ShowMainMenu); }
        if (pauseExitButton != null) { pauseExitButton.onClick.RemoveAllListeners(); pauseExitButton.onClick.AddListener(OnExitGameClicked); }

        // 8. Modal Panel Close
        if (modalCloseBtn != null) { modalCloseBtn.onClick.RemoveAllListeners(); modalCloseBtn.onClick.AddListener(CloseModal); }

        // 9. Touch Pedals
        WirePedal(steerLeftPedal, OnSteerLeftDown, OnSteerLeftUp);
        WirePedal(steerRightPedal, OnSteerRightDown, OnSteerRightUp);
        WirePedal(gasPedal, OnGasDown, OnGasUp);
        WirePedal(brakePedal, OnBrakeDown, OnBrakeUp);
        WirePedal(driftPedal, OnDriftDown, OnDriftUp);
    }

    private void WireMapButtons()
    {
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

    #region Interactive Modal System (Real Dialogs on Button Click!)
    public void OpenModal(string title, Sprite icon, string bodyText, string actionBtnText = "OK", System.Action onAction = null)
    {
        if (menuModalPanel == null) return;

        ProceduralUISound.Instance.PlayModalOpen();
        isModalOpen = true;
        menuModalPanel.SetActive(true);

        if (modalTitleText != null) modalTitleText.text = title;
        if (modalIconImage != null)
        {
            modalIconImage.gameObject.SetActive(icon != null);
            modalIconImage.sprite = icon;
        }
        if (modalBodyText != null) modalBodyText.text = bodyText;

        // Clear custom container
        if (modalCustomContainer != null)
        {
            for (int i = modalCustomContainer.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(modalCustomContainer.transform.GetChild(i).gameObject);
            }
        }

        if (modalActionBtn != null)
        {
            modalActionBtn.onClick.RemoveAllListeners();
            if (onAction != null)
            {
                modalActionBtn.onClick.AddListener(() => { onAction(); CloseModal(); });
            }
            else
            {
                modalActionBtn.onClick.AddListener(CloseModal);
            }
        }

        if (modalActionBtnText != null) modalActionBtnText.text = actionBtnText;

        // Animate modal pop
        menuModalPanel.transform.localScale = Vector3.one * 0.85f;
    }

    public void CloseModal()
    {
        ProceduralUISound.Instance.PlayClick();
        isModalOpen = false;
        if (menuModalPanel != null) menuModalPanel.SetActive(false);
    }
    #endregion

    #region Custom Main Menu Hooks (Rich Click Feedback & Real Functionality!)
    public void OnProfileClicked()
    {
        string info = $"RACER: RacerSrihari\n" +
                      $"RANK: Level {playerLevel} • Pro Driver\n" +
                      $"TOTAL RACES WON: 142\n" +
                      $"CAREER DISTANCE: 840 km\n" +
                      $"TOP RECORDED SPEED: 312 km/h\n" +
                      $"CURRENT RIDE: Mercedes-Benz CLS 53 AMG";
        OpenModal("DRIVER PROFILE", null, info, "CLAIM LEVEL REWARD", () => {
            playerCoins += 1000;
            ShowToast("🎁 +$1,000 Level Bonus Claimed!");
        });
    }

    public void OnCoinsClicked()
    {
        string info = $"CURRENT BALANCE: ${playerCoins:N0} CASH\n\n" +
                      $"[PACK 1] $10,000 Cash - $0.99\n" +
                      $"[PACK 2] $50,000 Cash - $4.99\n" +
                      $"[PACK 3] $200,000 Cash - $14.99\n\n" +
                      $"Earn more by dodging traffic and chaining close passes!";
        OpenModal("CASH VAULT", null, info, "FREE DAILY +$2,500", () => {
            playerCoins += 2500;
            ShowToast("💰 Added $2,500 Cash to your wallet!");
        });
    }

    public void OnGemsClicked()
    {
        string info = $"CURRENT BALANCE: 💎 {playerGems} DIAMONDS\n\n" +
                      $"[PACK 1] 100 Diamonds - $1.99\n" +
                      $"[PACK 2] 500 Diamonds - $7.99\n" +
                      $"[PACK 3] 1,500 Diamonds - $19.99\n\n" +
                      $"Use diamonds to unlock hypercars and special bodykits!";
        OpenModal("DIAMOND STORE", null, info, "FREE +50 GEMS", () => {
            playerGems += 50;
            ShowToast("💎 Added 50 Diamonds to your account!");
        });
    }

    public void OnMailClicked()
    {
        string info = $"INBOX: 2 Messages\n\n" +
                      $"1. [SYSTEM] Welcome to Highway Overtake v2.0! Enjoy 4-lane two-way expressway cruising.\n\n" +
                      $"2. [EVENT] Daily Highway Rush is live! Near-misses give x2 combo multiplier today!";
        OpenModal("MAILBOX", null, info, "CLAIM ALL REWARDS", () => {
            playerCoins += 1500;
            ShowToast("✉️ Claimed $1,500 Mailbox Rewards!");
        });
    }

    public void OnSettingsClicked()
    {
        string info = $"AUDIO VOLUME: 100%\n" +
                      $"GRAPHICS QUALITY: HIGH (60 FPS)\n" +
                      $"CAMERA MODE: DYNAMIC CHASE (Press C to switch)\n" +
                      $"HEADLIGHTS: AUTO LED (Press H to toggle)\n" +
                      $"CONTROLS: WASD / ARROW KEYS / ON-SCREEN TOUCH PEDALS";
        OpenModal("SETTINGS", null, info, "RESET HIGH SCORES", () => {
            ShowToast("⚙️ Settings Saved!");
        });
    }

    public void OnEventsClicked()
    {
        string info = $"🏆 ACTIVE EVENTS:\n\n" +
                      $"1. [HIGHWAY DRIFT] Midnight Sprint on Coastal Highway - DOUBLE CASH!\n\n" +
                      $"2. [SPEED RUN] Reach 240 KM/H within 60 seconds (Prize: 80 Gems)\n\n" +
                      $"3. [TRAFFIC ESCAPE] Overtake 25 vehicles without colliding (Prize: $10,000)";
        OpenModal("SPECIAL EVENTS", null, info, "JOIN HIGHWAY DRIFT", () => {
            StartHighwayGame();
        });
    }

    public void OnGarageClicked()
    {
        string info = $"GARAGE FLEET (1/3 Cars Unlocked):\n\n" +
                      $"1. Mercedes-Benz CLS 53 AMG [EQUIPPED]\n" +
                      $"   Top Speed: 285 km/h | Accel: 3.4s | Handling: 88%\n\n" +
                      $"2. Lamborghini Huracan EVO [LOCKED - Level 30]\n" +
                      $"   Top Speed: 325 km/h | Accel: 2.9s | Handling: 95%\n\n" +
                      $"3. Porsche 911 GT3 RS [LOCKED - Level 35]\n" +
                      $"   Top Speed: 318 km/h | Accel: 3.0s | Handling: 98%";
        OpenModal("SUPERCAR GARAGE", null, info, "TUNE VEHICLE", OnUpgradesClicked);
    }

    public void OnCustomizationClicked()
    {
        string info = $"PAINT SHOP - Choose a custom color for your Mercedes sports sedan:\n\n" +
                      $"• GLOSSY JET BLACK (Default)\n" +
                      $"• CRIMSON RACING RED\n" +
                      $"• ELECTRIC NEON BLUE\n" +
                      $"• SUNBURST GOLD\n" +
                      $"• EMERALD RACING GREEN\n\n" +
                      $"Click the action button below to apply a fresh Crimson Red racing finish!";
        OpenModal("PAINT CUSTOMIZATION", null, info, "PAINT: CRIMSON RED", () => {
            ApplyCarColor(new Color(0.85f, 0.08f, 0.12f, 1f));
            ShowToast("🎨 Applied Crimson Red Metallic Paint to your car!");
        });
    }

    public void OnStoreClicked()
    {
        string info = $"PRO SHOP & CRATES:\n\n" +
                      $"• STARTER RACER PACK: $25,000 + 100 Gems ($1.99)\n" +
                      $"• TURBO ENHANCEMENT PACK: Engine Stage 4 + Nitro ($4.99)\n" +
                      $"• VIP SEASON PASS: Instant access to all future highway maps!";
        OpenModal("GAME STORE", null, info, "BUY STARTER PACK", () => {
            playerCoins += 25000;
            playerGems += 100;
            ShowToast("🛒 Purchased Starter Pack! +$25,000 Cash & +100 Gems!");
        });
    }

    public void OnCarsClicked()
    {
        OnGarageClicked();
    }

    public void OnUpgradesClicked()
    {
        string info = $"PERFORMANCE TUNING (Mercedes CLS 53 AMG):\n\n" +
                      $"[STAGE 3] 3.0L Inline-6 Turbo Engine (+35 HP)\n" +
                      $"[STAGE 2] Twin-Scroll Turbocharger (+0.3 Bar Boost)\n" +
                      $"[STAGE 4] Michelin Pilot Sport Cup 2 Tires (Maximum Grip)\n" +
                      $"[STAGE 3] Dual Carbon Exhaust & Drift Handbrake";
        OpenModal("PERFORMANCE UPGRADES", null, info, "UPGRADE TURBO ($5,000)", () => {
            if (playerCoins >= 5000)
            {
                playerCoins -= 5000;
                ShowToast("🛠️ Turbo upgraded to Stage 3! Top Speed Increased!");
            }
            else
            {
                ShowToast("❌ Not enough coins! Need $5,000.");
            }
        });
    }

    public void OnMissionsClicked()
    {
        string info = $"DAILY OBJECTIVES:\n\n" +
                      $"1. [IN PROGRESS] Overtake 15 vehicles on Highway [13/15] ($2,000)\n\n" +
                      $"2. [IN PROGRESS] Drive 3,000 meters without crashing [1,850/3,000] (20 Gems)\n\n" +
                      $"3. [COMPLETED] Reach 200 KM/H speed [DONE] (Claimed $1,500)";
        OpenModal("DAILY MISSIONS", null, info, "CLAIM REWARDS", () => {
            playerCoins += 2000;
            ShowToast("🎯 Claimed $2,000 Daily Mission Bonus!");
        });
    }

    public void OnLeaderboardClicked()
    {
        string info = $"GLOBAL SPEED RANKINGS (Coastal Highway):\n\n" +
                      $"🥇 1. ApexPredator99 - Score: 142,500 (Overtakes: 85)\n" +
                      $"🥈 2. RacerSrihari (YOU) - Score: 98,400 (Overtakes: 58)\n" +
                      $"🥉 3. MidnightDrifter - Score: 87,200 (Overtakes: 51)\n" +
                      $"   4. TurboGhost - Score: 74,100 (Overtakes: 44)\n" +
                      $"   5. SpeedQueen - Score: 68,900 (Overtakes: 39)";
        OpenModal("GLOBAL LEADERBOARD", null, info, "CLOSE", null);
    }

    public void OnMapSelected(string mapName, bool isUnlocked)
    {
        ProceduralUISound.Instance.PlayClick();
        selectedLocation = mapName;
        UpdateMapSelectionVisuals(mapName);

        if (isUnlocked)
        {
            ShowToast($"📍 Selected: {mapName} (4 Lanes Two-Way Traffic • Ready to Play!)", 2.5f);
        }
        else
        {
            ShowToast($"🔒 {mapName} is Locked! Unlocks in future update. Coastal Highway is currently ready to play!", 3.0f);
        }
    }

    private void UpdateMapSelectionVisuals(string mapName)
    {
        if (selectedMapTitleText != null) selectedMapTitleText.text = mapName;
        if (selectedMapBadgeText != null) selectedMapBadgeText.text = (mapName == "Coastal Highway") ? "● READY TO PLAY" : "🔒 LOCKED";

        // Toggle selected outlines on map button feedbacks
        SetMapButtonFeedbackState(mapHighwayBtn, mapName == "Coastal Highway");
        SetMapButtonFeedbackState(mapAlpineBtn, mapName == "Alpine Circuit");
        SetMapButtonFeedbackState(mapDesertBtn, mapName == "Desert Rush");
        SetMapButtonFeedbackState(mapCityBtn, mapName == "City Night");
    }

    private void SetMapButtonFeedbackState(Button btn, bool isSelected)
    {
        if (btn == null) return;
        var feedback = btn.GetComponent<UIButtonFeedback>();
        if (feedback != null) feedback.SetSelected(isSelected);
    }

    private void ApplyCarColor(Color color)
    {
        var car = GameObject.Find("SportsSedan");
        if (car != null)
        {
            var renderers = car.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in renderers)
            {
                if (r.sharedMaterial != null && r.sharedMaterial.name.Contains("Body"))
                {
                    r.material.color = color;
                }
            }
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
        UpdateModalAnimation();
    }

    private void UpdateModalAnimation()
    {
        if (isModalOpen && menuModalPanel != null)
        {
            menuModalPanel.transform.localScale = Vector3.Lerp(menuModalPanel.transform.localScale, Vector3.one, Time.unscaledDeltaTime * 14f);
        }
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

        if (escPressed)
        {
            if (isModalOpen) CloseModal();
            else if (IsGameplayRunning) TogglePause();
        }
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
        if (popupText != null) popupText.text = msg;
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
