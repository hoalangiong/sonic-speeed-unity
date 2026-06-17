using UnityEngine;
using UnityEditor;

/// <summary>
/// Car data — stats for each selectable car.
/// </summary>
[System.Serializable]
public class CarData
{
    public string name;
    public string modelPath; // FBX path in Assets/
    public float maxSpeed;
    public float acceleration;
    public float handling;
    public Color color;

    public CarData(string name, string modelPath, float maxSpeed, float acceleration, float handling, Color color)
    {
        this.name = name;
        this.modelPath = modelPath;
        this.maxSpeed = maxSpeed;
        this.acceleration = acceleration;
        this.handling = handling;
        this.color = color;
    }
}

/// <summary>
/// Car selection screen — shown before race.
/// Player chooses from available cars, then race starts.
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
    private bool previewsCreated = false;

    void Start()
    {
        CreateCarPreviews();
    }

    void CreateCarPreviews()
    {
        previewCars = new GameObject[AvailableCars.Length];
        float spacing = 5f;
        float startX = -(AvailableCars.Length - 1) * spacing / 2f;

        for (int i = 0; i < AvailableCars.Length; i++)
        {
            var carData = AvailableCars[i];
            var prefab = UnityEngine.Resources.Load<GameObject>(carData.modelPath);

            // Try loading via path
            #if UNITY_EDITOR
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(carData.modelPath);
            #endif

            if (prefab != null)
            {
                var preview = Instantiate(prefab);
                preview.name = $"Preview_{carData.name}";
                preview.transform.position = new Vector3(startX + i * spacing, 0.5f, 15f);
                preview.transform.rotation = Quaternion.Euler(0, 160, 0);
                preview.transform.localScale = Vector3.one * 1.5f;

                // Remove physics from preview
                foreach (var col in preview.GetComponentsInChildren<Collider>())
                    Destroy(col);
                var rb = preview.GetComponent<Rigidbody>();
                if (rb != null) Destroy(rb);

                // Color it
                foreach (var renderer in preview.GetComponentsInChildren<Renderer>())
                {
                    foreach (var mat in renderer.materials)
                    {
                        mat.color = carData.color;
                        mat.SetFloat("_Metallic", 0.85f);
                        mat.SetFloat("_Glossiness", 0.9f);
                    }
                }

                previewCars[i] = preview;
            }
        }
        previewsCreated = true;
    }

    void Update()
    {
        if (!previewsCreated || raceStarted) return;

        // Rotate selected car, dim others
        for (int i = 0; i < previewCars.Length; i++)
        {
            if (previewCars[i] == null) continue;

            if (i == SelectedCarIndex)
            {
                previewCars[i].transform.Rotate(0, 30f * Time.deltaTime, 0);
                previewCars[i].SetActive(true);
            }
            else
            {
                previewCars[i].SetActive(true);
            }
        }
    }

    void DestroyPreviews()
    {
        if (previewCars != null)
        {
            foreach (var car in previewCars)
                if (car != null) Destroy(car);
        }
    }

    void OnGUI()
    {
        if (raceStarted) return;

        float sw = Screen.width;
        float sh = Screen.height;

        // Dark background
        GUI.color = new Color(0, 0, 0, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 40;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.yellow;
        GUI.Label(new Rect(0, sh * 0.05f, sw, 50), "SELECT YOUR CAR", titleStyle);

        // Car cards
        float cardWidth = sw * 0.28f;
        float cardHeight = sh * 0.6f;
        float startX = (sw - cardWidth * 3 - 40) / 2;
        float cardY = sh * 0.18f;

        for (int i = 0; i < AvailableCars.Length; i++)
        {
            var car = AvailableCars[i];
            float x = startX + i * (cardWidth + 20);

            // Card background
            bool selected = (i == SelectedCarIndex);
            GUI.color = selected ? new Color(0.2f, 0.4f, 0.8f, 0.9f) : new Color(0.15f, 0.15f, 0.2f, 0.9f);
            GUI.DrawTexture(new Rect(x, cardY, cardWidth, cardHeight), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Border if selected
            if (selected)
            {
                GUI.color = Color.yellow;
                GUI.DrawTexture(new Rect(x - 2, cardY - 2, cardWidth + 4, 4), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(x - 2, cardY + cardHeight - 2, cardWidth + 4, 4), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(x - 2, cardY, 4, cardHeight), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(x + cardWidth - 2, cardY, 4, cardHeight), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            // Car name
            GUIStyle nameStyle = new GUIStyle();
            nameStyle.fontSize = 18;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = car.color;
            GUI.Label(new Rect(x, cardY + 10, cardWidth, 30), car.name, nameStyle);

            // Color preview box
            GUI.color = car.color;
            GUI.DrawTexture(new Rect(x + cardWidth * 0.2f, cardY + 50, cardWidth * 0.6f, 60), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Stats
            GUIStyle statStyle = new GUIStyle();
            statStyle.fontSize = 14;
            statStyle.normal.textColor = Color.white;

            float statY = cardY + 130;
            GUI.Label(new Rect(x + 15, statY, cardWidth, 25), $"Speed:  {GetStars(car.maxSpeed, 250f)}", statStyle);
            GUI.Label(new Rect(x + 15, statY + 25, cardWidth, 25), $"Accel:  {GetStars(car.acceleration, 50f)}", statStyle);
            GUI.Label(new Rect(x + 15, statY + 50, cardWidth, 25), $"Handle: {GetStars(car.handling, 100f)}", statStyle);

            // Select button
            if (GUI.Button(new Rect(x + 10, cardY + cardHeight - 50, cardWidth - 20, 40), selected ? "✓ SELECTED" : "SELECT"))
            {
                SelectedCarIndex = i;
            }
        }

        // START RACE button
        GUIStyle startStyle = new GUIStyle(GUI.skin.button);
        startStyle.fontSize = 28;
        startStyle.fontStyle = FontStyle.Bold;

        if (GUI.Button(new Rect(sw / 2 - 120, sh * 0.88f, 240, 55), "START RACE"))
        {
            raceStarted = true;

            // Apply selected car stats to player vehicle
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
            }

            DestroyPreviews();
            gameObject.SetActive(false);
        }
    }

    string GetStars(float value, float max)
    {
        int stars = Mathf.RoundToInt((value / max) * 5);
        return new string('★', stars) + new string('☆', 5 - stars);
    }
}
