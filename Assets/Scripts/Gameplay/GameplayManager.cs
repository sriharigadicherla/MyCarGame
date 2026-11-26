using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Master Gameplay and Mission Controller.
/// Manages game modes (Free Roam, Checkpoint Sprint, Downtown Delivery),
/// checkpoint progression, countdown timers, score rewards, and victory/defeat conditions.
/// </summary>
public class GameplayManager : MonoBehaviour
{
    public enum GameMode
    {
        FreeRoam,
        CheckpointSprint,
        DowntownDelivery
    }

    [Header("Current Mode")]
    public GameMode currentMode = GameMode.CheckpointSprint;

    [Header("Mission Config")]
    public float sprintInitialTime = 75f;
    public float deliveryInitialTime = 90f;

    [Header("Checkpoints")]
    public List<Checkpoint> sprintCheckpoints = new List<Checkpoint>();
    public List<Checkpoint> deliveryCheckpoints = new List<Checkpoint>();

    [Header("Player & UI")]
    public Transform playerCar;
    public GameUIManager uiManager;

    // Runtime state
    public int CurrentCheckpointIndex { get; private set; } = 0;
    public float MissionTimer { get; private set; } = 0f;
    public int PlayerCash { get; private set; } = 1500;
    public bool IsMissionRunning { get; private set; } = false;

    private List<Checkpoint> activeCheckpointList = new List<Checkpoint>();
    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;

    void Start()
    {
        FindReferencesIfNull();
        if (playerCar != null)
        {
            playerStartPosition = playerCar.position;
            playerStartRotation = playerCar.rotation;
        }

        StartMission(currentMode);
    }

    private void FindReferencesIfNull()
    {
        if (playerCar == null)
        {
            var p = GameObject.Find("SportsSedan");
            if (p != null) playerCar = p.transform;
        }
        if (uiManager == null)
        {
            uiManager = Object.FindFirstObjectByType<GameUIManager>();
        }
    }

    public void StartFreeRoam()
    {
        currentMode = GameMode.FreeRoam;
        IsMissionRunning = false;
        DeactivateAllCheckpoints();

        if (uiManager != null)
        {
            uiManager.UpdateMissionHUD("FREE ROAM", "Explore the city & test vehicle handling", 0f, PlayerCash);
        }
    }

    public void StartMission(GameMode mode)
    {
        currentMode = mode;
        CurrentCheckpointIndex = 0;
        DeactivateAllCheckpoints();

        if (mode == GameMode.FreeRoam)
        {
            StartFreeRoam();
            return;
        }

        activeCheckpointList = (mode == GameMode.CheckpointSprint) ? sprintCheckpoints : deliveryCheckpoints;

        if (activeCheckpointList == null || activeCheckpointList.Count == 0)
        {
            // Auto-gather checkpoints if not pre-assigned in inspector
            GatherCheckpointsFromScene();
        }

        MissionTimer = (mode == GameMode.CheckpointSprint) ? sprintInitialTime : deliveryInitialTime;
        IsMissionRunning = true;

        ActivateCurrentCheckpoint();
        UpdateHUD();
    }

    private void GatherCheckpointsFromScene()
    {
        activeCheckpointList.Clear();
        Checkpoint[] found = Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        System.Array.Sort(found, (a, b) => a.checkpointIndex.CompareTo(b.checkpointIndex));
        activeCheckpointList.AddRange(found);
    }

    private void DeactivateAllCheckpoints()
    {
        foreach (var cp in sprintCheckpoints) if (cp != null) cp.SetActiveState(false);
        foreach (var cp in deliveryCheckpoints) if (cp != null) cp.SetActiveState(false);
        foreach (var cp in activeCheckpointList) if (cp != null) cp.SetActiveState(false);
    }

    private void ActivateCurrentCheckpoint()
    {
        DeactivateAllCheckpoints();
        if (CurrentCheckpointIndex < activeCheckpointList.Count)
        {
            var cp = activeCheckpointList[CurrentCheckpointIndex];
            if (cp != null) cp.SetActiveState(true);
        }
    }

    void Update()
    {
        if (IsMissionRunning)
        {
            MissionTimer -= Time.deltaTime;
            UpdateHUD();

            if (MissionTimer <= 0f)
            {
                MissionTimer = 0f;
                MissionFailed("TIME EXPIRED! You did not reach the checkpoint in time.");
            }
        }
    }

    private void UpdateHUD()
    {
        if (uiManager == null) return;

        if (currentMode == GameMode.FreeRoam)
        {
            uiManager.UpdateMissionHUD("FREE ROAM", "Cruising through the city", 0f, PlayerCash);
        }
        else
        {
            string title = (currentMode == GameMode.CheckpointSprint) ? "CHECKPOINT SPRINT" : "DOWNTOWN DELIVERY";
            string obj = $"Reach Checkpoint {CurrentCheckpointIndex + 1} / {activeCheckpointList.Count}";
            uiManager.UpdateMissionHUD(title, obj, MissionTimer, PlayerCash);
        }
    }

    public void OnCheckpointCollected(Checkpoint cp)
    {
        if (!IsMissionRunning) return;

        // Reward player
        MissionTimer += cp.bonusTime;
        PlayerCash += cp.bonusCash;

        // Play positive sound
        var audioMgr = Object.FindFirstObjectByType<CarAudioManager>();
        if (audioMgr != null) audioMgr.PlayCheckpointChime();

        CurrentCheckpointIndex++;

        if (CurrentCheckpointIndex >= activeCheckpointList.Count)
        {
            // Mission Completed!
            MissionCompleted();
        }
        else
        {
            ActivateCurrentCheckpoint();
            UpdateHUD();
        }
    }

    private void MissionCompleted()
    {
        IsMissionRunning = false;
        DeactivateAllCheckpoints();

        int reward = (currentMode == GameMode.CheckpointSprint) ? 1000 : 1500;
        PlayerCash += reward;

        string stats = $"MISSION ACCOMPLISHED!\nTime Remaining: {Mathf.RoundToInt(MissionTimer)}s\nReward Earned: ${reward:N0}\nTotal Cash: ${PlayerCash:N0}";
        if (uiManager != null)
        {
            uiManager.ShowMissionComplete(stats);
        }
    }

    private void MissionFailed(string reason)
    {
        IsMissionRunning = false;
        DeactivateAllCheckpoints();

        if (uiManager != null)
        {
            uiManager.ShowMissionFailed($"MISSION FAILED!\n{reason}");
        }
    }

    public void RestartCurrentMode()
    {
        if (playerCar != null)
        {
            playerCar.position = playerStartPosition;
            playerCar.rotation = playerStartRotation;
            var cc = playerCar.GetComponent<CarController>();
            if (cc != null) cc.ResetOrientation();
        }

        StartMission(currentMode);
    }

    public void StartNextMission()
    {
        if (currentMode == GameMode.CheckpointSprint)
        {
            StartMission(GameMode.DowntownDelivery);
        }
        else
        {
            StartMission(GameMode.CheckpointSprint);
        }
    }
}
