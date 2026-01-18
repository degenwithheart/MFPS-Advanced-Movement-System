using UnityEditor;
using UnityEngine;

// Editor utility to enable AdvancedMovement parkour define
[InitializeOnLoad]
public static class EnableAdvancedMovementParkour
{
    private const string Define = "ADVANCED_MOVEMENT_PARKOUR";

    static EnableAdvancedMovementParkour()
    {
        try
        {
            AddDefineToGroup(BuildTargetGroup.Standalone);
            AddDefineToGroup(BuildTargetGroup.Android);
            AddDefineToGroup(BuildTargetGroup.iOS);
            AddDefineToGroup(BuildTargetGroup.WebGL);
            Debug.Log("Enabled " + Define + " define for common build targets.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to add define: " + ex.Message);
        }
    }

    private static void AddDefineToGroup(BuildTargetGroup group)
    {
        string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        if (string.IsNullOrEmpty(symbols))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, Define);
            return;
        }

        var list = new System.Collections.Generic.List<string>(symbols.Split(';'));
        if (!list.Contains(Define))
        {
            list.Add(Define);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", list.ToArray()));
        }
    }
}
