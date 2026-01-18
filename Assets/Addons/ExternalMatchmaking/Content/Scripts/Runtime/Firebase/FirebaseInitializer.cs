using System;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

namespace MFPS.Addons.ExternalMatchmaking.Firebase
{
    public class FirebaseInitializer : MonoBehaviour
    {
        public static FirebaseInitializer Instance { get; private set; }

        public static FirebaseApp App { get; private set; }
        public static FirebaseAuth Auth { get; private set; }
        public static FirebaseFirestore Firestore { get; private set; }
        public static FirebaseUser CurrentUser { get; private set; }

        public bool IsInitialized { get; private set; }

        public event Action OnInitialized;
        public event Action OnUserAuthenticated;
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
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;

                if (dependencyStatus == DependencyStatus.Available)
                {
                    App = FirebaseApp.DefaultInstance;
                    Auth = FirebaseAuth.DefaultInstance;
                    Firestore = FirebaseFirestore.DefaultInstance;

                    IsInitialized = true;
                    Debug.Log("✓ Firebase initialized successfully");

                    OnInitialized?.Invoke();

                    // Auto-authenticate user
                    AuthenticateUser();
                }
                else
                {
                    IsInitialized = false;
                    string error = $"Could not resolve Firebase dependencies: {dependencyStatus}";
                    Debug.LogError(error);
                    OnError?.Invoke(error);
                }
            });
        }

        private void AuthenticateUser()
        {
            // Check if already signed in
            if (Auth.CurrentUser != null)
            {
                CurrentUser = Auth.CurrentUser;
                Debug.Log($"✓ Already authenticated: {CurrentUser.UserId}");
                OnUserAuthenticated?.Invoke();
                return;
            }

            // Anonymous sign-in (or implement your own auth)
            Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("Sign-in was canceled");
                    OnError?.Invoke("Authentication canceled");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError($"Sign-in failed: {task.Exception}");
                    OnError?.Invoke("Authentication failed");
                    return;
                }

                CurrentUser = task.Result.User;
                Debug.Log($"✓ Signed in anonymously: {CurrentUser.UserId}");
                OnUserAuthenticated?.Invoke();
            });
        }

        public string GetUserId()
        {
            return CurrentUser?.UserId ?? "unknown";
        }
    }
}
