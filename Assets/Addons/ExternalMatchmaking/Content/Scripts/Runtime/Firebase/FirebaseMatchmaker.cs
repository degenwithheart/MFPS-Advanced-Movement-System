using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Functions;

namespace MFPS.Addons.ExternalMatchmaking.Firebase
{
    [Serializable]
    public class MatchAssignment
    {
        public string matchId;
        public string roomName;
        public string photonRegion;
        public string gameMode;
        public string team;
        public long joinBy;
    }

    [Serializable]
    public class QueueStatus
    {
        public bool isInQueue;
        public float timeInQueue;
        public string status;
    }

    public class FirebaseMatchmaker : MonoBehaviour
    {
        public static FirebaseMatchmaker Instance { get; private set; }

        [Header("Settings")]
        public string defaultRegion = "US-West";
        public float queueTimeout = 300f; // 5 minutes
        public bool autoRetryOnFailure = true;

        [Header("Debug")]
        public bool verboseLogging = true;

        private FirebaseFirestore db;
        private FirebaseFunctions functions;
        private ListenerRegistration matchListener;
        private string currentUserId;
        private bool isInQueue = false;
        private Coroutine timeoutCoroutine;
        private float queueStartTime;

        // Events
        public event Action<MatchAssignment> OnMatchFound;
        public event Action<QueueStatus> OnQueueStatusChanged;
        public event Action<string> OnError;

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
            // Wait for Firebase initialization
            if (FirebaseInitializer.Instance != null && FirebaseInitializer.Instance.IsInitialized)
            {
                Initialize();
            }
            else
            {
                FirebaseInitializer.Instance.OnInitialized += Initialize;
            }
        }

        void OnDestroy()
        {
            LeaveQueue();
            matchListener?.Stop();
        }

        private void Initialize()
        {
            db = FirebaseFirestore.DefaultInstance;
            functions = FirebaseFunctions.DefaultInstance;
            currentUserId = FirebaseInitializer.Instance.GetUserId();

            if (string.IsNullOrEmpty(currentUserId))
            {
                LogError("User ID is null!");
                OnError?.Invoke("Not authenticated");
                return;
            }

            Log($"✓ Matchmaker initialized for user: {currentUserId}");

            // Setup listener for match assignments
            SetupMatchListener();
        }

        // ============================================
        // PUBLIC API
        // ============================================

