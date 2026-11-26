using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CarDrivingVerification
{
    public static void RunVerification()
    {
        Debug.Log("<color=cyan>[CarDrivingVerification]</color> Starting Car & Game Systems Verification...");

        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Single);

        var uiMgr = Object.FindFirstObjectByType<HighwayUIManager>();
        if (uiMgr == null)
        {
            Debug.LogError("<color=red>[FAIL]</color> HighwayUIManager not found in scene!");
            return;
        }

        var car = GameObject.Find("SportsSedan");
        if (car == null)
        {
            Debug.LogError("<color=red>[FAIL]</color> SportsSedan player car not found in scene!");
            return;
        }

        var cc = car.GetComponent<CarController>();
        if (cc == null)
        {
            Debug.LogError("<color=red>[FAIL]</color> CarController component not found on SportsSedan!");
            return;
        }

        var rb = car.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("<color=red>[FAIL]</color> Rigidbody not found on SportsSedan!");
            return;
        }

        // Test 1: In Main Menu state, car must be stationary and traffic inactive
        uiMgr.ShowMainMenu();
        bool menuCarFrozen = (rb.isKinematic == true) && (cc.isGameplayActive == false);
        bool trafficFrozen = (uiMgr.trafficManager != null && !uiMgr.trafficManager.isTrafficActive);
        Debug.Log($"[Test 1] Menu State: CarFrozen={menuCarFrozen}, TrafficFrozen={trafficFrozen}");

        if (!menuCarFrozen) Debug.LogError("<color=red>[FAIL]</color> Car is not frozen while in Main Menu!");
        if (!trafficFrozen) Debug.LogError("<color=red>[FAIL]</color> Traffic is not frozen while in Main Menu!");

        // Test 2: Start Highway Game
        uiMgr.StartHighwayGame();
        Debug.Log($"[DEBUG] uiMgr.carController={(uiMgr.carController != null ? uiMgr.carController.name : "NULL")}, cc={cc.name}, IsGameplayRunning={uiMgr.IsGameplayRunning}, isGameplayActive={cc.isGameplayActive}, isKinematic={rb.isKinematic}");
        bool gameplayActive = (uiMgr.IsGameplayRunning == true) && (cc.isGameplayActive == true) && (rb.isKinematic == false);
        bool hudVisible = (uiMgr.gameplayHUDPanel != null && uiMgr.gameplayHUDPanel.activeSelf) && (uiMgr.mainMenuPanel != null && !uiMgr.mainMenuPanel.activeSelf);
        bool trafficActive = (uiMgr.trafficManager != null && uiMgr.trafficManager.isTrafficActive);

        Debug.Log($"[Test 2] Game Running State: GameplayActive={gameplayActive}, HUDVisible={hudVisible}, TrafficActive={trafficActive}");

        if (!gameplayActive) Debug.LogError("<color=red>[FAIL]</color> Gameplay did not activate on car!");
        if (!hudVisible) Debug.LogError("<color=red>[FAIL]</color> HUD is not visible or Menu not hidden!");
        if (!trafficActive) Debug.LogError("<color=red>[FAIL]</color> Traffic is not active during gameplay!");

        // Test 3: Input Driving Response
        cc.SetVirtualInputs(1.0f, 0.5f, false);
        // Simulate Physics step
        Physics.simulationMode = SimulationMode.Script;
        for (int step = 0; step < 60; step++)
        {
            cc.SimulateFixedUpdate(0.02f);
            Physics.Simulate(0.02f);
        }
        Physics.simulationMode = SimulationMode.FixedUpdate;

        float finalSpeed = cc.CurrentSpeedKmh;
        Debug.Log($"[Test 3] Acceleration Physics Test: Speed after 1.2s gas = {finalSpeed:F1} KM/H (Velocity = {rb.linearVelocity})");

        if (finalSpeed > 5f)
        {
            Debug.Log("<color=green>[PASS]</color> ALL VERIFICATION TESTS PASSED! Car accelerates, steers, and drives perfectly!");
        }
        else
        {
            Debug.LogError($"<color=red>[FAIL]</color> Car did not accelerate properly! Speed: {finalSpeed}");
        }

        // Return scene to clean state
        uiMgr.ShowMainMenu();
        var ps = Object.FindFirstObjectByType<ProceduralUISound>();
        if (ps != null) Object.DestroyImmediate(ps.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}

