using UnityEngine;

/// <summary>
/// Track selection with visual previews.
/// Shows color-coded track preview + description for each track.
/// </summary>
public class TrackSelector : MonoBehaviour
{
    public static TrackData[] AvailableTracks = new TrackData[]
    {
        new TrackData("Highway", "Đường thẳng dài, dễ chạy", new Color(0.5f, 0.75f, 1f), new Color(0.2f, 0.45f, 0.12f), new Color(0.15f, 0.15f, 0.17f), new Color(0.7f, 0.8f, 0.9f), 300f, 30f, true, false),
        new TrackData("Coastal", "Ven biển, nắng đẹp", new Color(0.5f, 0.75f, 1f), new Color(0.2f, 0.45f, 0.12f), new Color(0.15f, 0.15f, 0.17f), new Color(0.7f, 0.8f, 0.9f), 150f, 100f, true, false),
        new TrackData("Tokyo Night", "Đêm, neon rực rỡ", new Color(0.02f, 0.02f, 0.08f), new Color(0.1f, 0.1f, 0.12f), new Color(0.2f, 0.2f, 0.25f), new Color(0.05f, 0.0f, 0.1f), 120f, 80f, false, true),
        new TrackData("Desert", "Hoàng hôn, tốc độ max", new Color(0.95f, 0.6f, 0.3f), new Color(0.8f, 0.6f, 0.3f), new Color(0.2f, 0.18f, 0.15f), new Color(0.9f, 0.7f, 0.4f), 200f, 120f, true, false),
        new TrackData("Snow Alps", "Tuyết, đường trơn", new Color(0.7f, 0.8f, 0.95f), new Color(0.9f, 0.92f, 0.95f), new Color(0.3f, 0.32f, 0.35f), new Color(0.85f, 0.88f, 0.92f), 130f, 90f, false, false),
        new TrackData("Neon Circuit", "Tron-style phát sáng", new Color(0.0f, 0.0f, 0.05f), new Color(0.02f, 0.02f, 0.05f), new Color(0.0f, 0.1f, 0.2f), new Color(0.0f, 0.05f, 0.1f), 160f, 110f, true, true),
        new TrackData("Miami Beach", "Hoàng hôn hồng, cọ", new Color(0.95f, 0.5f, 0.6f), new Color(0.9f, 0.85f, 0.6f), new Color(0.18f, 0.16f, 0.16f), new Color(0.95f, 0.6f, 0.7f), 170f, 110f, true, false),
        new TrackData("Volcano", "Núi lửa, lava đỏ", new Color(0.3f, 0.1f, 0.0f), new Color(0.15f, 0.08f, 0.03f), new Color(0.1f, 0.08f, 0.06f), new Color(0.4f, 0.15f, 0.0f), 140f, 95f, true, false),
        new TrackData("Space", "Không gian, sao lấp lánh", new Color(0.0f, 0.0f, 0.02f), new Color(0.05f, 0.05f, 0.08f), new Color(0.1f, 0.1f, 0.15f), new Color(0.0f, 0.0f, 0.03f), 180f, 130f, true, true),
    };

    public static int SelectedTrackIndex = 0; // 0 = Highway default

    private bool confirmed = false;
    private bool waitingForCarSelect = true;
    private Texture2D tex;
    private int scrollOffset = 0;

    void Start()
    {
        tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
    }

    void Update()
    {
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
        GUI.color = new Color(0.03f, 0.03f, 0.08f, 0.97f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), tex);
        GUI.color = Color.white;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = (int)(sh * 0.055f);
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.cyan;
        GUI.Label(new Rect(0, sh * 0.02f, sw, sh * 0.07f), "CHỌN ĐƯỜNG ĐUA", titleStyle);

        // Show 4 tracks at a time (scrollable)
        int visibleCount = 4;
        float cardWidth = sw * 0.22f;
        float cardHeight = sh * 0.7f;
        float gap = sw * 0.015f;
        float totalWidth = visibleCount * cardWidth + (visibleCount - 1) * gap;
        float startX = (sw - totalWidth) / 2;
        float cardY = sh * 0.11f;

        // Scroll buttons
        GUIStyle arrowStyle = new GUIStyle(GUI.skin.button);
        arrowStyle.fontSize = (int)(sh * 0.05f);
        if (scrollOffset > 0)
        {
            if (GUI.Button(new Rect(startX - sw * 0.04f, sh * 0.45f, sw * 0.035f, sh * 0.08f), "◀", arrowStyle))
                scrollOffset--;
        }
        if (scrollOffset + visibleCount < AvailableTracks.Length)
        {
            if (GUI.Button(new Rect(startX + totalWidth + sw * 0.005f, sh * 0.45f, sw * 0.035f, sh * 0.08f), "▶", arrowStyle))
                scrollOffset++;
        }

