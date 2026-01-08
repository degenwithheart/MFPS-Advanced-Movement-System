# MFPS Advanced Movement System

**Version 1.0.0** | *Parkour + Tribes-Style Skiing with Jetpack*

A comprehensive movement enhancement mod for **Multiplayer FPS Template (MFPS)** that adds professional parkour mechanics and Tribes-style skiing physics, transforming your FPS into a fast-paced, momentum-based shooter.

---

## 🎮 Features

### Parkour System
- **Intelligent Obstacle Detection** - Automatically detects vaultable, climbable, and step-able obstacles
- **Dynamic Actions** - Vault over, step up, climb, and wall run based on obstacle height and player speed
- **Advanced Climbing** - Hang from ledges, traverse horizontally, and climb up/down walls
- **Stamina System** - Limited hang time with stamina drain and recovery
- **Wall Running** - Run along vertical surfaces at high speed
- **Smooth Animations** - Root motion support with match target positioning
- **MFPS Integration** - Seamlessly works with weapons, camera, and player states

### Skiing System (Tribes-Style)
- **Momentum Physics** - Build and maintain speed through slopes and jumps
- **Slope Detection** - Automatic acceleration downhill, momentum retention uphill
- **Advanced Controls** - Directional air control, ground sticking, and turn friction
- **Speed Management** - Configurable max speed and friction values
- **Bunny Hopping** - Chain jumps to maintain momentum
- **Dynamic State System** - Smooth transitions between walking, running, and skiing

### Optional Jetpack
- **Fuel System** - Limited fuel with automatic refill after cooldown
- **Vertical Mobility** - Gain altitude for ski routes and tactical positioning
- **Speed Limiting** - Prevents excessive vertical velocity
- **UI Integration** - Optional fuel bar display
- **Balance Controls** - Configurable force, fuel consumption, and refill rates

---

## 📦 Package Contents

```
MFPS-Advanced-Movement/
├── README.md (this file)
├── CHANGELOG.md
├── LICENSE.md
│
├── Documentation/
│   ├── parkour_setup_guide.md         ✅ Included
│   ├── skiing_setup_guide.md          ✅ Included
│   ├── API_Reference.md               ✅ Included
│   └── prefab_creation_guide.md       ✅ Included
│
├── Scripts/
│   ├── ParkourSystem/
│   │   ├── IParkourCharacter.cs             ✅ Included
│   │   ├── ParkourController.cs             ✅ Included
│   │   ├── ClimbController.cs               ✅ Included
│   │   └── MFPS_IntegrationHelper.cs        ✅ Included
│   │
│   ├── SkiingSystem/
│   │   ├── SlopeMovement.cs                 ✅ Included
│   │   └── SlopeJetpack.cs                  ✅ Included
│   │
│   └── Modified/
│       └── bl_FirstPersonController.cs      ✅ Included
│
└── Prefabs/ (Create using prefab_creation_guide.md)
│   ├── ClimbPoint.prefab                    📝 Create manually
│   └── PlayerWithAdvancedMovement.prefab    📝 Create manually
```

---

## ⚙️ System Requirements

### Unity Version
- **Unity 2020.3 LTS** or newer
- **Unity 6000.3 LTS** (Recommended)
- Unity 2022.3+ (Tested and compatible)

### MFPS Version
- **MFPS 1.9+** (Required)
- MFPS 2.0+ (Fully compatible)

### Dependencies
- MFPS Core
- Photon PUN 2
- TextMeshPro (for UI)

### Platform Support
- ✅ Windows (Standalone)
- ✅ macOS (Standalone)
- ✅ Linux (Standalone)
- ✅ WebGL
- ⚠️ Mobile (Partial - requires input adaptation)
- ⚠️ Console (Untested)

---

## 🚀 Quick Start

### Installation (5 minutes)

1. **Backup Your Project**
   ```
   File → Save As... → [ProjectName]_Backup
   ```

2. **Import the Package**
   - Extract the zip file
   - Copy `Scripts/` and `Documentation/` folders to your MFPS project's `Assets/` directory
   - Wait for Unity to compile

