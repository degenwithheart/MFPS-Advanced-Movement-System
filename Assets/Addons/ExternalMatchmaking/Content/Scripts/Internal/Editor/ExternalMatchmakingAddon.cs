using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

namespace MFPS.Addons.ExternalMatchmaking.Editor
{
    /// <summary>
    /// Editor integration for External Matchmaking Addon
    /// Provides menu items for enabling/disabling and integrating the addon into MFPS
    /// </summary>
    public class ExternalMatchmakingAddon
    {
        private const string ADDON_NAME = "External Matchmaking";
        private const string ADDON_DEFINE = "MFPS_EXTERNAL_MATCHMAKING";
        private const string ADDON_FOLDER = "Assets/Addons/ExternalMatchmaking";

        #region Enable/Disable Addon

#if !MFPS_EXTERNAL_MATCHMAKING
        [MenuItem("MFPS/Addons/External Matchmaking/Enable Addon")]
        private static void EnableAddon()
        {
            AddScriptingDefine(ADDON_DEFINE);
            Debug.Log($"<color=green>{ADDON_NAME} enabled. Recompile required.</color>");
        }
#endif

#if MFPS_EXTERNAL_MATCHMAKING
        [MenuItem("MFPS/Addons/External Matchmaking/Disable Addon")]
        private static void DisableAddon()
        {
            RemoveScriptingDefine(ADDON_DEFINE);
            Debug.Log($"<color=yellow>{ADDON_NAME} disabled. Recompile required.</color>");
        }
#endif

        #endregion

        #region Integration

        [MenuItem("MFPS/Addons/External Matchmaking/Integrate Into MFPS")]
        private static void IntegrateIntoMFPS()
        {
            // Find MFPS Lobby in the scene
            var lobby = Object.FindObjectOfType<bl_Lobby>();
            if (lobby == null)
            {
                Debug.LogError("Could not find bl_Lobby in scene. Ensure you're in the main menu scene.");
                return;
            }

            // Check if already integrated
            var existingIntegration = lobby.GetComponent<MFPS.Addons.ExternalMatchmaking.MFPS_ExternalMatchmakingIntegration>();
            if (existingIntegration != null)
            {
                Debug.LogWarning($"{ADDON_NAME} already integrated.");
                return;
            }

            // Add integration component
            var integration = lobby.gameObject.AddComponent<MFPS.Addons.ExternalMatchmaking.MFPS_ExternalMatchmakingIntegration>();
            Debug.Log($"<color=green>{ADDON_NAME} integrated successfully.</color>");

            // Add required Firebase components
            AddFirebaseComponents(lobby.gameObject);

            // Setup UI integration
            SetupUIIntegration();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.SetDirty(lobby.gameObject);
            Selection.activeGameObject = lobby.gameObject;
            EditorGUIUtility.PingObject(lobby.gameObject);
        }

        [MenuItem("MFPS/Addons/External Matchmaking/Integrate Into MFPS", true)]
        private static bool IntegrateValidate()
        {
            var lobby = Object.FindObjectOfType<bl_Lobby>();
            return lobby != null;
        }

        #endregion

        #region Setup Firebase

        [MenuItem("MFPS/Addons/External Matchmaking/Setup Firebase SDK")]
        private static void SetupFirebaseSDK()
        {
            Debug.Log("Setting up Firebase SDK...");

            // Check if Firebase is already imported
            if (Directory.Exists("Assets/Firebase"))
            {
                Debug.Log("Firebase SDK already imported.");
                return;
            }

            // Open Firebase download page
            Application.OpenURL("https://firebase.google.com/download/unity");

            Debug.Log("Please download and import the Firebase Unity SDK, then run the integration again.");
        }

        #endregion

        #region Deploy Backend

        [MenuItem("MFPS/Addons/External Matchmaking/Deploy Backend Functions")]
        private static void DeployBackend()
        {
            string functionsPath = $"{ADDON_FOLDER}/functions";

            if (!Directory.Exists(functionsPath))
            {
                Debug.LogError($"Functions directory not found at {functionsPath}");
                return;
            }

            // Check if Firebase CLI is installed
            var cliCheck = ExecuteCommand("firebase --version");
            if (!cliCheck.Contains("firebase-tools"))
            {
                Debug.LogError("Firebase CLI not installed. Run: npm install -g firebase-tools");
                Application.OpenURL("https://firebase.google.com/docs/cli");
                return;
            }

            Debug.Log("Deploying Firebase Functions...");

            // Deploy functions
            var deployResult = ExecuteCommand($"cd \"{functionsPath}\" && firebase deploy --only functions");

            if (deployResult.Contains("Deploy complete"))
            {
                Debug.Log($"<color=green>Backend functions deployed successfully!</color>");
            }
            else
            {
                Debug.LogError("Failed to deploy functions. Check console for details.");
                Debug.Log(deployResult);
            }
        }