        // Track cards
        for (int vi = 0; vi < visibleCount; vi++)
        {
            int i = vi + scrollOffset;
            if (i >= AvailableTracks.Length) break;

            var track = AvailableTracks[i];
            float x = startX + vi * (cardWidth + gap);
            bool selected = (i == SelectedTrackIndex);

            // Card background
            GUI.color = selected ? new Color(0.1f, 0.25f, 0.5f, 0.95f) : new Color(0.08f, 0.08f, 0.12f, 0.9f);
            GUI.DrawTexture(new Rect(x, cardY, cardWidth, cardHeight), tex);
            GUI.color = Color.white;

            // Track landscape preview (rendered from colors)
            float previewH = cardHeight * 0.45f;
            // Sky
            GUI.color = track.skyColor;
            GUI.DrawTexture(new Rect(x + 5, cardY + 5, cardWidth - 10, previewH * 0.6f), tex);
            // Ground
            GUI.color = track.groundColor;
            GUI.DrawTexture(new Rect(x + 5, cardY + 5 + previewH * 0.6f, cardWidth - 10, previewH * 0.25f), tex);
            // Road
            GUI.color = track.roadColor;
            float roadW = cardWidth * 0.4f;
            GUI.DrawTexture(new Rect(x + (cardWidth - roadW) / 2, cardY + 5 + previewH * 0.5f, roadW, previewH * 0.4f), tex);
            // Road markings
            GUI.color = Color.yellow;
            GUI.DrawTexture(new Rect(x + cardWidth / 2 - 1, cardY + 5 + previewH * 0.55f, 2, previewH * 0.3f), tex);
            GUI.color = Color.white;

            // Night indicator
            if (track.isNight)
            {
                GUIStyle nightStyle = new GUIStyle();
                nightStyle.fontSize = (int)(sh * 0.03f);
                nightStyle.alignment = TextAnchor.MiddleCenter;
                nightStyle.normal.textColor = Color.yellow;
                GUI.Label(new Rect(x, cardY + 10, cardWidth, sh * 0.04f), "🌙", nightStyle);
            }

            // Track name
            GUIStyle nameStyle = new GUIStyle();
            nameStyle.fontSize = (int)(sh * 0.028f);
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x, cardY + previewH + 15, cardWidth, sh * 0.04f), track.name, nameStyle);

            // Description
            GUIStyle descStyle = new GUIStyle();
            descStyle.fontSize = (int)(sh * 0.022f);
            descStyle.alignment = TextAnchor.MiddleCenter;
            descStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            GUI.Label(new Rect(x, cardY + previewH + 15 + sh * 0.04f, cardWidth, sh * 0.03f), track.description, descStyle);

            // Features
            string features = "";
            if (track.hasRamps) features += "🛞Ramps ";
            if (track.isNight) features += "🌙Night ";
            if (track.trackScaleZ < 50) features += "➡️Thẳng ";
            descStyle.normal.textColor = new Color(0.4f, 0.9f, 0.4f);
            GUI.Label(new Rect(x, cardY + previewH + 15 + sh * 0.07f, cardWidth, sh * 0.03f), features, descStyle);

            // Difficulty
            string difficulty = track.trackScaleZ < 50 ? "⭐ Dễ" : track.trackScaleX > 170 ? "⭐⭐⭐ Khó" : "⭐⭐ Trung bình";
            descStyle.normal.textColor = new Color(1f, 0.8f, 0.2f);
            GUI.Label(new Rect(x, cardY + previewH + 15 + sh * 0.1f, cardWidth, sh * 0.03f), difficulty, descStyle);

            // Selected border
            if (selected)
            {
                GUI.color = Color.cyan;
                GUI.DrawTexture(new Rect(x - 2, cardY - 2, cardWidth + 4, 3), tex);
                GUI.DrawTexture(new Rect(x - 2, cardY + cardHeight - 1, cardWidth + 4, 3), tex);
                GUI.DrawTexture(new Rect(x - 2, cardY, 3, cardHeight), tex);
                GUI.DrawTexture(new Rect(x + cardWidth - 1, cardY, 3, cardHeight), tex);
                GUI.color = Color.white;
            }

            // Select button
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = (int)(sh * 0.022f);
            btnStyle.fontStyle = FontStyle.Bold;
            if (GUI.Button(new Rect(x + cardWidth * 0.1f, cardY + cardHeight - sh * 0.06f, cardWidth * 0.8f, sh * 0.045f),
                selected ? "✓ ĐÃ CHỌN" : "CHỌN", btnStyle))
            {
                SelectedTrackIndex = i;
            }
        }

        // GO button
        GUIStyle goStyle = new GUIStyle(GUI.skin.button);
        goStyle.fontSize = (int)(sh * 0.04f);
        goStyle.fontStyle = FontStyle.Bold;
        if (GUI.Button(new Rect(sw / 2 - sw * 0.1f, sh * 0.9f, sw * 0.2f, sh * 0.07f), "BẮT ĐẦU!", goStyle))
        {
            confirmed = true;
            gameObject.SetActive(false);
        }
    }
}
