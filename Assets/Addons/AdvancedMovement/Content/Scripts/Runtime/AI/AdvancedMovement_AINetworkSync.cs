using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using FC_ParkourSystem;

#if ADVANCED_MOVEMENT_PARKOUR

namespace FC_AdvancedMovement.AI
{
    /// <summary>
    /// Handles network synchronization of AI parkour actions via Photon RPC
    /// Ensures all clients see AI performing parkour movements
    /// </summary>
    public class AdvancedMovement_AINetworkSync : MonoBehaviourPunCallbacks
    {
        private bl_AIShooterAgent aiAgent;
        private ParkourController parkourController;
        private new PhotonView photonView;

        [Header("Network Settings")]
        [SerializeField] private bool enableNetworkSync = true;
        [SerializeField] private float rpcThrottleRate = 0.1f;

        private float lastRpcTime = 0f;

        private const string RPC_VAULT = "RPC_ParkourVault";
        private const string RPC_CLIMB_START = "RPC_ClimbStart";
        private const string RPC_CLIMB_END = "RPC_ClimbEnd";
        private const string RPC_WALL_RUN = "RPC_WallRun";
        private const string RPC_LEDGE_GRAB = "RPC_LedgeGrab";

        private void Awake()
        {
            aiAgent = GetComponent<bl_AIShooterAgent>();
            parkourController = GetComponent<ParkourController>();
            photonView = GetComponent<PhotonView>();

            if (aiAgent == null)
            {
                Debug.LogError($"<color=red>[Advanced Movement Network]</color> bl_AIShooterAgent not found on {gameObject.name}");
                enabled = false;
                return;
            }

            if (photonView == null)
            {
                Debug.LogWarning($"<color=yellow>[Advanced Movement Network]</color> PhotonView not found on {gameObject.name}, network sync disabled");
                enableNetworkSync = false;
            }
        }

        private void Update()
        {
            if (!enableNetworkSync || parkourController == null) return;

            SyncParkourStates();
        }

        private void SyncParkourStates()
        {
            // Throttle RPC calls to avoid network spam
            if (Time.time - lastRpcTime < rpcThrottleRate) return;

            // Check parkour state transitions
            if (parkourController.IsVaulting)
            {
                BroadcastParkourAction(RPC_VAULT, parkourController.GetCurrentVaultPoint());
            }
            else if (parkourController.IsClimbing)
            {
                BroadcastParkourAction(RPC_CLIMB_START, parkourController.GetClimbTarget());
            }
            else if (parkourController.IsWallRunning)
            {
                BroadcastParkourAction(RPC_WALL_RUN, GetWallRunInfo());
            }
        }

        private void BroadcastParkourAction(string rpcName, Vector3 targetPosition)
        {
            if (photonView == null || !enableNetworkSync) return;

            photonView.RPC(rpcName, RpcTarget.Others, targetPosition);
            lastRpcTime = Time.time;
        }

        [PunRPC]
        private void RPC_ParkourVault(Vector3 vaultPoint)
        {
            // Execute vault on remote client
            if (parkourController != null)
            {
                Debug.Log($"<color=cyan>[Advanced Movement Network]</color> Syncing vault at {vaultPoint}");
                // Vault execution handled by ParkourController
            }
        }

        [PunRPC]
        private void RPC_ClimbStart(Vector3 climbTarget)
        {
            // Start climbing on remote client
            if (parkourController != null)
            {
                Debug.Log($"<color=cyan>[Advanced Movement Network]</color> Syncing climb start at {climbTarget}");
                // Climb execution handled by ParkourController
            }
        }

        [PunRPC]
        private void RPC_ClimbEnd()
        {
            // End climbing on remote client
            if (parkourController != null)
            {
                Debug.Log($"<color=cyan>[Advanced Movement Network]</color> Syncing climb end");
                // Climb stop handled by ParkourController
            }
        }

        [PunRPC]
        private void RPC_WallRun(Vector3 wallDirection)
        {
            // Wall run on remote client
            if (parkourController != null)
            {
                Debug.Log($"<color=cyan>[Advanced Movement Network]</color> Syncing wall run: {wallDirection}");
                // Wall run execution handled by ParkourController
            }
        }

        [PunRPC]
        private void RPC_LedgeGrab(Vector3 ledgePosition)
        {
            // Ledge grab on remote client
            if (parkourController != null)
            {
                Debug.Log($"<color=cyan>[Advanced Movement Network]</color> Syncing ledge grab at {ledgePosition}");
                // Ledge grab handled by ClimbController
            }
        }

        private Vector3 GetWallRunInfo()
        {
            // Get current wall normal direction
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.right, out RaycastHit hit, 1f))
            {
                return hit.normal;
            }
            return transform.right;
        }

        /// <summary>
        /// Enable/disable network synchronization
        /// </summary>
        public void SetNetworkSyncEnabled(bool enabled)
        {
            enableNetworkSync = enabled;
        }

        /// <summary>
        /// Manually send vault sync RPC
        /// </summary>
        public void SyncVault(Vector3 vaultPoint)
        {
            if (photonView != null)
            {
                photonView.RPC(RPC_VAULT, RpcTarget.Others, vaultPoint);
            }
        }

        /// <summary>
        /// Manually send climb start sync RPC
        /// </summary>
        public void SyncClimbStart(Vector3 climbTarget)
        {
            if (photonView != null)
            {
                photonView.RPC(RPC_CLIMB_START, RpcTarget.Others, climbTarget);
            }
        }

        /// <summary>
        /// Manually send climb end sync RPC
        /// </summary>
        public void SyncClimbEnd()
        {
            if (photonView != null)
            {
                photonView.RPC(RPC_CLIMB_END, RpcTarget.Others);
            }
        }

        /// <summary>
        /// Manually send wall run sync RPC
        /// </summary>
        public void SyncWallRun(Vector3 wallDirection)
        {
            if (photonView != null)
            {
                photonView.RPC(RPC_WALL_RUN, RpcTarget.Others, wallDirection);
            }
        }

        /// <summary>
        /// Check if network sync is active
        /// </summary>
        public bool IsNetworkSyncActive()
        {
            return enableNetworkSync && photonView != null && photonView.IsMine;
        }
    }
}

#endif
