using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Advanced 3D Car Physics & Driving Controller.
/// Features:
/// - Smooth acceleration curve with realistic top speed limiter (up to 200 km/h)
/// - Progressive braking and automatic reverse shifting
/// - Handbrake drift dynamics with rear lateral slip
/// - Speed-sensitive steering (tightens at high speeds to avoid spinouts)
/// - Low center-of-mass and dynamic aerodynamic downforce (anti-roll / flip prevention)
/// - 6-speed automatic transmission simulation with RPM curve
/// - Universal input support: New Input System, Legacy Input, and On-screen Virtual Pedals
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Engine & Speed")]
    [Tooltip("Legacy speed alias for editor backwards compatibility")]
    public float speed { get => maxSpeedKmh; set => maxSpeedKmh = value; }
    [Tooltip("Legacy turnSpeed alias for editor backwards compatibility")]
    public float turnSpeed { get => steerSpeed * 10f; set => steerSpeed = value / 10f; }
    [Tooltip("Maximum forward speed in km/h")]
    public float maxSpeedKmh = 195f;
    [Tooltip("Maximum reverse speed in km/h")]
    public float maxReverseSpeedKmh = 48f;
    [Tooltip("Forward acceleration torque force")]
    public float accelerationRate = 26f;
    [Tooltip("Reverse acceleration rate")]
    public float reverseAccelerationRate = 16f;
    [Tooltip("Braking deceleration force")]
    public float brakePower = 38f;
    [Tooltip("Passive drag deceleration when coasting")]
    public float coastingDrag = 4.5f;

    [Header("Steering & Handling")]
    [Tooltip("Maximum steering angle at low speeds (degrees)")]
    public float maxSteerAngle = 32f;
    [Tooltip("Minimum steering angle at high speeds (degrees)")]
    public float minSteerAngle = 12f;
    [Tooltip("Steering interpolation speed")]
    public float steerSpeed = 7.5f;
    [Tooltip("Lateral tire grip (higher = less sliding)")]
    public float lateralTireGrip = 8.5f;
    [Tooltip("Drift grip during handbrake")]
    public float driftLateralGrip = 2.0f;

    [Header("Stability & Downforce")]
    [Tooltip("Vertical center of mass offset (negative = lower, prevents flipping)")]
    public float centerOfMassY = -0.38f;
    [Tooltip("Aerodynamic downforce factor at high speeds")]
    public float downforceFactor = 4.2f;

    [Header("Transmission & RPM")]
    public float idleRpm = 850f;
    public float maxRpm = 7200f;
    public int totalGears = 6;

    [Header("Gameplay State")]
    public bool isGameplayActive = false;

    // Public properties for UI and Audio
    public float CurrentSpeedKmh { get; private set; }
    public float ForwardVelocity { get; private set; }
    public float CurrentRpm { get; private set; }
    public int CurrentGear { get; private set; } = 1;
    public string CurrentGearName { get; private set; } = "N";
    public float CurrentSteerAngle { get; private set; }
    public bool IsBraking { get; private set; }
    public bool IsReversing { get; private set; }
    public bool IsHandbraking { get; private set; }
    public bool IsAccelerating { get; private set; }
    public float NormalizedSpeed => Mathf.Clamp01(CurrentSpeedKmh / maxSpeedKmh);
    public float NormalizedRpm => Mathf.Clamp01((CurrentRpm - idleRpm) / (maxRpm - idleRpm));

    // Internal physics
    private Rigidbody rb;
    private float moveInput;
    private float turnInput;
    private bool handbrakeInput;

    // Virtual inputs from UI on-screen buttons
    private float virtualMoveInput = 0f;
    private float virtualTurnInput = 0f;
    private bool virtualHandbrakeInput = false;
    private bool useVirtualInputs = false;

    // Gear ratios (top speed for each gear)
    private readonly float[] gearMaxSpeeds = new float[] { 35f, 68f, 105f, 142f, 175f, 210f };

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        SetupPhysicsProperties();
    }

    void Start()
    {
        SetupPhysicsProperties();
    }

    private void SetupPhysicsProperties()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.mass = 1450f;
        rb.linearDamping = 0.08f;
        rb.angularDamping = 1.2f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Lower center of mass below axle line to prevent rolling
        rb.centerOfMass = new Vector3(0f, centerOfMassY, 0.05f);
    }

    void Update()
    {
        ReadInputs();
        CalculateTransmissionAndRpm();
    }

    void FixedUpdate()
    {
        if (!isGameplayActive)
        {
            // Keep car stationary at rest while in Main Menu
            if (rb != null)
            {
                #if UNITY_6000_0_OR_NEWER
                Vector3 v = rb.linearVelocity;
                v.x = 0f;
                v.z = 0f;
                rb.linearVelocity = v;
                #else
                Vector3 v = rb.velocity;
                v.x = 0f;
                v.z = 0f;
                rb.velocity = v;
                #endif
            }
            return;
        }

        CalculateSpeeds();
        ApplySteering();
        ApplyEngineAndBraking();
        ApplyLateralTraction();
        ApplyDownforceAndAntiRoll();
    }

    public void ResetToStart()
    {
        transform.position = new Vector3(-1.9f, 0.15f, 0f);
        transform.rotation = Quaternion.identity;
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            #if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            #else
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            #endif
        }
        CurrentSpeedKmh = 0f;
        CurrentRpm = idleRpm;
        CurrentGear = 1;
        CurrentGearName = "N";
        isGameplayActive = false;
    }

    #region Input Processing
    private void ReadInputs()
    {
        if (!isGameplayActive)
        {
            moveInput = 0f;
            turnInput = 0f;
            handbrakeInput = true;
            return;
        }
        float rawMove = 0f;
        float rawTurn = 0f;
        bool rawHandbrake = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            var kb = Keyboard.current;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) rawMove += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) rawMove -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) rawTurn += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) rawTurn -= 1f;
            if (kb.spaceKey.isPressed) rawHandbrake = true;
        }
