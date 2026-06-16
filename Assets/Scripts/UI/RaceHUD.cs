using UnityEngine;

/// <summary>
/// Runtime HUD — shows speed, position, lap, nitro, drift on screen.
/// </summary>
public class RaceHUD : MonoBehaviour
{
    public VehicleController player;
    public RaceManager raceManager;
    public NitroSystem nitro;
    public DriftScoring drift;

    private GUIStyle bigStyle;
    private GUIStyle medStyle;
    private GUIStyle smallStyle;

    // Overtake notification
    private int lastPosition = 4;
    private float overtakeTime = -10f;
    private string overtakeText = "";

    // Speed lines
    private Texture2D lineTexture;

    void Start()
    {
        bigStyle = new GUIStyle();
        bigStyle.fontSize = 42;
        bigStyle.fontStyle = FontStyle.Bold;
        bigStyle.normal.textColor = Color.white;

        medStyle = new GUIStyle();
        medStyle.fontSize = 24;
        medStyle.fontStyle = FontStyle.Bold;
        medStyle.normal.textColor = Color.white;

        smallStyle = new GUIStyle();
        smallStyle.fontSize = 16;
        smallStyle.normal.textColor = Color.white;

        lineTexture = new Texture2D(1, 1);
        lineTexture.SetPixel(0, 0, Color.white);
        lineTexture.Apply();
    }

    void OnGUI()
    {
        if (player == null) return;

        float sw = Screen.width;
        float sh = Screen.height;

        // Speed — bottom left
        string speedText = Mathf.RoundToInt(player.CurrentSpeed).ToString("000");
        GUI.Label(new Rect(20, sh - 70, 200, 50), speedText, bigStyle);
        GUI.Label(new Rect(130, sh - 50, 100, 30), "km/h", smallStyle);

        // RPM bar — bottom left above speed
        float rpm = Mathf.Clamp01(player.CurrentSpeed / player.maxSpeed);
        GUI.color = rpm > 0.8f ? Color.red : Color.green;
        GUI.DrawTexture(new Rect(20, sh - 90, 150 * rpm, 12), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Position — top right
        if (raceManager != null)
        {
            GUI.Label(new Rect(sw - 100, 20, 80, 40), raceManager.GetPositionText(), bigStyle);
        }

        // Lap — top left
        if (raceManager != null)
        {
            GUI.Label(new Rect(20, 20, 200, 30), $"LAP {raceManager.PlayerLap}/{raceManager.totalLaps}", medStyle);
            GUI.Label(new Rect(20, 50, 200, 25), raceManager.GetTimeText(), smallStyle);
        }

        // Nitro bar — bottom center
        if (nitro != null)
        {
            float nitroWidth = 120;
            float nitroX = sw / 2 - nitroWidth / 2;
            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.DrawTexture(new Rect(nitroX, sh - 40, nitroWidth, 15), Texture2D.whiteTexture);
            GUI.color = nitro.IsActive ? Color.cyan : new Color(0, 0.6f, 1f);
            GUI.DrawTexture(new Rect(nitroX, sh - 40, nitroWidth * (nitro.CurrentNitro / 100f), 15), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(nitroX, sh - 58, nitroWidth, 20), "NITRO", smallStyle);
        }

        // Drift score — center
        if (drift != null && drift.IsDrifting)
        {
            GUIStyle driftStyle = new GUIStyle();
            driftStyle.fontSize = 30;
            driftStyle.fontStyle = FontStyle.Bold;
            driftStyle.normal.textColor = new Color(1f, 0.5f, 0f);
            driftStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(sw / 2 - 100, sh * 0.3f, 200, 40), "DRIFT!", driftStyle);
            driftStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(sw / 2 - 50, sh * 0.3f + 35, 100, 35), $"x{drift.Multiplier}", driftStyle);
            driftStyle.fontSize = 20;
            driftStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(sw / 2 - 50, sh * 0.3f + 65, 100, 25), $"+{Mathf.RoundToInt(drift.CurrentScore)}", driftStyle);
        }

        // Race finished
        if (raceManager != null && raceManager.RaceFinished)
        {
            GUIStyle finishStyle = new GUIStyle();
            finishStyle.fontSize = 50;
            finishStyle.fontStyle = FontStyle.Bold;
            finishStyle.normal.textColor = Color.yellow;
            finishStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(sw / 2 - 200, sh / 2 - 50, 400, 100), $"FINISH! {raceManager.GetPositionText()}", finishStyle);
        }

        // Grounded indicator (debug)
        GUI.Label(new Rect(sw - 150, sh - 30, 150, 25), $"Grounded: {player.IsGrounded}", smallStyle);

        // === OVERTAKE NOTIFICATION ===
        if (raceManager != null)
        {
            int currentPos = raceManager.PlayerPosition;
            if (currentPos < lastPosition)
            {
                overtakeTime = Time.time;
                overtakeText = $"OVERTAKE! +200";
            }
            lastPosition = currentPos;

            // === LAP NOTIFICATION ===
            if (Time.time - raceManager.LapNotifyTime < 2f && raceManager.LapNotifyNumber > 1)
            {
                float alpha = 1f - (Time.time - raceManager.LapNotifyTime) / 2f;
                GUIStyle lapStyle = new GUIStyle();
                lapStyle.fontSize = 48;
                lapStyle.fontStyle = FontStyle.Bold;
                lapStyle.alignment = TextAnchor.MiddleCenter;
                lapStyle.normal.textColor = new Color(0f, 0.8f, 1f, alpha);
                string lapMsg = raceManager.LapNotifyNumber > raceManager.totalLaps ? "FINAL LAP!" : $"LAP {raceManager.LapNotifyNumber}!";
                GUI.Label(new Rect(sw / 2 - 150, sh * 0.15f, 300, 60), lapMsg, lapStyle);
            }
        }

        if (Time.time - overtakeTime < 2f)
        {
            float alpha = 1f - (Time.time - overtakeTime) / 2f;
            GUIStyle overtakeStyle = new GUIStyle();
            overtakeStyle.fontSize = 34;
            overtakeStyle.fontStyle = FontStyle.Bold;
            overtakeStyle.alignment = TextAnchor.MiddleCenter;
            overtakeStyle.normal.textColor = new Color(0f, 1f, 0.5f, alpha);
            GUI.Label(new Rect(sw / 2 - 150, sh * 0.2f, 300, 50), overtakeText, overtakeStyle);
        }

        // === SPEED LINES ===
        if (player.CurrentSpeed > 150f)
        {
            float intensity = Mathf.Clamp01((player.CurrentSpeed - 150f) / 100f);
            int numLines = (int)(intensity * 15);
            GUI.color = new Color(1, 1, 1, intensity * 0.4f);

            for (int i = 0; i < numLines; i++)
            {
                float lineX = (i % 2 == 0) ? Random.Range(0f, sw * 0.15f) : Random.Range(sw * 0.85f, sw);
                float lineY = Random.Range(0, sh);
                float lineLength = Random.Range(30f, 80f) * intensity;
                GUI.DrawTexture(new Rect(lineX, lineY, 2, lineLength), lineTexture);
            }
            GUI.color = Color.white;
        }
    }
}
