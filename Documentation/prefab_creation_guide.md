# Prefab Creation Guide

This guide will help you create the two prefabs mentioned in the README:
1. **ClimbPoint.prefab** - For manual climb point placement
2. **PlayerWithAdvancedMovement.prefab** - Pre-configured player with all movement systems

---

## 1. Creating ClimbPoint.prefab

### Step 1: Create the GameObject

1. In Unity Hierarchy, right-click → `Create Empty`
2. Rename it to `ClimbPoint`
3. Reset Transform: Position (0, 0, 0), Rotation (0, 0, 0), Scale (1, 1, 1)

### Step 2: Add the ClimbPoint Component

1. Select the `ClimbPoint` GameObject
2. Click `Add Component`
3. Search for `ClimbPoint` (from ClimbController.cs)
4. Add the component

### Step 3: Create Child Objects

**Create MountPoint:**
1. Right-click `ClimbPoint` → `Create Empty`
2. Rename to `MountPoint`
3. Position it where the player's hands should grab (e.g., Y: 0, Z: -0.3)

**Create DismountPoint:**
1. Right-click `ClimbPoint` → `Create Empty`
2. Rename to `DismountPoint`
3. Position it where the player stands after climbing (e.g., Y: 1.5, Z: 0)

### Step 4: Configure ClimbPoint Component

1. Select `ClimbPoint`
2. In Inspector, find the `ClimbPoint` component
3. Drag `MountPoint` to the `Mount Point` field
4. Drag `DismountPoint` to the `Dismount Point` field
5. Set `Is Free Hang` based on whether there's a wall behind:
   - ☐ Unchecked = Braced hang (wall behind player)
   - ☑ Checked = Free hang (no wall support)

### Step 5: Add Visual Helper (Optional)

For easier placement in the editor:

1. Select `ClimbPoint`
2. Add `Icon` component (or attach a small cube mesh for visibility)
3. Or use Gizmos (already built into ClimbPoint.cs)

### Step 6: Create the Prefab

1. Create a `Prefabs` folder in your project: `Assets/Prefabs/`
2. Drag `ClimbPoint` from Hierarchy to the `Prefabs` folder
3. Delete the `ClimbPoint` from the Hierarchy (prefab is saved)

**Your ClimbPoint prefab structure:**
```
ClimbPoint
├── MountPoint (Transform)
└── DismountPoint (Transform)
```

### Usage in Your Level

To use the prefab:

1. Drag `ClimbPoint.prefab` from Project to your scene
2. Position it on a ledge or wall
3. Rotate it so the forward direction (blue arrow) points away from the wall
4. Adjust `MountPoint` and `DismountPoint` positions as needed
5. Connect multiple ClimbPoints via the `Connected Points` list for traversal

**Example Placement:**
```
[Wall]
  ↓
ClimbPoint_1 ←→ ClimbPoint_2 ←→ ClimbPoint_3
(Left)         (Middle)        (Right)
```

---

## 2. Creating PlayerWithAdvancedMovement.prefab

### Step 1: Start with Your MFPS Player

