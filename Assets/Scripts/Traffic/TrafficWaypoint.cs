using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Node in the road network waypoint graph that AI traffic follows.
/// </summary>
public class TrafficWaypoint : MonoBehaviour
{
    [Header("Connections")]
    public List<TrafficWaypoint> nextWaypoints = new List<TrafficWaypoint>();

    [Header("Speed & Lane")]
    public float targetSpeedKmh = 45f;
    public bool isIntersectionZone = false;

    public TrafficWaypoint GetRandomNext()
    {
        if (nextWaypoints == null || nextWaypoints.Count == 0) return null;
        if (nextWaypoints.Count == 1) return nextWaypoints[0];
        return nextWaypoints[Random.Range(0, nextWaypoints.Count)];
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.4f);

        if (nextWaypoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var next in nextWaypoints)
            {
                if (next != null)
                {
                    Gizmos.DrawLine(transform.position, next.transform.position);
                }
            }
        }
    }
}
