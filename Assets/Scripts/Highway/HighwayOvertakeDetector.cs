using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Detects thrilling highway overtakes and high-speed close passes (Near Misses).
/// Awards cash bonuses, maintains combo multipliers, triggers sound effects,
/// and feeds real-time overtake stats to the HUD.
/// </summary>
public class HighwayOvertakeDetector : MonoBehaviour
{
    [Header("Overtake Criteria")]
    public float minPlayerSpeedKmh = 60f;
    public float maxOvertakeLateralDist = 4.2f;
    public float closePassLateralDist = 2.4f;

    [Header("Scoring & Rewards")]
    public int baseOvertakeCash = 100;
    public int closePassCash = 250;
    public int baseOvertakeScore = 150;
    public int closePassScore = 400;
    public float comboDuration = 4.5f;

    [Header("References")]
    public Transform playerCar;
    public CarController carController;
    public HighwayTrafficManager trafficManager;

    // Public Stats for UI
    public int TotalOvertakes { get; private set; } = 0;
    public int TotalClosePasses { get; private set; } = 0;
    public int CurrentCombo { get; private set; } = 0;
    public float ComboTimer { get; private set; } = 0f;
    public int CurrentScore { get; private set; } = 0;
    public int CurrentCash { get; private set; } = 1500;

    // Delegates for UI events
    public System.Action<string, int, int> OnOvertakeEvent; // Message, cashEarned, combo

    public void ResetScore()
    {
        TotalOvertakes = 0;
        TotalClosePasses = 0;
        CurrentCombo = 0;
        ComboTimer = 0f;
        CurrentScore = 0;
        CurrentCash = 1500;
        lastVehicleZDiff.Clear();
    }

    // Tracking state for each traffic vehicle: was it behind or ahead?
    private Dictionary<int, float> lastVehicleZDiff = new Dictionary<int, float>();
    private float highSpeedBonusTimer = 0f;

    void Start()
    {
        FindReferencesIfNull();
    }

    private void FindReferencesIfNull()
    {
        if (playerCar == null)
        {
            var p = GameObject.Find("SportsSedan");
            if (p != null)
            {
                playerCar = p.transform;
                carController = p.GetComponent<CarController>();
            }
        }
        if (trafficManager == null)
        {
            trafficManager = Object.FindFirstObjectByType<HighwayTrafficManager>();
        }
    }

    void Update()
    {
        if (trafficManager != null && !trafficManager.isTrafficActive) return;
        if (playerCar == null || trafficManager == null)
        {
            FindReferencesIfNull();
            return;
        }

        UpdateComboTimer();
        UpdateHighSpeedBonus();
        CheckAllTrafficOvertakes();
    }

    private void UpdateComboTimer()
    {
        if (CurrentCombo > 0)
        {
            ComboTimer -= Time.deltaTime;
            if (ComboTimer <= 0f)
            {
                CurrentCombo = 0;
                ComboTimer = 0f;
            }
        }
    }

    private void UpdateHighSpeedBonus()
    {
        if (carController == null) return;
        float speed = carController.CurrentSpeedKmh;

        // Above 110 km/h: award high-speed cash bonus every second
        if (speed > 110f)
        {
            highSpeedBonusTimer += Time.deltaTime;
            if (highSpeedBonusTimer >= 1.0f)
            {
                highSpeedBonusTimer = 0f;
                int bonus = (int)((speed - 100f) * 0.5f);
                CurrentCash += bonus;
                CurrentScore += bonus * 2;
            }
        }
        else
        {
            highSpeedBonusTimer = 0f;
        }
    }

    private void CheckAllTrafficOvertakes()
    {
        float playerZ = playerCar.position.z;
        float playerX = playerCar.position.x;
        float playerSpeed = (carController != null) ? carController.CurrentSpeedKmh : 0f;

        var allCars = trafficManager.GetAllActiveCars();
        if (allCars == null) return;

        foreach (var car in allCars)
        {
            if (car == null) continue;

            int carId = car.GetInstanceID();
            float trafficZ = car.transform.position.z;
            float trafficX = car.transform.position.x;
            float zDiff = playerZ - trafficZ; // Negative = player is behind; Positive = player is ahead

            if (lastVehicleZDiff.TryGetValue(carId, out float prevZDiff))
            {
                // Did player just cross the car from behind to front?
                if (prevZDiff <= 0f && zDiff > 0f)
                {
                    // Check if player speed is high enough and lateral distance is on-road
                    float lateralDist = Mathf.Abs(playerX - trafficX);

                    if (playerSpeed >= minPlayerSpeedKmh && lateralDist <= maxOvertakeLateralDist)
                    {
                        RegisterOvertake(car, lateralDist, playerSpeed);
                    }
                }
            }

            lastVehicleZDiff[carId] = zDiff;
        }
    }

    private void RegisterOvertake(HighwayTrafficCar car, float lateralDist, float playerSpeed)
    {
        bool isOncoming = (car != null && car.isOncoming);
        bool isClosePass = (lateralDist <= closePassLateralDist);

        // Increase combo
        CurrentCombo++;
        ComboTimer = comboDuration;

        TotalOvertakes++;
        if (isClosePass) TotalClosePasses++;

        // Calculate rewards: Oncoming passes give extra thrill bonuses!
        int cashEarned;
        int scoreEarned;

        if (isOncoming)
        {
            cashEarned = isClosePass ? (closePassCash + 150) : (baseOvertakeCash + 50);
            scoreEarned = isClosePass ? (closePassScore + 250) : (baseOvertakeScore + 100);
        }
        else
        {
            cashEarned = isClosePass ? closePassCash : baseOvertakeCash;
            scoreEarned = isClosePass ? closePassScore : baseOvertakeScore;
        }

        // Apply combo multiplier
        float multiplier = 1.0f + (CurrentCombo - 1) * 0.25f;
        cashEarned = Mathf.RoundToInt(cashEarned * multiplier);
        scoreEarned = Mathf.RoundToInt(scoreEarned * multiplier);

        CurrentCash += cashEarned;
        CurrentScore += scoreEarned;

        // Sound effect
        var audioMgr = Object.FindFirstObjectByType<CarAudioManager>();
        if (audioMgr != null)
        {
            audioMgr.PlayCheckpointChime();
        }

        // Notify UI
        string eventMsg;
        if (isOncoming)
        {
            eventMsg = isClosePass ? $"⚡ ONCOMING CLOSE PASS! +${cashEarned} ⚡" : $"ONCOMING PASS! +${cashEarned}";
        }
        else
        {
            eventMsg = isClosePass ? $"⚡ CLOSE OVERTAKE! +${cashEarned} ⚡" : $"OVERTAKE! +${cashEarned}";
        }

        if (CurrentCombo > 1)
        {
            eventMsg += $" (COMBO x{CurrentCombo})";
        }

        OnOvertakeEvent?.Invoke(eventMsg, cashEarned, CurrentCombo);
    }
}
