using UnityEngine;

/// <summary>
/// Handles all visual aspects of the sports sedan:
/// - Front wheel steering angle animation
/// - 4-wheel rotation rolling matched to actual forward/reverse speed
/// - Tail / brake light emission brightening during braking and reversing
/// - Rear wheel skidmark trails during handbrake and power sliding
/// - Headlight toggling
/// </summary>
public class CarVisuals : MonoBehaviour
{
    [Header("Steering Wheel Pivots (Front)")]
    public Transform frontLeftPivot;
    public Transform frontRightPivot;

    [Header("Rolling Wheel Meshes")]
    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearLeftMesh;
    public Transform rearRightMesh;

    [Header("Wheel Settings")]
    public float wheelRadius = 0.34f;
    public float maxSteerAngle = 28f;

    [Header("Lighting Feedback")]
    public Material taillightMaterial;
    public Light leftBrakeLight;
    public Light rightBrakeLight;
    public Color normalTaillightEmission = new Color(1.2f, 0.08f, 0.08f, 1f);
    public Color brakeTaillightEmission = new Color(4.8f, 0.15f, 0.15f, 1f);

    [Header("Skid Trails")]
    public TrailRenderer leftSkidTrail;
    public TrailRenderer rightSkidTrail;

    private CarController carController;
    private Material runtimeTaillightMat;

    void Start()
    {
        carController = GetComponent<CarController>();
        SetupTaillightMaterial();
        SetupSkidTrails();
        SetupBrakeLights();
    }

