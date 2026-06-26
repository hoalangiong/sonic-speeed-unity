using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// Auto-setup scene with imported 3D car model.
/// Finds any .glb/.fbx in Assets/Models/ and sets up complete racing scene.
/// Unity Editor → Tools → Sonic Speeed → Setup With Imported Model
/// </summary>
public class AutoSceneBuilder : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Sonic Speeed/Setup With Imported Model")]
    public static void BuildScene()
    {
        // Clean existing scene objects
        var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        var toDelete = new System.Collections.Generic.List<GameObject>();
        foreach (var go in allObjects)
        {
            if (go != null && go.name != "Main Camera" && go.name != "Directional Light")
                toDelete.Add(go);
        }
        for (int i = toDelete.Count - 1; i >= 0; i--)
        {
            if (toDelete[i] != null) DestroyImmediate(toDelete[i]);
        }

        SetupRealisticLighting();
        var car = SetupCar();
        SetupRoad();
        SetupCamera(car);

        Debug.Log("✅ Scene built with realistic setup! Press Play to drive.");
    }

    static void SetupRealisticLighting()
    {
        // Sun — bright and warm
        var sunObj = GameObject.Find("Directional Light");
        if (sunObj != null)
        {
            sunObj.transform.rotation = Quaternion.Euler(40, -30, 0);
            var light = sunObj.GetComponent<Light>();
            light.intensity = 2.0f;
            light.color = new Color(1f, 0.97f, 0.9f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.5f;
        }

        // Bright ambient — no dark areas
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.7f, 0.85f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.9f, 0.9f, 0.85f);
        RenderSettings.ambientGroundColor = new Color(0.4f, 0.5f, 0.3f);

        // Light fog for depth
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.75f, 0.85f, 0.95f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 150;
        RenderSettings.fogEndDistance = 800;

        // Fill light from opposite side
        var fillLight = new GameObject("FillLight");
        var fl = fillLight.AddComponent<Light>();
        fl.type = LightType.Directional;
        fl.intensity = 0.6f;
        fl.color = new Color(0.7f, 0.8f, 1f);
        fillLight.transform.rotation = Quaternion.Euler(30, 150, 0);
        fl.shadows = LightShadows.None;
    }

    static GameObject SetupCar()
    {
        GameObject carModel = null;

        // Find imported model in Assets/Models/
        string modelsPath = Path.Combine(Application.dataPath, "Models");
        Debug.Log($"[DEBUG] dataPath = {Application.dataPath}");
        Debug.Log($"[DEBUG] modelsPath = {modelsPath}");
        Debug.Log($"[DEBUG] Directory.Exists = {Directory.Exists(modelsPath)}");

        // Try loading model directly by known asset path
        string[] tryPaths = {
            "Assets/Models/lamborghini.fbx",
            "Assets/Models/ferrari/ferrari.fbx",
            "Assets/Models/porsche/porsche.fbx",
            "Assets/Models/lamborghini.glb",
            "Assets/Models/scene.gltf",
        };

        foreach (string tryPath in tryPaths)
        {
            Debug.Log($"[DEBUG] Trying: {tryPath}");
            var tryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(tryPath);
            if (tryPrefab != null)
            {
                Debug.Log($"✅ Loaded model from: {tryPath}");
                // Create parent object for physics
                carModel = new GameObject("PlayerCar");
                // Spawn car on track (first waypoint position)
                float startAngle = 0;
                float rx = 150f + Mathf.Sin(startAngle * 3) * 20f;
                float rz = 100f + Mathf.Cos(startAngle * 2) * 15f;
                float startX = Mathf.Cos(startAngle) * rx;
                float startZ = Mathf.Sin(startAngle) * rz;
                carModel.transform.position = new Vector3(startX, 1.5f, startZ);

                // Instantiate visual model as child, rotated 180 so nose faces +Z
                var visual = (GameObject)PrefabUtility.InstantiatePrefab(tryPrefab);
                if (visual == null) visual = Instantiate(tryPrefab);
                visual.name = "CarVisual";
                visual.transform.SetParent(carModel.transform, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.Euler(180, 0, 0); // Flip from lying on back to normal

                // Scale visual to ~4.5m
                var bounds = GetBounds(carModel);
                float targetLength = 4.5f;
                float currentLength = Mathf.Max(bounds.size.z, bounds.size.x);
                if (currentLength > 0.01f)
                {
                    float scale = targetLength / currentLength;
                    visual.transform.localScale = Vector3.one * scale;
                }

                ApplyCarMaterials(carModel);
                break;
            }
        }

        if (carModel == null)
        {
            var modelFiles = Directory.GetFiles(modelsPath, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".fbx") || f.EndsWith(".glb") || f.EndsWith(".gltf") || f.EndsWith(".obj") || f.EndsWith(".blend"))
                .Where(f => !f.EndsWith(".meta"))
                .ToArray();

            Debug.Log($"Found {modelFiles.Length} model files in Assets/Models/");
            foreach (var mf in modelFiles) Debug.Log($"  → {mf}");

            if (modelFiles.Length > 0)
            {
                // Convert absolute path to Unity relative "Assets/..." path
                string fullPath = modelFiles[0].Replace("\\", "/");
                string assetPath = "Assets" + fullPath.Substring(Application.dataPath.Replace("\\", "/").Length);
                Debug.Log($"Loading model: {assetPath}");
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                Debug.Log($"Prefab loaded: {(prefab != null ? prefab.name : "NULL")}");
                if (prefab != null)
                {
                    carModel = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    if (carModel == null) carModel = Instantiate(prefab);
                    carModel.name = "PlayerCar";
                    // Spawn car on track (first waypoint position)
                float startAngle = 0;
                float rx = 150f + Mathf.Sin(startAngle * 3) * 20f;
                float rz = 100f + Mathf.Cos(startAngle * 2) * 15f;
                float startX = Mathf.Cos(startAngle) * rx;
                float startZ = Mathf.Sin(startAngle) * rz;
                carModel.transform.position = new Vector3(startX, 1.5f, startZ);
                    carModel.transform.rotation = Quaternion.identity;

                    // Auto-scale model to ~4.5m length
                    var bounds = GetBounds(carModel);
                    float targetLength = 4.5f;
                    float currentLength = bounds.size.z > bounds.size.x ? bounds.size.z : bounds.size.x;
                    if (currentLength > 0)
                    {
                        float scale = targetLength / currentLength;
                        carModel.transform.localScale = Vector3.one * scale;
                    }

                    // Apply realistic car materials
                    ApplyCarMaterials(carModel);

                    Debug.Log($"✅ Car model loaded: {assetPath}");
                }
            }
        }

        // Fallback: create placeholder if no model found
        if (carModel == null)
        {
            carModel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            carModel.name = "PlayerCar";
            carModel.transform.position = new Vector3(0, 1f, 0);
            carModel.transform.localScale = new Vector3(2f, 0.6f, 4.5f);

            var r = carModel.GetComponent<Renderer>();
            r.material = CreateMetallicPaint(new Color(1f, 0.8f, 0f));
            Debug.LogWarning("⚠️ No model found in Assets/Models/. Using placeholder cube. Drop a .glb/.fbx there and re-run.");
        }

        // Physics — VehicleController uses CharacterController internally, no Rigidbody needed
        // Remove any existing colliders/rigidbody from model
        foreach (var col in carModel.GetComponentsInChildren<Collider>())
            DestroyImmediate(col);
        var existingRb = carModel.GetComponent<Rigidbody>();
        if (existingRb != null) DestroyImmediate(existingRb);

        // Scripts — VehicleController.Start() adds CharacterController automatically
        var vc = carModel.AddComponent<VehicleController>();
        carModel.AddComponent<NitroSystem>();
        carModel.AddComponent<DriftScoring>();

        return carModel;
    }

    static void SetupRoad()
    {
        // Remove old road objects
        var oldRoad = GameObject.Find("Road");
        if (oldRoad != null) DestroyImmediate(oldRoad);
        var oldGrass = GameObject.Find("Grass");
        if (oldGrass != null) DestroyImmediate(oldGrass);
        var oldMarkings = GameObject.Find("RoadMarkings");
        if (oldMarkings != null) DestroyImmediate(oldMarkings);

        // Create road using many segments
        var roadParent = new GameObject("Road");
        int segments = 80;
        float roadWidth = 20f; // Wider road — easier to stay on

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2;
            float nextAngle = (float)(i + 1) / segments * Mathf.PI * 2;

            Vector3 pos = GetTrackPosition(angle);
            Vector3 nextPos = GetTrackPosition(nextAngle);
            Vector3 dir = (nextPos - pos).normalized;
            float segLength = Vector3.Distance(pos, nextPos);

            var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.name = $"RoadSeg_{i}";
            seg.transform.parent = roadParent.transform;
            seg.transform.position = (pos + nextPos) / 2 + Vector3.down * 0.5f;
            seg.transform.localScale = new Vector3(roadWidth, 1f, segLength + 0.5f);
            seg.transform.rotation = Quaternion.LookRotation(dir);

            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.15f, 0.15f, 0.17f);
            mat.SetFloat("_Glossiness", 0.6f);
            seg.GetComponent<Renderer>().material = mat;

            // No bounce
            var physMat = new PhysicsMaterial();
            physMat.bounciness = 0;
            physMat.dynamicFriction = 0.8f;
            physMat.staticFriction = 0.8f;
            physMat.bounceCombine = PhysicsMaterialCombine.Minimum;
            seg.GetComponent<Collider>().material = physMat;
        }

        // Grass ground
        var grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grass.name = "Grass";
        grass.transform.position = new Vector3(0, -1.5f, 0);
        grass.transform.localScale = new Vector3(500f, 1f, 500f);
        var grassMat = new Material(Shader.Find("Standard"));
        grassMat.color = new Color(0.2f, 0.45f, 0.12f);
        grassMat.SetFloat("_Glossiness", 0.1f);
        grass.GetComponent<Renderer>().material = grassMat;

        // Road markings — yellow center dashes
        var markParent = new GameObject("RoadMarkings");
        for (int i = 0; i < segments * 2; i += 2)
        {
            float angle = (float)i / (segments * 2) * Mathf.PI * 2;
            Vector3 pos = GetTrackPosition(angle);
            Vector3 nextPos = GetTrackPosition(angle + 0.05f);
            Vector3 dir = (nextPos - pos).normalized;

            var mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mark.name = "CenterLine";
            mark.transform.parent = markParent.transform;
            mark.transform.position = pos + Vector3.up * 0.01f;
            mark.transform.rotation = Quaternion.LookRotation(dir);
            mark.transform.localScale = new Vector3(0.2f, 0.02f, 3f);
            var markMat = new Material(Shader.Find("Standard"));
            markMat.color = new Color(1f, 0.85f, 0f);
            markMat.EnableKeyword("_EMISSION");
            markMat.SetColor("_EmissionColor", new Color(0.3f, 0.25f, 0f));
            mark.GetComponent<Renderer>().material = markMat;
            DestroyImmediate(mark.GetComponent<Collider>());
        }
    }

    /// <summary>Get position on track at given angle (0 to 2*PI)</summary>
    static Vector3 GetTrackPosition(float angle)
    {
        // Check if Highway (straight) track is selected
        // Highway = trackScaleZ < 50 (very narrow oval = almost straight)
        float rx = 150f;
        float rz = 100f;

        // Use stored track dimensions if available
        if (TrackSelector.AvailableTracks != null && TrackSelector.SelectedTrackIndex < TrackSelector.AvailableTracks.Length)
        {
            var track = TrackSelector.AvailableTracks[TrackSelector.SelectedTrackIndex];
            rx = track.trackScaleX;
            rz = track.trackScaleZ;
        }

        // If rz < 50 = highway mode (almost straight, very gentle curves)
        if (rz < 50f)
        {
            // Long straight road with very gentle S-curves
            float z = (angle / (Mathf.PI * 2)) * rx * 4f - rx * 2f; // -600 to +600
            float x = Mathf.Sin(angle * 0.5f) * 5f; // Very gentle curve (5m max deviation)
            return new Vector3(x, 0, z);
        }
        else
        {
            // Oval circuit
            float x = Mathf.Cos(angle) * rx + Mathf.Sin(angle * 3) * 20f;
            float z = Mathf.Sin(angle) * rz + Mathf.Cos(angle * 2) * 15f;
            return new Vector3(x, 0, z);
        }
    }

    // SetupEnvironment removed — trees spawn at runtime via RuntimeTreeSpawner

    static void SetupCamera(GameObject car)
    {
        var cam = Camera.main;
        if (cam == null) return;

        var chase = cam.gameObject.GetComponent<ChaseCamera>();
        if (chase == null) chase = cam.gameObject.AddComponent<ChaseCamera>();
        chase.target = car.transform;
        chase.distance = 6;
        chase.height = 2.5f;
        cam.fieldOfView = 65;
        cam.farClipPlane = 2000;
        cam.transform.position = car.transform.position + new Vector3(0, 3, -8);
    }

    static void ApplyCarMaterials(GameObject car)
    {
        foreach (var renderer in car.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                if (mat == null) continue;
                // Boost metallic/glossiness for all car parts
                mat.SetFloat("_Metallic", Mathf.Max(mat.GetFloat("_Metallic"), 0.5f));
                mat.SetFloat("_Glossiness", Mathf.Max(mat.GetFloat("_Glossiness"), 0.7f));
            }
        }
    }

    static Bounds GetBounds(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.one);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    static Material CreateMetallicPaint(Color color)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.SetFloat("_Metallic", 0.85f);
        mat.SetFloat("_Glossiness", 0.9f);
        return mat;
    }
#endif
}