#endif
        if (Mathf.Approximately(rawMove, 0f) && Mathf.Approximately(rawTurn, 0f))
        {
            try
            {
                rawMove = Input.GetAxisRaw("Vertical");
                rawTurn = Input.GetAxisRaw("Horizontal");
                if (Input.GetKey(KeyCode.Space)) rawHandbrake = true;
            }
            catch { }
        }

        // 2. Blend with Virtual UI Pedals/Buttons if active
        if (useVirtualInputs)
        {
            if (Mathf.Abs(virtualMoveInput) > 0.01f) rawMove = virtualMoveInput;
            if (Mathf.Abs(virtualTurnInput) > 0.01f) rawTurn = virtualTurnInput;
            if (virtualHandbrakeInput) rawHandbrake = true;
        }

        moveInput = Mathf.Clamp(rawMove, -1f, 1f);
        turnInput = Mathf.Clamp(rawTurn, -1f, 1f);
        handbrakeInput = rawHandbrake;
    }

    /// <summary>
    /// Feed virtual inputs from UI touch pedals / on-screen buttons
    /// </summary>
    public void SetVirtualInputs(float move, float steer, bool handbrake)
    {
        virtualMoveInput = move;
        virtualTurnInput = steer;
        virtualHandbrakeInput = handbrake;
        useVirtualInputs = (Mathf.Abs(move) > 0.01f || Mathf.Abs(steer) > 0.01f || handbrake);
        if (useVirtualInputs)
        {
            moveInput = Mathf.Clamp(move, -1f, 1f);
            turnInput = Mathf.Clamp(steer, -1f, 1f);
            handbrakeInput = handbrake;
        }
    }

    /// <summary>
    /// Explicit physics simulation step for testing and headless verification
    /// </summary>
    public void SimulateFixedUpdate(float dt)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (!isGameplayActive) return;
        CalculateSpeeds();
        ApplySteering();
        ApplyEngineAndBraking();
        ApplyLateralTraction();
        ApplyDownforceAndAntiRoll();
    }
    #endregion

    #region Physics Calculations
    private void CalculateSpeeds()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