        /// <summary>
        /// Join matchmaking queue for specified game mode
        /// </summary>
        public void JoinQueue(string gameMode, string region = null, int skillLevel = 1500)
        {
            if (isInQueue)
            {
                LogWarning("Already in queue!");
                return;
            }

            if (string.IsNullOrEmpty(currentUserId))
            {
                OnError?.Invoke("Not authenticated");
                return;
            }

            region = region ?? defaultRegion;

            Log($"Joining queue: {gameMode} in {region}");

            Dictionary<string, object> queueData = new Dictionary<string, object>
            {
                { "gameMode", gameMode },
                { "region", region },
                { "skillLevel", skillLevel },
                { "platform", Application.platform.ToString() },
                { "timestamp", FieldValue.ServerTimestamp },
                { "status", "waiting" }
            };

            db.Collection("matchmaking").Document("queues").Collection(currentUserId)
                .Document(currentUserId)
                .SetAsync(queueData)
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        isInQueue = true;
                        queueStartTime = Time.time;
                        Log("✓ Successfully joined queue");

                        // Start timeout coroutine
                        timeoutCoroutine = StartCoroutine(QueueTimeoutCoroutine());

                        // Notify status change
                        OnQueueStatusChanged?.Invoke(new QueueStatus
                        {
                            isInQueue = true,
                            timeInQueue = 0,
                            status = "waiting"
                        });
                    }
                    else
                    {
                        LogError($"Failed to join queue: {task.Exception}");
                        OnError?.Invoke("Failed to join queue");
                    }
                });
        }

        /// <summary>
        /// Leave the matchmaking queue
        /// </summary>
        public void LeaveQueue()
        {
            if (!isInQueue) return;

            Log("Leaving queue");

            // Stop timeout coroutine
            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }

            // Remove from queue
            db.Collection("matchmaking").Document("queues").Collection(currentUserId)
                .Document(currentUserId)
                .DeleteAsync()
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        isInQueue = false;
                        Log("✓ Successfully left queue");

                        OnQueueStatusChanged?.Invoke(new QueueStatus
                        {
                            isInQueue = false,
                            timeInQueue = 0,
                            status = "idle"
                        });
                    }
                    else
                    {
                        LogError($"Failed to leave queue: {task.Exception}");
                    }
                });
        }

        /// <summary>
        /// Report successful join to Photon room
        /// </summary>
        public void ReportMatchJoin(string matchId)
        {
            var data = new Dictionary<string, object>
            {
                { "matchId", matchId }
            };

            functions.GetHttpsCallable("onPlayerJoinedMatch")
                .CallAsync(data)
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        Log($"✓ Reported successful join to match {matchId}");
                    }
                    else
                    {
                        LogError($"Failed to report match join: {task.Exception}");
                    }
                });
        }

        /// <summary>
        /// Report match completion
        /// </summary>
        public void ReportMatchComplete(string matchId, Dictionary<string, object> stats = null)
        {
            var data = new Dictionary<string, object>
            {
                { "matchId", matchId },
                { "stats", stats ?? new Dictionary<string, object>() }
            };

            functions.GetHttpsCallable("onMatchComplete")
                .CallAsync(data)
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        Log($"✓ Reported match completion for {matchId}");
                    }
                    else
                    {
                        LogError($"Failed to report match completion: {task.Exception}");
                    }
                });
        }

        // ============================================
        // PRIVATE METHODS
        // ============================================

        private void SetupMatchListener()
        {
            Log("Setting up match listener");

            matchListener = db.Collection("matchmaking/playerMatches")
                .Document(currentUserId)
                .Listen(snapshot =>
                {
                    if (!snapshot.Exists)
                    {
                        // No match assignment
                        return;
                    }

                    var data = snapshot.ToDictionary();
                    var matchAssignment = new MatchAssignment
                    {
                        matchId = data.ContainsKey("matchId") ? data["matchId"].ToString() : "",
                        roomName = data.ContainsKey("roomName") ? data["roomName"].ToString() : "",
                        photonRegion = data.ContainsKey("photonRegion") ? data["photonRegion"].ToString() : "",
                        gameMode = data.ContainsKey("gameMode") ? data["gameMode"].ToString() : "",
                        team = data.ContainsKey("team") ? data["team"].ToString() : "",
                        joinBy = data.ContainsKey("joinBy") ? Convert.ToInt64(data["joinBy"]) : 0
                    };

                    Log($"🎯 Match found! Room: {matchAssignment.roomName}, Team: {matchAssignment.team}");

                    // Leave queue status
                    isInQueue = false;
                    if (timeoutCoroutine != null)
                    {
                        StopCoroutine(timeoutCoroutine);
                        timeoutCoroutine = null;
                    }

                    OnQueueStatusChanged?.Invoke(new QueueStatus
                    {
                        isInQueue = false,
                        timeInQueue = 0,
                        status = "matched"
                    });

                    // Notify match found
                    OnMatchFound?.Invoke(matchAssignment);
                });
        }

        private IEnumerator QueueTimeoutCoroutine()
        {
            float elapsed = 0;

            while (elapsed < queueTimeout)
            {
                elapsed += Time.deltaTime;

                // Update queue status every second
                if (Mathf.FloorToInt(elapsed) % 1 == 0)
                {
                    OnQueueStatusChanged?.Invoke(new QueueStatus
                    {
                        isInQueue = true,
                        timeInQueue = elapsed,
                        status = "waiting"
                    });
                }

                yield return null;
            }

            // Timeout reached
            LogWarning("Queue timeout reached, leaving queue");
            LeaveQueue();
            OnError?.Invoke("Queue timeout");
        }

        // ============================================
        // UTILITIES
        // ============================================

        private void Log(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[FirebaseMatchmaker] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[FirebaseMatchmaker] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[FirebaseMatchmaker] {message}");
        }
    }
}
