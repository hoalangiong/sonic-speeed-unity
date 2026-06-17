using UnityEngine;

/// <summary>
/// Track data — different circuits with unique vibes.
/// </summary>
[System.Serializable]
public class TrackData
{
    public string name;
    public string description;
    public Color skyColor;
    public Color groundColor;
    public Color roadColor;
    public Color fogColor;
    public float trackScaleX; // Oval X radius
    public float trackScaleZ; // Oval Z radius
    public bool hasRamps;
    public bool isNight;

    public TrackData(string name, string description, Color sky, Color ground, Color road, Color fog,
                     float scaleX, float scaleZ, bool ramps, bool night)
    {
        this.name = name;
        this.description = description;
        this.skyColor = sky;
        this.groundColor = ground;
        this.roadColor = road;
        this.fogColor = fog;
        this.trackScaleX = scaleX;
        this.trackScaleZ = scaleZ;
        this.hasRamps = ramps;
        this.isNight = night;
    }
}

/// <summary>
/// Track selection screen — shown after car select.
/// </summary>
public class TrackSelector : MonoBehaviour
{
    public static TrackData[] AvailableTracks = new TrackData[]
    {
        new TrackData(
            "Coastal Highway", "Đường ven biển, nắng đẹp",
            new Color(0.5f, 0.75f, 1f),           // sky blue
            new Color(0.2f, 0.45f, 0.12f),         // green grass
            new Color(0.15f, 0.15f, 0.17f),        // dark road
            new Color(0.7f, 0.8f, 0.9f),           // light fog
            150f, 100f, true, false
        ),
        new TrackData(
            "Tokyo Night", "Đường phố đêm, neon rực rỡ",
            new Color(0.02f, 0.02f, 0.08f),        // dark night sky
            new Color(0.1f, 0.1f, 0.12f),          // dark ground
            new Color(0.2f, 0.2f, 0.25f),          // grey road
            new Color(0.05f, 0.0f, 0.1f),          // purple fog
            120f, 80f, false, true
        ),
        new TrackData(
            "Desert Storm", "Sa mạc hoàng hôn, tốc độ max",
            new Color(0.95f, 0.6f, 0.3f),          // orange sky
            new Color(0.8f, 0.6f, 0.3f),           // sand
            new Color(0.2f, 0.18f, 0.15f),         // dusty road
            new Color(0.9f, 0.7f, 0.4f),           // sandy fog
            200f, 120f, true, false
        ),
        new TrackData(
            "Snow Alps", "Núi tuyết, đường trơn",
            new Color(0.7f, 0.8f, 0.95f),          // pale blue sky
            new Color(0.9f, 0.92f, 0.95f),         // snow white
            new Color(0.3f, 0.32f, 0.35f),         // wet road
            new Color(0.85f, 0.88f, 0.92f),        // white fog
            130f, 90f, false, false
        ),
        new TrackData(
            "Neon Circuit", "Tron-style, đường phát sáng",
            new Color(0.0f, 0.0f, 0.05f),          // black
            new Color(0.02f, 0.02f, 0.05f),        // near black
            new Color(0.0f, 0.1f, 0.2f),           // dark blue road
            new Color(0.0f, 0.05f, 0.1f),          // dark fog
            160f, 110f, true, true
        ),
    };

    public static int SelectedTrackIndex = 0;

    private bool confirmed = false;
    private bool waitingForCarSelect = true;

    void Update()
    {
        // Show track select after car select is done
        var carSelector = FindFirstObjectByType<CarSelector>();
        if (carSelector != null && carSelector.gameObject.activeSelf)
            waitingForCarSelect = true;
        else
            waitingForCarSelect = false;
    }

    void OnGUI()
    {
        if (waitingForCarSelect || confirmed) return;

        float sw = Screen.width;
        float sh = Screen.height;

        // Dark background
        GUI.color = new Color(0, 0, 0, 0.9f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 36;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.cyan;
        GUI.Label(new Rect(0, sh * 0.03f, sw, 45), "SELECT TRACK", titleStyle);

        // Track cards
        float cardWidth = sw / 5.5f;
        float cardHeight = sh * 0.65f;
        float startX = (sw - cardWidth * 5 - 40) / 2;
        float cardY = sh * 0.12f;

        for (int i = 0; i < AvailableTracks.Length; i++)
        {
            var track = AvailableTracks[i];
            float x = startX + i * (cardWidth + 10);
            bool selected = (i == SelectedTrackIndex);

            // Card bg
            GUI.color = selected ? new Color(0.1f, 0.3f, 0.6f, 0.9f) : new Color(0.12f, 0.12f, 0.15f, 0.9f);
            GUI.DrawTexture(new Rect(x, cardY, cardWidth, cardHeight), Texture2D.whiteTexture);

            // Sky color preview
            GUI.color = track.skyColor;
            GUI.DrawTexture(new Rect(x + 10, cardY + 40, cardWidth - 20, 40), Texture2D.whiteTexture);

            // Ground color preview
            GUI.color = track.groundColor;
            GUI.DrawTexture(new Rect(x + 10, cardY + 82, cardWidth - 20, 30), Texture2D.whiteTexture);

            // Road color preview
            GUI.color = track.roadColor;
            GUI.DrawTexture(new Rect(x + 30, cardY + 85, cardWidth - 60, 24), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Name
            GUIStyle nameStyle = new GUIStyle();
            nameStyle.fontSize = 13;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x, cardY + 8, cardWidth, 25), track.name, nameStyle);

            // Description
            GUIStyle descStyle = new GUIStyle();
            descStyle.fontSize = 11;
            descStyle.alignment = TextAnchor.MiddleCenter;
            descStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            GUI.Label(new Rect(x, cardY + 120, cardWidth, 25), track.description, descStyle);

            // Features
            descStyle.normal.textColor = new Color(0.5f, 0.8f, 0.5f);
            string features = "";
            if (track.hasRamps) features += "🛞 Ramps ";
            if (track.isNight) features += "🌙 Night";
            GUI.Label(new Rect(x, cardY + 145, cardWidth, 25), features, descStyle);

            // Border
            if (selected)
            {
                GUI.color = Color.cyan;
                GUI.DrawTexture(new Rect(x - 2, cardY - 2, cardWidth + 4, 3), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(x - 2, cardY + cardHeight - 1, cardWidth + 4, 3), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            // Select button
            if (GUI.Button(new Rect(x + 5, cardY + cardHeight - 40, cardWidth - 10, 30), selected ? "✓" : "SELECT"))
            {
                SelectedTrackIndex = i;
            }
        }

        // GO button
        if (GUI.Button(new Rect(sw / 2 - 100, sh * 0.88f, 200, 50), "GO RACE!"))
        {
            confirmed = true;
            gameObject.SetActive(false);
        }
    }
}
