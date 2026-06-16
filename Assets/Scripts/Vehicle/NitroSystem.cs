using UnityEngine;

/// <summary>
/// Nitro boost system — drains when active, refills when drifting.
/// </summary>
public class NitroSystem : MonoBehaviour
{
    [Header("Settings")]
    public float maxNitro = 100f;
    public float drainPerSecond = 30f;
    public float refillPerSecondDrift = 15f;
    public float minToActivate = 20f;

    public float CurrentNitro { get; private set; } = 50f;
    public bool IsActive { get; private set; }

    private VehicleController vehicle;

    void Start()
    {
        vehicle = GetComponent<VehicleController>();
        CurrentNitro = 50f;
    }

    void Update()
    {
        // Drain when active
        if (IsActive)
        {
            CurrentNitro -= drainPerSecond * Time.deltaTime;
            if (CurrentNitro <= 0)
            {
                CurrentNitro = 0;
                Deactivate();
            }
            vehicle.inputNitro = true;
        }
        else
        {
            vehicle.inputNitro = false;
        }

        // Refill when drifting
        if (vehicle.IsDrifting)
        {
            CurrentNitro = Mathf.Min(maxNitro, CurrentNitro + refillPerSecondDrift * Time.deltaTime);
        }
    }

    public void Activate()
    {
        if (CurrentNitro >= minToActivate && !IsActive)
        {
            IsActive = true;
        }
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
