# MFPS External Matchmaking Addon - Integration Guide

## Overview

The External Matchmaking Addon replaces MFPS's default Photon-based matchmaking with a Firebase-powered external matchmaking system that significantly reduces CCU costs by only connecting players to Photon when matches are ready.

## Features

- **60-80% CCU Reduction**: Players only connect to Photon during actual gameplay
- **Skill-Based Matchmaking**: ELO rating system with balanced team creation
- **Real-time Updates**: Live queue status and match notifications
- **Production Ready**: Comprehensive error handling and automatic cleanup
- **Easy Integration**: One-click integration with MFPS

## Installation

### Step 1: Import the Addon

1. Copy the `Assets/Addons/ExternalMatchmaking` folder into your MFPS project's `Assets/Addons/` directory
2. Copy the `functions/` folder to your Firebase project directory

### Step 2: Enable the Addon

1. Open Unity and go to `MFPS > Addons > External Matchmaking > Enable Addon`
2. Wait for recompilation
3. Go to `MFPS > Addons > External Matchmaking > Setup Firebase SDK`
4. Download and import the Firebase Unity SDK

### Step 3: Integrate with MFPS

1. Open your main menu scene
2. Go to `MFPS > Addons > External Matchmaking > Integrate Into MFPS`
3. This will automatically add all required components to your `bl_Lobby` object

### Step 4: Deploy Backend

1. Install Firebase CLI: `npm install -g firebase-tools`
2. Login: `firebase login`
3. Navigate to functions folder: `cd functions`
4. Install dependencies: `npm install`
5. Deploy: `firebase deploy --only functions`

## Configuration

### Firebase Project Setup

1. Create a new Firebase project at https://console.firebase.google.com
2. Enable Authentication (Anonymous sign-in)
3. Enable Firestore Database
4. Enable Cloud Functions
5. Download `google-services.json` and place in `Assets/`

### Matchmaking Settings

Edit the `MatchmakingManager` component on your `bl_Lobby` object:

```csharp
Game Mode: "TDM", "FFA", "CTF", "DOM"
Region: "US-West", "US-East", "EU-West", "Asia", etc.
Skill Level: Player's ELO rating (default: 1500)
```

### Photon Configuration

Set your Photon App ID in the `PhotonConnectionHandler` component:

```csharp
Photon App Id: "your-photon-app-id-here"
```

## Usage

### Basic Usage

The addon automatically replaces MFPS matchmaking. When players click "Play" in MFPS, they will:

1. Connect to Firebase backend (no CCU)
2. Join matchmaking queue (no CCU)
3. Wait for match (no CCU)
4. Connect to Photon when match found (CCU starts)
5. Play game (CCU active)
6. Disconnect after match (CCU ends)

### Advanced Usage

You can manually control matchmaking:

```csharp
using MFPS.Addons.ExternalMatchmaking.MFPS;

public class CustomMatchmaking : MonoBehaviour
{
    void StartMatchmaking()
    {
        var manager = MatchmakingManager.Instance;
        manager.gameMode = "CTF";
        manager.region = "EU-West";
        manager.skillLevel = 1800;
        manager.StartMatchmaking();
    }

    void CancelMatchmaking()
    {
        MatchmakingManager.Instance.CancelMatchmaking();
    }

    void OnMatchEnd()
    {
        var stats = new System.Collections.Generic.Dictionary<string, object>
        {
            { "duration", 600 },
            { "winner", "red" },
            { "score_red", 150 },
            { "score_blue", 120 }
        };
        MatchmakingManager.Instance.EndMatch(stats);
    }
}
```

## UI Integration

### Automatic UI

The addon creates a matchmaking UI panel. Configure it in the `MatchmakingManager` component:

- **Queue Panel**: The UI panel shown during matchmaking
- **Match Found Panel**: The UI panel shown when match is found
- **Queue Status Text**: Text displaying queue time and status
- **Match Found Text**: Text displaying match details

### Custom UI

Create your own UI and hook into the events:

