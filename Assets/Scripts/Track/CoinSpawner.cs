using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns collectible coins along the track.
/// Coins float, spin, and give points when collected.
/// </summary>
public class CoinSpawner : MonoBehaviour
{
    [Header("Settings")]
    public int coinsPerLap = 30;
    public float coinHeight = 1.5f;
    public float collectRadius = 3f;
    public float spinSpeed = 180f;

    [Header("State")]
    public int TotalCoinsCollected { get; private set; } = 0;

    private List<GameObject> coins = new List<GameObject>();
    private Transform player;
    private float feedbackTime = -10f;
    private string feedbackText = "";

    void Start()
    {
        player = GameObject.Find("PlayerCar")?.transform;
        SpawnCoins();
    }

    void SpawnCoins()
    {
        var tracks = TrackSelector.AvailableTracks;
        int idx = TrackSelector.SelectedTrackIndex;
        float rx = tracks[idx].trackScaleX;
        float rz = tracks[idx].trackScaleZ;

        for (int i = 0; i < coinsPerLap; i++)
        {
            float angle = (float)i / coinsPerLap * Mathf.PI * 2;
            // Offset slightly from center
            float offsetX = Mathf.Sin(angle * 5) * 3f;
            float x = Mathf.Cos(angle) * rx + offsetX;
            float z = Mathf.Sin(angle) * rz;

            var coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "Coin";
            coin.transform.position = new Vector3(x, coinHeight, z);
            coin.transform.localScale = new Vector3(1.2f, 0.1f, 1.2f);
            coin.transform.rotation = Quaternion.Euler(0, 0, 90);

            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.85f, 0f);
            mat.SetFloat("_Metallic", 0.9f);
            mat.SetFloat("_Glossiness", 0.95f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.5f, 0.4f, 0f));
            coin.GetComponent<Renderer>().material = mat;

            // Remove collider (we check distance manually)
            Destroy(coin.GetComponent<Collider>());

            coins.Add(coin);
        }
    }

    void Update()
    {
        if (player == null) return;

        // Spin all coins
        foreach (var coin in coins)
        {
            if (coin != null && coin.activeSelf)
            {
                coin.transform.Rotate(0, spinSpeed * Time.deltaTime, 0);
                // Bob up and down
                var pos = coin.transform.position;
                pos.y = coinHeight + Mathf.Sin(Time.time * 3f + pos.x) * 0.3f;
                coin.transform.position = pos;
            }
        }

        // Check collection
        for (int i = coins.Count - 1; i >= 0; i--)
        {
            if (coins[i] == null || !coins[i].activeSelf) continue;

            float dist = Vector3.Distance(player.position, coins[i].transform.position);
            if (dist < collectRadius)
            {
                CollectCoin(i);
            }
        }
    }

    void CollectCoin(int index)
    {
        coins[index].SetActive(false);
        TotalCoinsCollected++;

        // Fun feedback every 5 coins
        if (TotalCoinsCollected % 5 == 0)
        {
            string[] messages = { "Awesome! 🌟", "Amazing! ⭐", "Cool! 😎", "Nice! 🔥", "Perfect! 💎" };
            feedbackText = messages[Random.Range(0, messages.Length)];
            feedbackTime = Time.time;
        }
    }

    void OnGUI()
    {
        float sw = Screen.width;
        float sh = Screen.height;

        // Coin counter — top center
        GUIStyle coinStyle = new GUIStyle();
        coinStyle.fontSize = 22;
        coinStyle.fontStyle = FontStyle.Bold;
        coinStyle.alignment = TextAnchor.MiddleCenter;
        coinStyle.normal.textColor = new Color(1f, 0.85f, 0f);
        GUI.Label(new Rect(sw / 2 - 60, 10, 120, 30), $"🪙 {TotalCoinsCollected}", coinStyle);

        // Fun feedback popup
        if (Time.time - feedbackTime < 1.5f)
        {
            float alpha = 1f - (Time.time - feedbackTime) / 1.5f;
            GUIStyle fbStyle = new GUIStyle();
            fbStyle.fontSize = 32;
            fbStyle.fontStyle = FontStyle.Bold;
            fbStyle.alignment = TextAnchor.MiddleCenter;
            fbStyle.normal.textColor = new Color(1f, 1f, 0f, alpha);
            float yOffset = (Time.time - feedbackTime) * 30f;
            GUI.Label(new Rect(sw / 2 - 100, sh * 0.4f - yOffset, 200, 50), feedbackText, fbStyle);
        }
    }
}
