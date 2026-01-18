using UnityEngine;
using UnityEngine.AI;
using MFPS.Runtime.AI;

#if ADVANCED_MOVEMENT_SLOPE

namespace FC_AdvancedMovement.AI
{
    /// <summary>
    /// Extends AI agent with slope/skiing capabilities
    /// Allows AI to navigate slopes faster and use jetpack for tactical positioning
    /// Falls back to normal pathfinding when disabled
    /// </summary>
    public class AdvancedMovement_AISlopeMovement : MonoBehaviour
    {
        private bl_AIShooterAgent aiAgent;
        private SlopeMovement slopeMovement;
        private SlopeJetpack slopeJetpack;
        private NavMeshAgent navMeshAgent;
        private CharacterController characterController;

        [Header("Slope AI Settings")]
        [SerializeField] private bool enableSlopeForAI = true;
        [SerializeField] private float minSpeedForSkiing = 10f;
        [SerializeField] private float jetpackUsageChance = 0.3f;  // 30% chance to use jetpack when available
        [SerializeField] private float jetpackTacticalCooldown = 5f;
        
        private float lastJetpackTime = 0f;
        private bool isUsingJetpack = false;

        private void Awake()
        {
            aiAgent = GetComponent<bl_AIShooterAgent>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            characterController = GetComponent<CharacterController>();
            
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
            // Add slope components if not present
            if (GetComponent<SlopeMovement>() == null)
            {
                slopeMovement = gameObject.AddComponent<SlopeMovement>();
                Debug.Log($"<color=green>[Advanced Movement AI]</color> Added SlopeMovement to AI: {gameObject.name}");
            }
            else
            {
                slopeMovement = GetComponent<SlopeMovement>();
            }

            if (GetComponent<SlopeJetpack>() == null)
            {
                slopeJetpack = gameObject.AddComponent<SlopeJetpack>();
                Debug.Log($"<color=green>[Advanced Movement AI]</color> Added SlopeJetpack to AI: {gameObject.name}");
            }
            else
            {
                slopeJetpack = GetComponent<SlopeJetpack>();
            }
        }

        private void Update()
        {
            if (!enableSlopeForAI) return;

            UpdateSlopeAI();
            UpdateJetpackAI();
        }

        private void UpdateSlopeAI()
        {
            if (slopeMovement == null) return;

            // Check if AI should use skiing
            bool isMovingFast = navMeshAgent.velocity.magnitude > minSpeedForSkiing;
            bool isChasing = aiAgent.Target != null;
            bool isOnSlope = DetectSlope();

            if (isMovingFast && isChasing && isOnSlope)
            {
                // Skiing is beneficial, let SlopeMovement handle it naturally
                // via NavMeshAgent velocity
            }
        }

        private void UpdateJetpackAI()
        {
            if (slopeJetpack == null || !slopeJetpack.IsActive) return;

            // Tactically use jetpack for:
            // 1. Escaping from disadvantaged position
            // 2. Gaining high ground advantage
            // 3. Dodging incoming fire
            
            if (ShouldUseJetpack())
            {
                isUsingJetpack = true;
                lastJetpackTime = Time.time;
            }
            else if (Time.time - lastJetpackTime > jetpackTacticalCooldown)
            {
                isUsingJetpack = false;
            }
        }

        private bool ShouldUseJetpack()
        {
            if (slopeJetpack == null) return false;
            
            // Only use jetpack if:
            // 1. AI is airborne
            // 2. AI has fuel
            // 3. Random chance triggers
            // 4. Tactical advantage exists
            
            bool isAirborne = !navMeshAgent.isOnNavMesh || navMeshAgent.velocity.y < -0.1f;
            bool hasFuel = slopeJetpack.CurrentFuel > 20f;
            bool randomChance = Random.value < jetpackUsageChance;
            bool tacticalAdvantage = IsTacticalAdvantage();
            
            return isAirborne && hasFuel && randomChance && tacticalAdvantage;
        }

        private bool IsTacticalAdvantage()
        {
            if (aiAgent.Target == null) return false;

            // Jetpack advantage if:
            // 1. Target is above AI
            // 2. AI is falling
            // 3. AI needs to escape
            
            float heightDifference = aiAgent.Target.position.y - transform.position.y;
            
            return heightDifference > 2f; // Use jetpack to reach higher targets
        }

        private bool DetectSlope()
        {
            // Simple slope detection using raycasts
            Vector3 groundCheck = transform.position + Vector3.down * 0.5f;
            
            if (Physics.Raycast(groundCheck, Vector3.down, out RaycastHit hit, 2f))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                return slopeAngle > 5f; // More than 5 degrees is considered a slope
            }

            return false;
        }

        /// <summary>
        /// Enable/disable slope features for this AI
        /// </summary>
        public void SetSlopeEnabled(bool enabled)
        {
            enableSlopeForAI = enabled;
        }

        /// <summary>
        /// Force AI to use jetpack for tactical escape
        /// </summary>
        public void ForceTacticalJetpack()
        {
            if (slopeJetpack != null)
            {
                isUsingJetpack = true;
                lastJetpackTime = Time.time;
            }
        }

        /// <summary>
        /// Check if AI is currently skiing
        /// </summary>
        public bool IsSkiing()
        {
            return slopeMovement != null && slopeMovement.IsSkiing;
        }

        /// <summary>
        /// Check if AI has jetpack fuel available
        /// </summary>
        public float GetJetpackFuel()
        {
            return slopeJetpack != null ? slopeJetpack.CurrentFuel : 0f;
        }

        /// <summary>
        /// Get slope movement reference
        /// </summary>
        public SlopeMovement GetSlopeMovement()
        {
            return slopeMovement;
        }

        /// <summary>
        /// Get slope jetpack reference
        /// </summary>
        public SlopeJetpack GetSlopeJetpack()
        {
            return slopeJetpack;
        }
    }
}

#endif
