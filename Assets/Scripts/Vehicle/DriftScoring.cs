using UnityEngine;

/// <summary>
/// Drift scoring — detects drift, accumulates points with multiplier.
/// </summary>
public class DriftScoring : MonoBehaviour
{
    [Header("Settings")]
    public float minSpeed = 30f;
    public float minAngle = 10f;
    public float pointsPerSecond = 100f;
    public float multiplierInterval = 1.5f;
    public int maxMultiplier = 8;

    public bool IsDrifting { get; private set; }
    public float CurrentScore { get; private set; }
    public int Multiplier { get; private set; } = 1;
    public int TotalScore { get; private set; }

    private VehicleController vehicle;
    private float driftTimer;
    private GameHUD hud;

    void Start()
    {
        vehicle = GetComponent<VehicleController>();
        hud = FindFirstObjectByType<GameHUD>();
    }

    void Update()
    {
        bool wasDrifting = IsDrifting;
        IsDrifting = vehicle.IsDrifting;

        if (IsDrifting)
        {
            driftTimer += Time.deltaTime;
            CurrentScore += pointsPerSecond * Time.deltaTime;
            Multiplier = Mathf.Min(maxMultiplier, 1 + Mathf.FloorToInt(driftTimer / multiplierInterval));
        }
        else if (wasDrifting)
        {
            // Cash in drift score
            int earned = Mathf.RoundToInt(CurrentScore * Multiplier);
            TotalScore += earned;
            if (hud != null) hud.AddMoney(earned);

            // Reset
            CurrentScore = 0;
            Multiplier = 1;
            driftTimer = 0;
        }
    }
}
