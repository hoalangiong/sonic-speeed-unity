using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Car selector — shows REAL 3D cars in scene, player picks one.
/// No RenderTexture — just places cars in front of main camera.
/// </summary>
public class CarSelector : MonoBehaviour
{
    public static CarData[] AvailableCars = new CarData[]
    {
        new CarData("Lamborghini", "Assets/Models/lamborghini.fbx", 220f, 40f, 80f, new Color(1f, 0.8f, 0f)),
        new CarData("Ferrari F40", "Assets/Models/ferrari/ferrari.fbx", 250f, 35f, 70f, new Color(0.9f, 0.1f, 0.1f)),
        new CarData("Porsche 911", "Assets/Models/porsche/porsche.fbx", 200f, 45f, 95f, new Color(0.9f, 0.9f, 0.9f)),
    };

    public static int SelectedCarIndex = 0;

    private bool done = false;
    private GameObject displayCar;
    private Camera mainCam;
    private Vector3 savedCamPos;
    private Quaternion savedCamRot;

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam != null)
        {
            savedCamPos = mainCam.transform.position;
            savedCamRot = mainCam.transform.rotation;
            // Point camera at display area
            mainCam.transform.position = new Vector3(0, 2f, -6f);
            mainCam.transform.rotation = Quaternion.Euler(10, 0, 0);
        }
        ShowCar(SelectedCarIndex);
    }

    void ShowCar(int index)
    {
        // Remove old display car
        if (displayCar != null) Destroy(displayCar);

        // Load model
        GameObject prefab = null;
        #if UNITY_EDITOR
        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AvailableCars[index].modelPath);
        #endif

        if (prefab != null)
        {
            displayCar = Instantiate(prefab);
            displayCar.name = "DisplayCar";
            displayCar.transform.position = new Vector3(0, 0.5f, 0);
            displayCar.transform.rotation = Quaternion.Euler(0, 135, 0);
            displayCar.transform.localScale = Vector3.one * 1.2f;

            // Remove physics
            foreach (var col in displayCar.GetComponentsInChildren<Collider>()) Destroy(col);
            var rb = displayCar.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);

            // Apply color
            foreach (var renderer in displayCar.GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    mat.color = AvailableCars[index].color;
                    mat.SetFloat("_Metallic", 0.85f);
                    mat.SetFloat("_Glossiness", 0.9f);
                }
            }
        }
    }

    void Update()
    {
        if (done) return;
        // Rotate display car
        if (displayCar != null)
            displayCar.transform.Rotate(0, 30f * Time.deltaTime, 0);
    }

    void OnGUI()
    {
        if (done) return;

        float sw = Screen.width;
        float sh = Screen.height;

        // Semi-transparent top bar
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh * 0.12f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(0, sh * 0.7f, sw, sh * 0.3f), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = (int)(sh * 0.05f);
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.yellow;
        GUI.Label(new Rect(0, sh * 0.02f, sw, sh * 0.07f), "CHỌN XE", titleStyle);

        // Car name
        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = (int)(sh * 0.04f);
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.alignment = TextAnchor.MiddleCenter;
        nameStyle.normal.textColor = AvailableCars[SelectedCarIndex].color;
        GUI.Label(new Rect(0, sh * 0.71f, sw, sh * 0.05f), AvailableCars[SelectedCarIndex].name, nameStyle);

        // Stats
        float barY = sh * 0.77f;
        float barW = sw * 0.5f;
        float barX = (sw - barW) / 2;
        var car = AvailableCars[SelectedCarIndex];
        DrawBar(barX, barY, barW, "Tốc độ", car.maxSpeed / 250f, Color.red);
        DrawBar(barX, barY + sh * 0.05f, barW, "Tăng tốc", car.acceleration / 50f, Color.green);
        DrawBar(barX, barY + sh * 0.1f, barW, "Xử lý", car.handling / 100f, Color.cyan);

        // Left/Right arrows to switch car
        GUIStyle arrowStyle = new GUIStyle(GUI.skin.button);
        arrowStyle.fontSize = (int)(sh * 0.06f);

        if (GUI.Button(new Rect(sw * 0.05f, sh * 0.4f, sw * 0.1f, sh * 0.1f), "◀", arrowStyle))
        {
            SelectedCarIndex = (SelectedCarIndex - 1 + AvailableCars.Length) % AvailableCars.Length;
            ShowCar(SelectedCarIndex);
        }
        if (GUI.Button(new Rect(sw * 0.85f, sh * 0.4f, sw * 0.1f, sh * 0.1f), "▶", arrowStyle))
        {
            SelectedCarIndex = (SelectedCarIndex + 1) % AvailableCars.Length;
            ShowCar(SelectedCarIndex);
        }

        // CONFIRM button
        GUIStyle goStyle = new GUIStyle(GUI.skin.button);
        goStyle.fontSize = (int)(sh * 0.035f);
        goStyle.fontStyle = FontStyle.Bold;
        if (GUI.Button(new Rect(sw / 2 - sw * 0.12f, sh * 0.9f, sw * 0.24f, sh * 0.07f), "CHỌN XE NÀY →", goStyle))
        {
            Confirm();
        }
    }

    void Confirm()
    {
        done = true;

        // Apply stats + color to player car
        var player = GameObject.Find("PlayerCar");
        if (player != null)
        {
            var vc = player.GetComponent<VehicleController>();
            if (vc != null)
            {
                var car = AvailableCars[SelectedCarIndex];
                vc.maxSpeed = car.maxSpeed;
                vc.acceleration = car.acceleration;
                vc.turnSpeed = car.handling;
            }
            foreach (var renderer in player.GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    mat.color = AvailableCars[SelectedCarIndex].color;
                    mat.SetFloat("_Metallic", 0.85f);
                    mat.SetFloat("_Glossiness", 0.9f);
                }
            }
        }

        // Cleanup
        if (displayCar != null) Destroy(displayCar);
        // Restore camera
        if (mainCam != null)
        {
            mainCam.transform.position = savedCamPos;
            mainCam.transform.rotation = savedCamRot;
        }

        gameObject.SetActive(false);
    }

    void DrawBar(float x, float y, float width, string label, float value, Color color)
    {
        float sh = Screen.height;
        GUIStyle s = new GUIStyle();
        s.fontSize = (int)(sh * 0.02f);
        s.normal.textColor = Color.white;
        GUI.Label(new Rect(x, y, width * 0.3f, sh * 0.03f), label, s);

        float barX = x + width * 0.3f;
        float barW = width * 0.7f;
        float barH = sh * 0.02f;
        GUI.color = new Color(0.2f, 0.2f, 0.2f);
        GUI.DrawTexture(new Rect(barX, y + sh * 0.005f, barW, barH), Texture2D.whiteTexture);
        GUI.color = color;
        GUI.DrawTexture(new Rect(barX, y + sh * 0.005f, barW * Mathf.Clamp01(value), barH), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
