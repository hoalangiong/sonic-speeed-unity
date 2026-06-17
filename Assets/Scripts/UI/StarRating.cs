using UnityEngine;

/// <summary>
/// Star rating at end of race + celebration effects.
/// ⭐ = finish race, ⭐⭐ = top 2, ⭐⭐⭐ = 1st place
/// Shows confetti and fun messages for kids.
/// </summary>
public class StarRating : MonoBehaviour
{
    private RaceManager raceManager;
    private CoinSpawner coinSpawner;
    private bool shown = false;

    void Start()
    {
        raceManager = FindFirstObjectByType<RaceManager>();
        coinSpawner = FindFirstObjectByType<CoinSpawner>();
    }

    void OnGUI()
    {
        if (raceManager == null || !raceManager.RaceFinished) return;

        float sw = Screen.width;
        float sh = Screen.height;

        // Calculate stars
        int stars = 1; // 1 star for finishing
        if (raceManager.PlayerPosition <= 2) stars = 2;
        if (raceManager.PlayerPosition == 1) stars = 3;

        // Calculate bonus coins
        int bonusCoins = stars * 100;
        int totalCoins = (coinSpawner != null ? coinSpawner.TotalCoinsCollected : 0) + bonusCoins;

        // Star display
        GUIStyle starStyle = new GUIStyle();
        starStyle.fontSize = 50;
        starStyle.alignment = TextAnchor.MiddleCenter;
        starStyle.normal.textColor = Color.yellow;

        string starText = "";
        for (int i = 0; i < 3; i++)
            starText += (i < stars) ? "⭐" : "☆";

        GUI.Label(new Rect(sw / 2 - 100, sh * 0.35f, 200, 60), starText, starStyle);

        // Fun message based on position
        GUIStyle msgStyle = new GUIStyle();
        msgStyle.fontSize = 28;
        msgStyle.fontStyle = FontStyle.Bold;
        msgStyle.alignment = TextAnchor.MiddleCenter;

        string message = "";
        switch (raceManager.PlayerPosition)
        {
            case 1:
                message = "🏆 CHAMPION! Xuất sắc!";
                msgStyle.normal.textColor = Color.yellow;
                break;
            case 2:
                message = "🥈 Giỏi lắm! Gần thắng rồi!";
                msgStyle.normal.textColor = Color.white;
                break;
            case 3:
                message = "🥉 Tốt lắm! Cố gắng thêm!";
                msgStyle.normal.textColor = new Color(0.8f, 0.5f, 0.2f);
                break;
            default:
                message = "💪 Hoàn thành! Thử lại nhé!";
                msgStyle.normal.textColor = Color.white;
                break;
        }
        GUI.Label(new Rect(sw / 2 - 200, sh * 0.45f, 400, 40), message, msgStyle);

        // Coin reward
        GUIStyle coinStyle = new GUIStyle();
        coinStyle.fontSize = 22;
        coinStyle.alignment = TextAnchor.MiddleCenter;
        coinStyle.normal.textColor = new Color(1f, 0.85f, 0f);
        GUI.Label(new Rect(sw / 2 - 100, sh * 0.52f, 200, 30), $"🪙 +{bonusCoins} bonus!", coinStyle);
        GUI.Label(new Rect(sw / 2 - 100, sh * 0.57f, 200, 30), $"Total: {totalCoins} coins", coinStyle);

        // Confetti effect (simple colored dots)
        if (stars >= 2)
        {
            DrawConfetti(sw, sh);
        }
    }

    void DrawConfetti(float sw, float sh)
    {
        if (confettiTexture == null)
        {
            confettiTexture = new Texture2D(1, 1);
            confettiTexture.SetPixel(0, 0, Color.white);
            confettiTexture.Apply();
        }

        Color[] colors = { Color.red, Color.yellow, Color.green, Color.cyan, Color.magenta, new Color(1, 0.5f, 0) };

        for (int i = 0; i < 30; i++)
        {
            float t = Time.time * 2f + i * 0.5f;
            float x = Mathf.PerlinNoise(i * 0.3f, t * 0.5f) * sw;
            float y = (t * 50f + i * 30f) % sh;
            float size = 5f + Mathf.Sin(i + t) * 3f;

            GUI.color = colors[i % colors.Length];
            GUI.DrawTexture(new Rect(x, y, size, size), confettiTexture);
        }
        GUI.color = Color.white;
    }

    private Texture2D confettiTexture;
}