#if UNITY_6000_0_OR_NEWER
        Vector3 vel = rb.linearVelocity;
#else
        Vector3 vel = rb.velocity;
#endif
        ForwardVelocity = Vector3.Dot(vel, transform.forward);
        CurrentSpeedKmh = Mathf.Abs(ForwardVelocity) * 3.6f;
    }

    private void ApplySteering()
    {
        // Dynamic speed-sensitive steering: sharp at parking speeds, tight at highway speeds
        float speedRatio = Mathf.Clamp01(CurrentSpeedKmh / 140f);
        float currentMaxAngle = Mathf.Lerp(maxSteerAngle, minSteerAngle, speedRatio);

        float targetAngle = turnInput * currentMaxAngle;
        CurrentSteerAngle = Mathf.MoveTowards(CurrentSteerAngle, targetAngle, steerSpeed * 10f * Time.fixedDeltaTime);

        // Apply yaw rotation when the car is rolling
        if (Mathf.Abs(ForwardVelocity) > 0.1f)
        {
            float direction = ForwardVelocity >= 0f ? 1f : -1f;
            float turnAmount = (CurrentSteerAngle / maxSteerAngle) * 75f * direction * Time.fixedDeltaTime;

            // Reduce turning slightly if drifting with handbrake
            if (handbrakeInput) turnAmount *= 1.35f;

            Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }

    private void ApplyEngineAndBraking()
    {
        IsBraking = false;
        IsReversing = false;
        IsAccelerating = false;
        IsHandbraking = handbrakeInput;

        // Handbrake overrides normal propulsion
        if (handbrakeInput)
        {
            IsBraking = true;
            // Strong rear brake deceleration
            rb.AddForce(-transform.forward * Mathf.Sign(ForwardVelocity) * (brakePower * 1.25f), ForceMode.Acceleration);
            return;
        }

        if (moveInput > 0.05f)
        {
            // Forward Acceleration
            if (ForwardVelocity < -0.8f)
            {
                // Car is rolling backward, so W/Up acts as brake
                IsBraking = true;
                rb.AddForce(transform.forward * brakePower, ForceMode.Acceleration);
            }
            else
            {
                // Normal Forward Drive
                IsAccelerating = true;
                if (CurrentSpeedKmh < maxSpeedKmh)
                {
                    // Power curve: high torque at low speeds, tapering near max speed
                    float torqueCurve = Mathf.Clamp01(1.1f - (CurrentSpeedKmh / maxSpeedKmh));
                    Vector3 driveForce = transform.forward * (accelerationRate * moveInput * torqueCurve);
                    rb.AddForce(driveForce, ForceMode.Acceleration);
                }
            }
        }
        else if (moveInput < -0.05f)
        {
            // S / Down Key Pressed
            if (ForwardVelocity > 0.8f)
            {
                // Car is moving forward, so S/Down acts as BRAKE
                IsBraking = true;
                rb.AddForce(-transform.forward * (brakePower * Mathf.Abs(moveInput)), ForceMode.Acceleration);
            }
            else
            {
                // Car is stopped or already in reverse: REVERSE DRIVE
                IsReversing = true;
                if (CurrentSpeedKmh < maxReverseSpeedKmh)
                {
                    float reverseTorque = Mathf.Clamp01(1.0f - (CurrentSpeedKmh / maxReverseSpeedKmh));
                    Vector3 reverseForce = -transform.forward * (reverseAccelerationRate * Mathf.Abs(moveInput) * reverseTorque);
                    rb.AddForce(reverseForce, ForceMode.Acceleration);
                }
            }
        }
        else
        {
            // Coasting Drag (Engine Braking)
            if (Mathf.Abs(ForwardVelocity) > 0.2f)
            {
                Vector3 dragForce = -transform.forward * (Mathf.Sign(ForwardVelocity) * coastingDrag);
                rb.AddForce(dragForce, ForceMode.Acceleration);
            }
        }
    }

    private void ApplyLateralTraction()
    {
        // Counteracts sideways sliding to give responsive car grip
#if UNITY_6000_0_OR_NEWER
        Vector3 vel = rb.linearVelocity;
#else
        Vector3 vel = rb.velocity;
#endif
        float lateralVelocity = Vector3.Dot(vel, transform.right);
        float grip = handbrakeInput ? driftLateralGrip : lateralTireGrip;

        Vector3 correctiveFriction = -transform.right * (lateralVelocity * grip);
        rb.AddForce(correctiveFriction, ForceMode.Acceleration);
    }

    private void ApplyDownforceAndAntiRoll()
    {
        // Aerodynamic downforce scales quadratically with forward speed
        float speedRatio = CurrentSpeedKmh / 100f;
        float downforceAmount = downforceFactor * (speedRatio * speedRatio) * 9.81f;
        rb.AddForce(-Vector3.up * downforceAmount, ForceMode.Acceleration);

        // Keep car upright (prevent rolling upside down)
        float zAngle = transform.eulerAngles.z;
        if (zAngle > 180f) zAngle -= 360f;
        if (Mathf.Abs(zAngle) > 35f)
        {
            // Level out smoothly if knocked into air
            Quaternion targetUp = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0f);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetUp, Time.fixedDeltaTime * 4f));
        }
    }
    #endregion

    #region Transmission & RPM Simulation
    private void CalculateTransmissionAndRpm()
    {
        if (IsReversing)
        {
            CurrentGear = -1;
            CurrentGearName = "R";
            float revRatio = Mathf.Clamp01(CurrentSpeedKmh / maxReverseSpeedKmh);
            CurrentRpm = Mathf.Lerp(idleRpm, maxRpm * 0.75f, revRatio);
            return;
        }

        if (CurrentSpeedKmh < 1.0f && Mathf.Abs(moveInput) < 0.05f)
        {
            CurrentGear = 0;
            CurrentGearName = "N";
            CurrentRpm = Mathf.MoveTowards(CurrentRpm, idleRpm, Time.deltaTime * 1500f);
            return;
        }

        // Automatic 6-speed gear selection
        int calculatedGear = 1;
        for (int i = 0; i < gearMaxSpeeds.Length; i++)
        {
            if (CurrentSpeedKmh <= gearMaxSpeeds[i] || i == gearMaxSpeeds.Length - 1)
            {
                calculatedGear = i + 1;
                break;
            }
        }
        CurrentGear = calculatedGear;
        CurrentGearName = $"D{CurrentGear}";

        // Calculate RPM within current gear range
        float gearMinSpeed = (CurrentGear == 1) ? 0f : gearMaxSpeeds[CurrentGear - 2];
        float gearMaxSpeed = gearMaxSpeeds[CurrentGear - 1];
        float gearSpeedRange = Mathf.Max(gearMaxSpeed - gearMinSpeed, 10f);

        float gearProgress = Mathf.Clamp01((CurrentSpeedKmh - gearMinSpeed) / gearSpeedRange);
        float targetRpm = Mathf.Lerp(2200f, maxRpm, gearProgress);

        if (CurrentGear == 1 && CurrentSpeedKmh < 10f)
        {
            targetRpm = Mathf.Lerp(idleRpm, 2400f, CurrentSpeedKmh / 10f);
        }

        // Add throttle rev boost when accelerating
        if (IsAccelerating)
        {
            targetRpm = Mathf.Min(targetRpm + 400f, maxRpm);
        }

        CurrentRpm = Mathf.Lerp(CurrentRpm, targetRpm, Time.deltaTime * 10f);
    }
    #endregion

    /// <summary>
    /// Reset car orientation if stuck or overturned
    /// </summary>
    public void ResetOrientation()
    {
        transform.position += Vector3.up * 1.2f;
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;
        if (impact > 2.0f)
        {
            var audioMgr = Object.FindFirstObjectByType<CarAudioManager>();
            if (audioMgr != null) audioMgr.PlayCollisionSound(impact);

            var camCtrl = Object.FindFirstObjectByType<CarCameraController>();
            if (camCtrl != null) camCtrl.TriggerImpactShake(impact * 0.06f, 0.25f);
        }
    }
}
