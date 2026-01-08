using UnityEngine;
using UnityEngine.AI;
using MFPS.Runtime.AI;
using FC_ParkourSystem;

#if ADVANCED_MOVEMENT_PARKOUR

namespace FC_AdvancedMovement.AI
{
    /// <summary>
    /// Extends AI agent with parkour capabilities
    /// Allows AI to vault over obstacles and climb when parkour module is enabled
    /// Falls back to normal pathfinding when disabled
    /// </summary>
    public class AdvancedMovement_AIShooterAgent : MonoBehaviour
    {
        private bl_AIShooterAgent aiAgent;
        private ParkourController parkourController;
        private EnvironmentScanner environmentScanner;
        private NavMeshAgent navMeshAgent;
        
        [Header("Parkour AI Settings")]
        [SerializeField] private bool enableParkourForAI = true;
        [SerializeField] private LayerMask parkourObstacleLayer;
        [SerializeField] private float parkourDetectionRange = 3f;
        [SerializeField] private float parkourCooldown = 2f;
        
        private float lastParkourTime = 0f;
        private bool isParkourAvailable = true;

        private void Awake()
        {
            aiAgent = GetComponent<bl_AIShooterAgent>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            
            if (aiAgent == null)
            {
                Debug.LogError($"<color=red>[Advanced Movement AI]</color> bl_AIShooterAgent not found on {gameObject.name}");
                enabled = false;
                return;
            }

            InitializeAdvancedMovement();
        }

        private void InitializeAdvancedMovement()
        {
            // Add parkour components if not present
            if (GetComponent<ParkourController>() == null)
            {
                parkourController = gameObject.AddComponent<ParkourController>();
                Debug.Log($"<color=green>[Advanced Movement AI]</color> Added ParkourController to AI: {gameObject.name}");
            }
            else
            {
                parkourController = GetComponent<ParkourController>();
            }

            environmentScanner = new FC_ParkourSystem.EnvironmentScanner(
                transform,
                parkourObstacleLayer,
                parkourDetectionRange
            );
        }

        private void Update()
        {
            if (!enableParkourForAI || !isParkourAvailable) return;

            UpdateParkourCooldown();
            
            // Check if AI should attempt parkour
            if (ShouldAttemptParkour())
            {
                if (parkourController.CheckForParkour())
                {
                    lastParkourTime = Time.time;
                    isParkourAvailable = false;
                    Debug.Log($"<color=cyan>[Advanced Movement AI]</color> AI {gameObject.name} performing parkour action");
                }
            }
        }

        private bool ShouldAttemptParkour()
        {
            // Only attempt parkour when:
            // 1. AI is grounded
            // 2. AI is moving (chasing target)
            // 3. Cooldown has passed
            // 4. Not in combat situation where parkour would be disadvantageous
            
            if (aiAgent == null) return false;
            
            bool isMoving = navMeshAgent.velocity.magnitude > 0.5f;
            bool isTargeting = aiAgent.Target != null;
            bool cooldownExpired = Time.time - lastParkourTime >= parkourCooldown;
            
            return isMoving && isTargeting && cooldownExpired;
        }

        private void UpdateParkourCooldown()
        {
            if (!isParkourAvailable && Time.time - lastParkourTime >= parkourCooldown)
            {
                isParkourAvailable = true;
            }
        }

        /// <summary>
        /// Check if there's a parkour opportunity in front of the AI
        /// </summary>
        public bool CheckParkourAhead(out float obstacleHeight)
        {
            obstacleHeight = 0f;
            
            if (environmentScanner == null) return false;

            if (environmentScanner.DropLedgeCheck(transform.forward, out FC_ParkourSystem.ClimbLedgeData ledgeData))
            {
                obstacleHeight = ledgeData.ledgeHit.point.y - transform.position.y;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Force AI to attempt parkour action
        /// </summary>
        public void ForceParkourAction()
        {
            if (parkourController != null && !parkourController.InAction)
            {
                parkourController.CheckForParkour();
            }
        }

        /// <summary>
        /// Enable/disable parkour for this AI
        /// </summary>
        public void SetParkourEnabled(bool enabled)
        {
            enableParkourForAI = enabled;
        }

        /// <summary>
        /// Get parkour controller reference
        /// </summary>
        public ParkourController GetParkourController()
        {
            return parkourController;
        }

        /// <summary>
        /// Get environment scanner reference
        /// </summary>
        public FC_ParkourSystem.EnvironmentScanner GetEnvironmentScanner()
        {
            return environmentScanner;
        }
    }
}

#endif
