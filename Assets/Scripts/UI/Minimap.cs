using UnityEngine;

/// <summary>
/// On-screen minimap showing track outline + car positions.
/// </summary>
public class Minimap : MonoBehaviour
{
    public Transform player;
    public AIRacer[] aiRacers;

    private float mapSize = 120f;
    private float mapPadding = 10f;
    private Texture2D dotTexture;

    void Start()
    {
        // Create dot texture
        dotTexture = new Texture2D(1, 1);
        dotTexture.SetPixel(0, 0, Color.white);
        dotTexture.Apply();
    }

    void OnGUI()
    {
        if (player == null) return;

        float sw = Screen.width;
        float mapX = sw - mapSize - mapPadding;
        float mapY = mapPadding + 80; // Below position display

        // Map background
        GUI.color = new Color(0, 0, 0, 0.5f);
        GUI.DrawTexture(new Rect(mapX, mapY, mapSize, mapSize), dotTexture);
        GUI.color = Color.white;

        // Track outline (simplified oval)
        DrawTrackOutline(mapX, mapY);

        // Player dot (yellow)
        DrawDot(player.position, mapX, mapY, Color.yellow, 8);

        // AI dots
        if (aiRacers != null)
        {
            Color[] colors = { Color.red, new Color(0, 0.5f, 1f), Color.green };
            for (int i = 0; i < aiRacers.Length; i++)
            {
                if (aiRacers[i] != null)
                    DrawDot(aiRacers[i].transform.position, mapX, mapY, colors[i % 3], 6);
            }
        }
    }

    void DrawTrackOutline(float mapX, float mapY)
    {
        // Draw track as dots along oval
        GUI.color = new Color(1, 1, 1, 0.3f);
        for (int i = 0; i < 40; i++)
        {
            float angle = (float)i / 40 * Mathf.PI * 2;
            float rx = 150f + Mathf.Sin(angle * 3) * 20f;
            float rz = 100f + Mathf.Cos(angle * 2) * 15f;
            float x = Mathf.Cos(angle) * rx;
            float z = Mathf.Sin(angle) * rz;

            float dotX = mapX + WorldToMap(x, -200, 200, mapSize);
            float dotY = mapY + WorldToMap(z, -200, 200, mapSize);

            GUI.DrawTexture(new Rect(dotX, dotY, 3, 3), dotTexture);
        }
        GUI.color = Color.white;
    }

    void DrawDot(Vector3 worldPos, float mapX, float mapY, Color color, float size)
    {
        float dotX = mapX + WorldToMap(worldPos.x, -200, 200, mapSize) - size / 2;
        float dotY = mapY + WorldToMap(worldPos.z, -200, 200, mapSize) - size / 2;

        GUI.color = color;
        GUI.DrawTexture(new Rect(dotX, dotY, size, size), dotTexture);
        GUI.color = Color.white;
    }

    float WorldToMap(float worldVal, float worldMin, float worldMax, float mapSize)
    {
        return ((worldVal - worldMin) / (worldMax - worldMin)) * mapSize;
    }
}
