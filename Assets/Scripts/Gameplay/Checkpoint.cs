using UnityEngine;

/// <summary>
/// 3D Checkpoint Marker with rotating holographic chevron and glowing beacon.
/// Triggers milestone events when the player vehicle drives through it.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Config")]
    public int checkpointIndex = 0;
    public float bonusTime = 15f;
    public int bonusCash = 250;

    [Header("Visual Components")]
    public Transform rotatingChevron;
    public Light beaconLight;

    public bool IsActive { get; private set; } = false;

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (col == null)
        {
            var box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(8.5f, 5.0f, 4.0f);
            box.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }
    }

    void Update()
    {
        if (IsActive && rotatingChevron != null)
        {
            rotatingChevron.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);
            // Bob up and down
            float yOffset = Mathf.Sin(Time.time * 3f) * 0.15f;
            rotatingChevron.localPosition = new Vector3(0f, 2.5f + yOffset, 0f);
        }
    }

    public void SetActiveState(bool active)
    {
        IsActive = active;
        gameObject.SetActive(active);
        if (beaconLight != null) beaconLight.enabled = active;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsActive) return;

        // Check if player car
        var car = other.GetComponentInParent<CarController>();
        if (car != null)
        {
            var gm = Object.FindFirstObjectByType<GameplayManager>();
            if (gm != null)
            {
                gm.OnCheckpointCollected(this);
            }
        }
    }
}
