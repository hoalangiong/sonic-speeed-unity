using UnityEngine;

/// <summary>
/// Checkpoint and lap tracking system.
/// Place checkpoint triggers along the track.
/// </summary>
public class CheckpointSystem : MonoBehaviour
{
    [Header("Settings")]
    public int totalLaps = 3;
    public Transform[] checkpoints;

    public int CurrentLap { get; private set; } = 1;
    public int CurrentCheckpoint { get; private set; } = 0;
    public int TotalCheckpoints => checkpoints != null ? checkpoints.Length : 0;

    public System.Action<int> OnLapComplete;
    public System.Action<int> OnCheckpointReached;

    private float raceStartTime;
    private float[] lapTimes;

    void Start()
    {
        raceStartTime = Time.time;
        lapTimes = new float[totalLaps];
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Checkpoint")) return;

        int cpIndex = System.Array.IndexOf(checkpoints, other.transform);
        if (cpIndex < 0) return;

        // Must hit checkpoints in order
        if (cpIndex == CurrentCheckpoint)
        {
            CurrentCheckpoint++;
            OnCheckpointReached?.Invoke(CurrentCheckpoint);

            // Completed a lap
            if (CurrentCheckpoint >= checkpoints.Length)
            {
                lapTimes[CurrentLap - 1] = Time.time - raceStartTime;
                CurrentCheckpoint = 0;

                if (CurrentLap < totalLaps)
                {
                    CurrentLap++;
                    OnLapComplete?.Invoke(CurrentLap);
                }
                else
                {
                    // Race finished
                    OnLapComplete?.Invoke(-1); // -1 = finished
                }
            }
        }
    }

    public float GetCurrentLapTime()
    {
        return Time.time - raceStartTime;
    }

    public float GetBestLapTime()
    {
        float best = float.MaxValue;
        foreach (float t in lapTimes)
        {
            if (t > 0 && t < best) best = t;
        }
        return best == float.MaxValue ? 0 : best;
    }
}