3. **Replace Core Controller**
   - ⚠️ **IMPORTANT:** Backup `bl_FirstPersonController.cs` first
   - Replace with the modified version from `Scripts/Modified/`
   - Unity will recompile automatically

4. **Setup Player Prefab**
   - Select your player prefab
   - Add components:
     - `ParkourController`
     - `ClimbController`
     - `MFPS_IntegrationHelper`
     - `SlopeMovement`
     - `SlopeJetpack` (optional)

5. **Create Helper Prefabs (Optional)**
   - Follow the **[Prefab Creation Guide](Documentation/prefab_creation_guide.md)** to create:
     - `ClimbPoint.prefab` for manual climb point placement
     - `PlayerWithAdvancedMovement.prefab` for easy player spawning

6. **Configure Settings**
   - Adjust parkour detection ranges (see setup guides)
   - Tune skiing physics parameters
   - Configure jetpack fuel system (if enabled)

7. **Test in Play Mode**
   - Open your level or create a test scene
   - Test parkour actions (vault, climb, wall run)
   - Test skiing on slopes
   - Verify MFPS integration (weapons, camera, etc.)

**🎉 Done!** Your players now have advanced movement capabilities.

---

## 📖 Documentation

### Setup Guides
- **[Parkour Setup Guide](Documentation/parkour_setup_guide.md)** - Complete parkour system installation and configuration
- **[Skiing Setup Guide](Documentation/skiing_setup_guide.md)** - Tribes-style skiing and jetpack setup
- **[Prefab Creation Guide](Documentation/prefab_creation_guide.md)** - Create ClimbPoint and Player prefabs
- **[API Reference](Documentation/API_Reference.md)** - Script documentation for developers

### Quick Reference

**Parkour Actions:**
- **Vault** - Low obstacles (~1m), high speed required
- **Step Up** - Small steps (~0.6m), any speed
- **Climb** - Tall obstacles (~2m), automatic detection
- **Wall Run** - Vertical surfaces, very high speed required
- **Hang/Traverse** - Ledge grabbing with stamina system

**Skiing Controls:**
- **Auto-Activate** - Skiing starts at 8+ m/s automatically
- **Downhill** - Gain speed (slopes >15°)
- **Uphill** - Maintain momentum (75% retention)
- **Turning** - Wide turns preserve speed, sharp turns lose speed
- **Jumping** - Bunny hop to maintain momentum
- **Jetpack** - Hold jump in air for vertical boost

**Default Keybinds:**
- `Space` - Jump / Vault / Climb Up / Jetpack (hold)
- `E` - Drop to Hang / Interact
- `Ctrl` - Crouch / Slide
- `Shift` - Sprint (normal mode)
- `WASD` - Movement / Air Control

---

## 🎯 Use Cases

### Game Modes

**Flag Capture (CTF)**
- Ski routes to flag
- Parkour to base defenses
- Jetpack for flag grabs
- High-speed chases

**Deathmatch**
- Fast-paced combat skiing
- Vertical parkour for positioning
- Wall runs for flanking
- Dynamic aerial combat

**Team Deathmatch**
- Coordinated ski routes
- Parkour map control
- Jetpack team pushes
- Momentum-based tactics

**Battle Royale**
- Rapid zone traversal
- Parkour looting routes
- Jetpack height advantage
- Skiing escape mechanics

---

## 🛠️ Configuration Presets

### Casual (Easy to Learn)
**Parkour:**
- Lower detection ranges
- Generous auto-vault
- High stamina (15s)
- Slow stamina drain

**Skiing:**
- Higher min ski speed (10 m/s)
- Lower max speed (30 m/s)
- Higher friction (0.5)
- Gentle turning penalty (1.0)

**Jetpack:**
- High fuel capacity (150)
- Low consumption (15/s)
- Quick refill (20/s)
- Strong thrust (30)

---

### Competitive (Skill-Based)
**Parkour:**
- Precise detection
- Speed requirements enforced
- Medium stamina (10s)
- Standard drain rate

