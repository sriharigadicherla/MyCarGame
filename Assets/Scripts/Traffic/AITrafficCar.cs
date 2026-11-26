using UnityEngine;

/// <summary>
/// Autonomous AI Traffic Vehicle.
/// Follows road lane waypoints, obeys speed limits, stops behind vehicles and at red lights,
/// animates steering and rolling wheels, and controls brake lights.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class AITrafficCar : MonoBehaviour
{
    [Header("Navigation")]
    public TrafficWaypoint currentWaypoint;
    public float waypointArrivalDistance = 4.0f;
    public float topSpeedKmh = 50f;
    public float accelerationRate = 18f;
    public float brakeRate = 32f;
    public float turnSpeed = 65f;

    [Header("Sensor & Obstacle Avoidance")]
    public float sensorRange = 16f;
    public float stopDistance = 4.5f;
    public LayerMask obstacleLayers = ~0;

    [Header("Visuals")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;
    public MeshRenderer taillightRenderer;
    public Material taillightNormalMat;
    public Material taillightBrakeMat;

    // State
    public float CurrentSpeedKmh { get; private set; }
    public bool IsBraking { get; private set; }

    private Rigidbody rb;
    private float currentSteerAngle = 0f;
    private float stuckTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 1350f;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 1.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.centerOfMass = new Vector3(0f, -0.3f, 0f);
    }

    void Start()
    {
        if (currentWaypoint == null)
        {
            FindClosestWaypoint();
        }
    }

    public void Initialize(TrafficWaypoint startWaypoint)
    {
        currentWaypoint = startWaypoint;
        if (currentWaypoint != null)
        {
            transform.position = currentWaypoint.transform.position + Vector3.up * 0.15f;
            if (currentWaypoint.nextWaypoints.Count > 0 && currentWaypoint.nextWaypoints[0] != null)
            {
                transform.rotation = Quaternion.LookRotation(currentWaypoint.nextWaypoints[0].transform.position - transform.position);
            }
        }
    }

    private void FindClosestWaypoint()
    {
        TrafficWaypoint[] all = Object.FindObjectsByType<TrafficWaypoint>(FindObjectsSortMode.None);
        float bestDist = float.MaxValue;
        TrafficWaypoint best = null;
        foreach (var wp in all)
        {
            float d = Vector3.Distance(transform.position, wp.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = wp;
            }
        }
        currentWaypoint = best;
    }

    void FixedUpdate()
    {
#if UNITY_6000_0_OR_NEWER
        CurrentSpeedKmh = Vector3.Dot(rb.linearVelocity, transform.forward) * 3.6f;
#else
        CurrentSpeedKmh = Vector3.Dot(rb.velocity, transform.forward) * 3.6f;
#endif

        if (currentWaypoint == null) return;

        // 1. Waypoint Tracking
        Vector3 targetPos = currentWaypoint.transform.position;
        targetPos.y = transform.position.y;
        Vector3 dirToTarget = targetPos - transform.position;
        float distanceToWaypoint = dirToTarget.magnitude;

        if (distanceToWaypoint <= waypointArrivalDistance)
        {
            currentWaypoint = currentWaypoint.GetRandomNext();
            if (currentWaypoint == null)
            {
                FindClosestWaypoint();
                return;
            }
        }

        // 2. Obstacle & Red Light Detection
        bool shouldStop = CheckObstaclesAhead(out float obstacleDistance);

        // 3. Drive & Brake Logic
        float targetSpeed = shouldStop ? 0f : (currentWaypoint != null ? currentWaypoint.targetSpeedKmh : topSpeedKmh);

        if (shouldStop || CurrentSpeedKmh > targetSpeed + 2f)
        {
            // Apply Braking
            IsBraking = true;
            rb.AddForce(-transform.forward * brakeRate, ForceMode.Acceleration);
        }
        else if (CurrentSpeedKmh < targetSpeed)
        {
            // Apply Acceleration
            IsBraking = false;
            rb.AddForce(transform.forward * accelerationRate, ForceMode.Acceleration);
        }
        else
        {
            IsBraking = false;
        }

        // 4. Steering towards target
        if (dirToTarget.sqrMagnitude > 0.01f)
        {
            Vector3 localTarget = transform.InverseTransformPoint(targetPos);
            float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
            currentSteerAngle = Mathf.Clamp(targetAngle, -32f, 32f);

            if (Mathf.Abs(CurrentSpeedKmh) > 1.5f)
            {
                float turnAmount = (currentSteerAngle / 32f) * turnSpeed * Time.fixedDeltaTime;
                Quaternion turnRot = Quaternion.Euler(0f, turnAmount, 0f);
                rb.MoveRotation(rb.rotation * turnRot);
            }
        }

        // 5. Anti-drift lateral friction
#if UNITY_6000_0_OR_NEWER
        float latSpeed = Vector3.Dot(rb.linearVelocity, transform.right);
#else
        float latSpeed = Vector3.Dot(rb.velocity, transform.right);
#endif
        rb.AddForce(-transform.right * (latSpeed * 10f), ForceMode.Acceleration);

        // 6. Stuck Detection
        if (CurrentSpeedKmh < 1.0f && !shouldStop)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer > 7f)
            {
                stuckTimer = 0f;
                // Nudge car forward or pick next waypoint
                if (currentWaypoint != null)
                {
                    currentWaypoint = currentWaypoint.GetRandomNext();
                }
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    private bool CheckObstaclesAhead(out float hitDistance)
    {
        hitDistance = sensorRange;
        Vector3 origin = transform.position + Vector3.up * 0.55f + transform.forward * 1.8f;
        Vector3 forward = transform.forward;

        // Cast forward box or rays
        RaycastHit[] hits = Physics.SphereCastAll(origin, 0.9f, forward, sensorRange, obstacleLayers, QueryTriggerInteraction.Collide);

        foreach (var hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;

            // Check if it is a Red Light stop zone
            var stopZone = hit.collider.GetComponent<TrafficStopZone>();
            if (stopZone != null && stopZone.IsRedLight)
            {
                hitDistance = hit.distance;
                return true;
            }

            // Check if it is the Player car or another vehicle
            if (hit.collider.attachedRigidbody != null)
            {
                hitDistance = hit.distance;
                if (hit.distance < stopDistance + 6.0f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void Update()
    {
        UpdateWheelVisuals();
        UpdateBrakeLights();
    }

    private void UpdateWheelVisuals()
    {
        float speedRatio = (CurrentSpeedKmh / 3.6f) * Time.deltaTime / 0.34f * Mathf.Rad2Deg;

        // Roll wheels
        if (frontLeftWheel != null) frontLeftWheel.Rotate(Vector3.right, speedRatio, Space.Self);
        if (frontRightWheel != null) frontRightWheel.Rotate(Vector3.right, speedRatio, Space.Self);
        if (rearLeftWheel != null) rearLeftWheel.Rotate(Vector3.right, speedRatio, Space.Self);
        if (rearRightWheel != null) rearRightWheel.Rotate(Vector3.right, speedRatio, Space.Self);
    }

    private void UpdateBrakeLights()
    {
        if (taillightRenderer != null)
        {
            if (IsBraking && taillightBrakeMat != null)
            {
                taillightRenderer.sharedMaterial = taillightBrakeMat;
            }
            else if (taillightNormalMat != null)
            {
                taillightRenderer.sharedMaterial = taillightNormalMat;
            }
        }
    }
}
