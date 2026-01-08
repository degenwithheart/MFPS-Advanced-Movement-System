# Tribes-Style Skiing System - Complete Setup Guide

## 🎿 Overview

This system adds Tribes-style skiing mechanics to MFPS, allowing players to slide down slopes at high speed with momentum-based physics, air control, and an optional jetpack.

**Key Features:**
- Momentum-based skiing physics
- Downhill acceleration and uphill momentum retention
- Turning friction and speed management
- Air control and ground sticking
- Optional jetpack with fuel system
- Seamless integration with MFPS

---

## 📁 File Structure

Create this folder structure in your project:

```
Assets/
└── MFPS/
    └── Scripts/
        └── Player/
            └── Controller/
                └── bl_FirstPersonController.cs (modified MFPS controller)
└── Scripts/
    └── SlopeSystem/
        ├── SlopeMovement.cs
        ├── SlopeJetpack.cs (optional)
```

---

## 🎮 Step 1: Basic Setup

### 1.1 Create the Scripts
1. Copy `SlopeMovement.cs` and `SlopeJetpack.cs` into the `SlopeSystem` folder
2. **Replace** your existing `bl_FirstPersonController.cs` with the modified version
   - ⚠️ **IMPORTANT:** Backup your original `bl_FirstPersonController.cs` first!
3. Wait for Unity to compile
4. Check Console for any errors

### 1.2 Player Setup
1. Select your MFPS Player prefab
2. Add these components:
   - `SlopeMovement` component
   - `SlopeJetpack` component (optional, for jetpack feature)

---

## ⚙️ Step 2: Component Configuration

### 2.1 SlopeMovement Settings

**Skiing Physics (Tribes-Style):**
- `Min Speed To Ski`: **8.0** - Minimum speed required to enter skiing mode
- `Max Speed`: **35.0** - Absolute maximum horizontal speed (0 = unlimited)
- `Gravity`: **18.0** - Gravity force when airborne or descending
- `Ground Friction`: **4.0** - Friction during normal ground movement
- `Ski Friction`: **0.3** - Base friction while skiing (very low for momentum)

**Control Settings:**
- `Ground Control`: **25.0** - Directional control on ground
- `Ski Control`: **20.0** - Directional control while skiing
- `Air Control`: **1.2** - Directional control while airborne

**Physics Tuning:**
- `Ground Stick Force`: **20.0** - Force to keep player stuck to ground at high speed
- `Uphill Momentum Retention`: **0.75** - How much momentum is kept going uphill (0-1)
- `Downhill Speed Gain`: **0.6** - Speed multiplier when going downhill
- `Min Slope Angle For Boost`: **15.0** - Minimum slope angle to gain speed
- `Turning Friction`: **1.5** - Extra friction applied when turning while skiing

### 2.2 SlopeJetpack Settings (Optional)

**Jetpack Settings:**
- `Jetpack Force`: **25.0** - Upward acceleration force
- `Max Vertical Speed`: **12.0** - Maximum upward velocity
- `Jetpack Key`: **Space** - Key to activate jetpack

**Fuel Settings:**
- `Max Fuel`: **100.0** - Maximum jetpack fuel
- `Fuel Consume Rate`: **20.0** - Fuel consumed per second
- `Fuel Refill Rate`: **15.0** - Fuel refilled per second
- `Fuel Refill Delay`: **2.0** - Delay before fuel starts refilling

**UI (Optional):**
- `Fuel Bar`: Drag a UI Image component here to show fuel bar

### 2.3 bl_FirstPersonController Integration

The modified `bl_FirstPersonController.cs` automatically integrates with `SlopeMovement`. Key changes:

**Automatic Detection:**
- Detects `SlopeMovement` component on Awake
- If found, delegates all movement to skiing system
- Maintains compatibility with original MFPS features

**New Player State:**
- Added `PlayerState.Skiing` (value: 28)
- Automatically transitions between normal and skiing states

**Jump Integration:**
- Jump button now works with jetpack when held
- Single press = normal jump
- Hold = jetpack activation (if `SlopeJetpack` component present)

---

## 🎨 Step 3: Animator Setup (Optional)

If you want skiing-specific animations:

### 3.1 Add Animator Parameters

**Bool Parameters:**
- `IsSkiing` - Is player currently skiing

