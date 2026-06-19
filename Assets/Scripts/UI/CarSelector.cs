using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Car selector with 3D preview renders.
/// Creates a camera that renders each car model to a texture for display.
/// </summary>
public class CarSelector : MonoBehaviour
{
    public static CarData[] AvailableCars = new CarData[]
    {
        new CarData("Lamborghini Aventador", "Assets/Models/lamborghini.fbx", 220f, 40f, 80f, new Color(1f, 0.8f, 0f)),
        new CarData("Ferrari F40", "Assets/Models/ferrari/ferrari.fbx", 250f, 35f, 70f, new Color(0.9f, 0.1f, 0.1f)),
        new CarData("Porsche 911", "Assets/Models/porsche/porsche.fbx", 200f, 45f, 95f, new Color(0.9f, 0.9f, 0.9f)),
    };

    public static int SelectedCarIndex = 0;

    private bool raceStarted = false;
    private GameObject[] previewCars;
    private Camera previewCamera;
    private RenderTexture[] carTextures;
    private bool initialized = false;

    void Start()
    {
        SetupPreview();
    }

    void SetupPreview()
    {
        // Create preview camera (hidden, renders to texture)
        var camObj = new GameObject("PreviewCamera");
        camObj.transform.position = new Vector3(0, 2f, -6f);
        camObj.transform.rotation = Quaternion.Euler(10, 0, 0);
        previewCamera = camObj.AddComponent<Camera>();
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        previewCamera.cullingMask = 1 << 8; // Layer 8 for preview
        previewCamera.enabled = false; // Manual render

        // Create render textures + car previews
        carTextures = new RenderTexture[AvailableCars.Length];
        previewCars = new GameObject[AvailableCars.Length];

        for (int i = 0; i < AvailableCars.Length; i++)
        {
            carTextures[i] = new RenderTexture(256, 256, 16);

            // Load car model
            #if UNITY_EDITOR
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AvailableCars[i].modelPath);
            if (prefab != null)
            {
                var car = Instantiate(prefab);
                car.name = $"CarPreview_{i}";
                car.transform.position = new Vector3(i * 20f, -100f, 0); // Far below scene
                car.transform.rotation = Quaternion.Euler(0, 135, 0);
                car.transform.localScale = Vector3.one * 1.5f;

                // Set layer 8 for all renderers
                SetLayerRecursive(car, 8);

                // Apply car color
                foreach (var renderer in car.GetComponentsInChildren<Renderer>())
                {
                    foreach (var mat in renderer.materials)
                    {
                        mat.color = AvailableCars[i].color;
                        mat.SetFloat("_Metallic", 0.85f);
                        mat.SetFloat("_Glossiness", 0.9f);
                    }
                }

                // Remove colliders
                foreach (var col in car.GetComponentsInChildren<Collider>())
                    Destroy(col);

                previewCars[i] = car;

                // Render to texture
                RenderCarPreview(i);
            }
            #endif
        }

        // Add light for preview
        var lightObj = new GameObject("PreviewLight");
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.5f;
        light.cullingMask = 1 << 8;
        lightObj.transform.rotation = Quaternion.Euler(40, -30, 0);

