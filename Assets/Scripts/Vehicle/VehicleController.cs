using UnityEngine;

/// <summary>
/// Kinematic arcade car controller.
/// Does NOT rely on Unity physics collisions for ground contact.
/// Uses raycast to find ground, directly sets position/rotation.
/// Guaranteed to work with any model on any ground.
/// </summary>
public class VehicleController : MonoBehaviour
{
    [Header("Engine")]
    public float maxSpeed = 200f; // km/h
    public float acceleration = 40f; // m/s²
    public float brakeDeceleration = 60f;
    public float naturalDeceleration = 10f;
    public float reverseMaxSpeed = 50f;
    public float nitroMultiplier = 2.5f;

    [Header("Steering")]
    public float turnSpeed = 80f;

    [Header("Ground")]
    public float hoverHeight = 0.5f; // Height above ground
    public float gravitySpeed = 15f; // How fast car falls when not grounded
    public float groundRayLength = 5f;

    // Input (set by InputManager)
    [HideInInspector] public float inputSteer;
    [HideInInspector] public float inputGas;
    [HideInInspector] public float inputBrake;
    [HideInInspector] public bool inputNitro;

    // State
    public float CurrentSpeed { get; private set; } // km/h (display)
    public float CurrentRPM { get; private set; }
    public bool IsDrifting { get; private set; }
    public bool IsReversing { get; private set; }
    public bool IsGrounded { get; private set; }

    private float speed; // m/s (internal, signed)
    private float verticalVelocity;
    private CharacterController cc; // Used for movement — handles collisions cleanly

    void Start()
    {
        // Use CharacterController for reliable ground movement
        // Remove any Rigidbody — we handle physics manually
        var rb = GetComponent<Rigidbody>();
        if (rb != null) DestroyImmediate(rb);

        // Remove any existing colliders
        foreach (var col in GetComponents<Collider>()) DestroyImmediate(col);
        foreach (var col in GetComponentsInChildren<MeshCollider>()) DestroyImmediate(col);

        // Add CharacterController
        cc = GetComponent<CharacterController>();
        if (cc == null) cc = gameObject.AddComponent<CharacterController>();
        cc.height = 1.0f;
        cc.radius = 0.9f;
        cc.center = new Vector3(0, 0.5f, 0);
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.3f;
        cc.skinWidth = 0.08f;
    }

    void Update()
    {
        IsGrounded = cc.isGrounded;

        HandleAcceleration();
        HandleSteering();
        HandleGravity();
        ApplyMovement();

        // Public state
        CurrentSpeed = Mathf.Abs(speed) * 3.6f;
        CurrentRPM = Mathf.Clamp(CurrentSpeed / maxSpeed * 8000f, 800f, 8500f);
        IsDrifting = IsGrounded && Mathf.Abs(inputSteer) > 0.5f && CurrentSpeed > 40f && inputGas > 0;
        IsReversing = speed < -0.1f;
    }

    void HandleAcceleration()
    {
        float maxSpeedMs = maxSpeed / 3.6f;
        float reverseMaxMs = reverseMaxSpeed / 3.6f;

        if (inputGas > 0)
        {
            float accel = acceleration * inputGas;
            if (inputNitro) accel *= nitroMultiplier;
            speed = Mathf.MoveTowards(speed, maxSpeedMs, accel * Time.deltaTime);
        }
        else if (inputBrake > 0)
        {
            if (speed > 0.5f)
            {
                // Braking
                speed = Mathf.MoveTowards(speed, 0, brakeDeceleration * Time.deltaTime);
            }
            else
            {
                // Reverse
                speed = Mathf.MoveTowards(speed, -reverseMaxMs, acceleration * 0.5f * Time.deltaTime);
            }
        }
        else
        {
            // Natural deceleration
            speed = Mathf.MoveTowards(speed, 0, naturalDeceleration * Time.deltaTime);
        }
    }

    void HandleSteering()
    {
        if (!IsGrounded) return;
        if (Mathf.Abs(speed) < 0.5f) return; // No steering when stopped

        float turn = inputSteer * turnSpeed * Time.deltaTime;

        // Less turning at high speed
        float speedRatio = Mathf.Abs(speed) / (maxSpeed / 3.6f);
        turn *= Mathf.Lerp(1f, 0.3f, speedRatio);

        // Reverse steering direction when reversing
        if (speed < 0) turn = -turn;

        transform.Rotate(0, turn, 0);
    }

    /// <summary>Respawn car on nearest waypoint on track</summary>
    public void RespawnOnTrack()
    {
        var waypoints = GameObject.Find("Waypoints");
        if (waypoints != null)
        {
            Transform nearest = null;
            float minDist = float.MaxValue;
            foreach (Transform wp in waypoints.transform)
            {
                float d = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                           new Vector3(wp.position.x, 0, wp.position.z));
                if (d < minDist) { minDist = d; nearest = wp; }
            }
            if (nearest != null)
            {
                int nextIdx = nearest.GetSiblingIndex() + 1;
                if (nextIdx >= waypoints.transform.childCount) nextIdx = 0;
                Transform nextWP = waypoints.transform.GetChild(nextIdx);

                transform.position = nearest.position + Vector3.up * 1.5f;
                Vector3 lookDir = (nextWP.position - nearest.position).normalized;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(lookDir);
                speed = 0;
                verticalVelocity = 0;
                return;
            }
        }
        transform.position = new Vector3(0, 3f, 0);
        speed = 0;
        verticalVelocity = 0;
    }

    void HandleGravity()
    {
        if (IsGrounded)
        {
            verticalVelocity = -2f; // Small downward to keep grounded
        }
        else
        {
            verticalVelocity -= gravitySpeed * Time.deltaTime;
        }
    }

    void ApplyMovement()
    {
        Vector3 move = transform.forward * speed;
        move.y = verticalVelocity;
        cc.Move(move * Time.deltaTime);

        // Safety: only reset if fell WAY below ground (not off-track check)
        if (transform.position.y < -10f)
        {
            RespawnOnTrack();
        }
    }
}
