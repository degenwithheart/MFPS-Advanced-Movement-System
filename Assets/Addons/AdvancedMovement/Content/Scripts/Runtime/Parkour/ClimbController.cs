using UnityEngine;
using System.Collections;

namespace FC_ParkourSystem
{
    /// <summary>
    /// Controls climbing mechanics - hanging, ledge climbing, wall climbing
    /// Requires ADVANCED_MOVEMENT_PARKOUR define to be enabled
    /// </summary>
    public class ClimbController : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private LayerMask climbableLayer;
        [SerializeField] private float climbDetectionRange = 2f;

        [Header("Climbing Settings")]
        [SerializeField] private float climbSpeed = 2f;
        [SerializeField] private float climbUpDuration = 1f;

        [Header("Hang Settings")]
        [SerializeField] private float hangStamina = 10f;
        [SerializeField] private float staminaDrainRate = 1f;
        [SerializeField] private float staminaRecoverRate = 2f;

        public EnvironmentScanner envScanner { get; private set; }
        
        private IParkourCharacter parkourCharacter;
        private Transform playerTransform;
        private Animator animator;
        private ClimbPoint currentClimbPoint;
        private bool isHanging = false;
        private bool isClimbing = false;
        private float currentStamina;

        #region Initialization
        private void Awake()
        {
            parkourCharacter = GetComponent(typeof(IParkourCharacter)) as IParkourCharacter;
            playerTransform = transform;
            animator = GetComponent<Animator>();
            currentStamina = hangStamina;

            envScanner = new EnvironmentScanner(playerTransform, climbableLayer, climbDetectionRange);

            if (parkourCharacter == null)
            {
                Debug.LogError("ClimbController requires IParkourCharacter implementation");
                enabled = false;
            }
        }
        #endregion

        #region Update
        private void Update()
        {
            if (animator == null) return;

            if (isHanging)
            {
                UpdateHanging();
            }
            else if (isClimbing)
            {
                UpdateClimbing();
            }
        }
        #endregion

        #region Hanging Logic
        /// <summary>
        /// Start hanging from a ledge
        /// </summary>
        public void StartHanging(ClimbPoint climbPoint)
        {
            if (isHanging) return;

            currentClimbPoint = climbPoint;
            isHanging = true;
            parkourCharacter.OnStartParkourAction();

            // Set animation based on hang type
            if (climbPoint.isFreeHang)
            {
                animator.SetFloat("freeHang", 1);
                animator.SetTrigger("FreeHang");
            }
            else
            {
                animator.SetFloat("freeHang", 0);
                animator.SetTrigger("BracedHang");
            }

            Debug.Log("Started hanging on ledge");
        }

        /// <summary>
        /// Update hanging state
        /// </summary>
        private void UpdateHanging()
        {
            // Drain stamina while hanging
            currentStamina -= staminaDrainRate * Time.deltaTime;
            
            if (currentStamina <= 0)
            {
                // Out of stamina, fall
                StopHanging();
                return;
            }

            // Check for climb up input
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ClimbUpFromHang();
            }

            // Check for horizontal movement along ledge
            float horizontal = Input.GetAxis("Horizontal");
            if (Mathf.Abs(horizontal) > 0.1f && currentClimbPoint != null)
            {
                MoveAlongLedge(horizontal);
            }

