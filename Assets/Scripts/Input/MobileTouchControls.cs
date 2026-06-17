using UnityEngine;

/// <summary>
/// Mobile touch controls — Drive X style using Input.touches.
/// Divides screen into zones for reliable continuous input.
/// No GUI buttons — uses touch position detection (no "release" when finger moves).
/// </summary>
public class MobileTouchControls : MonoBehaviour
{
    public VehicleController vehicle;
    public NitroSystem nitro;

    // Touch zones (percentage of screen)
    // LEFT 30%: steering (left half = steer left, right half = steer right)
    // RIGHT 30%: top half = gas, bottom half = brake
    // MIDDLE: nitro tap

    private bool gasPressed;
    private bool brakePressed;
    private bool leftPressed;
    private bool rightPressed;

    void Update()
    {
        if (vehicle == null) return;

        // Reset all inputs each frame
        gasPressed = false;
        brakePressed = false;
        leftPressed = false;
        rightPressed = false;

        // Process all active touches
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    continue;

                ProcessTouch(touch.position);
            }
        }

        // Keyboard fallback (editor)
        if (Application.isEditor)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) gasPressed = true;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) brakePressed = true;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) leftPressed = true;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) rightPressed = true;
            if (Input.GetKey(KeyCode.LeftShift) && nitro != null) nitro.Activate();

            // Mouse click also works in editor
            if (Input.GetMouseButton(0))
            {
                ProcessTouch(Input.mousePosition);
            }
        }

        // Apply to vehicle
        vehicle.inputGas = gasPressed ? 1f : 0f;
        vehicle.inputBrake = brakePressed ? 1f : 0f;
        float steer = 0;
        if (leftPressed) steer -= 1f;
        if (rightPressed) steer += 1f;
        vehicle.inputSteer = steer;
    }

    void ProcessTouch(Vector2 pos)
    {
        float sw = Screen.width;
        float sh = Screen.height;

        float xPercent = pos.x / sw;
        float yPercent = pos.y / sh; // 0 = bottom, 1 = top

        // LEFT ZONE (0% - 35% width) = Steering
        if (xPercent < 0.35f)
        {
            // Left half of left zone = steer left
            if (xPercent < 0.175f)
                leftPressed = true;
            else
                rightPressed = true;
        }
        // RIGHT ZONE (65% - 100% width) = Gas/Brake
        else if (xPercent > 0.65f)
        {
            // Top half = Gas, Bottom half = Brake
            if (yPercent > 0.4f)
                gasPressed = true;
            else
                brakePressed = true;
        }
        // MIDDLE ZONE (35% - 65%) = Nitro (tap)
        else
        {
            if (nitro != null) nitro.Activate();
        }
    }

    void OnGUI()
    {
        float sw = Screen.width;
        float sh = Screen.height;

        // Draw zone indicators (semi-transparent)
        Texture2D tex = Texture2D.whiteTexture;

        // Left zone — steering indicators
        float zoneAlpha = 0.15f;
        float activeAlpha = 0.4f;

        // Left arrow zone
        GUI.color = leftPressed ? new Color(1f, 0.8f, 0f, activeAlpha) : new Color(1f, 1f, 1f, zoneAlpha);
        GUI.DrawTexture(new Rect(0, sh * 0.5f, sw * 0.175f, sh * 0.5f), tex);

        // Right arrow zone
        GUI.color = rightPressed ? new Color(1f, 0.8f, 0f, activeAlpha) : new Color(1f, 1f, 1f, zoneAlpha);
        GUI.DrawTexture(new Rect(sw * 0.175f, sh * 0.5f, sw * 0.175f, sh * 0.5f), tex);

        // Gas zone
        GUI.color = gasPressed ? new Color(0f, 1f, 0f, activeAlpha) : new Color(0f, 0.7f, 0f, zoneAlpha);
        GUI.DrawTexture(new Rect(sw * 0.65f, 0, sw * 0.35f, sh * 0.6f), tex);

        // Brake zone
        GUI.color = brakePressed ? new Color(1f, 0f, 0f, activeAlpha) : new Color(0.7f, 0f, 0f, zoneAlpha);
        GUI.DrawTexture(new Rect(sw * 0.65f, sh * 0.6f, sw * 0.35f, sh * 0.4f), tex);

        GUI.color = Color.white;

        // Labels
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = (int)(sh * 0.05f);
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = new Color(1, 1, 1, 0.6f);

        GUI.Label(new Rect(0, sh * 0.7f, sw * 0.175f, sh * 0.1f), "◀", labelStyle);
        GUI.Label(new Rect(sw * 0.175f, sh * 0.7f, sw * 0.175f, sh * 0.1f), "▶", labelStyle);
        GUI.Label(new Rect(sw * 0.65f, sh * 0.2f, sw * 0.35f, sh * 0.1f), "GAS", labelStyle);
        GUI.Label(new Rect(sw * 0.65f, sh * 0.7f, sw * 0.35f, sh * 0.1f), "BRAKE", labelStyle);

        // Nitro zone
        labelStyle.normal.textColor = new Color(0f, 0.8f, 1f, 0.6f);
        GUI.Label(new Rect(sw * 0.35f, sh * 0.8f, sw * 0.3f, sh * 0.1f), "⚡ NITRO", labelStyle);

        // Reset button (top left, small)
        GUIStyle resetStyle = new GUIStyle(GUI.skin.button);
        resetStyle.fontSize = (int)(sh * 0.025f);
        GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);
        if (GUI.Button(new Rect(sw * 0.02f, sh * 0.02f, sw * 0.08f, sh * 0.06f), "↺", resetStyle))
        {
            if (vehicle != null) vehicle.RespawnOnTrack();
        }
        GUI.backgroundColor = Color.white;
    }
}
