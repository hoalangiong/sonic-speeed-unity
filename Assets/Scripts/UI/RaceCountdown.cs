using UnityEngine;

/// <summary>
/// Race countdown (3-2-1-GO) + Finish screen.
/// Freezes all cars until GO, shows results when race ends.
/// </summary>
public class RaceCountdown : MonoBehaviour
{
    public VehicleController player;
    public AIRacer[] aiRacers;
    public RaceManager raceManager;

    private float countdownTimer = 4f; // 3, 2, 1, GO
    private bool raceStarted = false;
    private string countdownText = "";
    private float goDisplayTime = 0;

    void Start()
    {
        // Freeze player
        if (player != null)
            player.enabled = false;

        // Freeze AI
        if (aiRacers != null)
            foreach (var ai in aiRacers)
                if (ai != null) ai.enabled = false;
    }

    void Update()
    {
        if (raceStarted) return;

        countdownTimer -= Time.deltaTime;

        if (countdownTimer > 3f)
            countdownText = "";
        else if (countdownTimer > 2f)
            countdownText = "3";
        else if (countdownTimer > 1f)
            countdownText = "2";
        else if (countdownTimer > 0f)
            countdownText = "1";
        else if (!raceStarted)
        {
            countdownText = "GO!";
            goDisplayTime = Time.time;
            raceStarted = true;

            // Unfreeze
            if (player != null) player.enabled = true;
            if (aiRacers != null)
                foreach (var ai in aiRacers)
                    if (ai != null) ai.enabled = true;
        }
    }

    void OnGUI()
    {
        float sw = Screen.width;
        float sh = Screen.height;

        // Countdown display
        if (!raceStarted || (Time.time - goDisplayTime < 1f))
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 120;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = countdownText == "GO!" ? Color.green : Color.white;

            GUI.Label(new Rect(sw / 2 - 150, sh / 2 - 80, 300, 160), countdownText, style);
        }

        // Finish screen
        if (raceManager != null && raceManager.RaceFinished)
        {
            // Dark overlay
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 60;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = Color.yellow;

            GUI.Label(new Rect(sw / 2 - 200, sh * 0.25f, 400, 80), "RACE COMPLETE!", titleStyle);

            GUIStyle infoStyle = new GUIStyle();
            infoStyle.fontSize = 36;
            infoStyle.fontStyle = FontStyle.Bold;
            infoStyle.alignment = TextAnchor.MiddleCenter;
            infoStyle.normal.textColor = Color.white;

            GUI.Label(new Rect(sw / 2 - 150, sh * 0.4f, 300, 50), $"Position: {raceManager.GetPositionText()}", infoStyle);
            GUI.Label(new Rect(sw / 2 - 150, sh * 0.5f, 300, 50), $"Time: {raceManager.GetTimeText()}", infoStyle);

            // Restart button
            infoStyle.fontSize = 28;
            infoStyle.normal.textColor = Color.green;
            if (GUI.Button(new Rect(sw / 2 - 80, sh * 0.65f, 160, 50), "RACE AGAIN"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }
    }
}
