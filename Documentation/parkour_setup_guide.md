# Parkour System - Complete Setup Guide

## 📁 File Structure

Create this folder structure in your project:

```
Assets/
└── Scripts/
    └── ParkourSystem/
        ├── IParkourCharacter.cs
        ├── ParkourController.cs
        ├── ClimbController.cs
        └── MFPS_IntegrationHelper.cs
```

---

## 🎮 Step 1: Basic Setup

### 1.1 Create the Scripts
1. Copy all 4 scripts into the `ParkourSystem` folder
2. Wait for Unity to compile
3. Check Console for any errors

### 1.2 Player Setup
1. Select your MFPS Player prefab
2. Add these components (in this order):
   - `ParkourController`
   - `ClimbController`
   - `MFPS_IntegrationHelper`

---

## 🎨 Step 2: Animator Setup

### 2.1 Required Parameters
Add these parameters to your player's Animator:

**Float Parameters:**
- `Speed` - Movement speed magnitude
- `HorizontalSpeed` - Horizontal velocity
- `VerticalSpeed` - Vertical velocity (for jumping/falling)
- `freeHang` - 0 = braced hang, 1 = free hang
- `ClimbVertical` - Climbing up/down
- `ClimbHorizontal` - Climbing left/right

**Bool Parameters:**
- `IsGrounded` - Is player on ground
- `IsJumping` - Is player jumping
- `WallRunLeft` - Wall running on left side

**Integer Parameters:**
- `BodyState` - Current player state (0-28)

**Trigger Parameters:**
- `Jump` - Trigger jump animation
- `WallRun` - Trigger wall run
- `FreeHang` - Enter free hang
- `BracedHang` - Enter braced hang
- `ClimbUp` - Climb up from hang
- `StartClimb` - Start climbing wall
- `ClimbFinish` - Finish climbing

### 2.2 Animation States
Create these animation states (you can use placeholder animations initially):

**Locomotion:**
- Idle
- Walk
- Run
- Crouch
- Jump
- Land

**Parkour:**
- StepUp - Small obstacle (0.6m)
- MediumStepUp - Medium obstacle (1.2m)
- VaultOver - Vault over obstacle (1m)
- ClimbUp - Climb tall obstacle (2m)
- WallRun - Run on wall
- Slide - Slide on ground

**Climbing:**
- DropToHang - Drop to braced hang
- DropToFreeHang - Drop to free hang
- BracedHangIdle - Hang with wall support
- FreeHangIdle - Hang without wall support
- ClimbUpFromHang - Pull up from hang
- ClimbIdle - Idle while climbing
- ClimbMove - Moving while climbing

### 2.3 State Transitions
Set up transitions based on the triggers and parameters above.

---

## 🔧 Step 3: Component Configuration

### 3.1 ParkourController Settings

**Detection Settings:**
- `Obstacle Layer`: Set to layers that can be vaulted/climbed (e.g., "Default", "Ground")
- `Forward Ray Length`: 1.5 (detection distance)
- `Height Ray Length`: 3.0 (vertical detection)
- `Forward Ray Offset`: 0.5 (height of detection ray)

**Parkour Heights:**
- `Vault Height`: 1.0 (max vaultable height)
- `Step Up Height`: 0.6 (small step)
- `Medium Step Up Height`: 1.2 (medium step)
- `Climb Up Height`: 2.0 (max climbable)

**Speed Requirements:**
- `Min Vault Speed`: 3.0 (minimum speed to vault)
- `Min Wall Run Speed`: 5.0 (minimum speed for wall run)

**Wall Run Settings:**
- `Wall Run Duration`: 2.0 (seconds)
- `Wall Run Speed`: 6.0
- `Wall Run Gravity`: 2.0

### 3.2 ClimbController Settings

**Detection:**
- `Climbable Layer`: Same as obstacle layer
- `Climb Detection Range`: 2.0
- `Ledge Detection Height`: 2.5

**Climbing:**
- `Climb Speed`: 2.0 (vertical climb speed)
- `Horizontal Climb Speed`: 1.5
- `Climb Up Duration`: 1.0 (animation duration)

**Hang Settings:**
- `Hang Stamina`: 10.0 (seconds before falling)
- `Stamina Drain Rate`: 1.0
- `Stamina Recover Rate`: 2.0

### 3.3 MFPS_IntegrationHelper
This component auto-configures itself. Just verify:
- All required components are found (check Console)
- No error messages appear

---

## 🏗️ Step 4: Level Setup

### 4.1 Create Climbable Objects

**Option A: Simple Approach (Auto-Generate ClimbPoints)**
1. Create an obstacle (e.g., cube, wall, ledge)
2. Set its layer to your climbable layer
3. The system will auto-generate climb points

**Option B: Manual ClimbPoints (Better Control)**
1. Create an obstacle
2. Add empty GameObjects as children
3. Add `ClimbPoint` component to each
4. Configure each ClimbPoint:
   - Create child `MountPoint` (where player grabs)
   - Create child `DismountPoint` (where player climbs to)
   - Set `Is Free Hang` if no wall behind
   - Connect to neighboring points via `Connected Points` list

### 4.2 Example Climbable Wall
```
Wall (GameObject)
├── ClimbPoint_Left
│   ├── MountPoint (Transform)
│   └── DismountPoint (Transform)
├── ClimbPoint_Middle
│   ├── MountPoint (Transform)
│   └── DismountPoint (Transform)
└── ClimbPoint_Right
    ├── MountPoint (Transform)
    └── DismountPoint (Transform)
```

---

## ⌨️ Step 5: Input Setup

The system uses MFPS input by default:

