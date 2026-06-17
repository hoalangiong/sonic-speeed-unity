using UnityEngine;

/// <summary>
/// Mobile touch controls — on-screen buttons for gas/brake/steer/nitro.
/// Renders buttons using OnGUI for simplicity (no Canvas needed).
/// </summary>
public class MobileTouchControls : MonoBehaviour
{
    public VehicleController vehicle;
    public NitroSystem nitro;

    private bool gasPressed;
    private bool brakePressed;
    private bool leftPressed;
    private bool rightPressed;

    void Update()
    {
        if (vehicle == null) return;

        // Mobile touch
        vehicle.inputGas = gasPressed ? 1f : 0f;
        vehicle.inputBrake = brakePressed ? 1f : 0f;

        float steer = 0;
        if (leftPressed) steer -= 1f;
        if (rightPressed) steer += 1f;
        vehicle.inputSteer = steer;

        // Keyboard fallback (editor testing)
        if (Application.isEditor || !Application.isMobilePlatform)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vehicle.inputGas = 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vehicle.inputBrake = 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) vehicle.inputSteer = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) vehicle.inputSteer = 1f;
            if (Input.GetKey(KeyCode.LeftShift) && nitro != null) nitro.Activate();
        }
    }

    void OnGUI()
    {
        // Always show touch buttons (mobile + editor)
        float sw = Screen.width;
        float sh = Screen.height;

        // Drive X style — BIG buttons
        float btnW = sw * 0.12f; // Wide buttons
        float btnH = sh * 0.18f; // Tall buttons
        float smallBtn = sh * 0.13f;
        float padding = sw * 0.02f;

        // Custom style for big buttons
        GUIStyle bigBtnStyle = new GUIStyle(GUI.skin.button);
        bigBtnStyle.fontSize = (int)(btnH * 0.35f);
        bigBtnStyle.fontStyle = FontStyle.Bold;

        // === LEFT SIDE: Steering (big arrows) ===
        float leftY = sh - btnH - padding;

        // Left ◀
        GUI.backgroundColor = leftPressed ? new Color(1f, 0.8f, 0f) : new Color(0.15f, 0.15f, 0.2f, 0.9f);
        if (GUI.RepeatButton(new Rect(padding, leftY, btnW, btnH), "◀", bigBtnStyle))
            leftPressed = true;
        else
            leftPressed = false;

        // Right ▶
        GUI.backgroundColor = rightPressed ? new Color(1f, 0.8f, 0f) : new Color(0.15f, 0.15f, 0.2f, 0.9f);
        if (GUI.RepeatButton(new Rect(padding + btnW + padding, leftY, btnW, btnH), "▶", bigBtnStyle))
            rightPressed = true;
        else
            rightPressed = false;

        // === RIGHT SIDE: Gas / Brake (big, stacked) ===
        float rightX = sw - btnW - padding;

        // Gas ▲ (top right)
        GUI.backgroundColor = gasPressed ? new Color(0f, 1f, 0f) : new Color(0f, 0.5f, 0f, 0.9f);
        if (GUI.RepeatButton(new Rect(rightX, sh - btnH * 2 - padding * 2, btnW, btnH), "▲", bigBtnStyle))
            gasPressed = true;
        else
            gasPressed = false;

        // Brake ▼ (bottom right)
        GUI.backgroundColor = brakePressed ? new Color(1f, 0f, 0f) : new Color(0.5f, 0f, 0f, 0.9f);
        if (GUI.RepeatButton(new Rect(rightX, sh - btnH - padding, btnW, btnH), "▼", bigBtnStyle))
            brakePressed = true;
        else
            brakePressed = false;

        // === NITRO (middle right, smaller) ===
        GUIStyle nitroBtnStyle = new GUIStyle(GUI.skin.button);
        nitroBtnStyle.fontSize = (int)(smallBtn * 0.25f);
        nitroBtnStyle.fontStyle = FontStyle.Bold;

        GUI.backgroundColor = (nitro != null && nitro.CurrentNitro > 20f) ?
            new Color(0f, 0.6f, 1f, 0.95f) : new Color(0.2f, 0.2f, 0.3f, 0.6f);
        if (GUI.Button(new Rect(rightX - smallBtn - padding, sh - btnH - padding, smallBtn, smallBtn), "⚡N₂O", nitroBtnStyle))
        {
            if (nitro != null) nitro.Activate();
        }

        // === RESET (top left, small) ===
        GUIStyle resetStyle = new GUIStyle(GUI.skin.button);
        resetStyle.fontSize = (int)(sh * 0.025f);
        resetStyle.fontStyle = FontStyle.Bold;

        GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.85f);
        if (GUI.Button(new Rect(padding, padding, sw * 0.1f, sh * 0.07f), "RESET", resetStyle))
        {
            if (vehicle != null) vehicle.RespawnOnTrack();
        }

        GUI.backgroundColor = Color.white;
    }
}
