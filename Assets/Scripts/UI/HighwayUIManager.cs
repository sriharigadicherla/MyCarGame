using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Modern Highway Driving & Overtake HUD.
/// Coordinates the Speedometer, Gear Display, RPM, Overtake Counter,
/// Combo Multiplier, Floating Reward Pop-up, On-screen Pedals, and Pause Menu.
/// </summary>
public class HighwayUIManager : MonoBehaviour
{
    [Header("References")]
    public CarController carController;
    public CarVisuals carVisuals;
    public CarCameraController cameraController;
    public HighwayOvertakeDetector overtakeDetector;

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

    [Header("Pause Menu")]
    public GameObject pausePanel;

    private float popupTimer = 0f;
    private bool isPaused = false;

    // Virtual Inputs
    private float virtualMove = 0f;
    private float virtualSteer = 0f;
    private bool virtualHandbrake = false;

    void Start()
    {
        FindReferencesIfNull();
        if (pausePanel != null) pausePanel.SetActive(false);
        if (popupCanvasGroup != null) popupCanvasGroup.alpha = 0f;

        if (overtakeDetector != null)
        {
            overtakeDetector.OnOvertakeEvent += ShowOvertakePopup;
        }
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
    }

    void Update()
    {
        HandleKeyboardShortcuts();
        UpdateDashboard();
        UpdateOvertakeHUD();
        UpdatePopupAnimation();
        SendInputsToCar();
    }

    private void HandleKeyboardShortcuts()
    {
        bool escPressed = false;
        bool cPressed = false;
        bool hPressed = false;

        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame) escPressed = true;
            if (Keyboard.current.cKey.wasPressedThisFrame) cPressed = true;
            if (Keyboard.current.hKey.wasPressedThisFrame) hPressed = true;
        }
        #endif
        if (!escPressed)
        {
            try { if (Input.GetKeyDown(KeyCode.Escape)) escPressed = true; } catch { }
            try { if (Input.GetKeyDown(KeyCode.C)) cPressed = true; } catch { }
            try { if (Input.GetKeyDown(KeyCode.H)) hPressed = true; } catch { }
        }

        if (escPressed) TogglePause();
        if (cPressed) OnCamButtonClicked();
        if (hPressed) OnLightsButtonClicked();
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

        if (overtakeCountText != null)
        {
            overtakeCountText.text = $"OVERTAKES: {overtakeDetector.TotalOvertakes}";
        }
        if (scoreText != null)
        {
            scoreText.text = $"SCORE: {overtakeDetector.CurrentScore:N0}";
        }
        if (cashText != null)
        {
            cashText.text = $"${overtakeDetector.CurrentCash:N0}";
        }
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
        popupTimer = 1.8f;
        if (popupCanvasGroup != null) popupCanvasGroup.alpha = 1f;
    }

    private void UpdatePopupAnimation()
    {
        if (popupTimer > 0f)
        {
            popupTimer -= Time.deltaTime;
            if (popupCanvasGroup != null)
            {
                popupCanvasGroup.alpha = Mathf.Clamp01(popupTimer / 0.4f);
            }
        }
        else
        {
            if (popupCanvasGroup != null) popupCanvasGroup.alpha = 0f;
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

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    #endregion
}
