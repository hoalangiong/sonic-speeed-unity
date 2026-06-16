using UnityEngine;

/// <summary>
/// Smooth 3rd person chase camera — follows behind and above the car.
/// </summary>
public class ChaseCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // The car

    [Header("Camera Settings")]
    public float distance = 6f;
    public float height = 2.5f;
    public float lookAhead = 3f;
    public float smoothSpeed = 8f;
    public float rotationSmooth = 5f;

    [Header("Speed Effects")]
    public float speedFOVBoost = 15f;    // FOV increases with speed
    public float maxSpeed = 250f;

    [Header("Camera Shake")]
    public float shakeIntensity = 0.15f;
    private float shakeTimer = 0;

    private Camera cam;
    private float baseFOV;

    void Start()
    {
        cam = GetComponent<Camera>();
        baseFOV = cam.fieldOfView;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Desired position: behind the car (car moves in +forward, so behind = -forward)
        // BUT model nose faces +Z = +forward, so behind the car visually = -forward ✓
        // This means camera at -forward sees the BACK of the car ✓
        Vector3 desiredPosition = target.position
            - target.forward * distance
            + Vector3.up * height;

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);

        // Look ahead of car
        Vector3 lookTarget = target.position + target.forward * lookAhead + Vector3.up * 0.5f;
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSmooth);

        // Speed-based FOV
        VehicleController vehicle = target.GetComponent<VehicleController>();
        if (vehicle != null)
        {
            float speedRatio = vehicle.CurrentSpeed / maxSpeed;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, baseFOV + speedFOVBoost * speedRatio, Time.deltaTime * 3f);

            // Camera shake when nitro active
            NitroSystem nitroSys = target.GetComponent<NitroSystem>();
            if (nitroSys != null && nitroSys.IsActive)
            {
                shakeTimer += Time.deltaTime * 30f;
                Vector3 shakeOffset = new Vector3(
                    Mathf.PerlinNoise(shakeTimer, 0) - 0.5f,
                    Mathf.PerlinNoise(0, shakeTimer) - 0.5f,
                    0
                ) * shakeIntensity;
                transform.position += shakeOffset;
            }
        }
    }

    /// <summary>Trigger camera shake from external (e.g. collision)</summary>
    public void TriggerShake(float duration = 0.3f)
    {
        shakeTimer = duration * 30f;
    }
}
