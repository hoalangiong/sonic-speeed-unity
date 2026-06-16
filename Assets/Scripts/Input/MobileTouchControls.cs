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
        // Only show touch buttons on mobile (or always for testing)
        if (!Application.isMobilePlatform && !Application.isEditor) return;

        float sw = Screen.width;
        float sh = Screen.height;
        float btnSize = Mathf.Min(sw, sh) * 0.12f;
        float padding = 15f;

        // === LEFT SIDE: Steering ===
        float leftX = padding;
        float leftY = sh - btnSize * 2 - padding * 2;

        // Left button ◀
        GUI.backgroundColor = leftPressed ? new Color(1f, 0.8f, 0f) : new Color(0.2f, 0.2f, 0.3f, 0.85f);
        if (GUI.RepeatButton(new Rect(leftX, leftY + btnSize + padding, btnSize, btnSize), "◀"))
            leftPressed = true;
        else
            leftPressed = false;

        // Right button ▶
        GUI.backgroundColor = rightPressed ? new Color(1f, 0.8f, 0f) : new Color(0.2f, 0.2f, 0.3f, 0.85f);
        if (GUI.RepeatButton(new Rect(leftX + btnSize + padding, leftY + btnSize + padding, btnSize, btnSize), "▶"))
            rightPressed = true;
        else
            rightPressed = false;

        // === RIGHT SIDE: Gas / Brake / Nitro ===
        float rightX = sw - btnSize - padding;

        // Gas ▲
        GUI.backgroundColor = gasPressed ? new Color(0f, 1f, 0f) : new Color(0f, 0.6f, 0f, 0.85f);
        if (GUI.RepeatButton(new Rect(rightX, sh - btnSize * 2 - padding * 2, btnSize, btnSize), "▲"))
            gasPressed = true;
        else
            gasPressed = false;

        // Brake ▼
        GUI.backgroundColor = brakePressed ? new Color(1f, 0f, 0f) : new Color(0.6f, 0f, 0f, 0.85f);
        if (GUI.RepeatButton(new Rect(rightX, sh - btnSize - padding, btnSize, btnSize), "▼"))
            brakePressed = true;
        else
            brakePressed = false;

        // Nitro ⚡
        GUI.backgroundColor = (nitro != null && nitro.CurrentNitro > 20f) ?
            new Color(0f, 0.5f, 1f, 0.9f) : new Color(0.2f, 0.2f, 0.3f, 0.5f);
        if (GUI.Button(new Rect(rightX - btnSize - padding, sh - btnSize * 2 - padding * 2, btnSize, btnSize), "⚡N₂O"))
        {
            if (nitro != null) nitro.Activate();
        }

        // Reset button (top left)
        GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);
        if (GUI.Button(new Rect(padding, padding, btnSize * 1.2f, btnSize * 0.6f), "RESET"))
        {
            if (vehicle != null) vehicle.RespawnOnTrack();
        }

        GUI.backgroundColor = Color.white;
    }
}
