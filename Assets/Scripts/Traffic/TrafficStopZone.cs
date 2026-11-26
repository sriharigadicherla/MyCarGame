using UnityEngine;

/// <summary>
/// Stop line trigger zone placed at intersection approaches.
/// Tells AI traffic whether they must stop for a red signal.
/// </summary>
public class TrafficStopZone : MonoBehaviour
{
    public bool IsRedLight { get; set; } = false;

    void Awake()
    {
        // Ensure collider is trigger
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }
}
