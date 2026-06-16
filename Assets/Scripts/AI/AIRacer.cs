using UnityEngine;

/// <summary>
/// AI racing opponent — follows waypoints along the track.
/// Uses CharacterController same as player for consistent physics.
/// </summary>
public class AIRacer : MonoBehaviour
{
    [Header("Settings")]
    public float baseSpeed = 20f; // m/s (slower to stay visible)
    public float speedVariation = 4f;
    public float turnSpeed = 120f; // Faster turning to stay on track
    public float waypointThreshold = 20f;

    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Rubber-banding")]
    public Transform playerTransform;
    public float rubberBandStrength = 0.5f; // How much AI adjusts speed

    [Header("State")]
    public int CurrentWaypoint { get; private set; } = 0;
    public int CurrentLap { get; private set; } = 1;
    public float CurrentSpeed { get; private set; }

    private CharacterController cc;
    private float currentSpeed;
    private float targetSpeed;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        if (cc == null)
        {
            cc = gameObject.AddComponent<CharacterController>();
            cc.height = 1.0f;
            cc.radius = 0.9f;
            cc.center = new Vector3(0, 0.5f, 0);
            cc.slopeLimit = 45f;
            cc.stepOffset = 0.3f;
        }

        // Randomize speed slightly
        targetSpeed = baseSpeed + Random.Range(-speedVariation, speedVariation);
        currentSpeed = targetSpeed * 0.5f;

        // Remove rigidbody if exists
        var rb = GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Get current target waypoint
        Transform target = waypoints[CurrentWaypoint];
        Vector3 direction = target.position - transform.position;
        direction.y = 0;
        float distance = direction.magnitude;

        // Reached waypoint?
        if (distance < waypointThreshold)
        {
            CurrentWaypoint++;
            if (CurrentWaypoint >= waypoints.Length)
            {
                CurrentWaypoint = 0;
                CurrentLap++;
            }
            // Vary speed each waypoint
            targetSpeed = baseSpeed + Random.Range(-speedVariation, speedVariation);
        }

        // Steer toward waypoint
        if (direction.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        // Accelerate / maintain speed with rubber-banding
        float rubberBandModifier = 1f;
        if (playerTransform != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            // If AI is ahead (further along track) → slow down slightly
            // If AI is behind → speed up
            float playerDist = Vector3.Distance(playerTransform.position, waypoints[CurrentWaypoint].position);
            float aiDist = distance;
            if (aiDist < playerDist)
            {
                // AI is ahead — slow down
                rubberBandModifier = 1f - rubberBandStrength * 0.3f;
            }
            else if (distToPlayer > 50f)
            {
                // AI is far behind — catch up
                rubberBandModifier = 1f + rubberBandStrength * 0.5f;
            }
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed * rubberBandModifier, 15f * Time.deltaTime);

        // Apply movement
        Vector3 move = transform.forward * currentSpeed;
        // Gravity
        if (!cc.isGrounded)
            move.y = -20f;
        else
            move.y = -2f;

        cc.Move(move * Time.deltaTime);

        // Reset if fell off road
        if (transform.position.y < -3f)
        {
            if (waypoints != null && waypoints.Length > 0)
            {
                transform.position = waypoints[CurrentWaypoint].position + Vector3.up * 2f;
            }
        }

        CurrentSpeed = currentSpeed * 3.6f; // km/h
    }
}