**Float Parameters:**
- `SkiSpeed` - Current skiing speed (for animation blending)

### 3.2 Create Animation States

**Skiing States:**
- SkiIdle - Slow skiing/coasting
- SkiFast - High-speed skiing
- SkiTurn - Turning while skiing
- SkiJump - Jumping while skiing
- SkiLanding - Landing from a ski jump

### 3.3 Animation Transitions

Set up transitions based on speed and state:
- `IsSkiing == true && SkiSpeed > 15` → SkiFast
- `IsSkiing == true && SkiSpeed < 15` → SkiIdle
- `IsSkiing == true && IsJumping == true` → SkiJump

---

## 🗺️ Step 4: Level Design

### 4.1 Terrain Setup

**Slope Angles:**
- **Flat (0-5°):** Normal walking, minimal skiing
- **Gentle (5-15°):** Slow skiing, good for beginners
- **Medium (15-30°):** Fast skiing, main gameplay slopes
- **Steep (30-45°):** Very fast skiing, advanced routes
- **Extreme (45°+):** Character controller's slope limit

**Recommended Terrain Features:**
- Long, smooth slopes for speed building
- Hills and valleys for momentum play
- Jump ramps for aerial movement
- Flat areas for speed control

### 4.2 Collision Setup

**Important:**
- Use smooth terrain colliders (no sharp edges)
- Avoid small obstacles on ski routes
- Test collision normals (slopes should face up correctly)

### 4.3 Map Layout Tips

**Good Skiing Map Design:**
```
[High Point] → [Long Slope] → [Jump Ramp] → [Valley] → [Uphill] → [Flag/Objective]
```

- Place objectives at the end of ski routes
- Create multiple paths with different difficulty
- Add aerial shortcuts for skilled players
- Include flat areas for combat/respite

---

## ⌨️ Step 5: Input Setup

### 5.1 Default Controls

**Movement:**
- `WASD` / `Left Stick` - Direction control
- `Shift` / `L3` - Sprint (normal mode)
- `Ctrl` / `B` - Crouch

**Skiing:**
- `Space` / `A` - Jump / Jetpack (hold for jetpack)
- Auto-activates when speed > `Min Speed To Ski`

**How to Ski:**
1. Build speed by running downhill
2. Once speed > 8 m/s, skiing auto-activates
3. Maintain momentum by finding slopes
4. Use jetpack to gain altitude
5. Air control for mid-air adjustments

### 5.2 Advanced Techniques

**Bunny Hopping:**
- Jump just before hitting the ground
- Maintains more momentum than ground contact
- Use for crossing flat areas

**Route Chaining:**
- Link slopes together for maximum speed
- Use jetpack to reach higher ski routes
- Turn minimally to preserve momentum

**Combat Skiing:**
- Ski past enemies at high speed
- Use jetpack for evasive maneuvers
- Time jumps to dodge projectiles

---

## 🎯 Step 6: Testing

### 6.1 Test Checklist

**Basic Skiing:**
- [ ] Player enters skiing mode at correct speed
- [ ] Downhill acceleration works
- [ ] Uphill momentum retention works
- [ ] Turning applies friction
- [ ] Max speed is enforced
- [ ] Ground stick force keeps player on slopes

**Physics Behavior:**
- [ ] Smooth transitions between walking and skiing
- [ ] Air control works properly
- [ ] Landing doesn't kill momentum
- [ ] No stuttering on slopes
- [ ] Coyote time prevents accidental falls

**Jetpack (if enabled):**
- [ ] Jetpack activates when holding jump
- [ ] Fuel drains correctly
- [ ] Fuel refills after delay
- [ ] Max vertical speed is enforced
- [ ] UI fuel bar updates (if configured)

**MFPS Integration:**
- [ ] Normal movement still works
- [ ] Weapons function correctly
- [ ] Crouch/slide works
- [ ] Combat is balanced
- [ ] No conflicts with other systems

### 6.2 Debug Information

**Console Logs:**
The system provides these logs:
- Component initialization
- Skiing state changes
- Speed warnings (if approaching max)

**In-Game Debugging:**
Add this to `SlopeMovement.cs` for on-screen stats:
```csharp
void OnGUI()
{
    GUILayout.Label($"Speed: {CurrentSpeed:F2} m/s");
    GUILayout.Label($"Skiing: {IsSkiing}");
    GUILayout.Label($"Grounded: {controller.m_CharacterController.isGrounded}");
}
```

