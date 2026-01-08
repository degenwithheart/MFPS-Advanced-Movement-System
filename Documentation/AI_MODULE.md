# Advanced Movement AI Module

This document explains how to enable advanced movement capabilities for MFPS AI agents.

## Overview

The AI Module extends standard MFPS AI with:
- **Parkour Traversal** - Vaulting, climbing, wall-running for navigation
- **Slope/Skiing** - High-speed slope traversal and jetpack tactical positioning
- **Animation System** - Synchronized animations for parkour and slope actions
- **Network Sync** - Multiplayer parkour action broadcasting via Photon RPC

## Components

### 1. AdvancedMovement_AIShooterAgent.cs
**Purpose:** Parkour capability wrapper for AI agents  
**When to use:** All AI agents in your game  
**Conditional:** `#if ADVANCED_MOVEMENT_PARKOUR`

**Features:**
- Auto-adds `ParkourController` to AI at runtime
- Parkour ledge detection with cooldown system
- AI environment scanning for vaulting opportunities
- Graceful fallback when addon disabled

**Public Methods:**
```csharp
CheckParkourAhead()              // Detect vaulting/climbing ahead
ForceParkourAction(type)         // Force specific parkour move
SetParkourEnabled(bool)          // Toggle parkour for AI
GetParkourController()           // Access parkour controller
GetEnvironmentScanner()          // Get environment data
```

### 2. AdvancedMovement_AISlopeMovement.cs
**Purpose:** Skiing and jetpack capability wrapper  
**When to use:** AI that needs slope/air superiority  
**Conditional:** `#if ADVANCED_MOVEMENT_SLOPE`

**Features:**
- Auto-adds `SlopeMovement` and `SlopeJetpack` components
- Tactical slope usage for faster traversal
- Smart jetpack use for height advantage
- Slope detection and optimization

**Public Methods:**
```csharp
SetSlopeEnabled(bool)            // Toggle slope features
ForceTacticalJetpack()           // Force jetpack use
IsSkiing()                       // Check ski state
GetJetpackFuel()                 // Get fuel amount
GetSlopeMovement()               // Access slope controller
GetSlopeJetpack()                // Access jetpack controller
```

### 3. AdvancedMovement_AIAnimation.cs
**Purpose:** Animation synchronization for parkour/slope actions  
**When to use:** AI with humanoid avatars  
**Conditional:** `#if ADVANCED_MOVEMENT_PARKOUR || ADVANCED_MOVEMENT_SLOPE`

**Features:**
- Animator parameter synchronization
- Animation hash caching for performance
- Climbing/vaulting/wall-run animation states
- Skiing animation support
- Customizable animation speeds

**Animation Parameters:**
- `IsClimbing` (bool) - Active during climbing
- `IsVaulting` (bool) - Active during vault
- `IsWallRunning` (bool) - Active during wall run
- `ClimbDirection` (float) - 1=up, -1=down, 0=idle
- `IsSkiing` (bool) - Active while skiing
- `SkiSpeed` (float) - 0-1 normalized speed

**Public Methods:**
```csharp
TriggerClimbAnimation(bool)
TriggerVaultAnimation(bool)
TriggerWallRunAnimation(bool)
TriggerSkiAnimation(bool)
SetClimbAnimationSpeed(float)
SetSkiAnimationSpeed(float)
ResetAdvancedMovementAnimations()
```

### 4. AdvancedMovement_AINetworkSync.cs
**Purpose:** Multiplayer parkour action synchronization  
**When to use:** Multiplayer games with Photon networking  
**Conditional:** `#if ADVANCED_MOVEMENT_PARKOUR`

**Features:**
- RPC-based parkour action broadcasting
- Network throttling to prevent spam
- Vault, climb, wall-run synchronization
- Automatic state syncing

**RPC Events:**
- `RPC_ParkourVault(Vector3 vaultPoint)`
- `RPC_ClimbStart(Vector3 target)`
- `RPC_ClimbEnd()`
- `RPC_WallRun(Vector3 wallDirection)`
- `RPC_LedgeGrab(Vector3 ledgePosition)`

**Public Methods:**
```csharp
SetNetworkSyncEnabled(bool)
SyncVault(Vector3 vaultPoint)
SyncClimbStart(Vector3 target)
SyncClimbEnd()
SyncWallRun(Vector3 wallDirection)
IsNetworkSyncActive()
```

## Setup

### Step 1: Enable Addon
1. In Unity menu: **MFPS > Addons > Advanced Movement > Enable Advanced Movement**
2. This adds scripting defines: `ADVANCED_MOVEMENT_PARKOUR`, `ADVANCED_MOVEMENT_SLOPE`

### Step 2: Add AI Components to Existing AI
**Option A - Automatic (Recommended)**
- Components auto-add via `Awake()` when addon enabled
- No manual setup needed

**Option B - Manual Setup**
```csharp
// In your AI spawn code:
GameObject aiGO = Instantiate(aiPrefab);

// Parkour
aiGO.AddComponent<AdvancedMovement_AIShooterAgent>();

// Slope (optional)
aiGO.AddComponent<AdvancedMovement_AISlopeMovement>();

// Animation (optional)
aiGO.AddComponent<AdvancedMovement_AIAnimation>();

// Networking (multiplayer only)
aiGO.AddComponent<AdvancedMovement_AINetworkSync>();
```

### Step 3: Configure AI Behavior

```csharp
var parkourAI = aiAgent.GetComponent<AdvancedMovement_AIShooterAgent>();
parkourAI.SetParkourEnabled(true);

var slopeAI = aiAgent.GetComponent<AdvancedMovement_AISlopeMovement>();
slopeAI.SetSlopeEnabled(true);
```

