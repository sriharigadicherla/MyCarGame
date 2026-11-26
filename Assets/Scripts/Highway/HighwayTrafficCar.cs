using UnityEngine;

/// <summary>
/// Autonomous Highway Traffic Vehicle.
/// Cruises smoothly in its assigned highway lane:
/// - Lanes 0 & 1 (Left): Drives Forward (+Z)
/// - Lanes 2 & 3 (Right): Drives Oncoming (-Z, facing 180 degrees with headlights glowing)
/// Maintains safe distance from vehicles ahead, rolls wheels, and activates brake lights.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class HighwayTrafficCar : MonoBehaviour
{
    [Header("Lane & Speed")]
    public int laneIndex = 0;
    public float targetLaneX = 0f;
    public float desiredSpeedKmh = 75f;
    public bool isOncoming = false;
    public float accelerationRate = 18f;
    public float brakeRate = 35f;

    [Header("Sensor & Radar")]
    public float sensorRange = 32f;
    public float safeFollowDistance = 14f;

    [Header("Visuals")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;
    public MeshRenderer taillightRenderer;
    public Material taillightNormalMat;
    public Material taillightBrakeMat;

    // Public properties
    public float CurrentSpeedKmh { get; private set; }
    public bool IsBraking { get; private set; }

    private Rigidbody rb;
    private float wheelRadius = 0.34f;
    private float lateralSmoothSpeed = 4.5f;
    private float currentSpeedMs = 20f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 1400f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 1.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    public void Setup(int lane, float laneX, float speed, bool oncoming = false)
    {
        laneIndex = lane;
        targetLaneX = laneX;
        desiredSpeedKmh = speed;
        isOncoming = oncoming;
        currentSpeedMs = desiredSpeedKmh / 3.6f;

        if (rb == null) rb = GetComponent<Rigidbody>();
        Vector3 heading = isOncoming ? Vector3.back : Vector3.forward;
        Vector3 initVel = heading * currentSpeedMs;
        #if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = initVel;
        #else
        rb.velocity = initVel;
        #endif
        transform.rotation = Quaternion.Euler(0f, isOncoming ? 180f : 0f, 0f);
    }

    void FixedUpdate()
    {
        Vector3 heading = isOncoming ? Vector3.back : Vector3.forward;

        // 1. Forward Radar: Detect vehicle ahead in same lane & heading
        bool obstacleAhead = CheckVehicleAhead(out float distanceToObstacle, heading);

        // 2. Drive & Brake Logic (Strictly non-negative speed along heading)
        float effectiveTargetSpeedKmh = desiredSpeedKmh;
        if (obstacleAhead)
        {
            float speedFactor = Mathf.Clamp01((distanceToObstacle - safeFollowDistance) / (sensorRange - safeFollowDistance));
            effectiveTargetSpeedKmh = desiredSpeedKmh * speedFactor * 0.75f;
            if (distanceToObstacle < safeFollowDistance + 2.0f)
            {
                effectiveTargetSpeedKmh = 0f;
            }
        }

        float targetSpeedMs = effectiveTargetSpeedKmh / 3.6f;

        if (currentSpeedMs > targetSpeedMs + 0.3f)
        {
            IsBraking = true;
            currentSpeedMs = Mathf.MoveTowards(currentSpeedMs, targetSpeedMs, brakeRate * Time.fixedDeltaTime);
        }
        else
        {
            IsBraking = false;
            currentSpeedMs = Mathf.MoveTowards(currentSpeedMs, targetSpeedMs, accelerationRate * Time.fixedDeltaTime);
        }
        currentSpeedMs = Mathf.Max(0f, currentSpeedMs);
        CurrentSpeedKmh = currentSpeedMs * 3.6f;

        // 3. Lane Centering: smoothly align X to target lane position
        float curX = transform.position.x;
        float xDiff = targetLaneX - curX;
        float lateralCorrection = Mathf.Clamp(xDiff * lateralSmoothSpeed, -4.0f, 4.0f);

        Vector3 targetVelocity = heading * currentSpeedMs;
        targetVelocity.x = lateralCorrection;
        #if UNITY_6000_0_OR_NEWER
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
        #else
        targetVelocity.y = rb.velocity.y;
        rb.velocity = targetVelocity;
        #endif

        // Ensure car faces strictly along its highway lane (0 deg for forward, 180 deg for oncoming)
        transform.rotation = Quaternion.Euler(0f, isOncoming ? 180f : 0f, 0f);

        // 4. Animate Visual Wheels
        AnimateWheels();

        // 5. Update Brake Lights
        UpdateTaillights();
    }

    private bool CheckVehicleAhead(out float hitDistance, Vector3 heading)
    {
        hitDistance = sensorRange;
        Vector3 origin = transform.position + Vector3.up * 0.60f + heading * 2.3f;

        RaycastHit[] hits = Physics.SphereCastAll(origin, 0.65f, heading, sensorRange, ~0, QueryTriggerInteraction.Ignore);
        bool found = false;

        foreach (var hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;

            // Only detect other traffic cars or player car - NEVER road slabs, guardrails, or trees!
            var otherTraffic = hit.collider.GetComponentInParent<HighwayTrafficCar>();
            var playerSedan = hit.collider.GetComponentInParent<CarController>();
            if (otherTraffic == null && playerSedan == null) continue;

            // Check if obstacle is in or near this lane
            float obstacleX = hit.collider.transform.position.x;
            if (Mathf.Abs(obstacleX - targetLaneX) < 1.8f)
            {
                if (hit.distance < hitDistance)
                {
                    hitDistance = hit.distance;
                    found = true;
                }
            }
        }

        return found;
    }

    private void AnimateWheels()
    {
        float forwardSpeed = CurrentSpeedKmh / 3.6f;
        float rollAngle = (forwardSpeed / wheelRadius) * Mathf.Rad2Deg * Time.fixedDeltaTime;

        if (frontLeftWheel != null) frontLeftWheel.Rotate(Vector3.right, rollAngle, Space.Self);
        if (frontRightWheel != null) frontRightWheel.Rotate(Vector3.right, rollAngle, Space.Self);
        if (rearLeftWheel != null) rearLeftWheel.Rotate(Vector3.right, rollAngle, Space.Self);
        if (rearRightWheel != null) rearRightWheel.Rotate(Vector3.right, rollAngle, Space.Self);
    }

    private void UpdateTaillights()
    {
        if (taillightRenderer == null) return;
        Material targetMat = IsBraking ? taillightBrakeMat : taillightNormalMat;
        if (targetMat != null && taillightRenderer.sharedMaterial != targetMat)
        {
            taillightRenderer.sharedMaterial = targetMat;
        }
    }
}
