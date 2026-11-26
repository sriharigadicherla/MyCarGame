using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Advanced 3rd-Person Dynamic Car Camera Controller.
/// Features:
/// - Smooth position and rotation tracking with look-ahead dampening
/// - Dynamic speed-dependent distance and FOV (widens at high speeds)
/// - Collision avoidance / anti-clipping against buildings, walls, and ground
/// - 3 selectable camera view modes (Chase Medium, Chase Far, Hood/Bumper)
/// - Impact camera shake on collisions
/// </summary>
public class CarCameraController : MonoBehaviour
{
    public enum CameraViewMode
    {
        ChaseMedium,
        ChaseFar,
        Hood
    }

    [Header("Target Tracking")]
    public Transform target;
    public CameraViewMode currentMode = CameraViewMode.ChaseMedium;

    [Header("Chase Medium Preset")]
    public Vector3 mediumOffset = new Vector3(0f, 2.3f, -6.5f);
    public float mediumLookHeight = 1.15f;
    public Vector3 offset { get => mediumOffset; set => mediumOffset = value; }
    public float lookAtHeightOffset { get => mediumLookHeight; set => mediumLookHeight = value; }

    [Header("Chase Far Preset")]
    public Vector3 farOffset = new Vector3(0f, 3.4f, -8.5f);
    public float farLookHeight = 1.35f;

    [Header("Hood Preset")]
    public Vector3 hoodOffset = new Vector3(0f, 1.25f, 0.55f);
    public float hoodLookHeight = 1.10f;

    [Header("Smoothing")]
    public float positionSmoothSpeed = 12f;
    public float rotationSmoothSpeed = 14f;

    [Header("Dynamic Speed Adjustments")]
    public float maxSpeedPullback = 1.35f;
    public float baseFov = 60f;
    public float maxFov = 74f;

    [Header("Collision Avoidance")]
    public LayerMask collisionLayers = ~0;
    public float collisionRadius = 0.25f;
    public float collisionOffsetMargin = 0.35f;

    private Camera cam;
    private CarController carController;
    private float currentFov;
    private float shakeIntensity = 0f;
    private float shakeDuration = 0f;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        currentFov = baseFov;
    }

    void Start()
    {
        FindTargetIfNull();
    }

    private void FindTargetIfNull()
    {
        if (target == null)
        {
            GameObject car = GameObject.Find("SportsSedan");
            if (car != null)
            {
                target = car.transform;
                carController = car.GetComponent<CarController>();
            }
        }
        else if (carController == null)
        {
            carController = target.GetComponent<CarController>();
        }
    }

    void Update()
    {
        // Toggle camera mode with 'C' key
        bool toggleView = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame) toggleView = true;
#endif
        if (!toggleView)
        {
            try { if (Input.GetKeyDown(KeyCode.C)) toggleView = true; } catch { }
        }

        if (toggleView)
        {
            CycleCameraMode();
        }

        // Decay camera shake
        if (shakeDuration > 0f)
        {
            shakeDuration -= Time.deltaTime;
            if (shakeDuration <= 0f) shakeIntensity = 0f;
        }
    }

    public void CycleCameraMode()
    {
        currentMode = (CameraViewMode)(((int)currentMode + 1) % 3);
    }

    public void SetCameraMode(CameraViewMode mode)
    {
        currentMode = mode;
    }

    public void TriggerImpactShake(float intensity, float duration)
    {
        shakeIntensity = Mathf.Clamp(intensity, 0.1f, 1.5f);
        shakeDuration = Mathf.Clamp(duration, 0.05f, 0.8f);
    }

    void LateUpdate()
    {
        if (target == null)
        {
            FindTargetIfNull();
            if (target == null) return;
        }

        float speedRatio = 0f;
        if (carController != null)
        {
            speedRatio = carController.NormalizedSpeed;
        }

        // 1. Determine active preset offsets
        Vector3 baseOffset;
        float lookHeight;

        switch (currentMode)
        {
            case CameraViewMode.Hood:
                baseOffset = hoodOffset;
                lookHeight = hoodLookHeight;
                break;
            case CameraViewMode.ChaseFar:
                baseOffset = farOffset;
                lookHeight = farLookHeight;
                break;
            case CameraViewMode.ChaseMedium:
            default:
                baseOffset = mediumOffset;
                lookHeight = mediumLookHeight;
                break;
        }

        // Apply dynamic speed pullback in chase views
        if (currentMode != CameraViewMode.Hood)
        {
            baseOffset.z -= speedRatio * maxSpeedPullback;
            baseOffset.y += speedRatio * 0.35f;
        }

        // 2. Compute Desired Position in World Space
        Vector3 desiredPosition = target.TransformPoint(baseOffset);
        Vector3 lookTarget = target.position + Vector3.up * lookHeight;

        // 3. Collision Avoidance (anti-clipping against environment)
        if (currentMode != CameraViewMode.Hood)
        {
            Vector3 origin = lookTarget;
            Vector3 direction = desiredPosition - origin;
            float targetDistance = direction.magnitude;

            if (targetDistance > 0.1f)
            {
                direction.Normalize();
                RaycastHit[] hits = Physics.SphereCastAll(origin, collisionRadius, direction, targetDistance, collisionLayers, QueryTriggerInteraction.Ignore);
                float closestHit = targetDistance;
                bool foundObstacle = false;

                foreach (var h in hits)
                {
                    // Ignore target car and any child parts of the car
                    if (h.collider == null) continue;
                    if (h.collider.transform == target || h.collider.transform.IsChildOf(target)) continue;

                    if (h.distance < closestHit && h.distance > 0.8f)
                    {
                        closestHit = h.distance;
                        foundObstacle = true;
                    }
                }

                if (foundObstacle)
                {
                    float safeDistance = Mathf.Max(closestHit - collisionOffsetMargin, 2.5f);
                    desiredPosition = origin + direction * safeDistance;
                }
            }
        }

        // 4. Apply Camera Shake if active
        if (shakeDuration > 0f)
        {
            Vector3 shake = Random.insideUnitSphere * (shakeIntensity * 0.25f);
            desiredPosition += shake;
        }

        // 5. Smoothly Move and Rotate Camera
        float posSpeed = (currentMode == CameraViewMode.Hood) ? 40f : positionSmoothSpeed;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * posSpeed);

        Vector3 forwardDir = lookTarget - transform.position;
        if (forwardDir.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(forwardDir, Vector3.up);
            float rotSpeed = (currentMode == CameraViewMode.Hood) ? 40f : rotationSmoothSpeed;
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotSpeed);
        }

        // 6. Dynamic Field Of View
        if (cam != null)
        {
            float targetFov = Mathf.Lerp(baseFov, maxFov, speedRatio);
            currentFov = Mathf.Lerp(currentFov, targetFov, Time.deltaTime * 6f);
            cam.fieldOfView = currentFov;
        }
    }
}
