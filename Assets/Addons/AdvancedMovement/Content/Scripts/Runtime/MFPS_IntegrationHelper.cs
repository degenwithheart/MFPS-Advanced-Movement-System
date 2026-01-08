using UnityEngine;
using MFPS.InputManager;
using FC_ParkourSystem;
using Photon.Pun;
using System.Collections;

namespace FC_AdvancedMovement
{
    /// <summary>
    /// Integration helper for connecting FC Advanced Movement System with MFPS
    /// Handles parkour actions, slope skiing, and state synchronization
    /// Supports both Parkour and Slope modules independently
    /// </summary>
    public class MFPS_IntegrationHelper : MonoBehaviour, IParkourCharacter
    {
        #region Components
        private ParkourController parkourController;
        private ClimbController climbController;
        private bl_FirstPersonController firstPersonController;
        private bl_PlayerAnimationsBase playerAnimations;
        private bl_PlayerRagdollBase playerRagdoll;
        private CharacterController characterController;
        private bl_PlayerCameraSwitcher cameraSwitcher;
        private bl_PlayerNetwork playerNetwork;
        private bl_PlayerReferences playerReferences;
        private Camera playerCamera;
        private PhotonView photonView;
        #endregion

        #region State Management
        private PlayerState lastBodyState = PlayerState.Idle;
        private bool isParkourActive = false;
        private bool isClimbingActive = false;
        private Coroutine currentParkourCoroutine;
        #endregion

        #region IParkourCharacter Implementation
        public bool UseRootMotion { get; set; } = true; // Enable root motion for parkour animations

        public Vector3 MoveDir
        {
            get
            {
                if (firstPersonController == null) return Vector3.zero;
                float horizontal = bl_GameInput.Horizontal;
                float vertical = bl_GameInput.Vertical;
                return new Vector3(horizontal, 0, vertical).normalized;
            }
        }

        public bool IsGrounded => firstPersonController != null && firstPersonController.isGrounded;

        public float Gravity => Physics.gravity.y;

        public Animator Animator
        {
            get => playerReferences?.PlayerAnimator;
            set
            {
                if (playerReferences != null)
                    playerReferences.PlayerAnimator = value;
            }
        }

        public bool PreventParkourAction
        {
            get
            {
                if (firstPersonController == null) return true;
                
                // Prevent parkour during these states
                return firstPersonController.State == PlayerState.Sliding ||
                       firstPersonController.State == PlayerState.InVehicle ||
                       isParkourActive ||
                       isClimbingActive;
            }
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
        }

        private void OnEnable()
        {
            // Subscribe to MFPS events if needed
            bl_EventHandler.onLocalPlayerSpawn += OnLocalPlayerSpawn;
            bl_EventHandler.onLocalPlayerDeath += OnLocalPlayerDeath;
        }

        private void OnDisable()
        {
            // Unsubscribe from MFPS events
            bl_EventHandler.onLocalPlayerSpawn -= OnLocalPlayerSpawn;
            bl_EventHandler.onLocalPlayerDeath -= OnLocalPlayerDeath;
        }

        private void Update()
        {
            if (!photonView.IsMine) return;

#if ADVANCED_MOVEMENT_PARKOUR
            HandleParkourInputs();
#endif
            UpdateAnimatorParameters();

            // Only update FPC if not controlled by parkour
            if (!isParkourActive && firstPersonController != null)
            {
                firstPersonController.OnUpdate();
            }
        }

        private void FixedUpdate()
        {
            if (!photonView.IsMine) return;

            // Handle mouse look during parkour
            if (isParkourActive && firstPersonController != null)
            {
                firstPersonController.mouseLook.Update();
                firstPersonController.mouseLook.UpdateLook(
                    firstPersonController.transform, 
                    firstPersonController.headRoot
                );
            }
            else if (firstPersonController != null)
            {
                firstPersonController.FixedUpdate();
            }

            UpdateCamera();
        }

        private void LateUpdate()
        {
            if (!photonView.IsMine) return;

            if (!isParkourActive && firstPersonController != null)
            {
                firstPersonController.OnLateUpdate();
            }
        }
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            Debug.Log("<color=cyan>[Advanced Movement]</color> Initializing Integration Helper...");

            photonView = GetComponent<PhotonView>();
            playerReferences = GetComponent<bl_PlayerReferences>();

            if (playerReferences == null)
            {
                Debug.LogError("<color=red>[Advanced Movement]</color> bl_PlayerReferences is missing on " + gameObject.name);
                enabled = false;
                return;
            }

