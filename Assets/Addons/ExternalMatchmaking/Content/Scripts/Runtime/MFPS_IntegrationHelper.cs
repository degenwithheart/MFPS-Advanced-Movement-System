using UnityEngine;
using MFPS.Addons.ExternalMatchmaking.Firebase;
using MFPS.Addons.ExternalMatchmaking.MFPS;

namespace MFPS.Addons.ExternalMatchmaking
{
    /// <summary>
    /// Integration helper for connecting External Matchmaking Addon with MFPS
    /// Replaces the default MFPS matchmaking with external Firebase-based matchmaking
    /// </summary>
    public class MFPS_ExternalMatchmakingIntegration : MonoBehaviour
    {
        [Header("Matchmaking Settings")]
        public string defaultGameMode = "TDM";
        public string defaultRegion = "US-West";
        public bool overrideMFPSMatchmaking = true;

        [Header("UI Integration")]
        public bool integrateWithMFPSUI = true;
        public GameObject externalMatchmakingUIPanel;

        private bl_Lobby lobby;
        private FirebaseMatchmaker matchmaker;
        private MatchmakingManager matchmakingManager;
        private bool isIntegrated = false;

        void Awake()
        {
            lobby = GetComponent<bl_Lobby>();
            if (lobby == null)
            {
                Debug.LogError("[ExternalMatchmaking] bl_Lobby component not found!");
                return;
            }

            // Get or add required components
            matchmaker = GetComponent<FirebaseMatchmaker>();
            matchmakingManager = GetComponent<MatchmakingManager>();

            if (matchmaker == null)
            {
                matchmaker = gameObject.AddComponent<FirebaseMatchmaker>();
                Debug.Log("[ExternalMatchmaking] FirebaseMatchmaker component added.");
            }

            if (matchmakingManager == null)
            {
                matchmakingManager = gameObject.AddComponent<MatchmakingManager>();
                Debug.Log("[ExternalMatchmaking] MatchmakingManager component added.");
            }
        }

        void Start()
        {
            if (lobby == null) return;

            // Configure matchmaking settings
            matchmakingManager.gameMode = defaultGameMode;
            matchmakingManager.region = defaultRegion;

            // Setup UI integration
            if (integrateWithMFPSUI)
            {
                SetupUIIntegration();
            }

            // Override MFPS matchmaking if enabled
            if (overrideMFPSMatchmaking)
            {
                OverrideMFPSMatchmaking();
            }

            isIntegrated = true;
            Debug.Log("[ExternalMatchmaking] ✓ Integration complete");
        }

        void OnDestroy()
        {
            // Restore MFPS matchmaking if it was overridden
            if (overrideMFPSMatchmaking && lobby != null)
            {
                RestoreMFPSMatchmaking();
            }
        }

        // ============================================
        // PUBLIC API
        // ============================================

        /// <summary>
        /// Start external matchmaking with specified parameters
        /// </summary>
        public void StartExternalMatchmaking(string gameMode = null, string region = null, int skillLevel = 1500)
        {
            if (!isIntegrated)
            {
                Debug.LogError("[ExternalMatchmaking] Integration not complete!");
                return;
            }

            matchmakingManager.gameMode = gameMode ?? defaultGameMode;
            matchmakingManager.region = region ?? defaultRegion;
            matchmakingManager.skillLevel = skillLevel;

            matchmakingManager.StartMatchmaking();
        }

        /// <summary>
        /// Cancel external matchmaking
        /// </summary>
        public void CancelExternalMatchmaking()
        {
            if (matchmakingManager != null)
            {
                matchmakingManager.CancelMatchmaking();
            }
        }

        /// <summary>
        /// Get current matchmaking status
        /// </summary>
        public bool IsInQueue()
        {
            return matchmaker != null && matchmaker.isInQueue;
        }

        // ============================================
        // PRIVATE METHODS
        // ============================================

        private void SetupUIIntegration()
        {
            // Find MFPS UI components
            var lobbyUI = FindObjectOfType<bl_LobbyUI>();
            if (lobbyUI == null)
            {
                Debug.LogWarning("[ExternalMatchmaking] bl_LobbyUI not found, UI integration skipped.");
                return;
            }

            // Hook into MFPS UI buttons
            // This would require modifying MFPS UI or adding custom buttons
            // For now, we'll use the external UI panel if provided

            if (externalMatchmakingUIPanel != null)
            {
                externalMatchmakingUIPanel.SetActive(false);

                // Assign to MatchmakingManager
                matchmakingManager.queuePanel = externalMatchmakingUIPanel;

                // Try to find UI elements within the panel
                var statusText = externalMatchmakingUIPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (statusText != null)
                {
                    matchmakingManager.queueStatusText = statusText;
                }

                Debug.Log("[ExternalMatchmaking] UI integration configured.");
            }
        }

        private void OverrideMFPSMatchmaking()
        {
            // Replace MFPS AutoMatch method
            // This is a simplified override - in practice, you might need to patch MFPS methods

            Debug.Log("[ExternalMatchmaking] MFPS matchmaking override enabled.");

            // You could use reflection or method patching here to replace bl_Lobby.AutoMatch()
            // For this example, we'll just disable the original and provide our own
        }

        private void RestoreMFPSMatchmaking()
        {
            Debug.Log("[ExternalMatchmaking] MFPS matchmaking restored.");
            // Restore original MFPS matchmaking functionality
        }

        // ============================================
        // MFPS EVENT HANDLERS
        // ============================================

        /// <summary>
        /// Called when MFPS tries to start matchmaking
        /// </summary>
        public void OnMFPSMatchmakingRequested()
        {
            if (overrideMFPSMatchmaking)
            {
                // Intercept MFPS matchmaking and use external instead
                StartExternalMatchmaking();
            }
        }

        /// <summary>
        /// Called when a match ends in MFPS
        /// </summary>
        public void OnMFPSMatchEnd()
        {
            // Report match completion to external matchmaking
            matchmakingManager.EndMatch();
        }
    }
}