        #endregion

        #region Documentation

        [MenuItem("MFPS/Addons/External Matchmaking/Documentation")]
        private static void OpenDocumentation()
        {
            string docPath = $"{ADDON_FOLDER}/Documentation/INTEGRATION_GUIDE.md";
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(docPath);
            if (asset != null)
            {
                AssetDatabase.OpenAsset(asset);
            }
            else
            {
                Debug.LogWarning($"Documentation not found at {docPath}");
                // Fallback to README
                string readmePath = "README.md";
                asset = AssetDatabase.LoadAssetAtPath<TextAsset>(readmePath);
                if (asset != null)
                {
                    AssetDatabase.OpenAsset(asset);
                }
            }
        }

        [MenuItem("MFPS/Addons/External Matchmaking/Online Documentation")]
        private static void OpenOnlineDocs()
        {
            Application.OpenURL("https://github.com/username/mfps-external-matchmaking");
        }

        #endregion

        #region Private Methods

        private static void AddFirebaseComponents(GameObject target)
        {
            // Add Firebase Initializer
            if (target.GetComponent<MFPS.Addons.ExternalMatchmaking.Firebase.FirebaseInitializer>() == null)
            {
                target.AddComponent<MFPS.Addons.ExternalMatchmaking.Firebase.FirebaseInitializer>();
                Debug.Log("FirebaseInitializer added.");
            }

            // Add Firebase Matchmaker
            if (target.GetComponent<MFPS.Addons.ExternalMatchmaking.Firebase.FirebaseMatchmaker>() == null)
            {
                target.AddComponent<MFPS.Addons.ExternalMatchmaking.Firebase.FirebaseMatchmaker>();
                Debug.Log("FirebaseMatchmaker added.");
            }

            // Add Photon Connection Handler
            if (target.GetComponent<MFPS.Addons.ExternalMatchmaking.Firebase.PhotonConnectionHandler>() == null)
            {
                target.AddComponent<MFPS.Addons.ExternalMatchmaking.Firebase.PhotonConnectionHandler>();
                Debug.Log("PhotonConnectionHandler added.");
            }

            // Add Post Match Handler
            if (target.GetComponent<MFPS.Addons.ExternalMatchmaking.MFPS.PostMatchHandler>() == null)
            {
                target.AddComponent<MFPS.Addons.ExternalMatchmaking.MFPS.PostMatchHandler>();
                Debug.Log("PostMatchHandler added.");
            }

            // Add Matchmaking Manager
            if (target.GetComponent<MFPS.Addons.ExternalMatchmaking.MFPS.MatchmakingManager>() == null)
            {
                target.AddComponent<MFPS.Addons.ExternalMatchmaking.MFPS.MatchmakingManager>();
                Debug.Log("MatchmakingManager added.");
            }
        }

        private static void SetupUIIntegration()
        {
            // Find bl_LobbyUI
            var lobbyUI = Object.FindObjectOfType<bl_LobbyUI>();
            if (lobbyUI == null)
            {
                Debug.LogWarning("bl_LobbyUI not found. UI integration skipped.");
                return;
            }

            // Create matchmaking UI panel if it doesn't exist
            var matchmakingPanel = GameObject.Find("ExternalMatchmakingPanel");
            if (matchmakingPanel == null)
            {
                matchmakingPanel = new GameObject("ExternalMatchmakingPanel");
                matchmakingPanel.transform.SetParent(lobbyUI.transform, false);

                // Add Canvas Group for visibility control
                var canvasGroup = matchmakingPanel.AddComponent<UnityEngine.CanvasGroup>();
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                // Add RectTransform setup
                var rectTransform = matchmakingPanel.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                Debug.Log("External Matchmaking UI panel created. Configure it manually.");
            }
        }

        private static void AddScriptingDefine(string define)
        {
            var target = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
            if (!defines.Contains(define))
            {
                defines += ";" + define;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defines);
            }
        }

        private static void RemoveScriptingDefine(string define)
        {
            var target = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
            if (defines.Contains(define))
            {
                defines = defines.Replace(define, "").Replace(";;", ";").Trim(';');
                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defines);
            }
        }

        private static string ExecuteCommand(string command)
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{command}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return output + error;
        }

        #endregion
    }
}