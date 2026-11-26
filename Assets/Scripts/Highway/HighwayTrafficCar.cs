using UnityEngine;

/// <summary>
/// Autonomous Highway Traffic Vehicle.
/// Cruises smoothly forward in its assigned highway lane, maintains safe distance from vehicles ahead,
/// rolls wheels realistically, and activates brake lights when decelerating.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class HighwayTrafficCar : MonoBehaviour
{
    [Header("Lane & Speed")]
    public int laneIndex = 0;
    public float targetLaneX = 0f;
    public float desiredSpeedKmh = 75f;
    public float accelerationRate = 14f;
    public float brakeRate = 28f;

    [Header("Sensor & Radar")]
    public float sensorRange = 28f;
    public float safeFollowDistance = 12f;
    public LayerMask obstacleLayers = ~0;

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
    private float lateralSmoothSpeed = 4.0f;

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

    public void Setup(int lane, float laneX, float speed)
    {
        laneIndex = lane;
        targetLaneX = laneX;
        desiredSpeedKmh = speed;

        if (rb == null) rb = GetComponent<Rigidbody>();
        #if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = new Vector3(0f, 0f, desiredSpeedKmh / 3.6f);
        #else
        rb.velocity = new Vector3(0f, 0f, desiredSpeedKmh / 3.6f);
        #endif
        transform.rotation = Quaternion.identity;
    }

    void FixedUpdate()
    {
        #if UNITY_6000_0_OR_NEWER
        CurrentSpeedKmh = rb.linearVelocity.z * 3.6f;
        #else
        CurrentSpeedKmh = rb.velocity.z * 3.6f;
        #endif

        // 1. Forward Radar: Detect vehicle ahead in same lane
        bool obstacleAhead = CheckVehicleAhead(out float distanceToObstacle);

        // 2. Drive & Brake Logic
        float effectiveTargetSpeed = desiredSpeedKmh;
        if (obstacleAhead)
        {
            float speedFactor = Mathf.Clamp01((distanceToObstacle - safeFollowDistance) / (sensorRange - safeFollowDistance));
            effectiveTargetSpeed = desiredSpeedKmh * speedFactor * 0.75f;
            if (distanceToObstacle < safeFollowDistance + 2.0f)
            {
                effectiveTargetSpeed = 0f;
            }
        }

        if (CurrentSpeedKmh > effectiveTargetSpeed + 1.5f || (obstacleAhead && distanceToObstacle < safeFollowDistance + 4.0f))
        {
            // Apply braking
            IsBraking = true;
            rb.AddForce(Vector3.back * brakeRate, ForceMode.Acceleration);
        }
        else if (CurrentSpeedKmh < effectiveTargetSpeed)
        {
            // Apply acceleration
            IsBraking = false;
            rb.AddForce(Vector3.forward * accelerationRate, ForceMode.Acceleration);
        }
        else
        {
            IsBraking = false;
        }

        // 3. Lane Centering: smoothly align X to target lane position
        float curX = transform.position.x;
        float xDiff = targetLaneX - curX;
        float lateralCorrection = Mathf.Clamp(xDiff * lateralSmoothSpeed, -4.0f, 4.0f);
        #if UNITY_6000_0_OR_NEWER
        Vector3 curVel = rb.linearVelocity;
        curVel.x = lateralCorrection;
        rb.linearVelocity = curVel;
        #else
        Vector3 curVel = rb.velocity;
        curVel.x = lateralCorrection;
        rb.velocity = curVel;
        #endif

        // Ensure car faces strictly forward along highway (+Z)
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        // 4. Animate Visual Wheels
        AnimateWheels();

        // 5. Update Brake Lights
        UpdateTaillights();
    }

    private bool CheckVehicleAhead(out float hitDistance)
    {
        hitDistance = sensorRange;
        Vector3 origin = transform.position + Vector3.up * 0.5f + Vector3.forward * 2.2f;

        RaycastHit[] hits = Physics.SphereCastAll(origin, 0.85f, Vector3.forward, sensorRange, obstacleLayers, QueryTriggerInteraction.Ignore);
        bool found = false;

        foreach (var hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;

            // Check if it's another vehicle in or near this lane
            if (Mathf.Abs(hit.transform.position.x - targetLaneX) < 2.0f)
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
