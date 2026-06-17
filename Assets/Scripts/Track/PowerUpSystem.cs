using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Power-up boxes on track. Drive through = random power-up.
/// Shield, Speed Boost, Magnet, Rocket.
/// </summary>
public class PowerUpSystem : MonoBehaviour
{
    public enum PowerUpType { None, Shield, SpeedBoost, Magnet, Rocket }

    [Header("Settings")]
    public int numPowerUps = 8;
    public float collectRadius = 4f;
    public float respawnTime = 10f;

    [Header("State")]
    public PowerUpType ActivePowerUp { get; private set; } = PowerUpType.None;
    public float PowerUpTimer { get; private set; } = 0;

    private List<GameObject> powerUpBoxes = new List<GameObject>();
    private List<float> respawnTimers = new List<float>();
    private Transform player;
    private VehicleController vehicle;
    private float originalMaxSpeed;
    private float activateTime = -10f;
    private string activateText = "";

    void Start()
    {
        player = GameObject.Find("PlayerCar")?.transform;
        if (player != null) vehicle = player.GetComponent<VehicleController>();
        if (vehicle != null) originalMaxSpeed = vehicle.maxSpeed;
        SpawnPowerUps();
    }

    void SpawnPowerUps()
    {
        var tracks = TrackSelector.AvailableTracks;
        int idx = TrackSelector.SelectedTrackIndex;
        float rx = tracks[idx].trackScaleX;
        float rz = tracks[idx].trackScaleZ;

        for (int i = 0; i < numPowerUps; i++)
        {
            float angle = (float)i / numPowerUps * Mathf.PI * 2 + 0.3f;
            float x = Mathf.Cos(angle) * rx;
            float z = Mathf.Sin(angle) * rz;

            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "PowerUp";
            box.transform.position = new Vector3(x, 2f, z);
            box.transform.localScale = new Vector3(2f, 2f, 2f);

            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.2f, 0.5f, 1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.1f, 0.3f, 0.8f));
            box.GetComponent<Renderer>().material = mat;
            Destroy(box.GetComponent<Collider>());

            powerUpBoxes.Add(box);
            respawnTimers.Add(0);
        }
    }

    void Update()
    {
        if (player == null) return;

        // Spin power-up boxes
        foreach (var box in powerUpBoxes)
        {
            if (box != null && box.activeSelf)
            {
                box.transform.Rotate(0, 90f * Time.deltaTime, 0);
                var pos = box.transform.position;
                pos.y = 2f + Mathf.Sin(Time.time * 2f + pos.x * 0.1f) * 0.5f;
                box.transform.position = pos;
            }
        }

        // Check collection
        for (int i = 0; i < powerUpBoxes.Count; i++)
        {
            if (powerUpBoxes[i] == null || !powerUpBoxes[i].activeSelf) continue;

            float dist = Vector3.Distance(player.position, powerUpBoxes[i].transform.position);
            if (dist < collectRadius)
            {
                CollectPowerUp(i);
            }
        }

        // Handle respawn
        for (int i = 0; i < respawnTimers.Count; i++)
        {
            if (respawnTimers[i] > 0)
            {
                respawnTimers[i] -= Time.deltaTime;
                if (respawnTimers[i] <= 0 && powerUpBoxes[i] != null)
                    powerUpBoxes[i].SetActive(true);
            }
        }

        // Active power-up timer
        if (ActivePowerUp != PowerUpType.None)
        {
            PowerUpTimer -= Time.deltaTime;
            if (PowerUpTimer <= 0)
            {
                DeactivatePowerUp();
            }
        }

        // Magnet effect — attract nearby coins
        if (ActivePowerUp == PowerUpType.Magnet)
        {
            var coinSpawner = FindFirstObjectByType<CoinSpawner>();
            if (coinSpawner != null)
            {
                // Coins attracted handled by CoinSpawner checking larger radius
            }
        }
    }

    void CollectPowerUp(int index)
    {
        powerUpBoxes[index].SetActive(false);
        respawnTimers[index] = respawnTime;

        // Random power-up
        PowerUpType[] types = { PowerUpType.Shield, PowerUpType.SpeedBoost, PowerUpType.Magnet, PowerUpType.Rocket };
        ActivePowerUp = types[Random.Range(0, types.Length)];
        PowerUpTimer = 5f; // 5 seconds

        ActivatePowerUp();
    }

    void ActivatePowerUp()
    {
        activateTime = Time.time;

        switch (ActivePowerUp)
        {
            case PowerUpType.Shield:
                activateText = "🛡️ SHIELD!";
                break;
            case PowerUpType.SpeedBoost:
                activateText = "⚡ SPEED BOOST!";
                if (vehicle != null) vehicle.maxSpeed = originalMaxSpeed * 1.5f;
                break;
            case PowerUpType.Magnet:
                activateText = "🧲 MAGNET!";
                break;
            case PowerUpType.Rocket:
                activateText = "🎯 ROCKET!";
                // Slow down nearest AI
                var aiRacers = FindObjectsByType<AIRacer>(FindObjectsSortMode.None);
                if (aiRacers.Length > 0)
                {
                    float minDist = float.MaxValue;
                    AIRacer nearest = null;
                    foreach (var ai in aiRacers)
                    {
                        float d = Vector3.Distance(player.position, ai.transform.position);
                        if (d < minDist) { minDist = d; nearest = ai; }
                    }
                    if (nearest != null) nearest.baseSpeed *= 0.5f; // Slow 50% for duration
                }
                break;
        }
    }

    void DeactivatePowerUp()
    {
        switch (ActivePowerUp)
        {
            case PowerUpType.SpeedBoost:
                if (vehicle != null) vehicle.maxSpeed = originalMaxSpeed;
                break;
            case PowerUpType.Rocket:
                // Restore AI speed
                var aiRacers = FindObjectsByType<AIRacer>(FindObjectsSortMode.None);
                foreach (var ai in aiRacers)
                    ai.baseSpeed = 20f; // Restore default
                break;
        }
        ActivePowerUp = PowerUpType.None;
    }

    void OnGUI()
    {
        float sw = Screen.width;
        float sh = Screen.height;

        // Active power-up indicator
        if (ActivePowerUp != PowerUpType.None)
        {
            GUIStyle puStyle = new GUIStyle();
            puStyle.fontSize = 20;
            puStyle.fontStyle = FontStyle.Bold;
            puStyle.alignment = TextAnchor.MiddleCenter;
            puStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(sw / 2 - 80, 40, 160, 30), $"{activateText} {PowerUpTimer:F0}s", puStyle);
        }

        // Activation popup
        if (Time.time - activateTime < 1.5f)
        {
            float alpha = 1f - (Time.time - activateTime) / 1.5f;
            GUIStyle popStyle = new GUIStyle();
            popStyle.fontSize = 36;
            popStyle.fontStyle = FontStyle.Bold;
            popStyle.alignment = TextAnchor.MiddleCenter;
            popStyle.normal.textColor = new Color(0.2f, 0.8f, 1f, alpha);
            GUI.Label(new Rect(sw / 2 - 150, sh * 0.25f, 300, 50), activateText, popStyle);
        }
    }
}
