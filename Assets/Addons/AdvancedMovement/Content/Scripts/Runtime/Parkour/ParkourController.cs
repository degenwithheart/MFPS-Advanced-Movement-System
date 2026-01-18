using UnityEngine;
using System.Collections;

namespace FC_ParkourSystem
{
    /// <summary>
    /// Controls parkour actions like vaulting, climbing, and stepping up
    /// Requires ADVANCED_MOVEMENT_PARKOUR define to be enabled
    /// </summary>
    public class ParkourController : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float forwardRayLength = 1.5f;
        [SerializeField] private float heightRayLength = 3f;
        [SerializeField] private float forwardRayOffset = 0.5f;

        [Header("Parkour Heights")]
        [SerializeField] private float vaultHeight = 1.0f;
        [SerializeField] private float stepUpHeight = 0.6f;
        [SerializeField] private float mediumStepUpHeight = 1.2f;
        [SerializeField] private float climbUpHeight = 2.0f;

        [Header("Speed Requirements")]
        [SerializeField] private float minWallRunSpeed = 5f;

        [Header("Animation Settings")]
        [SerializeField] private float animationTransitionTime = 0.15f;

        [Header("Wall Run Settings")]
        [SerializeField] private float wallRunDuration = 2f;
        [SerializeField] private float wallRunSpeed = 6f;
        [SerializeField] private float wallRunGravity = 2f;

        private IParkourCharacter parkourCharacter;
        private Transform playerTransform;
        private Animator animator;
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private ParkourAction currentAction;
        private bool inAction = false;

        public bool ControlledByParkour => inAction;
        public bool InAction => inAction;

        #region Parkour Actions Enum
        public enum ParkourAction
        {
            None,
            StepUp,
            MediumStepUp,
            VaultOver,
            VaultOn,
            ClimbUp,
            WallRun,
            VerticalJump
        }
        #endregion

        #region Initialization
        private void Awake()
        {
            parkourCharacter = GetComponent(typeof(IParkourCharacter)) as IParkourCharacter;
            playerTransform = transform;
            animator = GetComponent<Animator>();

            if (parkourCharacter == null)
            {
                Debug.LogError("ParkourController requires a component implementing IParkourCharacter");
                enabled = false;
                return;
            }

            if (animator == null)
            {
                Debug.LogError("ParkourController requires an Animator component");
                enabled = false;
                return;
            }
        }
        #endregion

        #region Main Parkour Detection
        /// <summary>
        /// Check if any parkour action is possible and execute it
        /// </summary>
        public bool CheckForParkour()
        {
            if (inAction || parkourCharacter.PreventParkourAction)
                return false;

            // Check for obstacles in front
            if (!DetectObstacle(out RaycastHit hit))
                return false;

            // Determine what parkour action to perform based on obstacle height
            float obstacleHeight = hit.point.y - playerTransform.position.y;
            Vector3 obstacleNormal = hit.normal;

            // Check if there's space on top of the obstacle
            bool hasSpaceOnTop = CheckSpaceOnTop(hit.point, out Vector3 topPosition);

            // Determine action based on height and space
            if (obstacleHeight <= stepUpHeight)
            {
                PerformStepUp(topPosition, obstacleNormal);
                return true;
            }
            else if (obstacleHeight <= mediumStepUpHeight)
            {
                PerformMediumStepUp(topPosition, obstacleNormal);
                return true;
            }
            else if (obstacleHeight <= vaultHeight && hasSpaceOnTop)
            {
                PerformVaultOver(topPosition, obstacleNormal);
                return true;
            }
            else if (obstacleHeight <= climbUpHeight)
            {
                PerformClimbUp(topPosition, obstacleNormal);
                return true;
            }

            // Check for wall run
            if (CanWallRun(hit))
            {
                PerformWallRun(hit);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Perform vertical jump
        /// </summary>
        public void VerticalJump()
        {
            if (inAction || !parkourCharacter.IsGrounded)
                return;

            StartCoroutine(ExecuteVerticalJump());
        }
        #endregion

        #region Detection Methods
        private bool DetectObstacle(out RaycastHit hit)
        {
            Vector3 origin = playerTransform.position + Vector3.up * forwardRayOffset;
            Vector3 direction = playerTransform.forward;

            return Physics.Raycast(origin, direction, out hit, forwardRayLength, obstacleLayer);
        }

        private bool CheckSpaceOnTop(Vector3 obstaclePoint, out Vector3 topPosition)
        {
            // Cast ray from above the obstacle downward to find the top surface
            Vector3 rayOrigin = obstaclePoint + Vector3.up * heightRayLength + playerTransform.forward * 0.5f;
            
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, heightRayLength * 2f, obstacleLayer))
            {
                topPosition = hit.point;
                return true;
            }

            topPosition = obstaclePoint + Vector3.up * vaultHeight;
            return false;
        }

        private bool CanWallRun(RaycastHit hit)
        {
            // Check if player is moving fast enough
            if (parkourCharacter == null) return false;

            float speed = GetComponent<bl_FirstPersonController>()?.VelocityMagnitude ?? 0f;
            if (speed < minWallRunSpeed) return false;

            // Check if wall is suitable (vertical enough)
            float wallAngle = Vector3.Angle(hit.normal, Vector3.up);
            return wallAngle > 70f && wallAngle < 110f;
        }
        #endregion