            // Get all required components
            firstPersonController = playerReferences.firstPersonController as bl_FirstPersonController;
            playerAnimations = playerReferences.playerAnimations;
            playerRagdoll = playerReferences.playerRagdoll;
            characterController = playerReferences.characterController;
            cameraSwitcher = GetComponent<bl_PlayerCameraSwitcher>();
            playerNetwork = playerReferences.playerNetwork as bl_PlayerNetwork;
            playerCamera = playerReferences.playerCamera;

#if ADVANCED_MOVEMENT_PARKOUR
            parkourController = GetComponent<ParkourController>();
            climbController = GetComponent<ClimbController>();
#endif

            if (AreRequiredComponentsMissing())
            {
                Debug.LogError("<color=red>[Advanced Movement]</color> Missing required components for MFPS_IntegrationHelper on " + gameObject.name);
                enabled = false;
                return;
            }

            // Configure animator for parkour
            if (Animator != null)
            {
                Animator.applyRootMotion = false; // Will be enabled during parkour actions
            }

            Debug.Log("<color=green>[Advanced Movement]</color> Integration Helper initialized successfully");
        }

        private bool AreRequiredComponentsMissing()
        {
            bool missing = firstPersonController == null ||
                          playerAnimations == null || 
                          characterController == null ||
                          playerCamera == null ||
                          photonView == null;

            if (missing)
            {
                if (firstPersonController == null) Debug.LogError("<color=red>[Advanced Movement]</color> bl_FirstPersonController is missing");
                if (playerAnimations == null) Debug.LogError("<color=red>[Advanced Movement]</color> PlayerAnimations is missing");
                if (characterController == null) Debug.LogError("<color=red>[Advanced Movement]</color> CharacterController is missing");
                if (playerCamera == null) Debug.LogError("<color=red>[Advanced Movement]</color> PlayerCamera is missing");
                if (photonView == null) Debug.LogError("<color=red>[Advanced Movement]</color> PhotonView is missing");
            }

#if ADVANCED_MOVEMENT_PARKOUR
            bool parkourMissing = parkourController == null || climbController == null;
            if (parkourMissing)
            {
                if (parkourController == null) Debug.LogWarning("<color=yellow>[Advanced Movement]</color> ParkourController is missing (Parkour module disabled)");
                if (climbController == null) Debug.LogWarning("<color=yellow>[Advanced Movement]</color> ClimbController is missing (Parkour module disabled)");
            }
#endif

            return missing;
        }
        #endregion

        #region Input Handling
#if ADVANCED_MOVEMENT_PARKOUR
        private void HandleParkourInputs()
        {
            if (firstPersonController == null || parkourController == null) return;

            // Don't handle inputs during active parkour
            if (isParkourActive) return;

            // Jump/Vault input
            if (bl_GameInput.Jump(GameInputType.Down))
            {
                HandleJumpOrVault();
            }

            // Drop/Hang input
            if (bl_GameInput.Interact(GameInputType.Down))
            {
                HandleDropToHang();
            }

            // Slide input (crouch while running)
            if (bl_GameInput.Crouch(GameInputType.Down) && 
                firstPersonController.State == PlayerState.Running &&
                firstPersonController.VelocityMagnitude > firstPersonController.WalkSpeed)
            {
                HandleSlide();
            }
        }

        private void HandleJumpOrVault()
        {
            if (!IsGrounded || PreventParkourAction) return;

            // Check if parkour action is possible
            if (parkourController.CheckForParkour())
            {
                // Parkour controller will handle the action
                return;
            }

            // Otherwise, do normal jump
            if (firstPersonController != null && firstPersonController.isGrounded)
            {
                firstPersonController.DoJump();
            }
        }

        private void HandleDropToHang()
        {
            if (climbController == null || isParkourActive) return;

            // Check if we can drop to hang
            if (climbController.envScanner.DropLedgeCheck(transform.forward, out ClimbLedgeData ledgeData))
            {
                var currentLedge = ledgeData.ledgeHit.transform;
                var newPoint = climbController.GetNearestPoint(
                    currentLedge, 
                    ledgeData.ledgeHit.point, 
                    false, 
                    obstacleCheck: false
                );

                if (newPoint == null) return;

                OnStartParkourAction();

                // Check if it's a wall or free hang
                var wallCheck = climbController.CheckWall(newPoint);
                if (wallCheck.HasValue && wallCheck.Value.isWall)
                {
                    // Braced hang (wall behind)
                    Animator.SetFloat("freeHang", 0);
                    currentParkourCoroutine = StartCoroutine(
                        MoveToTarget(
                            newPoint.transform, 
                            "DropToHang", 
                            0.50f, 
                            0.90f, 
                            rotateToLedge: true, 
                            matchStart: AvatarTarget.LeftFoot
                        )
                    );
                    firstPersonController.State = PlayerState.BracedHangClimb;
                }
                else
                {
                    // Free hang (no wall behind)
                    Animator.SetFloat("freeHang", 1);
                    currentParkourCoroutine = StartCoroutine(
                        MoveToTarget(
                            newPoint.transform, 
                            "DropToFreeHang", 
                            0.50f, 
                            0.89f, 
                            rotateToLedge: true, 
                            matchStart: AvatarTarget.LeftFoot
                        )
                    );
                    firstPersonController.State = PlayerState.FreeHangClimb;
                }

                isClimbingActive = true;
            }
        }