        initialized = true;
    }

    void RenderCarPreview(int index)
    {
        if (previewCamera == null || previewCars[index] == null) return;

        previewCamera.targetTexture = carTextures[index];
        previewCamera.transform.position = previewCars[index].transform.position + new Vector3(3f, 1.5f, -5f);
        previewCamera.transform.LookAt(previewCars[index].transform.position + Vector3.up * 0.5f);
        previewCamera.Render();
        previewCamera.targetTexture = null;
    }

    void Update()
    {
        if (raceStarted || !initialized) return;

        // Rotate selected car preview
        if (previewCars[SelectedCarIndex] != null)
        {
            previewCars[SelectedCarIndex].transform.Rotate(0, 40f * Time.deltaTime, 0);
            RenderCarPreview(SelectedCarIndex);
        }
    }

    void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    void Cleanup()
    {
        if (previewCars != null)
            foreach (var car in previewCars)
                if (car != null) Destroy(car);
        if (previewCamera != null)
            Destroy(previewCamera.gameObject);
        // Find and destroy preview light
        var light = GameObject.Find("PreviewLight");
        if (light != null) Destroy(light);
    }

    void OnGUI()
    {
        if (raceStarted) return;

        float sw = Screen.width;
        float sh = Screen.height;

        // Dark background
        GUI.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = (int)(sh * 0.06f);
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.yellow;
        GUI.Label(new Rect(0, sh * 0.02f, sw, sh * 0.08f), "CHỌN XE", titleStyle);

        // Car cards with real 3D render
        float cardWidth = sw * 0.3f;
        float cardHeight = sh * 0.7f;
        float startX = (sw - cardWidth * 3 - sw * 0.04f) / 2;
        float cardY = sh * 0.12f;

        for (int i = 0; i < AvailableCars.Length; i++)
        {
            var car = AvailableCars[i];
            float x = startX + i * (cardWidth + sw * 0.02f);
            bool selected = (i == SelectedCarIndex);

            // Card background
            GUI.color = selected ? new Color(0.15f, 0.3f, 0.6f, 0.95f) : new Color(0.1f, 0.1f, 0.15f, 0.9f);
            GUI.DrawTexture(new Rect(x, cardY, cardWidth, cardHeight), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 3D Car render preview
            if (carTextures[i] != null)
            {
                GUI.DrawTexture(new Rect(x + 10, cardY + 10, cardWidth - 20, cardHeight * 0.5f), carTextures[i]);
            }

            // Selected border
            if (selected)
            {
                GUI.color = Color.yellow;
                GUI.DrawTexture(new Rect(x - 3, cardY - 3, cardWidth + 6, 3), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(x - 3, cardY + cardHeight, cardWidth + 6, 3), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(x - 3, cardY, 3, cardHeight + 3), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(x + cardWidth, cardY, 3, cardHeight + 3), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            // Car name
            GUIStyle nameStyle = new GUIStyle();
            nameStyle.fontSize = (int)(sh * 0.03f);
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = car.color;
            GUI.Label(new Rect(x, cardY + cardHeight * 0.52f, cardWidth, sh * 0.04f), car.name, nameStyle);

            // Stats bars
            float statY = cardY + cardHeight * 0.6f;
            float barWidth = cardWidth * 0.6f;
            float barX = x + (cardWidth - barWidth) / 2;

            DrawStatBar(barX, statY, barWidth, "Speed", car.maxSpeed / 250f, Color.red);
            DrawStatBar(barX, statY + sh * 0.06f, barWidth, "Accel", car.acceleration / 50f, Color.green);
            DrawStatBar(barX, statY + sh * 0.12f, barWidth, "Handle", car.handling / 100f, Color.cyan);

            // Select button
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = (int)(sh * 0.025f);
            btnStyle.fontStyle = FontStyle.Bold;
            if (GUI.Button(new Rect(x + cardWidth * 0.1f, cardY + cardHeight - sh * 0.06f, cardWidth * 0.8f, sh * 0.05f),
                selected ? "✓ SELECTED" : "CHỌN", btnStyle))
            {
                SelectedCarIndex = i;
            }
        }

        // START button
        GUIStyle startStyle = new GUIStyle(GUI.skin.button);
        startStyle.fontSize = (int)(sh * 0.04f);
        startStyle.fontStyle = FontStyle.Bold;
        if (GUI.Button(new Rect(sw / 2 - sw * 0.12f, sh * 0.9f, sw * 0.24f, sh * 0.07f), "TIẾP TỤC →", startStyle))
        {
            raceStarted = true;

            // Apply car stats + color
            var player = GameObject.Find("PlayerCar");
            if (player != null)
            {
                var vc = player.GetComponent<VehicleController>();
                if (vc != null)
                {
                    var carData = AvailableCars[SelectedCarIndex];
                    vc.maxSpeed = carData.maxSpeed;
                    vc.acceleration = carData.acceleration;
                    vc.turnSpeed = carData.handling;
                }

                var carColor = AvailableCars[SelectedCarIndex].color;
                foreach (var renderer in player.GetComponentsInChildren<Renderer>())
                {
                    foreach (var mat in renderer.materials)
                    {
                        mat.color = carColor;
                        mat.SetFloat("_Metallic", 0.85f);
                        mat.SetFloat("_Glossiness", 0.9f);
                    }
                }
            }

            Cleanup();
            gameObject.SetActive(false);
        }
    }

    void DrawStatBar(float x, float y, float width, string label, float value, Color barColor)
    {
        float sh = Screen.height;
        float barHeight = sh * 0.02f;

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = (int)(sh * 0.02f);
        labelStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x, y, width, sh * 0.025f), label, labelStyle);

        // Background
        GUI.color = new Color(0.2f, 0.2f, 0.2f);
        GUI.DrawTexture(new Rect(x, y + sh * 0.025f, width, barHeight), Texture2D.whiteTexture);

        // Fill
        GUI.color = barColor;
        GUI.DrawTexture(new Rect(x, y + sh * 0.025f, width * Mathf.Clamp01(value), barHeight), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