---

## 🛠️ Step 7: Fine-Tuning

### 7.1 Speed Balance

**If skiing feels too slow:**
- Increase `Downhill Speed Gain` (0.6 → 0.8)
- Decrease `Ski Friction` (0.3 → 0.2)
- Increase `Gravity` (18 → 22)

**If skiing feels too fast:**
- Decrease `Max Speed` (35 → 30)
- Increase `Turning Friction` (1.5 → 2.0)
- Increase `Ski Friction` (0.3 → 0.5)

### 7.2 Control Balance

**If control feels too loose:**
- Increase `Ski Control` (20 → 25)
- Increase `Air Control` (1.2 → 1.5)
- Decrease `Ground Stick Force` (20 → 15)

**If control feels too tight:**
- Decrease `Ski Control` (20 → 15)
- Decrease `Air Control` (1.2 → 0.8)
- Increase `Ground Stick Force` (20 → 25)

### 7.3 Jetpack Balance

**If jetpack feels too weak:**
- Increase `Jetpack Force` (25 → 30)
- Increase `Max Vertical Speed` (12 → 15)
- Increase `Max Fuel` (100 → 150)

**If jetpack feels too strong:**
- Decrease `Jetpack Force` (25 → 20)
- Decrease `Max Vertical Speed` (12 → 10)
- Increase `Fuel Consume Rate` (20 → 30)

---

## 🐛 Troubleshooting

### Problem: Skiing doesn't activate
**Solutions:**
- Check `Min Speed To Ski` isn't too high (try 5-6)
- Verify `SlopeMovement` component is attached
- Ensure slopes are steep enough (>15°)
- Check `bl_FirstPersonController` detects `SlopeMovement`

### Problem: Player falls through slopes
**Solutions:**
- Increase `Ground Stick Force` (20 → 30)
- Check terrain colliders are continuous
- Reduce `Max Speed` if too fast
- Verify `CharacterController.stepOffset` is appropriate

### Problem: Can't gain speed on slopes
**Solutions:**
- Check `Downhill Speed Gain` (should be 0.4-1.0)
- Verify `Min Slope Angle For Boost` isn't too high
- Reduce `Ski Friction` (0.3 → 0.2)
- Ensure slopes are facing correct direction (normal up)

### Problem: Landing kills all momentum
**Solutions:**
- Check `OnLand()` function in `bl_FirstPersonController.cs`
- Verify skiing state is preserved on landing
- Increase `Uphill Momentum Retention` (0.75 → 0.9)
- Ensure `Ground Stick Force` isn't too aggressive

### Problem: Player gets stuck on small obstacles
**Solutions:**
- Increase `CharacterController.stepOffset` (0.4 → 0.8)
- Use smoother terrain/colliders
- Remove small obstacles from ski routes
- Adjust `Ground Stick Force`

### Problem: Turning is too difficult
**Solutions:**
- Increase `Ski Control` (20 → 30)
- Decrease `Turning Friction` (1.5 → 1.0)
- Increase `Air Control` for aerial adjustments
- Practice maintaining momentum through turns

### Problem: Jetpack doesn't work
**Solutions:**
- Verify `SlopeJetpack` component is attached
- Check `SlopeMovement.CalculateMovement()` receives jetpack input
- Ensure fuel is available (`CurrentFuel > 0`)
- Verify jump button is being held (not just pressed)

---

## 🎓 Advanced Customization

### Adding Speed Trails

Add visual feedback for high-speed skiing:

```csharp
// In SlopeMovement.cs
[Header("Visual Effects")]
public ParticleSystem speedTrail;
public float trailSpeedThreshold = 20f;

void Update()
{
    if (speedTrail != null)
    {
        if (IsSkiing && CurrentSpeed > trailSpeedThreshold)
        {
            if (!speedTrail.isPlaying) speedTrail.Play();
        }
        else if (speedTrail.isPlaying) speedTrail.Stop();
    }
}
```

### Custom Slope Types

Add different friction values for different terrain:

