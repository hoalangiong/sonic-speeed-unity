using UnityEngine;

/// <summary>
/// Engine audio — synthesizes V12-like sound using AudioSource pitch shifting.
/// Attach an AudioSource with an engine loop clip.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class EngineAudio : MonoBehaviour
{
    [Header("Vehicle")]
    public VehicleController vehicle;

    [Header("Audio Settings")]
    public float minPitch = 0.5f;
    public float maxPitch = 2.5f;
    public float minVolume = 0.3f;
    public float maxVolume = 0.9f;
    public float pitchSmooth = 5f;

    private AudioSource audioSource;
    private float targetPitch;
    private float targetVolume;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.Play();
    }

    void Update()
    {
        if (vehicle == null) return;

        float speedRatio = vehicle.CurrentSpeed / vehicle.maxSpeed;

        // Pitch: idle (low) → redline (high)
        targetPitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);

        // Volume: louder when accelerating
        targetVolume = Mathf.Lerp(minVolume, maxVolume, speedRatio);
        if (vehicle.inputGas > 0) targetVolume += 0.1f;

        // Nitro boost — extra pitch
        if (vehicle.inputNitro) targetPitch += 0.3f;

        // Smooth transitions
        audioSource.pitch = Mathf.Lerp(audioSource.pitch, targetPitch, Time.deltaTime * pitchSmooth);
        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * pitchSmooth);
    }
}
