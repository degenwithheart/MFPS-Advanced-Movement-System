using System;
using System.Collections;
using UnityEngine;
using MFPS.Addons.ExternalMatchmaking.Firebase;
using TMPro;

namespace MFPS.Addons.ExternalMatchmaking.MFPS
{
    public class MatchmakingManager : MonoBehaviour
    {
        public static MatchmakingManager Instance { get; private set; }

        [Header("Matchmaking Settings")]
        public string gameMode = "TDM"; // TDM, FFA, CTF, DOM
        public string region = "US-West";
        public int skillLevel = 1500;

        [Header("UI References")]
        public GameObject queuePanel;
        public GameObject matchFoundPanel;
        public TextMeshProUGUI queueStatusText;
        public TextMeshProUGUI matchFoundText;

        private bool isInitialized = false;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            Initialize();
        }

        // ============================================
        // INITIALIZATION
        // ============================================

        private void Initialize()
        {
            // Wait for Firebase
            if (FirebaseInitializer.Instance == null)
            {
                Debug.LogError("[MatchmakingManager] FirebaseInitializer not found in scene!");
                return;
            }

            FirebaseInitializer.Instance.OnInitialized += OnFirebaseInitialized;
            FirebaseInitializer.Instance.OnError += OnFirebaseError;

            // Subscribe to matchmaker events
            FirebaseMatchmaker.Instance.OnMatchFound += OnMatchFound;
            FirebaseMatchmaker.Instance.OnQueueStatusChanged += OnQueueStatusChanged;
            FirebaseMatchmaker.Instance.OnError += OnMatchmakingError;

            // Subscribe to Photon events
            PhotonConnectionHandler.Instance.OnConnectedToPhoton += OnConnectedToPhoton;
            PhotonConnectionHandler.Instance.OnJoinedRoom += OnJoinedRoom;
            PhotonConnectionHandler.Instance.OnConnectionFailed += OnPhotonConnectionFailed;
            PhotonConnectionHandler.Instance.OnJoinRoomFailed += OnPhotonJoinFailed;

            // Subscribe to post-match events
            PostMatchHandler.Instance.OnMatchEnded += OnMatchEnded;
            PostMatchHandler.Instance.OnDisconnectedAfterMatch += OnDisconnectedAfterMatch;

            isInitialized = true;
            Debug.Log("[MatchmakingManager] ✓ Initialized successfully");
        }

        // ============================================
        // PUBLIC API
        // ============================================

        /// <summary>
        /// Start matchmaking for the configured game mode
        /// </summary>
        public void StartMatchmaking()
        {
            if (!isInitialized)
            {
                Debug.LogError("[MatchmakingManager] Not initialized yet");
                return;
            }

            Debug.Log($"[MatchmakingManager] Starting matchmaking: {gameMode} in {region}");

            // Show queue UI
            if (queuePanel != null) queuePanel.SetActive(true);
            if (matchFoundPanel != null) matchFoundPanel.SetActive(false);

            // Join queue
            FirebaseMatchmaker.Instance.JoinQueue(gameMode, region, skillLevel);
        }

        /// <summary>
        /// Cancel matchmaking
        /// </summary>
        public void CancelMatchmaking()
        {
            Debug.Log("[MatchmakingManager] Cancelling matchmaking");

            FirebaseMatchmaker.Instance.LeaveQueue();

            // Hide UI
            if (queuePanel != null) queuePanel.SetActive(false);
            if (matchFoundPanel != null) matchFoundPanel.SetActive(false);
        }

        /// <summary>
        /// Call when the match has ended
        /// </summary>
        public void EndMatch(Dictionary<string, object> stats = null)
        {
            PostMatchHandler.Instance.OnMatchEnd(stats);
        }

        // ============================================
        // EVENT HANDLERS
        // ============================================

        private void OnFirebaseInitialized()
        {
            Debug.Log("[MatchmakingManager] Firebase initialized");
        }

        private void OnFirebaseError(string error)
        {
            Debug.LogError($"[MatchmakingManager] Firebase error: {error}");
            ShowError("Firebase initialization failed: " + error);
        }

        private void OnMatchFound(MatchAssignment match)
        {
            Debug.Log($"[MatchmakingManager] Match found: {match.roomName}");

            // Update UI
            if (queuePanel != null) queuePanel.SetActive(false);
            if (matchFoundPanel != null) matchFoundPanel.SetActive(true);
            if (matchFoundText != null)
            {
                matchFoundText.text = $"Match Found!\nRoom: {match.roomName}\nTeam: {match.team}";
            }

            // Set current match for post-match handling
            PostMatchHandler.Instance.SetCurrentMatch(match.matchId);
        }

        private void OnQueueStatusChanged(QueueStatus status)
        {
            if (queueStatusText != null)
            {
                if (status.isInQueue)
                {
                    queueStatusText.text = $"In Queue\nTime: {Mathf.FloorToInt(status.timeInQueue)}s\nStatus: {status.status}";
                }
                else
                {
                    queueStatusText.text = "Not in queue";
                }
            }
        }

        private void OnMatchmakingError(string error)
        {
            Debug.LogError($"[MatchmakingManager] Matchmaking error: {error}");
            ShowError("Matchmaking error: " + error);

            // Hide UI
            if (queuePanel != null) queuePanel.SetActive(false);
            if (matchFoundPanel != null) matchFoundPanel.SetActive(false);
        }

        private void OnConnectedToPhoton()
        {
            Debug.Log("[MatchmakingManager] Connected to Photon");
        }

        private void OnJoinedRoom()
        {
            Debug.Log("[MatchmakingManager] Joined Photon room - match starting!");

            // Hide match found UI (game is starting)
            if (matchFoundPanel != null) matchFoundPanel.SetActive(false);
        }

        private void OnPhotonConnectionFailed(string error)
        {
            Debug.LogError($"[MatchmakingManager] Photon connection failed: {error}");
            ShowError("Failed to connect to game server: " + error);

            // Hide UI
            if (matchFoundPanel != null) matchFoundPanel.SetActive(false);
        }

        private void OnPhotonJoinFailed(string error)
        {
            Debug.LogError($"[MatchmakingManager] Failed to join room: {error}");
            ShowError("Failed to join match: " + error);

            // Hide UI
            if (matchFoundPanel != null) matchFoundPanel.SetActive(false);
        }

        private void OnMatchEnded()
        {
            Debug.Log("[MatchmakingManager] Match ended, disconnecting...");
        }

        private void OnDisconnectedAfterMatch()
        {
            Debug.Log("[MatchmakingManager] Disconnected after match - back to menu");

            // Reset UI state
            if (queuePanel != null) queuePanel.SetActive(false);
            if (matchFoundPanel != null) matchFoundPanel.SetActive(false);
        }

        // ============================================
        // UTILITIES
        // ============================================

        private void ShowError(string message)
        {
            // You can implement your own error display system here
            Debug.LogError($"[MatchmakingManager] ERROR: {message}");

            // For now, just show a simple message
            if (queueStatusText != null)
            {
                queueStatusText.text = $"ERROR:\n{message}";
            }
        }
    }
}