**Skiing:**
- Lower min ski speed (8 m/s)
- Moderate max speed (35 m/s)
- Low friction (0.3)
- Moderate turning penalty (1.5)

**Jetpack:**
- Standard fuel (100)
- Standard consumption (20/s)
- Standard refill (15/s)
- Standard thrust (25)

---

### Hardcore (Maximum Skill Ceiling)
**Parkour:**
- Tight detection ranges
- High speed requirements
- Low stamina (7s)
- Fast stamina drain

**Skiing:**
- Very low min ski speed (6 m/s)
- High max speed (45 m/s)
- Minimal friction (0.2)
- High turning penalty (2.5)

**Jetpack:**
- Low fuel capacity (75)
- High consumption (30/s)
- Slow refill (10/s)
- Weak thrust (20)

---

## 🔧 Troubleshooting

### Common Issues

**Parkour not triggering?**
- Check obstacle layer mask settings
- Verify detection ranges aren't too small
- Ensure player has required speed (vault/wall run)

**Skiing not activating?**
- Speed must exceed `minSpeedToSki` (default 8 m/s)
- Slopes must be >15° for speed gain
- Check `SlopeMovement` component is attached

**Player falling through slopes?**
- Increase `groundStickForce` (20 → 30)
- Verify terrain colliders are continuous
- Check `CharacterController.slopeLimit`