1. Open your MFPS project
2. Find your player prefab (usually in `Assets/MFPS/Content/Prefabs/Players/`)
3. Drag it into the scene (don't modify the original yet)
4. Rename the instance to `PlayerWithAdvancedMovement`

### Step 2: Add Parkour System Components

1. Select `PlayerWithAdvancedMovement`
2. Add `ParkourController`:
   - Click `Add Component` → Search `ParkourController`
   - Configure settings (or use defaults)
3. Add `ClimbController`:
   - Click `Add Component` → Search `ClimbController`
   - Configure settings (or use defaults)
4. Add `MFPS_IntegrationHelper`:
   - Click `Add Component` → Search `MFPS_IntegrationHelper`
   - It will auto-configure on Start

### Step 3: Add Skiing System Components

1. Select `PlayerWithAdvancedMovement`
2. Add `SlopeMovement`:
   - Click `Add Component` → Search `SlopeMovement`
   - Configure physics settings
3. Add `SlopeJetpack` (optional):
   - Click `Add Component` → Search `SlopeJetpack`
   - Configure fuel settings
   - Assign `Fuel Bar` UI element if using UI

### Step 4: Replace bl_FirstPersonController Script

**⚠️ IMPORTANT: Backup First!**

1. Find the `bl_FirstPersonController` component on your player
2. Note all the current settings (take a screenshot)
3. Remove the old component
4. Add the modified `bl_FirstPersonController` from this package
5. Reconfigure the settings to match your screenshot

### Step 5: Configure Component Settings

**ParkourController Recommended Settings:**
```
Obstacle Layer: Default, Ground
Forward Ray Length: 1.5
Vault Height: 1.0
Step Up Height: 0.6
Medium Step Up Height: 1.2
Climb Up Height: 2.0
Min Vault Speed: 3.0
Min Wall Run Speed: 5.0
```

**ClimbController Recommended Settings:**
```
Climbable Layer: Default, Ground
Climb Detection Range: 2.0
Climb Speed: 2.0
Hang Stamina: 10.0
Stamina Drain Rate: 1.0
Stamina Recover Rate: 2.0
```

**SlopeMovement Recommended Settings:**
```
Min Speed To Ski: 8.0
Max Speed: 35.0
Gravity: 18.0
Ski Friction: 0.3
Ground Control: 25.0
Ski Control: 20.0
Air Control: 1.2
Downhill Speed Gain: 0.6
Uphill Momentum Retention: 0.75
Turning Friction: 1.5
```

**SlopeJetpack Recommended Settings (if used):**
```
Jetpack Force: 25.0
Max Vertical Speed: 12.0
Max Fuel: 100.0
Fuel Consume Rate: 20.0
Fuel Refill Rate: 15.0
Fuel Refill Delay: 2.0
```

### Step 6: Setup UI (Optional Jetpack Fuel Bar)

If using the jetpack with UI:

1. Find or create a Canvas in your player prefab
2. Add a UI Image for the fuel bar:
   - Right-click Canvas → `UI` → `Image`
   - Rename to `JetpackFuelBar`
   - Set `Image Type` to `Filled`
   - Set `Fill Method` to `Horizontal`
3. Drag the `JetpackFuelBar` to `SlopeJetpack` → `Fuel Bar` field

### Step 7: Verify Integration

Before creating the prefab, test in Play Mode:

1. Press Play
2. Test basic movement (walk, run, jump)
3. Test parkour actions (vault, climb)
4. Test skiing (run downhill, build speed)
5. Test jetpack (hold jump in air)
6. Verify weapons work correctly
7. Check camera follows properly

### Step 8: Create the Prefab

1. If everything works, stop Play Mode
2. Create a `Prefabs` folder: `Assets/Prefabs/`
3. Drag `PlayerWithAdvancedMovement` from Hierarchy to `Prefabs` folder
4. You can now delete it from Hierarchy

### Step 9: Test the Prefab

1. Drag `PlayerWithAdvancedMovement.prefab` into a test scene
2. Test all features again
3. Make sure it spawns correctly in multiplayer

**Your Player prefab structure:**
```
PlayerWithAdvancedMovement
├── [MFPS Components]
│   ├── bl_PlayerReferences
│   ├── bl_FirstPersonController (Modified)
│   ├── bl_GunManager
│   └── ... other MFPS components
├── [Parkour Components]
│   ├── ParkourController
│   ├── ClimbController
│   └── MFPS_IntegrationHelper
└── [Skiing Components]
    ├── SlopeMovement
    └── SlopeJetpack (optional)
```

---

## Alternative: Script to Auto-Create Prefabs

If you want to create these prefabs via script:

### Create ClimbPoint via Script

```csharp
using UnityEngine;
using UnityEditor;
using FC_ParkourSystem;

public class CreateClimbPointPrefab : MonoBehaviour
{
    [MenuItem("Tools/Create ClimbPoint Prefab")]
    static void CreatePrefab()
    {
        // Create main object
        GameObject climbPoint = new GameObject("ClimbPoint");
        ClimbPoint cp = climbPoint.AddComponent<ClimbPoint>();

        // Create mount point
        GameObject mountPoint = new GameObject("MountPoint");
        mountPoint.transform.parent = climbPoint.transform;
        mountPoint.transform.localPosition = new Vector3(0, 0, -0.3f);
        cp.mountPoint = mountPoint.transform;

        // Create dismount point
        GameObject dismountPoint = new GameObject("DismountPoint");
        dismountPoint.transform.parent = climbPoint.transform;
        dismountPoint.transform.localPosition = new Vector3(0, 1.5f, 0);
        cp.dismountPoint = dismountPoint.transform;

        // Save as prefab
        string path = "Assets/Prefabs/ClimbPoint.prefab";
        PrefabUtility.SaveAsPrefabAsset(climbPoint, path);
        
        // Clean up
        DestroyImmediate(climbPoint);
        
        Debug.Log($"ClimbPoint prefab created at {path}");
    }
}
```

---

## Troubleshooting

### ClimbPoint Prefab Issues

**Problem: Can't find MountPoint/DismountPoint**
- Make sure they're children of ClimbPoint
- Check the references are assigned in the ClimbPoint component

**Problem: Player snaps to wrong position**
- Adjust MountPoint position (where hands grab)
- Adjust DismountPoint position (where player stands after climbing)
- Check ClimbPoint rotation (forward should point away from wall)

### Player Prefab Issues

**Problem: Scripts missing after adding components**
- Make sure all scripts are compiled without errors
- Check console for missing script warnings
- Verify namespace imports are correct

**Problem: Player can't move after adding systems**
- Check bl_FirstPersonController is the modified version
- Verify MFPS_IntegrationHelper initialized correctly
- Look for console errors on player spawn

**Problem: Parkour/Skiing not working**
- Verify all components are attached
- Check layer masks are configured
- Test detection ranges with Gizmos
- Ensure ground detection is working

---

## Prefab Variants (Advanced)

You can create variants for different game modes:

### Competitive Preset
```
PlayerWithAdvancedMovement_Competitive
- Higher skill requirements
- Lower stamina
- Faster movement
```

### Casual Preset
```
PlayerWithAdvancedMovement_Casual
- Lower skill requirements
- Higher stamina
- Easier controls
```

To create variants:
1. Duplicate the base prefab
2. Rename it (e.g., `_Competitive`)
3. Adjust the component values
4. Save as a new prefab

---

## Next Steps

After creating these prefabs:

1. ✅ Test in a simple scene first
2. ✅ Configure settings for your game's balance
3. ✅ Create level geometry that supports the movement
4. ✅ Add ClimbPoints to your maps manually
5. ✅ Test in multiplayer if applicable
6. ✅ Create additional prefab variants as needed

---

**You now have:**
- `ClimbPoint.prefab` - Ready to place on ledges
- `PlayerWithAdvancedMovement.prefab` - Ready to spawn in game

Both prefabs are reusable across all your levels!

---

*For more information, see the Setup Guides in the Documentation folder.*