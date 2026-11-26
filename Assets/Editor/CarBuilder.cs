using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

[InitializeOnLoad]
public class CarBuilder
{
    private const string PREF_KEY = "SportsSedan_Mercedes_CLS_v2";

    static CarBuilder()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorPrefs.GetBool(PREF_KEY, false))
            {
                BuildCompleteCar();
                EditorPrefs.SetBool(PREF_KEY, true);
            }
        };
    }

    [MenuItem("Tools/Build Modern Sports Sedan")]
    public static void BuildCompleteCar()
    {
        Debug.Log("<color=cyan>[CarBuilder]</color> Starting Mercedes-AMG CLS Sports Coupe Generation...");

        // 1. Ensure Directories exist
        EnsureDirectory("Assets/Materials/Car");
        EnsureDirectory("Assets/Prefabs");
        EnsureDirectory("Assets/Models");
        EnsureDirectory("Assets/Textures");

        // 2. Create / Update Materials
        Dictionary<string, Material> materials = CreateOrUpdateMaterials();

        // 3. Re-import models to ensure meshes and materials are ready
        AssetDatabase.ImportAsset("Assets/Textures/LicensePlate_AMGM0019.png", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Models/SportsSedan_Body.obj", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Models/SportsSedan_Wheel_Right.obj", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/Models/SportsSedan_Wheel_Left.obj", ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        // 4. Open MainScene
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Single);

        // 5. Build Car GameObject hierarchy
        GameObject carRoot = SetupCarHierarchy(materials);

        // 6. Setup Camera
        SetupCamera(carRoot);

        // 7. Save Scene & Prefab
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // Save as clean Prefab
        string prefabPath = "Assets/Prefabs/SportsSedan.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(carRoot, prefabPath, InteractionMode.AutomatedAction);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=green>[CarBuilder]</color> Mercedes-AMG CLS Sports Coupe successfully built, materials assigned, and scene saved!");
    }

    private static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folderName = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }

    public static Dictionary<string, Material> CreateOrUpdateMaterials()
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");
        if (litShader == null) litShader = Shader.Find("Diffuse");

        var dict = new Dictionary<string, Material>();

        // 1. Obsidian Black Metallic Body Paint
        dict["Mat_Body_GlossyBlack"] = GetOrCreateMat("Mat_Body_GlossyBlack", litShader,
            new Color(0.015f, 0.015f, 0.018f, 1f), metallic: 0.94f, smoothness: 0.96f);

        // 2. Tinted Smoked Obsidian Glass
        dict["Mat_Glass_Tinted"] = GetOrCreateMat("Mat_Glass_Tinted", litShader,
            new Color(0.04f, 0.05f, 0.07f, 0.95f), metallic: 0.15f, smoothness: 0.98f);

        // 3. Machined Sports Alloy Rim (Diamond Cut Face)
        dict["Mat_Rim_Alloy"] = GetOrCreateMat("Mat_Rim_Alloy", litShader,
            new Color(0.88f, 0.89f, 0.91f, 1f), metallic: 0.96f, smoothness: 0.92f);

        // 4. Inner Black / Titanium Spoke Pockets (AMG Bi-color)
        dict["Mat_Rim_InnerBlack"] = GetOrCreateMat("Mat_Rim_InnerBlack", litShader,
            new Color(0.03f, 0.03f, 0.035f, 1f), metallic: 0.92f, smoothness: 0.90f);

        // 5. Dark Satin Tire Rubber
        dict["Mat_Tire_Rubber"] = GetOrCreateMat("Mat_Tire_Rubber", litShader,
            new Color(0.11f, 0.11f, 0.11f, 1f), metallic: 0.0f, smoothness: 0.22f);

        // 6. Silver Alloy AMG Brake Caliper
        dict["Mat_Brake_Caliper"] = GetOrCreateMat("Mat_Brake_Caliper", litShader,
            new Color(0.78f, 0.80f, 0.82f, 1f), metallic: 0.90f, smoothness: 0.85f);

        // 7. Machined Steel Cross-drilled Brake Rotor
        dict["Mat_Brake_Disc"] = GetOrCreateMat("Mat_Brake_Disc", litShader,
            new Color(0.55f, 0.56f, 0.58f, 1f), metallic: 0.94f, smoothness: 0.72f);

        // 8. Dark Aerodynamic Front Splitter, Air Intakes & Diffuser
        dict["Mat_Carbon_Trim"] = GetOrCreateMat("Mat_Carbon_Trim", litShader,
            new Color(0.06f, 0.06f, 0.06f, 1f), metallic: 0.25f, smoothness: 0.40f);

        // 9. Multibeam LED Torch DRL Headlight (Emissive Cool White)
        dict["Mat_Headlight_LED"] = GetOrCreateMat("Mat_Headlight_LED", litShader,
            new Color(0.95f, 0.98f, 1.0f, 1f), metallic: 0.2f, smoothness: 0.95f, emission: new Color(2.5f, 2.7f, 3.2f, 1f));

        // 10. Radiant Crimson LED Taillight
        dict["Mat_Taillight_LED"] = GetOrCreateMat("Mat_Taillight_LED", litShader,
            new Color(0.85f, 0.05f, 0.05f, 1f), metallic: 0.2f, smoothness: 0.90f, emission: new Color(2.8f, 0.15f, 0.15f, 1f));

        // 11. Polished Mirror Chrome (Star Emblem, Grille Twin Louvres, Window Trim)
        dict["Mat_Chrome"] = GetOrCreateMat("Mat_Chrome", litShader,
            new Color(0.96f, 0.97f, 0.98f, 1f), metallic: 1.0f, smoothness: 0.98f);

        // 12. Reflective Mirror Face
        dict["Mat_Mirror_Glass"] = GetOrCreateMat("Mat_Mirror_Glass", litShader,
            new Color(0.92f, 0.94f, 0.97f, 1f), metallic: 1.0f, smoothness: 0.98f);

        // 13. Dark Charcoal Interior (Dashboard & MBUX Cockpit)
        dict["Mat_Interior"] = GetOrCreateMat("Mat_Interior", litShader,
            new Color(0.10f, 0.10f, 0.12f, 1f), metallic: 0.05f, smoothness: 0.30f);

        // 14. European License Plate ("AM GM 0019")
        Material plateMat = GetOrCreateMat("Mat_License_Plate", litShader,
            Color.white, metallic: 0.05f, smoothness: 0.70f);
        Texture2D plateTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/LicensePlate_AMGM0019.png");
        if (plateTex != null)
        {
            if (plateMat.HasProperty("_BaseMap")) plateMat.SetTexture("_BaseMap", plateTex);
            if (plateMat.HasProperty("_MainTex")) plateMat.SetTexture("_MainTex", plateTex);
        }
        dict["Mat_License_Plate"] = plateMat;

        return dict;
    }

    private static Material GetOrCreateMat(string matName, Shader shader, Color baseColor, float metallic, float smoothness, Color? emission = null)
    {
        string path = $"Assets/Materials/Car/{matName}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = shader;
        }

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", baseColor);

        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

        if (emission.HasValue)
        {
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emission.Value);
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
        }

        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static GameObject SetupCarHierarchy(Dictionary<string, Material> materials)
    {
        // 1. Locate existing CarBody or create root
        GameObject car = GameObject.Find("CarBody");
        if (car == null) car = GameObject.Find("SportsSedan");
        if (car == null)
        {
            car = new GameObject("SportsSedan");
        }
        else
        {
            car.name = "SportsSedan";
        }

        // Clean up old child objects to ensure fresh clean hierarchy
        var childrenToRemove = new List<GameObject>();
        for (int i = 0; i < car.transform.childCount; i++)
        {
            childrenToRemove.Add(car.transform.GetChild(i).gameObject);
        }
        foreach (var child in childrenToRemove)
        {
            Object.DestroyImmediate(child);
        }

        // Remove old primitive renderer/filter on root if any
        var oldFilter = car.GetComponent<MeshFilter>();
        if (oldFilter != null) Object.DestroyImmediate(oldFilter);
        var oldRenderer = car.GetComponent<MeshRenderer>();
        if (oldRenderer != null) Object.DestroyImmediate(oldRenderer);

        // Normalize root transform
        car.transform.position = new Vector3(0f, 0.02f, 0f);
        car.transform.rotation = Quaternion.identity;
        car.transform.localScale = Vector3.one;

        // Setup BoxCollider sized accurately for the sports coupe
        BoxCollider boxCol = car.GetComponent<BoxCollider>();
        if (boxCol == null) boxCol = car.AddComponent<BoxCollider>();
        boxCol.center = new Vector3(0f, 0.58f, 0.02f);
        boxCol.size = new Vector3(1.92f, 0.82f, 4.88f);

        // Setup Rigidbody
        Rigidbody rb = car.GetComponent<Rigidbody>();
        if (rb == null) rb = car.AddComponent<Rigidbody>();
        rb.mass = 1450f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        // Setup CarController
        CarController cc = car.GetComponent<CarController>();
        if (cc == null) cc = car.AddComponent<CarController>();
        cc.speed = 18f;
        cc.turnSpeed = 75f;

        // Setup CarVisuals
        CarVisuals cv = car.GetComponent<CarVisuals>();
        if (cv == null) cv = car.AddComponent<CarVisuals>();

        // 2. Instantiate Car Body Mesh
        GameObject bodyAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/SportsSedan_Body.obj");
        GameObject bodyObj = Object.Instantiate(bodyAsset, car.transform);
        bodyObj.name = "Body";
        bodyObj.transform.localPosition = Vector3.zero;
        bodyObj.transform.localRotation = Quaternion.identity;
        bodyObj.transform.localScale = Vector3.one;

        ApplyMaterialsToRenderers(bodyObj, materials);

        // 3. Create Wheel Hierarchy
        GameObject wheelsParent = new GameObject("Wheels");
        wheelsParent.transform.SetParent(car.transform, false);

        GameObject wheelLeftAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/SportsSedan_Wheel_Left.obj");
        GameObject wheelRightAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/SportsSedan_Wheel_Right.obj");

        // Front Left Wheel
        var flPivot = new GameObject("Wheel_FL");
        flPivot.transform.SetParent(wheelsParent.transform, false);
        flPivot.transform.localPosition = new Vector3(-0.88f, 0.34f, 1.43f);
        var flMesh = Object.Instantiate(wheelLeftAsset, flPivot.transform);
        flMesh.name = "Mesh";
        flMesh.transform.localPosition = Vector3.zero;
        flMesh.transform.localRotation = Quaternion.identity;
        flMesh.transform.localScale = Vector3.one;
        ApplyMaterialsToRenderers(flMesh, materials);

        // Front Right Wheel
        var frPivot = new GameObject("Wheel_FR");
        frPivot.transform.SetParent(wheelsParent.transform, false);
        frPivot.transform.localPosition = new Vector3(0.88f, 0.34f, 1.43f);
        var frMesh = Object.Instantiate(wheelRightAsset, frPivot.transform);
        frMesh.name = "Mesh";
        frMesh.transform.localPosition = Vector3.zero;
        frMesh.transform.localRotation = Quaternion.identity;
        frMesh.transform.localScale = Vector3.one;
        ApplyMaterialsToRenderers(frMesh, materials);

        // Rear Left Wheel
        var rlPivot = new GameObject("Wheel_RL");
        rlPivot.transform.SetParent(wheelsParent.transform, false);
        rlPivot.transform.localPosition = new Vector3(-0.88f, 0.34f, -1.43f);
        var rlMesh = Object.Instantiate(wheelLeftAsset, rlPivot.transform);
        rlMesh.name = "Mesh";
        rlMesh.transform.localPosition = Vector3.zero;
        rlMesh.transform.localRotation = Quaternion.identity;
        rlMesh.transform.localScale = Vector3.one;
        ApplyMaterialsToRenderers(rlMesh, materials);

        // Rear Right Wheel
        var rrPivot = new GameObject("Wheel_RR");
        rrPivot.transform.SetParent(wheelsParent.transform, false);
        rrPivot.transform.localPosition = new Vector3(0.88f, 0.34f, -1.43f);
        var rrMesh = Object.Instantiate(wheelRightAsset, rrPivot.transform);
        rrMesh.name = "Mesh";
        rrMesh.transform.localPosition = Vector3.zero;
        rrMesh.transform.localRotation = Quaternion.identity;
        rrMesh.transform.localScale = Vector3.one;
        ApplyMaterialsToRenderers(rrMesh, materials);

        // Hook up CarVisuals references
        cv.frontLeftPivot = flPivot.transform;
        cv.frontRightPivot = frPivot.transform;
        cv.frontLeftMesh = flMesh.transform;
        cv.frontRightMesh = frMesh.transform;
        cv.rearLeftMesh = rlMesh.transform;
        cv.rearRightMesh = rrMesh.transform;
        cv.maxSteerAngle = 28f;
        cv.wheelRadius = 0.34f;

        // 4. Headlights (Physical Spotlights for nighttime driving)
        GameObject lightsParent = new GameObject("Lights");
        lightsParent.transform.SetParent(car.transform, false);

        CreateHeadlight(lightsParent, "Headlight_L", new Vector3(-0.70f, 0.62f, 2.28f));
        CreateHeadlight(lightsParent, "Headlight_R", new Vector3(0.70f, 0.62f, 2.28f));

        return car;
    }

    private static void CreateHeadlight(GameObject parent, string name, Vector3 pos)
    {
        GameObject lightObj = new GameObject(name);
        lightObj.transform.SetParent(parent.transform, false);
        lightObj.transform.localPosition = pos;
        lightObj.transform.localRotation = Quaternion.Euler(5f, 0f, 0f);

        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Spot;
        light.range = 32f;
        light.spotAngle = 48f;
        light.innerSpotAngle = 32f;
        light.intensity = 2.5f;
        light.color = new Color(0.95f, 0.98f, 1.0f);
        light.shadows = LightShadows.Soft;
    }

    private static void ApplyMaterialsToRenderers(GameObject root, Dictionary<string, Material> materials)
    {
        MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in renderers)
        {
            Material[] sharedMats = mr.sharedMaterials;
            for (int i = 0; i < sharedMats.Length; i++)
            {
                if (sharedMats[i] != null)
                {
                    string cleanName = sharedMats[i].name.Replace(" (Instance)", "");
                    if (materials.ContainsKey(cleanName))
                    {
                        sharedMats[i] = materials[cleanName];
                    }
                }
            }
            mr.sharedMaterials = sharedMats;
        }
    }

    private static void SetupCamera(GameObject target)
    {
        GameObject camObj = GameObject.FindWithTag("MainCamera");
        if (camObj == null)
        {
            Camera cam = Object.FindFirstObjectByType<Camera>();
            if (cam != null) camObj = cam.gameObject;
        }

        if (camObj != null)
        {
            camObj.transform.position = new Vector3(0f, 2.1f, -5.6f);
            camObj.transform.rotation = Quaternion.Euler(13f, 0f, 0f);

            var follower = camObj.GetComponent<CarCameraController>();
            if (follower == null) follower = camObj.AddComponent<CarCameraController>();
            follower.target = target.transform;
            follower.offset = new Vector3(0f, 2.1f, -5.6f);
            follower.lookAtHeightOffset = 0.95f;
            follower.positionSmoothSpeed = 10f;
            follower.rotationSmoothSpeed = 12f;
        }
    }
}
