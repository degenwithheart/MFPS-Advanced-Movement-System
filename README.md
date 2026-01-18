# MFPS Addons Integration Guide

This guide provides comprehensive setup and integration instructions for the MFPS addons included in this project.

## Addons Overview

### 1. Advanced Movement Addon
Advanced movement system that adds parkour and slope skiing capabilities to MFPS players.

**Features:**
- Parkour system with vaulting, climbing, and wall running
- Slope skiing with momentum-based movement
- Seamless integration with MFPS first-person controller
- Network synchronization for multiplayer
- Modular design (enable/disable parkour and slope modules independently)

### 2. External Matchmaking Addon
Firebase-powered matchmaking system that reduces Photon CCU costs by 60-80%.

**Features:**
- External matchmaking using Firebase backend
- Skill-based matchmaking with ELO ratings
- Real-time queue status updates
- Automatic Photon connection when matches are found
- Comprehensive game mode support (TDM, FFA, CTF, DOM)

## Prerequisites

### For Both Addons
- Unity 2020.3 or later
- MFPS (Multiplayer FPS) framework
- Photon Unity Networking 2 (PUN2)

### For External Matchmaking Addon
- Firebase project with Authentication, Firestore, and Cloud Functions enabled
- Firebase Unity SDK
- Node.js and Firebase CLI for backend deployment

## Installation

### Step 1: Import Addons
Both addons are already included in the `Assets/Addons/` folder. No additional import steps are required.

### Step 2: Enable Required Scripting Defines

#### For Advanced Movement:
1. Go to `MFPS > Addons > Advanced Movement > Enable Parkour Module` (optional)
2. Go to `MFPS > Addons > Advanced Movement > Enable Slope Module` (optional)
3. Wait for Unity to recompile

#### For External Matchmaking:
1. Go to `MFPS > Addons > External Matchmaking > Enable Addon`
2. Wait for Unity to recompile

## Advanced Movement Setup

### Basic Integration

1. **Open your gameplay scene** (where the player prefab is instantiated)
2. **Integrate the addon:**
   - Go to `MFPS > Addons > Advanced Movement > Integrate Into Scene`
   - This automatically adds the `MFPS_IntegrationHelper` component to your player
3. **Configure components** (if needed):
   - The integration automatically adds required parkour and slope components
   - Parkour components are only added if the Parkour module is enabled
   - Slope components are only added if the Slope module is enabled

### Manual Setup (Alternative)

If automatic integration doesn't work:

1. **Add the integration helper:**
   ```csharp
   // Add to your player prefab's bl_FirstPersonController gameObject
   gameObject.AddComponent<FC_AdvancedMovement.MFPS_IntegrationHelper>();
   ```

2. **Add parkour components** (if Parkour module enabled):
   ```csharp
   gameObject.AddComponent<FC_ParkourSystem.ParkourController>();
   gameObject.AddComponent<FC_ParkourSystem.ClimbController>();
   ```

3. **Add slope components** (if Slope module enabled):
   ```csharp
   gameObject.AddComponent<SlopeMovement>();
   gameObject.AddComponent<SlopeJetpack>();
   ```

### Configuration

The `MFPS_IntegrationHelper` component handles all integration automatically. Key settings:

- **Use Root Motion**: Enabled for parkour animations
- **Gravity**: Uses Unity's physics gravity
- **Prevent Parkour During**: Configurable states where parkour is disabled

### Controls

#### Parkour Controls (when Parkour module enabled):
- **Jump + Forward**: Vault over obstacles
- **Interact (E) + Forward**: Drop to hang from ledges
- **Crouch + Sprint**: Slide (if on suitable surface)

#### Slope Controls (when Slope module enabled):
- **Automatic**: Slope skiing activates when sliding down steep surfaces
- **Momentum-based**: Speed increases with slope steepness

### Network Synchronization

The addon automatically synchronizes:
- Parkour actions across all clients
- Player state changes
- Animation states

### Troubleshooting

**Common Issues:**

1. **"Missing required components"**
   - Ensure `bl_PlayerReferences` is attached to the player
   - Verify all MFPS components are present

2. **Parkour not working**
   - Check that Parkour module is enabled via scripting define
   - Ensure parkour components are attached

3. **Slope skiing not activating**
   - Check that Slope module is enabled
   - Verify slope components are attached

4. **Animation issues**
   - Ensure animator has root motion disabled initially
   - Check that player animations are properly set up

## External Matchmaking Setup

### Firebase Project Setup

1. **Create Firebase project:**
   - Go to https://console.firebase.google.com
   - Create a new project

2. **Enable required services:**
   - **Authentication**: Enable Anonymous sign-in
   - **Firestore Database**: Create database in production mode
   - **Cloud Functions**: Enable Cloud Functions