**Jetpack not working?**
- Must be in air when activating
- Check fuel is available
- Verify `SlopeJetpack` component attached
- Hold jump button (don't just tap)

**Weapons visible during parkour?**
- Check `gunManager` reference in MFPS_IntegrationHelper
- Verify `BlockAllWeapons()` is called
- Test with `ForceEndParkourAction()` if stuck

**Animation issues?**
- Verify all animator parameters exist
- Check animation state names match exactly
- Ensure transitions are configured
- Test `applyRootMotion` toggles correctly

**Performance problems?**
- Reduce raycast frequency (every 2-3 frames)
- Disable gizmos in builds
- Use object pooling for ClimbPoints
- Optimize animator state machines

---

## 🎨 Level Design Tips

### Parkour Maps
- **Vertical Spaces** - Multi-level buildings, towers, walls
- **Obstacle Variety** - Mix of vaultable, climbable, step-able
- **ClimbPoint Placement** - Logical ledges, windows, pipes
- **Wall Run Paths** - Long vertical surfaces for chaining
- **Cover Integration** - Parkour-accessible cover positions

### Skiing Maps
- **Slope Networks** - Connected hills and valleys
- **Route Variety** - Multiple paths, different difficulties
- **Jump Gaps** - Aerial challenges requiring speed
- **Flat Zones** - Combat areas at route intersections
- **Height Variation** - Jetpack-accessible high ground

### Combined Maps
- **Hybrid Zones** - Transition between parkour and skiing
- **Vertical + Horizontal** - Climb to ski routes, ski to parkour
- **Strategic Objectives** - Require both systems to reach
- **Dynamic Flow** - Multiple movement options at all times

---

## 🤝 Compatibility

### Compatible MFPS Systems
- ✅ Weapon System
- ✅ Camera System
- ✅ Player States
- ✅ Health/Damage
- ✅ Team System
- ✅ Kill Feed
- ✅ Scoreboard
- ✅ Audio System
- ✅ UI System
- ✅ Photon Networking

### Compatible Third-Party Assets
- ✅ Invector Character Controller (with custom integration)
- ✅ Easy Character Movement (with modifications)
- ✅ Final IK (for animation enhancement)
- ✅ DOTween (for smooth transitions)
- ⚠️ Other movement systems (may require custom integration)

### Known Conflicts
- ❌ Other parkour systems (disable one)
- ❌ Alternative physics controllers (use MFPS default)
- ⚠️ Custom gravity systems (may interfere with skiing)

---

## 📝 Changelog

### Version 1.0.0 (Initial Release)
- ✨ Complete parkour system with 7 action types
- ✨ Tribes-style skiing physics implementation
- ✨ Optional jetpack with fuel system
- ✨ MFPS integration layer
- ✨ Climbing with stamina mechanics
- ✨ Wall running support
- ✨ Example scenes and prefabs
- 📖 Complete documentation suite
- 🐛 Fixed: Landing animation during skiing
- 🐛 Fixed: Coyote time integration
- 🐛 Fixed: Weapon hiding during parkour

[View full changelog](CHANGELOG.md)

---

## 🎓 Learning Resources

### Example Scenes (Create Your Own)
You can create test scenes to practice with the systems:
- **Parkour Course** - Urban environment with vaultable obstacles, climbable walls, and ledges
- **Skiing Map** - Mountain terrain with slopes, jumps, and valleys
- **Combined Map** - Hybrid level requiring both parkour and skiing skills

**Tip:** Start with Unity terrain tools to create skiing slopes, then add buildings/structures for parkour.

---

## 💡 Tips for Best Results

### Performance Optimization
- Use occlusion culling for large ski maps
- Implement LOD for distant parkour objects
- Reduce raycast frequency on lower-end hardware
- Disable debug gizmos in production builds
- Profile regularly with Unity Profiler

### Balance Considerations
- Test all movement combinations (ski→parkour→ski)
- Adjust speeds for your weapon TTK
- Consider map size when setting max speeds
- Balance jetpack fuel for vertical map design
- Playtest with skill variety (new vs. experienced)

### Multiplayer Best Practices
- Sync skiing state for visual consistency
- Ensure parkour animations replicate
- Test with high latency (100+ ms)
- Implement client-side prediction
- Add anti-cheat speed validation

---

## 📜 License

**MIT License**

Copyright (c) 2025 MFPS Advanced Movement System

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

---

## 🙏 Credits

**Development:**
- Core parkour system design and implementation
- Tribes-style skiing physics implementation
- MFPS integration layer
- Documentation and examples

**Inspired By:**
- Tribes: Ascend (skiing physics)
- Mirror's Edge (parkour fluidity)
- Titanfall 2 (movement chaining)
- Apex Legends (momentum mechanics)

**Special Thanks:**
- Lovatto Studio for MFPS
- Unity Technologies
- Photon Engine
- The MFPS community

**Third-Party Assets:**
- MFPS by Lovatto Studio
- Photon PUN 2 by Exit Games

---

## 📞 Support

### Documentation
- [Complete Setup Guides](Documentation/)
- [API Reference](Documentation/API_Reference.md)
- [FAQ](#) (Coming soon)

## 🚧 Roadmap

### Version 1.1 (Planned)
- [ ] Slide-to-ski transitions
- [ ] Advanced wall climbing (vertical movement)
- [ ] Grappling hook integration
- [ ] Double jump mechanics
- [ ] Slope-specific sound effects

### Version 1.2 (Planned)
- [ ] Mobile input optimization
- [ ] Console controller support
- [ ] VR compatibility mode
- [ ] Additional parkour actions (dive roll, vault slide)
- [ ] Advanced jetpack modes (dash, hover)

### Version 2.0 (Future)
- [ ] Complete animation pack
- [ ] Visual effects library
- [ ] Map editor tools
- [ ] Built-in tutorial system
- [ ] Mod API for extensions

---

## ⭐ Final Notes

This system transforms MFPS into a **fast-paced, momentum-based shooter** where movement mastery is just as important as aim. Whether you're building a competitive skiing game, a parkour-focused battle royale, or a hybrid movement shooter, this mod provides the foundation for incredible gameplay.

**Remember:** Great movement systems require great maps. Invest time in level design that rewards skilled movement and you'll create truly memorable gameplay experiences.

**Happy developing!** 🚀

---

*If this system helped your project, consider leaving a review, sharing with the community, or contributing improvements. Every bit helps make this better for everyone!*

**Version 1.0.0** | Last Updated: January 2025 | [GitHub Repository](https://github.com/degenwithheart/MFPS-Advanced-Movement-System) | [Documentation](Documentation/)