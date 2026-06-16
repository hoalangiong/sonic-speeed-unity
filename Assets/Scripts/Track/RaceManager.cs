using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Race manager — handles laps, checkpoints, ranking, race state.
/// </summary>
public class RaceManager : MonoBehaviour
{
    [Header("Race Settings")]
    public int totalLaps = 3;
    public float checkpointRadius = 20f;

    [Header("References")]
    public Transform player;
    public AIRacer[] aiRacers;
    public Transform[] checkpoints; // Waypoints also act as checkpoints

    // Race state
    public int PlayerLap { get; private set; } = 1;
    public int PlayerCheckpoint { get; private set; } = 0;
    public int PlayerPosition { get; private set; } = 1;
    public bool RaceFinished { get; private set; } = false;
    public float RaceTime { get; private set; } = 0;

    // Lap notification
    public float LapNotifyTime { get; private set; } = -10f;
    public int LapNotifyNumber { get; private set; } = 0;

    // Ramp boost
    public float RampBoostEndTime { get; private set; } = 0;

    private VehicleController playerVehicle;

    void Start()
    {
        if (player != null)
            playerVehicle = player.GetComponent<VehicleController>();
    }

    void Update()
    {
        if (RaceFinished) return;

        RaceTime += Time.deltaTime;

        UpdatePlayerCheckpoints();
        UpdateRanking();
        CheckRampBoost();
    }

    void UpdatePlayerCheckpoints()
    {
        if (player == null || checkpoints == null || checkpoints.Length == 0) return;

        Transform nextCP = checkpoints[PlayerCheckpoint];
        float dist = Vector3.Distance(player.position, nextCP.position);

        if (dist < checkpointRadius)
        {
            PlayerCheckpoint++;
            if (PlayerCheckpoint >= checkpoints.Length)
            {
                PlayerCheckpoint = 0;
                PlayerLap++;
                LapNotifyTime = Time.time;
                LapNotifyNumber = PlayerLap;
                if (PlayerLap > totalLaps)
                {
                    RaceFinished = true;
                }
            }
        }
    }

    void CheckRampBoost()
    {
        if (player == null || playerVehicle == null) return;

        // Detect if player is on a ramp (not grounded + moving fast = just launched off ramp)
        if (!playerVehicle.IsGrounded && playerVehicle.CurrentSpeed > 80f)
        {
            RampBoostEndTime = Time.time + 2f; // 2 second speed boost
        }

        // Apply boost
        if (Time.time < RampBoostEndTime)
        {
            playerVehicle.inputNitro = true; // Reuse nitro multiplier for ramp boost
        }
    }

    void UpdateRanking()
    {
        if (aiRacers == null) return;

        // Calculate progress: lap * checkpoints + currentCheckpoint
        int totalCPs = checkpoints != null ? checkpoints.Length : 1;
        float playerProgress = (PlayerLap - 1) * totalCPs + PlayerCheckpoint;

        int position = 1;
        foreach (var ai in aiRacers)
        {
            float aiProgress = (ai.CurrentLap - 1) * totalCPs + ai.CurrentWaypoint;
            if (aiProgress > playerProgress)
                position++;
        }

        PlayerPosition = position;
    }

    public string GetPositionText()
    {
        switch (PlayerPosition)
        {
            case 1: return "1st";
            case 2: return "2nd";
            case 3: return "3rd";
            default: return PlayerPosition + "th";
        }
    }

    public string GetTimeText()
    {
        int minutes = (int)(RaceTime / 60);
        int seconds = (int)(RaceTime % 60);
        int ms = (int)((RaceTime * 100) % 100);
        return $"{minutes}:{seconds:D2}.{ms:D2}";
    }
}
