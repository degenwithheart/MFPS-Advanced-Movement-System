using UnityEngine;

#if ADVANCED_MOVEMENT_SLOPE

namespace FC_AdvancedMovement
{
    /// <summary>
    /// Implements tribes-style skiing/slope physics
    /// Requires ADVANCED_MOVEMENT_SLOPE define to be enabled
    /// </summary>
    [RequireComponent(typeof(bl_FirstPersonController))]
    public class SlopeMovement : MonoBehaviour
{
    [Header("Skiing Physics (Tribes-Style)")]
    [Tooltip("Minimum speed required to enter skiing mode.")]
    public float minSpeedToSki = 8f;

    [Tooltip("Absolute maximum horizontal speed (0 = unlimited).")]
    public float maxSpeed = 35f;

    [Tooltip("Gravity force applied when airborne or descending.")]
    public float gravity = 18f;

    [Tooltip("Friction applied during normal ground movement.")]
    public float groundFriction = 4f;

    [Tooltip("Base friction applied while skiing.")]
    public float skiFriction = 0.3f;

    [Tooltip("Directional control on ground.")]
    public float groundControl = 25f;

    [Tooltip("Directional control while skiing.")]
    public float skiControl = 20f;

    [Tooltip("Directional control while airborne.")]
    public float airControl = 1.2f;

    [Tooltip("Force used to keep the player stuck to the ground at high speed.")]
    public float groundStickForce = 20f;

    [Tooltip("How much momentum is retained when going uphill (0–1).")]
    [Range(0f, 1f)]
    public float uphillMomentumRetention = 0.75f;

    [Header("Skiing Balance")]
    [Tooltip("Speed gained when going downhill.")]
    [Range(0.1f, 2f)]
    public float downhillSpeedGain = 0.6f;

    [Tooltip("Minimum slope angle required to gain downhill speed.")]
    public float minSlopeAngleForBoost = 15f;

    [Tooltip("Extra friction applied when turning while skiing.")]
    [Range(0f, 5f)]
    public float turningFriction = 1.5f;

    public bool IsSkiing { get; private set; }
    public float CurrentSpeed { get; private set; }

    private bl_FirstPersonController controller;
    private SlopeJetpack jetpack;

    private Vector3 velocity;
    private Vector3 groundNormal = Vector3.up;
    private bool wasGrounded;
    private Vector3 lastMoveDirection;

    void Start()
    {
        controller = GetComponent<bl_FirstPersonController>();
        jetpack = GetComponent<SlopeJetpack>();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        groundNormal = hit.normal;
    }

    public Vector3 CalculateMovement(Vector2 input, float baseSpeed, bool jumpPressed, bool jumpHeld)
    {
        bool grounded = controller.isGrounded;
        Vector3 inputDir = (transform.forward * input.y + transform.right * input.x).normalized;

        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        CurrentSpeed = horizontalVelocity.magnitude;

        IsSkiing = grounded && CurrentSpeed > minSpeedToSki;

        if (grounded)
        {
            // --- Jump / Ground Stick ---
            if (jumpPressed && !jumpHeld)
            {
                velocity.y = controller.jumpSpeed;
            }
            else if (!jumpPressed && velocity.y < 0f)
            {
                velocity.y = -groundStickForce;
            }

            float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
            bool onSlope = slopeAngle > 5f;

            if (IsSkiing)
            {
                // Speed cap
                if (maxSpeed > 0f && CurrentSpeed > maxSpeed)
                {
                    horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
                    velocity.x = horizontalVelocity.x;
                    velocity.z = horizontalVelocity.z;
                }

                // Slope acceleration / deceleration
                if (onSlope && slopeAngle > minSlopeAngleForBoost)
                {
                    Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
                    float slopeFactor = Mathf.Clamp01((slopeAngle - minSlopeAngleForBoost) / 45f);

                    float verticalDir = Vector3.Dot(velocity.normalized, Vector3.up);

                    if (verticalDir < 0f)
                        velocity += slopeDir * gravity * slopeFactor * downhillSpeedGain * Time.deltaTime;
                    else
                        velocity += slopeDir * gravity * slopeFactor * uphillMomentumRetention * Time.deltaTime;
                }

                // Turning friction
                if (input.magnitude > 0.1f)
                {
                    float angle = Vector3.Angle(lastMoveDirection, inputDir);
                    float turnLoss = Mathf.Clamp01(angle / 90f) * turningFriction;
                    velocity = Vector3.Lerp(velocity, velocity * (1f - turnLoss * Time.deltaTime), Time.deltaTime * 5f);
                }

                float effectiveFriction = skiFriction * (onSlope ? 1f : 2f);
                velocity.x *= (1f - effectiveFriction * Time.deltaTime);
                velocity.z *= (1f - effectiveFriction * Time.deltaTime);

                if (input.magnitude > 0.1f)
                {
                    Vector3 controlForce = inputDir * skiControl * Time.deltaTime;
                    float speedRatio = maxSpeed > 0f ? Mathf.Clamp01(CurrentSpeed / maxSpeed) : 0f;
                    controlForce *= Mathf.Lerp(1f, 0.6f, speedRatio);
                    velocity += controlForce;

                    if (input.y < -0.1f)
                    {
                        Vector3 forwardVel = transform.forward * Vector3.Dot(velocity, transform.forward);
                        velocity -= forwardVel * groundFriction * Time.deltaTime;
                    }

                    lastMoveDirection = inputDir;
                }
            }
            else
            {
                Vector3 targetVel = inputDir * baseSpeed;
                velocity.x = Mathf.Lerp(velocity.x, targetVel.x, groundFriction * Time.deltaTime);
                velocity.z = Mathf.Lerp(velocity.z, targetVel.z, groundFriction * Time.deltaTime);
            }

            wasGrounded = true;
        }
        else
        {
            IsSkiing = false;

            if (maxSpeed > 0f)
            {
                Vector3 flatVel = new Vector3(velocity.x, 0f, velocity.z);
                if (flatVel.magnitude > maxSpeed)
                {
                    flatVel = flatVel.normalized * maxSpeed;
                    velocity.x = flatVel.x;
                    velocity.z = flatVel.z;
                }
            }

            if (jetpack != null && jetpack.IsActive)
            {
                velocity += jetpack.CalculateJetpackForce() * Time.deltaTime;
            }
            else
            {
                velocity.y += Physics.gravity.y * controller.m_GravityMultiplier * Time.deltaTime;
            }

            float airMultiplier = wasGrounded ? airControl * 2.5f : airControl * 1.8f;
            if (input.magnitude > 0.1f)
            {
                velocity += inputDir * groundControl * airMultiplier * Time.deltaTime;
            }

            wasGrounded = false;
        }

        return velocity;
    }
}
}

#endif
