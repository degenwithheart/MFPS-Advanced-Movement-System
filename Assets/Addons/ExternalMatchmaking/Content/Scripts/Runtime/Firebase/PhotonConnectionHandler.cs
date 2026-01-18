using System;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using MFPS.Addons.ExternalMatchmaking.Firebase;

namespace MFPS.Addons.ExternalMatchmaking.Firebase
{
    public class PhotonConnectionHandler : MonoBehaviourPunCallbacks
    {
        public static PhotonConnectionHandler Instance { get; private set; }

        [Header("Settings")]
        public string photonAppId = "your-photon-app-id"; // Set in inspector
        public string photonAppVersion = "1.0";
        public bool autoConnectOnMatchFound = true;

        private MatchAssignment currentMatch;
        private bool isConnecting = false;
        private bool isConnected = false;

        // Events
        public event Action OnConnectedToPhoton;
        public event Action OnJoinedRoom;
        public event Action<string> OnConnectionFailed;
        public event Action<string> OnJoinRoomFailed;

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
            // Subscribe to matchmaker events
            FirebaseMatchmaker.Instance.OnMatchFound += OnMatchFound;
        }

        void OnDestroy()
        {
            if (FirebaseMatchmaker.Instance != null)
            {
                FirebaseMatchmaker.Instance.OnMatchFound -= OnMatchFound;
            }
        }

        // ============================================
        // PUBLIC API
        // ============================================

        /// <summary>
        /// Connect to Photon with specific region and join room
        /// </summary>
        public void ConnectAndJoinRoom(MatchAssignment match)
        {
            if (isConnecting || isConnected)
            {
                Debug.LogWarning("[PhotonHandler] Already connecting or connected");
                return;
            }

            currentMatch = match;
            isConnecting = true;

            Debug.Log($"[PhotonHandler] Connecting to Photon region: {match.photonRegion}");

            // Configure Photon
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = photonAppId;
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = photonAppVersion;
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = match.photonRegion;

            // Connect
            PhotonNetwork.ConnectUsingSettings();
        }

        /// <summary>
        /// Disconnect from Photon
        /// </summary>
        public void Disconnect()
        {
            if (PhotonNetwork.IsConnected)
            {
                Debug.Log("[PhotonHandler] Disconnecting from Photon");
                PhotonNetwork.Disconnect();
            }

            isConnected = false;
            isConnecting = false;
            currentMatch = null;
        }

        // ============================================
        // PHOTON CALLBACKS
        // ============================================

        public override void OnConnectedToMaster()
        {
            Debug.Log("[PhotonHandler] ✓ Connected to Photon Master");

            isConnecting = false;
            isConnected = true;
            OnConnectedToPhoton?.Invoke();

            // Now join the specific room
            if (currentMatch != null)
            {
                Debug.Log($"[PhotonHandler] Joining room: {currentMatch.roomName}");
                PhotonNetwork.JoinRoom(currentMatch.roomName);
            }
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"[PhotonHandler] ✓ Joined room: {currentMatch.roomName}");

            // Report successful join to Firebase
            FirebaseMatchmaker.Instance.ReportMatchJoin(currentMatch.matchId);

            OnJoinedRoom?.Invoke();
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"[PhotonHandler] ✗ Failed to join room: {message}");

            isConnecting = false;
            OnJoinRoomFailed?.Invoke(message);

            // Cleanup
            Disconnect();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"[PhotonHandler] Disconnected: {cause}");

            isConnected = false;
            isConnecting = false;
        }

        public override void OnConnectionFail(DisconnectCause cause)
        {
            Debug.LogError($"[PhotonHandler] Connection failed: {cause}");

            isConnecting = false;
            OnConnectionFailed?.Invoke(cause.ToString());

            // Cleanup
            Disconnect();
        }

        // ============================================
        // PRIVATE METHODS
        // ============================================

        private void OnMatchFound(MatchAssignment match)
        {
            if (!autoConnectOnMatchFound) return;

            Debug.Log("[PhotonHandler] Match found, connecting to Photon");
            ConnectAndJoinRoom(match);
        }
    }
}
