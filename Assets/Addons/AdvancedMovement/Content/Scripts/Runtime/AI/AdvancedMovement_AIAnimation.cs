using UnityEngine;
using FC_ParkourSystem;
using FC_AdvancedMovement;

#if ADVANCED_MOVEMENT_PARKOUR || ADVANCED_MOVEMENT_SLOPE

namespace FC_AdvancedMovement.AI
{
    /// <summary>
    /// Extends AI animation system with parkour and slope-specific animations
    /// Handles animation parameter synchronization for advanced movement features
    /// </summary>
    public class AdvancedMovement_AIAnimation : MonoBehaviour
    {
        private bl_AIAnimation aiAnimation;
        private Animator animator;
        private ParkourController parkourController;
        private ClimbController climbController;
        private SlopeMovement slopeMovement;

        [Header("Animation Hash Caching")]
        private int hashIsClimbing;
        private int hashIsVaulting;
        private int hashIsWallRunning;
        private int hashClimbDirection;
        private int hashIsSkiing;
        private int hashSkiSpeed;

        [Header("Animation Settings")]
        [SerializeField] private float climbAnimationSpeed = 1f;
        [SerializeField] private float skiAnimationSpeed = 1f;

        private void Awake()
        {
            aiAnimation = GetComponent<bl_AIAnimation>();
            animator = GetComponent<Animator>();
            parkourController = GetComponent<ParkourController>();
            climbController = GetComponent<ClimbController>();
            
            // Try to get SlopeMovement if available
            var slopeType = System.Type.GetType("FC_AdvancedMovement.SlopeMovement");
            if (slopeType != null)
            {
                slopeMovement = GetComponent(slopeType) as SlopeMovement;
            }

            if (animator == null)
            {
                Debug.LogError($"<color=red>[Advanced Movement AI Animation]</color> Animator not found on {gameObject.name}");
                enabled = false;
                return;
            }

            CacheAnimationHashes();
        }

        private void CacheAnimationHashes()
        {
            hashIsClimbing = Animator.StringToHash("IsClimbing");
            hashIsVaulting = Animator.StringToHash("IsVaulting");
            hashIsWallRunning = Animator.StringToHash("IsWallRunning");
            hashClimbDirection = Animator.StringToHash("ClimbDirection");
            hashIsSkiing = Animator.StringToHash("IsSkiing");
            hashSkiSpeed = Animator.StringToHash("SkiSpeed");

            Debug.Log($"<color=cyan>[Advanced Movement AI Animation]</color> Animation hashes cached for {gameObject.name}");
        }

        private void Update()
        {
            if (animator == null) return;

            UpdateParkourAnimations();
            UpdateSlopeAnimations();
        }

        private void UpdateParkourAnimations()
        {
            if (parkourController == null) return;

#if ADVANCED_MOVEMENT_PARKOUR
            // Update climbing animations
            bool isClimbing = parkourController.IsClimbing;
            animator.SetBool(hashIsClimbing, isClimbing);

            if (isClimbing)
            {
                // Set climb direction based on input/controller state
                float climbDir = GetClimbDirection();
                animator.SetFloat(hashClimbDirection, climbDir, 0.1f, Time.deltaTime);
                animator.speed = climbAnimationSpeed;
            }

            // Update vaulting animations
            bool isVaulting = parkourController.IsVaulting;
            animator.SetBool(hashIsVaulting, isVaulting);

            // Update wall running animations
            bool isWallRunning = parkourController.IsWallRunning;
            animator.SetBool(hashIsWallRunning, isWallRunning);

            // Reset animator speed if not in parkour state
            if (!isClimbing && !isVaulting && !isWallRunning)
            {
                animator.speed = 1f;
            }
#endif
        }

        private void UpdateSlopeAnimations()
        {
            if (slopeMovement == null) return;

#if ADVANCED_MOVEMENT_SLOPE
            // Update skiing animations
            bool isSkiing = slopeMovement.IsSkiing;
            animator.SetBool(hashIsSkiing, isSkiing);

            if (isSkiing)
            {
                // Set ski speed based on velocity
                float velocity = slopeMovement.CurrentSpeed;
                float normalizedSpeed = Mathf.Clamp01(velocity / slopeMovement.maxSpeed);
                
                animator.SetFloat(hashSkiSpeed, normalizedSpeed, 0.1f, Time.deltaTime);
                animator.speed = skiAnimationSpeed;
            }
            else
            {
                animator.speed = 1f;
            }
#endif
        }

        private float GetClimbDirection()
        {
            // Returns: -1 (down), 0 (idle), 1 (up)
            // AI should climb up unless escaping danger
            
            if (climbController == null) return 0f;

            // Check if AI is climbing upward or downward
            Vector3 climbVelocity = climbController.GetClimbVelocity();
            
            if (climbVelocity.magnitude < 0.1f)
                return 0f; // Idle
            
            return climbVelocity.y > 0 ? 1f : -1f;
        }

        /// <summary>
        /// Trigger climbing animation state
        /// </summary>
        public void TriggerClimbAnimation(bool climbing)
        {
            if (animator != null)
            {
                animator.SetBool(hashIsClimbing, climbing);
            }
        }

        /// <summary>
        /// Trigger vaulting animation state
        /// </summary>
        public void TriggerVaultAnimation(bool vaulting)
        {
            if (animator != null)
            {
                animator.SetBool(hashIsVaulting, vaulting);
            }
        }

        /// <summary>
        /// Trigger wall running animation state
        /// </summary>
        public void TriggerWallRunAnimation(bool wallRunning)
        {
            if (animator != null)
            {
                animator.SetBool(hashIsWallRunning, wallRunning);
            }
        }

        /// <summary>
        /// Trigger skiing animation state
        /// </summary>
        public void TriggerSkiAnimation(bool skiing)
        {
            if (animator != null)
            {
                animator.SetBool(hashIsSkiing, skiing);
            }
        }

        /// <summary>
        /// Set climb animation speed multiplier
        /// </summary>
        public void SetClimbAnimationSpeed(float speed)
        {
            climbAnimationSpeed = Mathf.Clamp(speed, 0.5f, 2f);
        }

        /// <summary>
        /// Set ski animation speed multiplier
        /// </summary>
        public void SetSkiAnimationSpeed(float speed)
        {
            skiAnimationSpeed = Mathf.Clamp(speed, 0.5f, 2f);
        }

        /// <summary>
        /// Reset all advanced movement animation states
        /// </summary>
        public void ResetAdvancedMovementAnimations()
        {
            if (animator == null) return;

            animator.SetBool(hashIsClimbing, false);
            animator.SetBool(hashIsVaulting, false);
            animator.SetBool(hashIsWallRunning, false);
            animator.SetBool(hashIsSkiing, false);
            animator.speed = 1f;
        }
    }
}

#endif
