using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD — Drive X style: speedometer, RPM bar, nitro gauge, drift score, money.
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("Vehicle")]
    public VehicleController vehicle;
    public NitroSystem nitro;
    public DriftScoring drift;

    [Header("Speed")]
    public TextMeshProUGUI speedText;
    public Image rpmBar;

    [Header("Nitro")]
    public Image nitroBar;
    public Image nitroGlow;

    [Header("Drift")]
    public GameObject driftPanel;
    public TextMeshProUGUI driftMultiplierText;
    public TextMeshProUGUI driftScoreText;

    [Header("Info")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI positionText;

    private int money = 0;

    void Update()
    {
        if (vehicle == null) return;

        // Speed
        int speed = Mathf.RoundToInt(vehicle.CurrentSpeed);
        speedText.text = speed.ToString("000");

        // RPM bar (0-1)
        float rpmNorm = Mathf.InverseLerp(800f, 8500f, vehicle.CurrentRPM);
        rpmBar.fillAmount = rpmNorm;
        rpmBar.color = rpmNorm > 0.8f ? Color.red : Color.green;

        // Nitro
        if (nitro != null)
        {
            nitroBar.fillAmount = nitro.CurrentNitro / 100f;
            nitroGlow.enabled = nitro.IsActive;
        }

        // Drift
        if (drift != null)
        {
            bool isDrifting = drift.IsDrifting;
            driftPanel.SetActive(isDrifting);
            if (isDrifting)
            {
                driftMultiplierText.text = "x" + drift.Multiplier.ToString();
                driftScoreText.text = "+" + Mathf.RoundToInt(drift.CurrentScore).ToString();
            }
        }

        // Money
        moneyText.text = "$" + money.ToString("N0");
    }

    public void AddMoney(int amount)
    {
        money += amount;
    }
}