        #region Parkour Action Execution
        private void PerformStepUp(Vector3 targetPos, Vector3 normal)
        {
            currentAction = ParkourAction.StepUp;
            targetPosition = targetPos;
            targetRotation = Quaternion.LookRotation(-normal);
            StartCoroutine(ExecuteParkourAction("StepUp", 0.2f, 0.5f));
        }

        private void PerformMediumStepUp(Vector3 targetPos, Vector3 normal)
        {
            currentAction = ParkourAction.MediumStepUp;
            targetPosition = targetPos;
            targetRotation = Quaternion.LookRotation(-normal);
            StartCoroutine(ExecuteParkourAction("MediumStepUp", 0.25f, 0.6f));
        }

        private void PerformVaultOver(Vector3 targetPos, Vector3 normal)
        {
            currentAction = ParkourAction.VaultOver;
            targetPosition = targetPos + playerTransform.forward * 1.5f; // Land on the other side
            targetRotation = Quaternion.LookRotation(-normal);
            StartCoroutine(ExecuteParkourAction("VaultOver", 0.3f, 0.7f));
        }

        private void PerformClimbUp(Vector3 targetPos, Vector3 normal)
        {
            currentAction = ParkourAction.ClimbUp;
            targetPosition = targetPos;
            targetRotation = Quaternion.LookRotation(-normal);
            StartCoroutine(ExecuteParkourAction("ClimbUp", 0.35f, 0.75f));
        }

        private void PerformWallRun(RaycastHit hit)
        {
            currentAction = ParkourAction.WallRun;
            Vector3 wallNormal = hit.normal;
            Vector3 wallDirection = Vector3.Cross(wallNormal, Vector3.up);
            
            // Determine if running left or right on wall
            float dot = Vector3.Dot(wallDirection, playerTransform.right);
            if (dot < 0) wallDirection = -wallDirection;

            StartCoroutine(ExecuteWallRun(wallDirection, wallNormal));
        }
        #endregion

        #region Coroutines
        private IEnumerator ExecuteParkourAction(string animationName, float matchStart, float matchEnd)
        {
            inAction = true;
            parkourCharacter.OnStartParkourAction();

            // Play animation
            animator.CrossFade(animationName, animationTransitionTime);
            
            // Wait for animation to start
            yield return null;
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
            {
                yield return null;
            }

            // Match target position and rotation
            animator.MatchTarget(
                targetPosition,
                targetRotation,
                AvatarTarget.Root,
                new MatchTargetWeightMask(Vector3.one, 1f),
                matchStart,
                matchEnd
            );

            // Wait for animation to complete
            float normalizedTime = 0f;
            while (normalizedTime < 1f)
            {
                if (animator.IsInTransition(0))
                {
                    yield return null;
                    continue;
                }

                normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                yield return null;
            }

            // End parkour action
            inAction = false;
            currentAction = ParkourAction.None;
            parkourCharacter.OnEndParkourAction();
        }

        private IEnumerator ExecuteVerticalJump()
        {
            inAction = true;
            animator.SetTrigger("Jump");

            // Use the IParkourCharacter's jump handler
            yield return parkourCharacter.HandleVerticalJump();

            // Short delay for jump to execute
            yield return new WaitForSeconds(0.3f);

            inAction = false;
        }

        private IEnumerator ExecuteWallRun(Vector3 wallDirection, Vector3 wallNormal)
        {
            inAction = true;
            parkourCharacter.OnStartParkourAction();

            // Determine wall run side (left or right)
            bool isLeftWall = Vector3.Dot(wallDirection, playerTransform.right) < 0;
            animator.SetBool("WallRunLeft", isLeftWall);
            animator.SetTrigger("WallRun");

            float elapsedTime = 0f;
            CharacterController controller = GetComponent<CharacterController>();
            Vector3 velocity = wallDirection * wallRunSpeed;

            // Execute wall run
            while (elapsedTime < wallRunDuration)
            {
                // Check if still near wall
                if (!Physics.Raycast(playerTransform.position, -wallNormal, 1.5f, obstacleLayer))
                    break;

                // Apply movement
                velocity.y -= wallRunGravity * Time.deltaTime;
                if (controller != null && controller.enabled)
                {
                    controller.Move(velocity * Time.deltaTime);
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // End wall run
            animator.SetBool("WallRunLeft", false);
            inAction = false;
            currentAction = ParkourAction.None;
            parkourCharacter.OnEndParkourAction();
        }
        #endregion

        #region Public Accessors
        public bool IsVaulting => inAction && (currentAction == ParkourAction.VaultOver || currentAction == ParkourAction.VaultOn);
        public bool IsClimbing => inAction && (currentAction == ParkourAction.ClimbUp || currentAction == ParkourAction.VerticalJump);
        public bool IsWallRunning => inAction && currentAction == ParkourAction.WallRun;
        
        public Vector3 GetCurrentVaultPoint() => targetPosition;
        public Vector3 GetClimbTarget() => targetPosition;
        #endregion

        #region Debug
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (playerTransform == null) playerTransform = transform;

            // Draw forward detection ray
            Vector3 origin = playerTransform.position + Vector3.up * forwardRayOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + playerTransform.forward * forwardRayLength);

            // Draw target position when in action
            if (inAction)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(targetPosition, 0.3f);
                Gizmos.DrawLine(playerTransform.position, targetPosition);
            }

            // Draw current action label
            UnityEditor.Handles.Label(
                playerTransform.position + Vector3.up * 3f,
                $"Parkour Action: {currentAction}\nIn Action: {inAction}"
            );
        }
#endif
        #endregion
    }
}