**Default Controls:**
- `Space` / `Jump` - Jump / Vault / Climb Up
- `E` / `Interact` - Drop to Hang
- `Ctrl` / `Crouch` - Crouch / Slide (when running)
- `Shift` / `Run` - Sprint

**To Customize:**
Edit `MFPS_IntegrationHelper.cs` in the `HandleParkourInputs()` method.

---

## 🎯 Step 6: Testing

### 6.1 Test Checklist

**Basic Movement:**
- [ ] Player can walk/run/crouch normally
- [ ] Jumping works
- [ ] Camera follows correctly

**Parkour Actions:**
- [ ] Can vault over 1m obstacles
- [ ] Can step up on 0.6m obstacles
- [ ] Can climb 2m obstacles
- [ ] Movement is disabled during parkour
- [ ] Weapons are hidden during parkour
- [ ] Camera switches to third-person during parkour

**Climbing:**
- [ ] Can drop to hang on ledges
- [ ] Can climb up from hang
- [ ] Can move left/right along ledge
- [ ] Stamina drains while hanging
- [ ] Falls when stamina depleted

**Wall Running:**
- [ ] Can wall run at high speed
- [ ] Gravity is reduced during wall run
- [ ] Animation plays correctly

### 6.2 Debug Tools

**Gizmos:**
- ParkourController shows detection rays (yellow)
- ClimbController shows climb points (green/cyan)
- MFPS_IntegrationHelper shows velocity (green) and input (blue)

**Console Logs:**
- Component initialization messages
- Parkour action start/end
- State changes

---

## 🐛 Troubleshooting

### Problem: Parkour actions don't trigger
**Solutions:**
- Check layer mask includes your obstacles
- Verify detection ranges aren't too small
- Make sure player has enough speed (for vaulting)
- Check Console for "PreventParkourAction" messages

### Problem: Player gets stuck during parkour
**Solutions:**
- Increase animation transition times
- Check MatchTarget values in animations
- Call `ForceEndParkourAction()` in MFPS_IntegrationHelper

### Problem: Animations don't play
**Solutions:**
- Verify all animator parameters exist
- Check animation state names match exactly
- Ensure transitions are set up correctly
- Check animator controller is assigned

### Problem: Weapons visible during parkour
**Solutions:**
- Verify `gunManager` is not null
- Check `BlockAllWeapons()` is called
- Manually hide weapon renderers in `OnStartParkourAction()`

### Problem: Camera doesn't follow during parkour
**Solutions:**
- Check `UpdateCamera()` is being called
- Verify `playerCamera` reference is set
- Adjust camera lerp speed in `UpdateCamera()`

---

## 🎓 Advanced Customization

### Adding New Parkour Actions

1. Add new enum value to `ParkourAction` in `ParkourController`
2. Create detection method (e.g., `DetectSlideUnder()`)
3. Create execution method (e.g., `PerformSlideUnder()`)
4. Add coroutine for animation (e.g., `ExecuteSlideUnder()`)
5. Add animation state and parameters to Animator
6. Call from `CheckForParkour()`

### Custom Character Controllers

To use with non-MFPS controllers:

1. Implement `IParkourCharacter` interface
2. Create your own integration helper (similar to MFPS_IntegrationHelper)
3. Override `OnStartParkourAction()` and `OnEndParkourAction()`
4. Attach ParkourController and ClimbController

### Performance Optimization

- Use object pooling for temporary ClimbPoints
- Reduce raycast frequency (check every 2-3 frames)
- Optimize animator state machine
- Use LOD for parkour animations
- Disable parkour on far-away players (multiplayer)

---

## 📚 Additional Resources

### Recommended Animation Packs
- Mixamo - Free parkour animations
- Motion Matching Pack - Professional parkour
- Kubold Parkour System - Unity Asset Store

### Tutorial Videos
1. Setting up Animator Controller
2. Creating Climb Points
3. Custom Parkour Actions
4. Multiplayer Synchronization

---

## 📝 API Reference

### ParkourController
```csharp
// Check and execute parkour action
bool CheckForParkour()

// Execute vertical jump
void VerticalJump()

// Cancel current action
void CancelParkourAction()

// Get current action
ParkourAction GetCurrentAction()

// Properties
bool ControlledByParkour { get; }
bool InAction { get; }
```

### ClimbController
```csharp
// Start hanging
void StartHanging(ClimbPoint point)

// Stop hanging
void StopHanging()

// Start climbing
void StartClimbing(Vector3 position)

// Stop climbing
void StopClimbing()

// Check for wall
WallCheckData? CheckWall(ClimbPoint point)

// Get nearest climb point
ClimbPoint GetNearestPoint(Transform ledge, Vector3 position)

// Properties
bool IsHanging { get; }
bool IsClimbing { get; }
float CurrentStamina { get; }
```

### MFPS_IntegrationHelper
```csharp
// Force end parkour
void ForceEndParkourAction()

// Check status
bool IsParkourActive()
bool IsClimbing()
```

---

## ✅ Final Checklist

Before going live:

- [ ] All scripts compile without errors
- [ ] All animator parameters configured
- [ ] All animation states created
- [ ] Layer masks configured correctly
- [ ] Test all parkour actions
- [ ] Test climbing and hanging
- [ ] Test wall running
- [ ] Verify multiplayer sync (if applicable)
- [ ] Performance tested (60+ FPS)
- [ ] No console errors during gameplay

---

## 🎉 You're Done!

Your parkour system is now fully integrated with MFPS. Players can vault, climb, hang, and wall-run seamlessly with the FPS movement system!

For support or questions, check the inline code documentation or create an issue on GitHub.