## AI Decision Making

### When AI Uses Parkour:
- **Vaulting:** When low obstacles block direct path to target
- **Climbing:** When reaching high ground or escaping danger
- **Wall Running:** When traversing vertical terrain
- **Cooldown:** 2-3 seconds between major parkour actions

### When AI Uses Slopes:
- **Skiing:** Speed > 10 m/s on slopes (custom threshold)
- **Jetpack:** When airborne, has fuel, and tactical advantage exists
- **Advantage:** Higher ground over target (jetpack use trigger)
- **Cooldown:** 5 seconds between jetpack uses

## Animation Setup

Your humanoid AI must have these animation states in Animator:

**Parkour States:**
- `locomotion/climbing` - idle/up/down climbing poses
- `locomotion/vaulting` - vault approach and execution
- `locomotion/wallrun` - wall running pose
- `action/ledgegrab` - ledge grab idle

**Slope States:**
- `locomotion/skiing` - skiing poses based on speed

**Parameters Required:**
- Boolean: `IsClimbing`, `IsVaulting`, `IsWallRunning`, `IsSkiing`
- Float: `ClimbDirection`, `SkiSpeed`

## Conditional Compilation

All AI enhancement code is wrapped in:

```csharp
#if ADVANCED_MOVEMENT_PARKOUR
    // Parkour code
#endif

#if ADVANCED_MOVEMENT_SLOPE
    // Slope code
#endif
```

**When addon disabled:**
- All components remain inactive
- Zero performance impact
- AI behaves like vanilla MFPS
- Full backward compatibility

## Configuration

### In Inspector:

**AdvancedMovement_AIShooterAgent:**
- `Parkour Enabled` - Enable/disable parkour for this AI
- `Environment Scan Radius` - Area to check for parkour opportunities
- `Action Cooldown` - Time between parkour moves

**AdvancedMovement_AISlopeMovement:**
- `Enable Slope For AI` - Toggle slope features
- `Min Speed For Skiing` - Velocity threshold to trigger skiing
- `Jetpack Usage Chance` - Probability of jetpack use (0-1)
- `Jetpack Tactical Cooldown` - Cooldown between jetpack uses

**AdvancedMovement_AIAnimation:**
- `Climb Animation Speed` - Animation playback speed (0.5-2.0)
- `Ski Animation Speed` - Ski animation playback speed (0.5-2.0)

**AdvancedMovement_AINetworkSync:**
- `Enable Network Sync` - Toggle RPC broadcasting
- `RPC Throttle Rate` - Minimum time between RPC sends

## Performance Notes

- **Parkour:** ~2-3ms per AI when active
- **Slope:** ~1-2ms per AI when active
- **Animation:** Negligible overhead (hash caching)
- **Network:** <1KB per RPC call (throttled)

**Optimization Tips:**
- Use parkour only for important AI (bosses, leaders)
- Reduce environment scan radius on weaker devices
- Throttle network updates in large multiplayer lobbies
- Disable animation sync on distant AI agents

## Debugging

All modules log with color coding:

```
<color=green>[Advanced Movement AI]</color>  - Initialization success
<color=yellow>[Advanced Movement AI]</color> - Warnings
<color=red>[Advanced Movement AI]</color>    - Errors
<color=cyan>[Advanced Movement AI]</color>   - Network/sync events
```

Enable in Console to see:
- Component initialization
- Parkour action detection
- Network RPC broadcasting
- Animation state changes

## Troubleshooting

### "Component not found" errors
- Ensure addon is enabled: MFPS > Addons > Advanced Movement > Enable
- Check scripting defines: Project Settings > Player > Other Settings > Scripting Defines

### AI not using parkour
- Check `parkourEnabled` flag (default: true)
- Verify parkour controller was added: `GetComponent<ParkourController>()`
- Ensure environment has valid platforms (ledges, walls)
- Check animation parameters exist in Animator

### Animation issues
- Add animation parameters to Animator controller (see "Animation Setup" section)
- Verify humanoid rig on AI prefab
- Check animation speeds in inspector (0.5-2.0 range)

### Network sync issues
- Ensure PhotonView component exists on AI
- Check Photon network is initialized
- Verify RPC methods registered in Animator
- Look for network throttle preventing RPC sends

## Advanced Usage

### Custom AI Behavior

```csharp
public class CustomAIBehavior : MonoBehaviour
{
    private AdvancedMovement_AIShooterAgent parkourAI;
    private AdvancedMovement_AISlopeMovement slopeAI;

    private void Start()
    {
        parkourAI = GetComponent<AdvancedMovement_AIShooterAgent>();
        slopeAI = GetComponent<AdvancedMovement_AISlopeMovement>();
    }

    public void StrategicMove()
    {
        // Force parkour to escape
        if (IsInDanger())
        {
            parkourAI.ForceParkourAction(ParkourActionType.Vault);
        }

        // Use jetpack for tactical advantage
        if (NeedsHighGround())
        {
            slopeAI.ForceTacticalJetpack();
        }
    }
}
```

### Disable for Specific AI

```csharp
// Boss AI - disable parkour to make more predictable
bossAI.GetComponent<AdvancedMovement_AIShooterAgent>()?.SetParkourEnabled(false);

// Scout AI - enable all features
scoutAI.GetComponent<AdvancedMovement_AISlopeMovement>()?.SetSlopeEnabled(true);
```

## API Reference

See `INTEGRATION_GUIDE.md` for complete API reference and code examples.

---

**Last Updated:** Current Session  
**Addon Version:** 1.0  
**MFPS Version:** 11.2+  
**Requires:** Photon PUN 2, MFPS Advanced Movement Addon