```csharp
[Header("Terrain Types")]
public float snowFriction = 0.3f;
public float iceFriction = 0.1f;
public float grassFriction = 2.0f;

// Detect terrain type via raycasting or triggers
// Apply appropriate friction value
```

### Multiplayer Skiing

For networked skiing, sync these values:

```csharp
// Add to PhotonView observed components
[PunRPC]
void SyncSkiingState(bool isSkiing, float speed)
{
    // Update remote player's skiing visuals
    IsSkiing = isSkiing;
    CurrentSpeed = speed;
}

// Call in Update() if photonView.IsMine
photonView.RPC("SyncSkiingState", RpcTarget.Others, IsSkiing, CurrentSpeed);
```

### Slope-Based Damage

Add damage for hitting slopes at high speed:

```csharp
void OnControllerColliderHit(ControllerColliderHit hit)
{
    if (CurrentSpeed > 30f)
    {
        float impactAngle = Vector3.Angle(velocity.normalized, hit.normal);
        if (impactAngle > 90f) // Hit head-on
        {
            float damage = (CurrentSpeed - 30f) * 2f;
            // Apply damage
        }
    }
}
```

---

## 📚 Physics Explanation

### How Skiing Works

**1. Speed Detection:**
```csharp
IsSkiing = grounded && CurrentSpeed > minSpeedToSki;
```
- Skiing activates automatically when speed threshold is reached
- Returns to normal movement when speed drops below threshold

**2. Slope Acceleration:**
```csharp
if (onSlope && slopeAngle > minSlopeAngleForBoost)
{
    Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
    velocity += slopeDir * gravity * slopeFactor * downhillSpeedGain;
}
```
- Calculates slope direction from surface normal
- Applies gravity-based acceleration down slopes
- Scaled by `downhillSpeedGain` and slope angle

**3. Uphill Momentum:**
```csharp
if (verticalDir > 0f) // Going uphill
{
    velocity += slopeDir * gravity * slopeFactor * uphillMomentumRetention;
}
```
- Maintains momentum when going uphill
- Gradual deceleration based on `uphillMomentumRetention`

**4. Turning Friction:**
```csharp
float angle = Vector3.Angle(lastMoveDirection, inputDir);
float turnLoss = Mathf.Clamp01(angle / 90f) * turningFriction;
velocity = Vector3.Lerp(velocity, velocity * (1f - turnLoss), Time.deltaTime * 5f);
```
- Sharp turns cause more speed loss
- Encourages smooth, wide turns for speed maintenance

**5. Ground Stick:**
```csharp
if (jumpPressed && !jumpHeld)
{
    velocity.y = controller.jumpSpeed;
}
else if (!jumpPressed && velocity.y < 0f)
{
    velocity.y = -groundStickForce;
}
```
- Pulls player down to prevent bouncing on slopes
- Only applies downward force, doesn't interfere with jumps

---

## ✅ Final Checklist

Before going live:

- [ ] All scripts compile without errors
- [ ] `SlopeMovement` component configured
- [ ] `SlopeJetpack` component configured (if used)
- [ ] `bl_FirstPersonController.cs` replaced successfully
- [ ] Slopes built with appropriate angles (15-45°)
- [ ] Terrain colliders are smooth and continuous
- [ ] Max speed prevents physics breakage
- [ ] Jetpack fuel system balanced
- [ ] Controls feel responsive
- [ ] No conflicts with MFPS features
- [ ] Tested in multiplayer (if applicable)
- [ ] Performance tested (60+ FPS on slopes)
- [ ] UI elements configured (fuel bar, etc.)

---

## 🎉 You're Done!

Your Tribes-style skiing system is now fully integrated with MFPS! Players can experience fast-paced, momentum-based movement with smooth skiing physics, air control, and optional jetpack gameplay.

**Recommended Settings for Different Playstyles:**

**Casual (Easy to control):**
- Min Speed To Ski: 10
- Max Speed: 30
- Ski Friction: 0.5
- Turning Friction: 1.0

**Competitive (Skill-based):**
- Min Speed To Ski: 8
- Max Speed: 40
- Ski Friction: 0.3
- Turning Friction: 2.0

**Extreme (Maximum speed):**
- Min Speed To Ski: 6
- Max Speed: 50
- Ski Friction: 0.2
- Turning Friction: 2.5

---

For support or questions, check the inline code documentation or reach out to the MFPS community!