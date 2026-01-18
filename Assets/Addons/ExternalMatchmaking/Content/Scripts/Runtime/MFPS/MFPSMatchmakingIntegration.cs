// MFPS Integration Example
// This shows how to integrate the external matchmaking system with MFPS

using UnityEngine;
using MFPS.Addons.ExternalMatchmaking.MFPS;
using MFPS.Addons.ExternalMatchmaking.Firebase;

namespace MFPS.Addons.ExternalMatchmaking.MFPS
{
    public class MFPS_ExternalMatchmakingIntegration : MonoBehaviour
{
    void Start()
    {
        // Initialize the matchmaking system
        // Make sure all required components are in the scene:
        // - FirebaseInitializer
        // - FirebaseMatchmaker
        // - PhotonConnectionHandler
        // - PostMatchHandler
        // - MatchmakingManager

        Debug.Log("MFPS External Matchmaking Integration Ready");
    }

    // Call this from your MFPS menu "Play" button instead of the default matchmaking
    public void StartExternalMatchmaking()
    {
        // Configure based on MFPS settings
        var manager = MatchmakingManager.Instance;

        // Get game mode from MFPS (you'll need to adapt this)
        string gameMode = GetMFPSGameMode(); // "TDM", "FFA", etc.
        string region = GetMFPSRegion();     // "US-West", etc.
        int skillLevel = GetPlayerSkill();   // Player's ELO

        manager.gameMode = gameMode;
        manager.region = region;
        manager.skillLevel = skillLevel;

        // Start matchmaking
        manager.StartMatchmaking();
    }

    // Call this when the MFPS match ends
    public void OnMFPSMatchEnd()
    {
        // Collect match stats from MFPS
        var matchStats = new System.Collections.Generic.Dictionary<string, object>
        {
            { "duration", GetMatchDuration() },
            { "winner", GetWinningTeam() },
            { "score_team_a", GetTeamScore("A") },
            { "score_team_b", GetTeamScore("B") },
            { "player_count", GetPlayerCount() }
        };

        // End the match (this will disconnect from Photon)
        MatchmakingManager.Instance.EndMatch(matchStats);
    }

    // Helper methods - implement these based on your MFPS integration
    private string GetMFPSGameMode()
    {
        // Return current MFPS game mode (TDM, FFA, etc.)
        // You'll need to get this from MFPS's game mode selection
        return "TDM"; // Default
    }

    private string GetMFPSRegion()
    {
        // Return selected region from MFPS
        // You might want to add region selection to MFPS UI
        return "US-West"; // Default
    }

    private int GetPlayerSkill()
    {
        // Return player's skill level from MFPS save data
        // This could be stored in PlayerPrefs or MFPS's save system
        return PlayerPrefs.GetInt("PlayerSkill", 1500);
    }

    private float GetMatchDuration()
    {
        // Return match duration from MFPS
        return 600f; // 10 minutes example
    }

    private string GetWinningTeam()
    {
        // Return winning team from MFPS
        return "red"; // or "blue"
    }

    private int GetTeamScore(string team)
    {
        // Return team score from MFPS
        return team == "red" ? 150 : 120;
    }

    private int GetPlayerCount()
    {
        // Return player count from MFPS
        return 16;
    }
    }
}