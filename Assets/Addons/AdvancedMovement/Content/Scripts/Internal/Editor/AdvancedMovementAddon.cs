using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace FC_AdvancedMovement.Editor
{
    /// <summary>
    /// Editor integration for Advanced Movement Addon
    /// Provides menu items for enabling modules and integrating into scenes
    /// </summary>
    public class AdvancedMovementAddon
    {
        private const string ADDON_NAME = "Advanced Movement";
        private const string PARKOUR_DEFINE = "ADVANCED_MOVEMENT_PARKOUR";
        private const string SLOPE_DEFINE = "ADVANCED_MOVEMENT_SLOPE";

        #region Enable/Disable Module Defines

        #region Parkour Module
#if !ADVANCED_MOVEMENT_PARKOUR
        [MenuItem("MFPS/Addons/Advanced Movement/Enable Parkour Module")]
        private static void EnableParkourModule()
        {
            AddScriptingDefine(PARKOUR_DEFINE);
            Debug.Log($"<color=green>Parkour Module enabled. Recompile required.</color>");
        }
#endif

#if ADVANCED_MOVEMENT_PARKOUR
        [MenuItem("MFPS/Addons/Advanced Movement/Disable Parkour Module")]
        private static void DisableParkourModule()
        {
            RemoveScriptingDefine(PARKOUR_DEFINE);
            Debug.Log($"<color=yellow>Parkour Module disabled. Recompile required.</color>");
        }
#endif
        #endregion

        #region Slope Module
#if !ADVANCED_MOVEMENT_SLOPE
        [MenuItem("MFPS/Addons/Advanced Movement/Enable Slope Module")]
        private static void EnableSlopeModule()
        {
            AddScriptingDefine(SLOPE_DEFINE);
            Debug.Log($"<color=green>Slope Module enabled. Recompile required.</color>");
        }
#endif

#if ADVANCED_MOVEMENT_SLOPE
        [MenuItem("MFPS/Addons/Advanced Movement/Disable Slope Module")]
        private static void DisableSlopeModule()
        {
            RemoveScriptingDefine(SLOPE_DEFINE);
            Debug.Log($"<color=yellow>Slope Module disabled. Recompile required.</color>");
        }
#endif
        #endregion

        #endregion

        #region Scene Integration

        [MenuItem("MFPS/Addons/Advanced Movement/Integrate Into Scene")]
        private static void Integrate()
        {
            // Find the player in the scene
            bl_FirstPersonController fpc = UnityEngine.Object.FindObjectOfType<bl_FirstPersonController>();
            if (fpc == null)
            {
                Debug.LogError("Could not find bl_FirstPersonController in scene. Ensure player is present.");
                return;
            }

            // Check if integration helper already exists
            var helper = fpc.GetComponent<FC_AdvancedMovement.MFPS_IntegrationHelper>();
            if (helper != null)
            {
                Debug.LogWarning("Advanced Movement already integrated on this player.");
                return;
            }

            // Add integration helper
            helper = fpc.gameObject.AddComponent<FC_AdvancedMovement.MFPS_IntegrationHelper>();
            Debug.Log($"<color=green>Advanced Movement integrated successfully.</color>");

            // Add parkour components if module is enabled
#if ADVANCED_MOVEMENT_PARKOUR
            if (fpc.GetComponent<FC_ParkourSystem.ParkourController>() == null)
            {
                fpc.gameObject.AddComponent<FC_ParkourSystem.ParkourController>();
                Debug.Log($"<color=green>Parkour Controller added.</color>");
            }

            if (fpc.GetComponent<FC_ParkourSystem.ClimbController>() == null)
            {
                fpc.gameObject.AddComponent<FC_ParkourSystem.ClimbController>();
                Debug.Log($"<color=green>Climb Controller added.</color>");
            }
#endif

            // Add slope components if module is enabled
#if ADVANCED_MOVEMENT_SLOPE
            if (fpc.GetComponent<SlopeMovement>() == null)
            {
                fpc.gameObject.AddComponent<SlopeMovement>();
                Debug.Log($"<color=green>Slope Movement added.</color>");
            }

            if (fpc.GetComponent<SlopeJetpack>() == null)
            {
                fpc.gameObject.AddComponent<SlopeJetpack>();
                Debug.Log($"<color=green>Slope Jetpack added.</color>");
            }
#endif

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorUtility.SetDirty(fpc.gameObject);
            Selection.activeGameObject = fpc.gameObject;
            EditorGUIUtility.PingObject(fpc.gameObject);
        }

        [MenuItem("MFPS/Addons/Advanced Movement/Integrate Into Scene", true)]
        private static bool IntegrateValidate()
        {
            var fpc = UnityEngine.Object.FindObjectOfType<bl_FirstPersonController>();
            var gm = UnityEngine.Object.FindObjectOfType<bl_GameManager>();
            return (fpc != null && gm != null);
        }

        #endregion

        #region Documentation

        [MenuItem("MFPS/Addons/Advanced Movement/Documentation")]
        private static void OpenDocumentation()
        {
            string docPath = "Assets/Addons/AdvancedMovement/Documentation/INTEGRATION_GUIDE.md";
            var asset = AssetDatabase.LoadAssetAtPath(docPath, typeof(TextAsset)) as TextAsset;
            if (asset != null)
            {
                AssetDatabase.OpenAsset(asset);
            }
            else
            {
                Debug.LogWarning($"Documentation not found at {docPath}");
            }
        }

        #endregion

        #region Helper Methods

        private static void AddScriptingDefine(string define)
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';');
            var list = new System.Collections.Generic.List<string>(defines);

            if (!list.Contains(define))
            {
                list.Add(define);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", list.ToArray()));
            }
        }

        private static void RemoveScriptingDefine(string define)
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';');
            var list = new System.Collections.Generic.List<string>(defines);

            if (list.Contains(define))
            {
                list.Remove(define);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", list.ToArray()));
            }
        }

        #endregion
    }
}
