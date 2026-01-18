using System;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Matchmaking.Firebase;

namespace MFPS.Addons.ExternalMatchmaking.MFPS
{
    public class PostMatchHandler : MonoBehaviourPunCallbacks
    {
        public static PostMatchHandler Instance { get; private set; }

        [Header("Settings")]
        public float disconnectDelay = 2f; // Delay before disconnecting after match end
        public bool autoReportCompletion = true;

        private string currentMatchId;
        private bool matchEnded = false;

        // Events
        public event Action OnMatchEnded;
        public event Action OnDisconnectedAfterMatch;

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
            // Subscribe to Photon events
            PhotonNetwork.AddCallbackTarget(this);
        }

        void OnDestroy()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.RemoveCallbackTarget(this);
            }
        }

        // ============================================
        // PUBLIC API
        // ============================================

        /// <summary>
        /// Set the current match ID for tracking
        /// </summary>
        public void SetCurrentMatch(string matchId)
        {
            currentMatchId = matchId;
            matchEnded = false;

            Debug.Log($"[PostMatchHandler] Set current match: {matchId}");
        }

        /// <summary>
        /// Call when the match has ended (game over)
        /// </summary>
        public void OnMatchEnd(Dictionary<string, object> matchStats = null)
        {
            if (matchEnded)
            {
                Debug.LogWarning("[PostMatchHandler] Match already ended");
                return;
            }

            matchEnded = true;
            Debug.Log("[PostMatchHandler] Match ended, starting disconnect sequence");

            OnMatchEnded?.Invoke();

            // Report match completion
            if (autoReportCompletion && !string.IsNullOrEmpty(currentMatchId))
            {
                FirebaseMatchmaker.Instance.ReportMatchComplete(currentMatchId, matchStats);
            }

            // Start disconnect sequence
            StartCoroutine(DisconnectSequence());
        }

        /// <summary>
        /// Force immediate disconnect
        /// </summary>
        public void ForceDisconnect()
        {
            StopAllCoroutines();
            PhotonConnectionHandler.Instance.Disconnect();
            OnDisconnectedAfterMatch?.Invoke();
        }

        // ============================================
        // PHOTON CALLBACKS
        // ============================================

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"[PostMatchHandler] Disconnected from Photon: {cause}");

            OnDisconnectedAfterMatch?.Invoke();

            // Reset state
            currentMatchId = null;
            matchEnded = false;
        }

        // ============================================
        // PRIVATE METHODS
        // ============================================

        private IEnumerator DisconnectSequence()
        {
            // Wait a bit before disconnecting (let players see final scores, etc.)
            yield return new WaitForSeconds(disconnectDelay);

            Debug.Log("[PostMatchHandler] Disconnecting from Photon after match");

            // Disconnect from Photon
            PhotonConnectionHandler.Instance.Disconnect();
        }
    }
}