        private void HandleSlide()
        {
            if (firstPersonController != null && firstPersonController.canSlide)
            {
                firstPersonController.DoSlide();
                Debug.Log("<color=cyan>[Advanced Movement]</color> Slide initiated");
            }
        }
#endif
        #endregion

        #region Parkour Action Management
        public void OnStartParkourAction()
        {
            isParkourActive = true;

            // Disable FPC
            if (firstPersonController != null)
            {
                firstPersonController.Stop();
                firstPersonController.enabled = false;
            }

            // Disable character controller collision
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            // Disable camera switcher
            if (cameraSwitcher != null)
            {
                cameraSwitcher.enabled = false;
            }

            // Disable ragdoll physics for animation matching
            if (playerRagdoll != null)
            {
                playerRagdoll.SetActiveRagdollPhysics(false); // Keep kinematic for animation
            }

            // Hide weapons during parkour
            if (playerReferences.gunManager != null)
            {
                playerReferences.gunManager.BlockAllWeapons();
            }

            // Enable root motion for parkour animation
            if (Animator != null)
            {
                Animator.applyRootMotion = true;
            }

            Debug.Log("<color=cyan>[Advanced Movement]</color> Parkour action started");
        }

        public void OnEndParkourAction()
        {
            isParkourActive = false;
            isClimbingActive = false;

            // Re-enable FPC
            if (firstPersonController != null)
            {
                firstPersonController.enabled = true;
            }

            // Re-enable character controller
            if (characterController != null)
            {
                characterController.enabled = true;
            }

            // Re-enable camera switcher
            if (cameraSwitcher != null)
            {
                cameraSwitcher.enabled = true;
            }

            // Disable ragdoll physics
            if (playerRagdoll != null)
            {
                playerRagdoll.SetActiveRagdollPhysics(false);
            }

            // Show weapons again
            if (playerReferences.gunManager != null)
            {
                playerReferences.gunManager.ReleaseWeapons(false);
            }

            // Disable root motion, return to FPC control
            if (Animator != null)
            {
                Animator.applyRootMotion = false;
            }

            // Reset to idle state if grounded
            if (firstPersonController != null && firstPersonController.isGrounded)
            {
                firstPersonController.State = PlayerState.Idle;
            }

            currentParkourCoroutine = null;

            Debug.Log("<color=cyan>[Advanced Movement]</color> Parkour action ended");
        }

#if ADVANCED_MOVEMENT_PARKOUR
        private IEnumerator MoveToTarget(
            Transform target, 
            string animationState, 
            float matchStartTime, 
            float matchTargetTime, 
            bool rotateToLedge, 
            AvatarTarget matchStart)
        {
            // Start animation
            if (Animator != null)
            {
                Animator.CrossFade(animationState, 0.1f);

                // Wait for animation to start
                yield return null;
                while (!Animator.GetCurrentAnimatorStateInfo(0).IsName(animationState))
                {
                    yield return null;
                }

                // Match target position and rotation
                if (rotateToLedge)
                {
                    Animator.MatchTarget(
                        target.position, 
                        target.rotation, 
                        matchStart, 
                        new MatchTargetWeightMask(Vector3.one, 1f), 
                        matchStartTime, 
                        matchTargetTime
                    );
                }
                else
                {
                    Animator.MatchTarget(
                        target.position, 
                        transform.rotation, 
                        matchStart, 
                        new MatchTargetWeightMask(Vector3.one, 0f), 
                        matchStartTime, 
                        matchTargetTime
                    );
                }

                // Wait for animation to complete
                float normalizedTime = 0f;
                while (normalizedTime < 1f)
                {
                    if (Animator.IsInTransition(0))
                    {
                        yield return null;
                        continue;
                    }

                    normalizedTime = Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    yield return null;
                }
            }

            OnEndParkourAction();
        }
#endif

        public IEnumerator HandleVerticalJump()
        {
            if (firstPersonController != null && firstPersonController.isGrounded)
            {
                if (Animator != null)
                    Animator.SetTrigger("Jump");
                firstPersonController.DoJump();
            }
            yield break;
        }
        #endregion

        #region Animation & State Updates
        private void UpdateAnimatorParameters()
        {
            if (Animator == null || firstPersonController == null) return;

            // Update basic movement parameters
            Animator.SetFloat("Speed", firstPersonController.VelocityMagnitude);
            Animator.SetFloat("HorizontalSpeed", firstPersonController.GetLocalVelocity().x);
            Animator.SetFloat("VerticalSpeed", firstPersonController.Velocity.y);
            Animator.SetBool("IsGrounded", firstPersonController.isGrounded);
            Animator.SetBool("IsJumping", firstPersonController.State == PlayerState.Jumping);

            // Update body state
            UpdateBodyState();
        }