```csharp
using MFPS.Addons.ExternalMatchmaking.Firebase;

void Start()
{
    FirebaseMatchmaker.Instance.OnMatchFound += OnMatchFound;
    FirebaseMatchmaker.Instance.OnQueueStatusChanged += OnQueueStatusChanged;
}

void OnMatchFound(MatchAssignment match)
{
    // Show match found UI
    matchFoundPanel.SetActive(true);
    matchText.text = $"Match Found!\nRoom: {match.roomName}\nTeam: {match.team}";
}

void OnQueueStatusChanged(QueueStatus status)
{
    if (status.isInQueue)
    {
        queueText.text = $"In Queue\nTime: {status.timeInQueue}s";
    }
}
```

## Game Mode Configuration

The backend supports these game modes:

| Mode | Min Players | Max Players | Team Size | Description |
|------|-------------|-------------|-----------|-------------|
| TDM  | 8          | 16         | 8        | Team Deathmatch |
| FFA  | 6          | 12         | 1        | Free For All |
| CTF  | 10         | 20         | 10       | Capture The Flag |
| DOM  | 12         | 24         | 12       | Domination |

## Monitoring

### Firebase Console

- **Functions**: Monitor matchmaking request volume
- **Firestore**: Track active matches and player queues
- **Authentication**: Monitor concurrent users

### Cost Optimization

- **CCU Reduction**: 60-80% reduction in Photon CCU usage
- **Serverless**: Firebase Functions scale automatically
- **Database**: Firestore costs scale with usage

## Troubleshooting

### Common Issues

1. **"Firebase not initialized"**
   - Ensure `google-services.json` is in Assets/
   - Check Firebase SDK is imported
   - Verify project settings

2. **"Failed to join queue"**
   - Check Firestore security rules
   - Verify Firebase Functions are deployed
   - Check authentication status

3. **"Photon connection failed"**
   - Verify Photon App ID
   - Check region configuration
   - Ensure Photon PUN 2 is installed

4. **"Match not found"**
   - Check Cloud Functions logs
   - Verify game mode configuration
   - Check player skill ranges

### Debug Mode

Enable verbose logging in components:

```csharp
FirebaseMatchmaker.Instance.verboseLogging = true;
```

### Logs

Check these log locations:
- Unity Console: `[ExternalMatchmaking]`, `[FirebaseMatchmaker]`
- Firebase Functions: `firebase functions:log`
- Firestore: Firebase Console → Firestore

## API Reference

### MatchmakingManager

```csharp
void StartMatchmaking()                    // Start matchmaking
void CancelMatchmaking()                   // Cancel queue
void EndMatch(Dictionary<string, object>) // End match with stats
string gameMode                            // Game mode (TDM, FFA, etc.)
string region                              // Region (US-West, etc.)
int skillLevel                             // Player skill level
```

### FirebaseMatchmaker

```csharp
void JoinQueue(string, string, int)        // Join queue
void LeaveQueue()                          // Leave queue
void ReportMatchJoin(string)               // Report successful join
void ReportMatchComplete(string, dict)     // Report match end
bool isInQueue                             // Queue status
```

### PhotonConnectionHandler

```csharp
void ConnectAndJoinRoom(MatchAssignment)   // Connect to Photon
void Disconnect()                          // Disconnect from Photon
string photonAppId                         // Photon App ID
```

## Advanced Configuration

### Custom Skill System

Implement your own skill calculation:

```csharp
int GetPlayerSkill()
{
    // Your skill calculation logic
    return PlayerPrefs.GetInt("PlayerELO", 1500);
}
```

### Custom Matchmaking Logic

Modify `functions/config.js` for custom game modes and regions.

### Custom UI

Create completely custom UI by subscribing to events and not using the built-in panels.

## Support

For issues:
1. Check this documentation
2. Review Firebase Functions logs
3. Check Unity console output
4. Verify all components are properly integrated

## Version History

- **v1.0.0**: Initial release with TDM, FFA, CTF, DOM support
- **v1.1.0**: Added custom UI support and advanced configuration
- **v1.2.0**: Improved error handling and monitoring

## License

This addon is provided for use with MFPS. Ensure compliance with Photon, Firebase, and MFPS terms of service.