            // Check for drop input
            if (Input.GetKeyDown(KeyCode.C))
            {
                StopHanging();
            }
        }

        /// <summary>
        /// Climb up from hanging position
        /// </summary>
        private void ClimbUpFromHang()
        {
            if (currentClimbPoint == null) return;

            isHanging = false;
            animator.SetTrigger("ClimbUp");

            // Move to climb up position
            StartCoroutine(MoveToPosition(currentClimbPoint.dismountPoint.position, climbUpDuration));
        }

        /// <summary>
        /// Move horizontally along ledge
        /// </summary>
        private void MoveAlongLedge(float direction)
        {
            if (currentClimbPoint == null || currentClimbPoint.connectedPoints.Count == 0)
                return;

            // Find nearest connected point in direction
            ClimbPoint nearestPoint = FindNearestConnectedPoint(direction);
            if (nearestPoint != null)
            {
                currentClimbPoint = nearestPoint;
                StartCoroutine(MoveToPosition(nearestPoint.mountPoint.position, 0.5f));
            }
        }

        /// <summary>
        /// Stop hanging and fall/land
        /// </summary>
        public void StopHanging()
        {
            isHanging = false;
            currentClimbPoint = null;
            parkourCharacter.OnEndParkourAction();

            // Recover stamina after hanging
            StartCoroutine(RecoverStamina());

            Debug.Log("Stopped hanging");
        }
        #endregion

        #region Wall Detection
        /// <summary>
        /// Check if there's a wall behind the player (for braced hang vs free hang)
        /// </summary>
        public WallCheckData? CheckWall(ClimbPoint point)
        {
            if (point == null) return null;

            Vector3 origin = point.transform.position;
            Vector3 direction = -point.transform.forward;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, 1f, climbableLayer))
            {
                return new WallCheckData
                {
                    isWall = true,
                    wallHit = hit,
                    distanceToWall = hit.distance
                };
            }

            return new WallCheckData
            {
                isWall = false,
                distanceToWall = float.MaxValue
            };
        }
        #endregion

        #region Climbing Logic
        /// <summary>
        /// Start climbing a wall
        /// </summary>
        public void StartClimbing(Vector3 climbPosition)
        {
            isClimbing = true;
            parkourCharacter.OnStartParkourAction();
            animator.SetTrigger("StartClimb");

            Debug.Log("Started climbing");
        }

        /// <summary>
        /// Update climbing state
        /// </summary>
        private void UpdateClimbing()
        {
            // Get input
            float vertical = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxis("Horizontal");

            // Apply climbing movement
            Vector3 climbMovement = new Vector3(horizontal, vertical, 0) * climbSpeed * Time.deltaTime;
            playerTransform.Translate(climbMovement, Space.Self);

            // Update animation
            animator.SetFloat("ClimbVertical", vertical);
            animator.SetFloat("ClimbHorizontal", horizontal);

            // Check for climb finish (reached top)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                FinishClimbing();
            }

            // Check for drop
            if (Input.GetKeyDown(KeyCode.C))
            {
                StopClimbing();
            }
        }

        /// <summary>
        /// Finish climbing (reached top)
        /// </summary>
        private void FinishClimbing()
        {
            isClimbing = false;
            animator.SetTrigger("ClimbFinish");
            parkourCharacter.OnEndParkourAction();

            Debug.Log("Finished climbing");
        }

        /// <summary>
        /// Stop climbing and fall
        /// </summary>
        public void StopClimbing()
        {
            isClimbing = false;
            parkourCharacter.OnEndParkourAction();

            Debug.Log("Stopped climbing");
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Get nearest climb point on a ledge
        /// </summary>
        public ClimbPoint GetNearestPoint(Transform ledge, Vector3 position, bool checkConnected = true, bool obstacleCheck = true)
        {
            ClimbPoint[] points = ledge.GetComponentsInChildren<ClimbPoint>();
            
            if (points.Length == 0)
            {
                // Create a temporary climb point if none exists
                GameObject tempPoint = new GameObject("TempClimbPoint");
                tempPoint.transform.position = position;
                tempPoint.transform.parent = ledge;
                tempPoint.transform.forward = -ledge.forward;
                
                ClimbPoint newPoint = tempPoint.AddComponent<ClimbPoint>();
                
                // Create mount and dismount points
                GameObject mount = new GameObject("MountPoint");
                mount.transform.position = position - ledge.forward * 0.3f;
                mount.transform.parent = tempPoint.transform;
                newPoint.mountPoint = mount.transform;
                
                GameObject dismount = new GameObject("DismountPoint");
                dismount.transform.position = position + Vector3.up * 1.5f;
                dismount.transform.parent = tempPoint.transform;
                newPoint.dismountPoint = dismount.transform;
                
                return newPoint;
            }

            ClimbPoint nearest = points[0];
            float minDistance = Vector3.Distance(position, points[0].transform.position);

            foreach (var point in points)
            {
                float distance = Vector3.Distance(position, point.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = point;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Find nearest connected climb point in direction
        /// </summary>
        private ClimbPoint FindNearestConnectedPoint(float direction)
        {
            if (currentClimbPoint == null || currentClimbPoint.connectedPoints.Count == 0)
                return null;

            ClimbPoint nearest = null;
            float minDistance = float.MaxValue;
            Vector3 playerRight = playerTransform.right;

            foreach (var point in currentClimbPoint.connectedPoints)
            {
                Vector3 toPoint = point.transform.position - currentClimbPoint.transform.position;
                float dot = Vector3.Dot(toPoint.normalized, playerRight);

                // Check if point is in the desired direction
                if ((direction > 0 && dot > 0) || (direction < 0 && dot < 0))
                {
                    float distance = toPoint.magnitude;
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = point;
                    }
                }
            }

            return nearest;
        }

        /// <summary>
        /// Move player to target position smoothly
        /// </summary>
        private System.Collections.IEnumerator MoveToPosition(Vector3 targetPosition, float duration)
        {
            Vector3 startPosition = playerTransform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                playerTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            playerTransform.position = targetPosition;
        }

        /// <summary>
        /// Recover stamina over time
        /// </summary>
        private System.Collections.IEnumerator RecoverStamina()
        {
            while (currentStamina < hangStamina)
            {
                currentStamina += staminaRecoverRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, hangStamina);
                yield return null;
            }
        }
        #endregion

        #region Public Accessors
        public bool IsHanging => isHanging;
        public bool IsClimbing => isClimbing;
        public float CurrentStamina => currentStamina;
        public float MaxStamina => hangStamina;
        
        public Vector3 GetClimbVelocity()
        {
            if (currentClimbPoint == null) return Vector3.zero;
            return (currentClimbPoint.transform.position - playerTransform.position).normalized * 2f;
        }
        #endregion

        #region Debug
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (playerTransform == null) playerTransform = transform;

            // Draw detection range
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(playerTransform.position + Vector3.up * 1.5f, climbDetectionRange);

            // Draw current climb point connection
            if (currentClimbPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(playerTransform.position, currentClimbPoint.transform.position);
                Gizmos.DrawWireSphere(currentClimbPoint.transform.position, 0.3f);
            }

            // Draw state info
            UnityEditor.Handles.Label(
                playerTransform.position + Vector3.up * 3.5f,
                $"Hanging: {isHanging}\nClimbing: {isClimbing}\nStamina: {currentStamina:F1}/{hangStamina}"
            );
        }
#endif
        #endregion
    }
}