        private void UpdateBodyState()
        {
            if (firstPersonController == null) return;

            PlayerState currentState = firstPersonController.State;

            // Only update if state changed
            if (currentState == lastBodyState) return;

            // Map PlayerState to animator integer
            int bodyStateValue = GetBodyStateValue(currentState);
            Animator.SetInteger("BodyState", bodyStateValue);

            lastBodyState = currentState;

            Debug.Log($"<color=cyan>[Advanced Movement]</color> BodyState updated: {currentState} -> {bodyStateValue}");
        }

        private int GetBodyStateValue(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.Idle: return 0;
                case PlayerState.Walking: return 1;
                case PlayerState.Running: return 2;
                case PlayerState.Crouching: return 3;
                case PlayerState.Jumping: return 4;
                case PlayerState.Climbing: return 5;
                case PlayerState.Sliding: return 6;
                case PlayerState.Dropping: return 7;
                case PlayerState.Gliding: return 8;
                case PlayerState.InVehicle: return 9;
                case PlayerState.Stealth: return 10;
                case PlayerState.VaultOver: return 11;
                case PlayerState.VaultOn: return 12;
                case PlayerState.MediumStepUp: return 13;
                case PlayerState.ClimbUp: return 14;
                case PlayerState.StepUp: return 15;
                case PlayerState.MediumStepUpM: return 16;
                case PlayerState.LandFromFall: return 17;
                case PlayerState.LandAndStepForward: return 18;
                case PlayerState.LandOnSpot: return 19;
                case PlayerState.FallingToRoll: return 20;
                case PlayerState.FreeHangClimb: return 21;
                case PlayerState.BracedHangClimb: return 22;
                case PlayerState.JumpDown: return 23;
                case PlayerState.JumpFromHang: return 24;
                case PlayerState.JumpFromFreeHang: return 25;
                case PlayerState.BracedHangTryJumpUp: return 26;
                case PlayerState.WallRun: return 27;
                case PlayerState.Skiing: return 28;
                default: return 0;
            }
        }
        #endregion

        #region Camera Management
        private void UpdateCamera()
        {
            if (playerCamera == null || firstPersonController == null) return;

            // During parkour, smoothly follow the player with third-person offset
            if (isParkourActive)
            {
                Vector3 targetPosition = firstPersonController.transform.position + 
                                        firstPersonController.transform.up * 1.6f - 
                                        firstPersonController.transform.forward * 2.5f;

                playerCamera.transform.position = Vector3.Lerp(
                    playerCamera.transform.position, 
                    targetPosition, 
                    Time.deltaTime * 8f
                );

                // Look at player
                Vector3 lookTarget = firstPersonController.transform.position + Vector3.up * 1.6f;
                playerCamera.transform.LookAt(lookTarget);
            }
        }
        #endregion

        #region Event Handlers
        private void OnLocalPlayerSpawn()
        {
            // Reset states on spawn
            isParkourActive = false;
            isClimbingActive = false;
            lastBodyState = PlayerState.Idle;

            if (currentParkourCoroutine != null)
            {
                StopCoroutine(currentParkourCoroutine);
                currentParkourCoroutine = null;
            }

            OnEndParkourAction();
        }

        private void OnLocalPlayerDeath()
        {
            // Clean up on death
            if (currentParkourCoroutine != null)
            {
                StopCoroutine(currentParkourCoroutine);
                currentParkourCoroutine = null;
            }

            isParkourActive = false;
            isClimbingActive = false;
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Force end any active parkour action (for debugging or emergency stops)
        /// </summary>
        public void ForceEndParkourAction()
        {
            if (currentParkourCoroutine != null)
            {
                StopCoroutine(currentParkourCoroutine);
                currentParkourCoroutine = null;
            }

            OnEndParkourAction();
        }

        /// <summary>
        /// Check if currently performing a parkour action
        /// </summary>
        public bool IsParkourActive()
        {
            return isParkourActive;
        }

        /// <summary>
        /// Check if currently in climbing state
        /// </summary>
        public bool IsClimbing()
        {
            return isClimbingActive;
        }
        #endregion

        #region Debug
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (firstPersonController == null) return;

            // Draw velocity direction
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                transform.position, 
                transform.position + firstPersonController.Velocity
            );

            // Draw move direction
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                transform.position, 
                transform.position + MoveDir * 2f
            );

            // Draw state label
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2.5f,
                $"State: {firstPersonController.State}\nParkour: {isParkourActive}\nClimbing: {isClimbingActive}"
            );
        }
#endif
        #endregion
    }
}
