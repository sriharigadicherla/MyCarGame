using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Master Driving Game UI Controller.
/// Coordinates the Speedometer, Gear Display, RPM Gauge, Mission Status,
/// On-screen Pedals/Controls, Pause Menu, and Mission Result Panels.
/// </summary>
public class GameUIManager : MonoBehaviour
{
    [Header("Car Reference")]
    public CarController carController;
    public CarVisuals carVisuals;
    public CarCameraController cameraController;

    [Header("Speedometer & Dash")]
    public Text speedText;
    public Text gearText;
    public Text rpmText;
    public Image speedGaugeBar;
    public Image rpmGaugeBar;

    [Header("Mission Status")]
    public Text missionTitleText;
    public Text missionObjectiveText;
    public Text timerText;
    public Text scoreCashText;

    [Header("Menus")]
    public GameObject pauseMenuPanel;
    public GameObject missionCompletePanel;
    public GameObject missionFailedPanel;
    public Text completeStatsText;
    public Text failedStatsText;

    [Header("Audio & Settings")]
    public Text audioButtonText;

    // Virtual Pedal States for Mobile / Touch / Mouse
    private float virtualMove = 0f;
    private float virtualSteer = 0f;
    private bool virtualHandbrake = false;
    private bool isPaused = false;
    private bool isAudioMuted = false;

    void Start()
    {
        FindCarReferencesIfNull();
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (missionCompletePanel != null) missionCompletePanel.SetActive(false);
        if (missionFailedPanel != null) missionFailedPanel.SetActive(false);
    }

    private void FindCarReferencesIfNull()
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
    }

    void Update()
    {
        HandlePauseInput();
        UpdateDashboard();
        SendVirtualInputsToCar();
    }

    private void HandlePauseInput()
    {
        bool escapePressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) escapePressed = true;
#endif
        if (!escapePressed)
        {
            try { if (Input.GetKeyDown(KeyCode.Escape)) escapePressed = true; } catch { }
        }

        if (escapePressed)
        {
            TogglePause();
        }
    }

    private void UpdateDashboard()
    {
        if (carController == null)
        {
            FindCarReferencesIfNull();
            if (carController == null) return;
        }

        // 1. Speedometer
        int speed = Mathf.RoundToInt(carController.CurrentSpeedKmh);
        if (speedText != null)
        {
            speedText.text = speed.ToString();
        }

        if (speedGaugeBar != null)
        {
            speedGaugeBar.fillAmount = carController.NormalizedSpeed;
        }

        // 2. Gear Indicator
        if (gearText != null)
        {
            gearText.text = carController.CurrentGearName;
            // Color feedback: Red for reverse, Cyan for drive, Yellow for neutral
            if (carController.CurrentGearName == "R") gearText.color = new Color(1f, 0.2f, 0.2f);
            else if (carController.CurrentGearName == "N") gearText.color = new Color(1f, 0.85f, 0.2f);
            else gearText.color = new Color(0.2f, 0.9f, 1f);
        }

        // 3. RPM
        if (rpmText != null)
        {
            rpmText.text = $"{Mathf.RoundToInt(carController.CurrentRpm):N0} RPM";
        }
        if (rpmGaugeBar != null)
        {
            rpmGaugeBar.fillAmount = carController.NormalizedRpm;
        }
    }

    #region Virtual Pedals & UI Inputs
    public void OnGasDown() { virtualMove = 1.0f; }
    public void OnGasUp() { virtualMove = 0f; }

    public void OnBrakeDown() { virtualMove = -1.0f; }
    public void OnBrakeUp() { virtualMove = 0f; }

    public void OnSteerLeftDown() { virtualSteer = -1.0f; }
    public void OnSteerLeftUp() { virtualSteer = 0f; }

    public void OnSteerRightDown() { virtualSteer = 1.0f; }
    public void OnSteerRightUp() { virtualSteer = 0f; }

    public void OnHandbrakeDown() { virtualHandbrake = true; }
    public void OnHandbrakeUp() { virtualHandbrake = false; }

    public void OnCameraSwitchClick()
    {
        if (cameraController != null) cameraController.CycleCameraMode();
    }

    public void OnHeadlightsToggleClick()
    {
        if (carVisuals != null) carVisuals.ToggleHeadlights();
    }

    public void OnResetCarClick()
    {
        if (carController != null) carController.ResetOrientation();
    }

    private void SendVirtualInputsToCar()
    {
        if (carController != null)
        {
            carController.SetVirtualInputs(virtualMove, virtualSteer, virtualHandbrake);
        }
    }
    #endregion

    #region Mission HUD & Panels
    public void UpdateMissionHUD(string title, string objective, float remainingTime, int score)
    {
        if (missionTitleText != null) missionTitleText.text = title;
        if (missionObjectiveText != null) missionObjectiveText.text = objective;

        if (timerText != null)
        {
            if (remainingTime > 0f)
            {
                int mins = Mathf.FloorToInt(remainingTime / 60f);
                int secs = Mathf.FloorToInt(remainingTime % 60f);
                int millis = Mathf.FloorToInt((remainingTime * 100f) % 100f);
                timerText.text = $"{mins:00}:{secs:00}.{millis:00}";
            }
            else
            {
                timerText.text = "--:--";
            }
        }

        if (scoreCashText != null)
        {
            scoreCashText.text = $"${score:N0}";
        }
    }

    public void ShowMissionComplete(string stats)
    {
        if (missionCompletePanel != null)
        {
            missionCompletePanel.SetActive(true);
            if (completeStatsText != null) completeStatsText.text = stats;
        }
    }

    public void ShowMissionFailed(string reason)
    {
        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(true);
            if (failedStatsText != null) failedStatsText.text = reason;
        }
    }
    #endregion

    #region Pause & Settings
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(isPaused);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    public void RestartMission()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (missionCompletePanel != null) missionCompletePanel.SetActive(false);
        if (missionFailedPanel != null) missionFailedPanel.SetActive(false);

        var gm = Object.FindFirstObjectByType<GameplayManager>();
        if (gm != null) gm.RestartCurrentMode();
    }

    public void SetFreeRoamMode()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (missionCompletePanel != null) missionCompletePanel.SetActive(false);
        if (missionFailedPanel != null) missionFailedPanel.SetActive(false);

        var gm = Object.FindFirstObjectByType<GameplayManager>();
        if (gm != null) gm.StartFreeRoam();
    }

    public void SetNextMission()
    {
        Time.timeScale = 1f;
        if (missionCompletePanel != null) missionCompletePanel.SetActive(false);

        var gm = Object.FindFirstObjectByType<GameplayManager>();
        if (gm != null) gm.StartNextMission();
    }

    public void ToggleAudio()
    {
        isAudioMuted = !isAudioMuted;
        AudioListener.volume = isAudioMuted ? 0f : 1f;
        if (audioButtonText != null)
        {
            audioButtonText.text = isAudioMuted ? "AUDIO: OFF" : "AUDIO: ON";
        }
    }
    #endregion
}
