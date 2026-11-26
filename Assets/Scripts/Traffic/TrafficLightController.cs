using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controls traffic light cycles at city intersections (North-South vs East-West).
/// Cycles: Green -> Yellow -> All-Red -> Green (cross street) -> Yellow -> All-Red.
/// Updates signal lamp materials and stop zone triggers.
/// </summary>
public class TrafficLightController : MonoBehaviour
{
    public enum SignalState
    {
        NorthSouth_Green,
        NorthSouth_Yellow,
        All_Red_1,
        EastWest_Green,
        EastWest_Yellow,
        All_Red_2
    }

    [Header("Cycle Durations (Seconds)")]
    public float greenDuration = 12f;
    public float yellowDuration = 3f;
    public float redBufferDuration = 1.5f;

    [Header("Stop Zones")]
    public List<TrafficStopZone> northSouthStopZones = new List<TrafficStopZone>();
    public List<TrafficStopZone> eastWestStopZones = new List<TrafficStopZone>();

    [Header("Lamps - North/South")]
    public List<MeshRenderer> nsRedLamps = new List<MeshRenderer>();
    public List<MeshRenderer> nsYellowLamps = new List<MeshRenderer>();
    public List<MeshRenderer> nsGreenLamps = new List<MeshRenderer>();

    [Header("Lamps - East/West")]
    public List<MeshRenderer> ewRedLamps = new List<MeshRenderer>();
    public List<MeshRenderer> ewYellowLamps = new List<MeshRenderer>();
    public List<MeshRenderer> ewGreenLamps = new List<MeshRenderer>();

    [Header("Signal Materials")]
    public Material matRed;
    public Material matYellow;
    public Material matGreen;
    public Material matOff;

    private SignalState currentState = SignalState.NorthSouth_Green;
    private float timer = 0f;

    void Start()
    {
        LoadMaterialsIfNull();
        SetState(SignalState.NorthSouth_Green);
    }

    private void LoadMaterialsIfNull()
    {
        if (matRed == null) matRed = Resources.Load<Material>("Mat_TrafficLight_Red");
        if (matYellow == null) matYellow = Resources.Load<Material>("Mat_TrafficLight_Yellow");
        if (matGreen == null) matGreen = Resources.Load<Material>("Mat_TrafficLight_Green");
        if (matOff == null) matOff = Resources.Load<Material>("Mat_TrafficLight_Off");
    }

    void Update()
    {
        timer += Time.deltaTime;

        switch (currentState)
        {
            case SignalState.NorthSouth_Green:
                if (timer >= greenDuration) SetState(SignalState.NorthSouth_Yellow);
                break;
            case SignalState.NorthSouth_Yellow:
                if (timer >= yellowDuration) SetState(SignalState.All_Red_1);
                break;
            case SignalState.All_Red_1:
                if (timer >= redBufferDuration) SetState(SignalState.EastWest_Green);
                break;
            case SignalState.EastWest_Green:
                if (timer >= greenDuration) SetState(SignalState.EastWest_Yellow);
                break;
            case SignalState.EastWest_Yellow:
                if (timer >= yellowDuration) SetState(SignalState.All_Red_2);
                break;
            case SignalState.All_Red_2:
                if (timer >= redBufferDuration) SetState(SignalState.NorthSouth_Green);
                break;
        }
    }

    private void SetState(SignalState newState)
    {
        currentState = newState;
        timer = 0f;

        bool nsGreen = (currentState == SignalState.NorthSouth_Green);
        bool nsYellow = (currentState == SignalState.NorthSouth_Yellow);
        bool nsRed = !nsGreen && !nsYellow;

        bool ewGreen = (currentState == SignalState.EastWest_Green);
        bool ewYellow = (currentState == SignalState.EastWest_Yellow);
        bool ewRed = !ewGreen && !ewYellow;

        // Update stop zones
        foreach (var sz in northSouthStopZones) if (sz != null) sz.IsRedLight = nsRed;
        foreach (var sz in eastWestStopZones) if (sz != null) sz.IsRedLight = ewRed;

        // Update lamp visuals
        UpdateLamps(nsRedLamps, nsRed ? matRed : matOff);
        UpdateLamps(nsYellowLamps, nsYellow ? matYellow : matOff);
        UpdateLamps(nsGreenLamps, nsGreen ? matGreen : matOff);

        UpdateLamps(ewRedLamps, ewRed ? matRed : matOff);
        UpdateLamps(ewYellowLamps, ewYellow ? matYellow : matOff);
        UpdateLamps(ewGreenLamps, ewGreen ? matGreen : matOff);
    }

    private void UpdateLamps(List<MeshRenderer> lamps, Material mat)
    {
        if (mat == null) return;
        foreach (var r in lamps)
        {
            if (r != null) r.sharedMaterial = mat;
        }
    }
}