    private void SetupTaillightMaterial()
    {
        // Find existing taillight material if not assigned in inspector
        if (taillightMaterial == null)
        {
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in renderers)
            {
                foreach (var mat in r.materials)
                {
                    if (mat != null && mat.name.Contains("Mat_Taillight_LED"))
                    {
                        runtimeTaillightMat = mat;
                        break;
                    }
                }
                if (runtimeTaillightMat != null) break;
            }
        }
        else
        {
            runtimeTaillightMat = taillightMaterial;
        }
    }

    private void SetupBrakeLights()
    {
        // Look for Lights parent or create glow lights
        Transform lightsTr = transform.Find("Lights");
        if (lightsTr == null)
        {
            var go = new GameObject("Lights");
            go.transform.SetParent(transform, false);
            lightsTr = go.transform;
        }

        if (leftBrakeLight == null)
        {
            var lb = new GameObject("BrakeLight_L");
            lb.transform.SetParent(lightsTr, false);
            lb.transform.localPosition = new Vector3(-0.65f, 0.72f, -2.35f);
            leftBrakeLight = lb.AddComponent<Light>();
            leftBrakeLight.type = LightType.Point;
            leftBrakeLight.range = 3.5f;
            leftBrakeLight.color = Color.red;
            leftBrakeLight.intensity = 0.4f;
            leftBrakeLight.enabled = false;
        }

        if (rightBrakeLight == null)
        {
            var rb = new GameObject("BrakeLight_R");
            rb.transform.SetParent(lightsTr, false);
            rb.transform.localPosition = new Vector3(0.65f, 0.72f, -2.35f);
            rightBrakeLight = rb.AddComponent<Light>();
            rightBrakeLight.type = LightType.Point;
            rightBrakeLight.range = 3.5f;
            rightBrakeLight.color = Color.red;
            rightBrakeLight.intensity = 0.4f;
            rightBrakeLight.enabled = false;
        }
    }

    private void SetupSkidTrails()
    {
        // Automatically create and configure rear wheel skid trails if not assigned
        if (leftSkidTrail == null && rearLeftMesh != null)
        {
            leftSkidTrail = CreateSkidTrail("SkidTrail_L", rearLeftMesh.parent != null ? rearLeftMesh.parent : rearLeftMesh);
        }
        if (rightSkidTrail == null && rearRightMesh != null)
        {
            rightSkidTrail = CreateSkidTrail("SkidTrail_R", rearRightMesh.parent != null ? rearRightMesh.parent : rearRightMesh);
        }
    }

    private TrailRenderer CreateSkidTrail(string name, Transform parent)
    {
        var trObj = new GameObject(name);
        trObj.transform.SetParent(parent, false);
        // Position at ground contact point of tire
        trObj.transform.localPosition = new Vector3(0f, -wheelRadius + 0.04f, 0f);

        TrailRenderer tr = trObj.AddComponent<TrailRenderer>();
        tr.time = 2.5f;
        tr.startWidth = 0.22f;
        tr.endWidth = 0.22f;
        tr.autodestruct = false;
        tr.emitting = false;

        // Dark asphalt rubber tire track color
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.1f, 0.1f, 0.1f), 0f),
                new GradientColorKey(new Color(0.12f, 0.12f, 0.12f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.85f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        tr.colorGradient = grad;

        // Simple default material
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null) tr.material = new Material(shader);

        return tr;
    }

    void Update()
    {
        if (carController == null) return;

        UpdateSteering();
        UpdateWheelRotation();
        UpdateBrakeLights();
        UpdateSkidTrails();
    }

    private void UpdateSteering()
    {
        // Front steering pivots rotate smoothly on Y axis
        float steerAngle = carController.CurrentSteerAngle;
        if (frontLeftPivot != null)
            frontLeftPivot.localRotation = Quaternion.Euler(0f, steerAngle, 0f);
        if (frontRightPivot != null)
            frontRightPivot.localRotation = Quaternion.Euler(0f, steerAngle, 0f);
    }

    private void UpdateWheelRotation()
    {
        // Wheel rolling rotation around local X axis matched to real physical speed
        float forwardVelocity = carController.ForwardVelocity;
        if (Mathf.Abs(forwardVelocity) > 0.02f)
        {
            float rollDelta = (forwardVelocity * Time.deltaTime / wheelRadius) * Mathf.Rad2Deg;

            if (frontLeftMesh != null) frontLeftMesh.Rotate(Vector3.right, rollDelta, Space.Self);
            if (frontRightMesh != null) frontRightMesh.Rotate(Vector3.right, rollDelta, Space.Self);
            if (rearLeftMesh != null) rearLeftMesh.Rotate(Vector3.right, rollDelta, Space.Self);
            if (rearRightMesh != null) rearRightMesh.Rotate(Vector3.right, rollDelta, Space.Self);
        }
    }

    private void UpdateBrakeLights()
    {
        bool braking = carController.IsBraking || carController.IsReversing;

        if (runtimeTaillightMat != null)
        {
            Color targetEmission = braking ? brakeTaillightEmission : normalTaillightEmission;
            runtimeTaillightMat.SetColor("_EmissionColor", targetEmission);
            runtimeTaillightMat.EnableKeyword("_EMISSION");
        }

        if (leftBrakeLight != null)
        {
            leftBrakeLight.enabled = braking;
            leftBrakeLight.intensity = braking ? 1.6f : 0.3f;
        }
        if (rightBrakeLight != null)
        {
            rightBrakeLight.enabled = braking;
            rightBrakeLight.intensity = braking ? 1.6f : 0.3f;
        }
    }

    private void UpdateSkidTrails()
    {
        // Emit skidmarks when handbraking or drifting at speed
        bool isSkidding = carController.IsHandbraking && (carController.CurrentSpeedKmh > 12f);

        if (leftSkidTrail != null) leftSkidTrail.emitting = isSkidding;
        if (rightSkidTrail != null) rightSkidTrail.emitting = isSkidding;
    }

    /// <summary>
    /// Toggle front headlights with H key or UI button
    /// </summary>
    public void ToggleHeadlights()
    {
        Transform lightsTr = transform.Find("Lights");
        if (lightsTr == null) return;

        var headL = lightsTr.Find("Headlight_L");
        var headR = lightsTr.Find("Headlight_R");
        if (headL != null)
        {
            var l = headL.GetComponent<Light>();
            if (l != null) l.enabled = !l.enabled;
        }
        if (headR != null)
        {
            var l = headR.GetComponent<Light>();
            if (l != null) l.enabled = !l.enabled;
        }
    }
}
