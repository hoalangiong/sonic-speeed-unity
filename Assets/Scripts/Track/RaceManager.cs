using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Race manager — SIMPLE checkpoint + lap system.
/// NEVER teleports the player. Only tracks progress.
/// Checkpoints must be hit in order. Lap = all checkpoints cleared once.
/// </summary>
public class RaceManager : MonoBehaviour
{
    [Header("Race Settings")]
    public int totalLaps = 3;
    public float checkpointRadius = 15f;

    [Header("References")]
    public Transform player;
    public AIRacer[] aiRacers;
    public Transform[] checkpoints;

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

    // Internal — track which checkpoints player has passed this lap
    private bool[] checkpointsPassed;
    private int totalCheckpointsPassed = 0;

    private VehicleController playerVehicle;

    void Start()
    {
        if (player != null)
            playerVehicle = player.GetComponent<VehicleController>();

        if (checkpoints != null)
            checkpointsPassed = new bool[checkpoints.Length];
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

        // Only check the NEXT expected checkpoint (must be sequential)
        int nextCP = PlayerCheckpoint;
        if (nextCP >= checkpoints.Length) return; // All passed this lap

        Transform cpTransform = checkpoints[nextCP];
        if (cpTransform == null) return;

        float dist = Vector3.Distance(
            new Vector3(player.position.x, 0, player.position.z),
            new Vector3(cpTransform.position.x, 0, cpTransform.position.z)
        );

        if (dist < checkpointRadius)
        {
            // Mark this checkpoint as passed
            checkpointsPassed[nextCP] = true;
            PlayerCheckpoint++;
            totalCheckpointsPassed++;

            // Check if all checkpoints passed = 1 lap complete
            if (PlayerCheckpoint >= checkpoints.Length)
            {
                CompleteLap();
            }
        }
    }

    void CompleteLap()
    {
        PlayerLap++;
        LapNotifyTime = Time.time;
        LapNotifyNumber = PlayerLap;

        // Reset for next lap
        PlayerCheckpoint = 0;
        checkpointsPassed = new bool[checkpoints.Length];

        if (PlayerLap > totalLaps)
        {
            RaceFinished = true;
        }
    }

    void UpdateRanking()
    {
        if (aiRacers == null) return;

        int totalCPs = checkpoints != null ? checkpoints.Length : 1;
        float playerProgress = (PlayerLap - 1) * totalCPs + PlayerCheckpoint;

        int position = 1;
        foreach (var ai in aiRacers)
        {
            if (ai == null) continue;
            float aiProgress = (ai.CurrentLap - 1) * totalCPs + ai.CurrentWaypoint;
            if (aiProgress > playerProgress)
                position++;
        }

        PlayerPosition = position;
    }

    void CheckRampBoost()
    {
        if (player == null || playerVehicle == null) return;

        if (!playerVehicle.IsGrounded && playerVehicle.CurrentSpeed > 80f)
        {
            RampBoostEndTime = Time.time + 2f;
        }

        if (Time.time < RampBoostEndTime)
        {
            playerVehicle.inputNitro = true;
        }
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