3. **Download configuration:**
   - Download `google-services.json` from Project Settings
   - Place it in `Assets/` folder

### Unity Setup

1. **Import Firebase SDK:**
   - Go to `MFPS > Addons > External Matchmaking > Setup Firebase SDK`
   - Download Firebase Unity SDK from the opened URL
   - Import the SDK into your Unity project

2. **Integrate with MFPS:**
   - Open your main menu scene
   - Go to `MFPS > Addons > External Matchmaking > Integrate Into MFPS`
   - This adds the integration component to your `bl_Lobby` object

### Backend Deployment

1. **Install Firebase CLI:**
   ```bash
   npm install -g firebase-tools
   ```

2. **Login to Firebase:**
   ```bash
   firebase login
   ```

3. **Deploy functions:**
   - Go to `MFPS > Addons > External Matchmaking > Deploy Backend Functions`
   - Or manually:
     ```bash
     cd functions
     npm install
     firebase deploy --only functions
     ```

### Configuration

#### Matchmaking Settings
Edit the `MFPS_ExternalMatchmakingIntegration` component on your `bl_Lobby`:

```csharp
Default Game Mode: "TDM"  // TDM, FFA, CTF, DOM
Default Region: "US-West" // US-West, US-East, EU-West, Asia
Override MFPS Matchmaking: true  // Replace default MFPS matchmaking
```

#### Photon Settings
Set your Photon App ID in the `PhotonConnectionHandler` component:
```csharp
Photon App Id: "your-photon-app-id-here"
```

### Game Modes

| Mode | Min Players | Max Players | Team Size | Description |
|------|-------------|-------------|-----------|-------------|
| TDM  | 8          | 16         | 8        | Team Deathmatch |
| FFA  | 6          | 12         | 1        | Free For All |
| CTF  | 10         | 20         | 10       | Capture The Flag |
| DOM  | 12         | 24         | 12       | Domination |

### Usage

#### Automatic Integration
The addon automatically replaces MFPS matchmaking. When players click "Play":

1. Connect to Firebase (no CCU cost)
2. Join matchmaking queue (no CCU cost)
3. Wait for match (no CCU cost)
4. Connect to Photon when match found (CCU starts)
5. Play game (CCU active)
6. Disconnect after match (CCU ends)

#### Manual Control
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
        var stats = new Dictionary<string, object>
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

### UI Integration

#### Automatic UI
The addon creates default UI panels. Configure in `MatchmakingManager`:

- **Queue Panel**: Shown during matchmaking
- **Match Found Panel**: Shown when match is ready
- **Status Texts**: Display queue time and match details

#### Custom UI
```csharp
using MFPS.Addons.ExternalMatchmaking.Firebase;

void Start()
{
    FirebaseMatchmaker.Instance.OnMatchFound += OnMatchFound;
    FirebaseMatchmaker.Instance.OnQueueStatusChanged += OnQueueStatusChanged;
}

void OnMatchFound(MatchAssignment match)
{
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

### Monitoring

#### Firebase Console
- **Functions**: Monitor matchmaking request volume
- **Firestore**: Track active matches and player queues
- **Authentication**: Monitor concurrent users

#### Cost Optimization
- **CCU Reduction**: 60-80% reduction in Photon CCU usage
- **Serverless**: Firebase Functions scale automatically
- **Database**: Firestore costs scale with usage

### Troubleshooting

**Common Issues:**

1. **"Firebase not initialized"**
   - Ensure `google-services.json` is in `Assets/`
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

**Debug Mode:**
```csharp
FirebaseMatchmaker.Instance.verboseLogging = true;
```

**Logs:**
- Unity Console: `[ExternalMatchmaking]`, `[FirebaseMatchmaker]`
- Firebase Functions: `firebase functions:log`
- Firestore: Firebase Console → Firestore

## Advanced Configuration

### Custom Skill System
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
Create completely custom UI by subscribing to events and disabling built-in panels.

## Support

### For Advanced Movement Issues:
- Check Unity console for `[Advanced Movement]` logs
- Ensure all MFPS components are properly attached
- Verify module defines are enabled

### For External Matchmaking Issues:
1. Check this documentation
2. Review Firebase Functions logs: `firebase functions:log`
3. Check Unity console for `[ExternalMatchmaking]` logs
4. Verify Firebase project configuration

## Version History

### Advanced Movement
- **v1.0.0**: Initial release with parkour and slope systems

### External Matchmaking
- **v1.0.0**: Initial release with TDM, FFA, CTF, DOM support
- **v1.1.0**: Added custom UI support and advanced configuration
- **v1.2.0**: Improved error handling and monitoring

## License

These addons are provided for use with MFPS. Ensure compliance with Photon, Firebase, and MFPS terms of service.