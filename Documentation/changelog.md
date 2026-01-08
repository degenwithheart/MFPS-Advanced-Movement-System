# Changelog

All notable changes to the MFPS Advanced Movement System will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2025-01-08

### 🎉 Initial Release

First public release of the MFPS Advanced Movement System combining parkour mechanics and Tribes-style skiing physics.

#### ✨ Added - Parkour System

**Core Features:**
- Intelligent obstacle detection system using raycasts
- 7 parkour action types (StepUp, MediumStepUp, VaultOver, VaultOn, ClimbUp, WallRun, VerticalJump)
- Automatic action selection based on obstacle height and player speed
- Root motion animation support with match target positioning
- Speed-gated actions (vault requires 3 m/s, wall run requires 5 m/s)

**Climbing System:**
- Ledge hang mechanics with braced hang and free hang modes
- Stamina system (10s default, configurable drain/recovery rates)
- Horizontal ledge traversal with connected ClimbPoints
- Automatic ClimbPoint generation for simple obstacles
- Manual ClimbPoint placement for precise level design
- Wall detection for hang type determination

**Components:**
- `IParkourCharacter` interface for character controller abstraction
- `ParkourController` for action detection and execution
- `ClimbController` for climbing and hanging mechanics
- `MFPS_IntegrationHelper` for seamless MFPS integration
- `ClimbPoint` system for manual climb point placement
- `EnvironmentScanner` helper class for detection

#### ✨ Added - Skiing System

**Core Features:**
- Tribes-style momentum-based physics
- Automatic skiing activation at configurable speed threshold (default 8 m/s)
- Downhill acceleration with configurable slope angle requirements
- Uphill momentum retention (default 75%)
- Maximum speed limiting (default 35 m/s, configurable)
- Ground stick force to prevent bouncing on slopes
- Turning friction system (sharp turns = speed loss)

**Physics Implementation:**
- Slope-based acceleration/deceleration
- Air control for mid-air directional adjustments
- Friction zones based on terrain
- Bunny hop support for momentum chaining
- Coyote time integration for smooth ground detection

**Components:**
- `SlopeMovement` for skiing physics calculation
- Modified `bl_FirstPersonController` with skiing integration
- `PlayerState.Skiing` enum value (28) added to MFPS

#### ✨ Added - Jetpack System (Optional)

**Core Features:**
- Fuel-based jetpack system (100 fuel default)
- Configurable thrust force and max vertical speed
- Automatic fuel drain during use (20/s default)
- Delayed fuel refill after cooldown (2s delay, 15/s refill)
- Speed limiting to prevent physics breakage
- Optional UI fuel bar integration

**Components:**
- `SlopeJetpack` component with complete fuel management
- UI Image support for fuel bar visualization

#### 📖 Added - Documentation

**Setup Guides:**
- Complete parkour setup guide with animator configuration
- Complete skiing setup guide with physics tuning
- Prefab creation guide for ClimbPoint and Player prefabs
- API reference documentation for developers

**Configuration Presets:**
- Casual mode settings (easy to learn)
- Competitive mode settings (skill-based)
- Hardcore mode settings (maximum skill ceiling)

#### 🔧 Added - Integration Features

**MFPS Integration:**
- Seamless weapon hiding during parkour actions
- Camera switching for third-person parkour view
- Player state synchronization with MFPS systems
- Photon networking compatibility
- Input system integration (PC and mobile)
- UI state icon updates (stand/crouch indicators)

**Character Controller Modifications:**
- Modified `bl_FirstPersonController` with skiing delegation
- Jump input differentiation (tap vs. hold for jetpack)
- Skiing state detection and transitions
- Coyote time implementation for smooth ground detection
- Maintained backward compatibility with all MFPS features

#### 🎨 Added - Visual & Audio

**Debug Visualization:**
- Gizmos for parkour detection rays (editor only)
- ClimbPoint connection visualization
- Speed and state debugging displays
- Velocity direction indicators

**Animation Support:**
- Root motion toggle for parkour actions
- Animator parameter mapping for all states
- Match target positioning for precise animation
- Smooth transitions between states

#### 🐛 Fixed

**Initial Bug Fixes:**
- Landing animation no longer triggers during high-speed skiing
- Coyote time prevents unexpected falls at slope edges
- Weapon visibility correctly managed during parkour
- Character controller collision disabled during parkour animations
- Ground detection works correctly with skiing momentum
- Jump input no longer interferes with jetpack activation
- Stamina recovery starts correctly after hanging
- Wall run doesn't activate on shallow angles

#### 🔒 Security

**Anti-Cheat Considerations:**
- Speed limiting enforced server-side (multiplayer ready)
- Physics calculations use Time.deltaTime for consistency
- No client-side teleportation vulnerabilities
- Action validation prevents impossible parkour sequences

#### ⚡ Performance

**Optimizations:**
- Raycast frequency controlled (configurable per-frame check)
- Gizmos only render in editor builds
- Object pooling support for temporary ClimbPoints
- Efficient animator parameter updates
- Minimal garbage allocation in physics calculations

---

## [Unreleased]

### Planned for v1.1

#### 🔮 Future Features
- Slide-to-ski transition system
- Advanced wall climbing with vertical movement
- Grappling hook integration for extended mobility
- Double jump mechanics
- Slope-specific sound effects (ice, snow, grass)
- Speed trail particle effects
- Additional parkour actions (dive roll, vault slide)

#### 📱 Platform Support
- Mobile input optimization and touch controls
- Console controller support (PlayStation, Xbox)
- VR compatibility mode exploration

#### 🎨 Content Additions
- Complete animation pack with motion capture
- Visual effects library (speed trails, dust clouds)
- Sound effect pack (footsteps, impacts, wind)
- Example maps and tutorials

#### 🔧 Developer Tools
- Map editor tools for ClimbPoint placement
- Built-in tutorial system for players
- Mod API for custom movement extensions
- Performance profiling tools

---

## Version History

### Version Numbering

This project uses [Semantic Versioning](https://semver.org/):
- **MAJOR** version for incompatible API changes
- **MINOR** version for new functionality in a backward compatible manner
- **PATCH** version for backward compatible bug fixes

### Support Policy

- **Current Version (1.0.x):** Full support, active development
- **Previous Minor (0.x.x):** Security fixes only
- **Older Versions:** No longer supported

---

## Upgrade Guide

### From Future Versions

When upgrading to a new version:

1. **Backup Your Project** before upgrading
2. Read the changelog for breaking changes
3. Update scripts in the recommended order
4. Test in a separate scene first
5. Re-configure any modified settings
6. Test multiplayer compatibility if applicable

---

## Contributing

We welcome contributions! See [CONTRIBUTING.md](CONTRIBUTING.md) for details.

### How to Report Bugs

When reporting bugs, include:
- Unity version
- MFPS version
- System version number
- Steps to reproduce
- Expected vs actual behavior
- Console logs if applicable

### How to Request Features

When requesting features:
- Describe the feature clearly
- Explain use cases
- Provide examples or references
- Indicate priority (nice to have vs. essential)

---

## Credits

### Development Team
- Core parkour system design and implementation
- Tribes-style skiing physics implementation
- MFPS integration layer development
- Documentation and example creation

### Inspired By
- **Tribes: Ascend** - Skiing physics and momentum mechanics
- **Mirror's Edge** - Parkour fluidity and animation flow
- **Titanfall 2** - Movement chaining and wall running
- **Apex Legends** - Modern momentum-based movement

### Special Thanks
- Lovatto Studio for MFPS framework
- Unity Technologies for the engine
- Exit Games for Photon networking
- The MFPS community for feedback and testing

---

## License

This project is licensed under the MIT License - see [LICENSE.md](LICENSE.md) for details.

---

*For questions or support, visit the [GitHub repository](https://github.com/your-repo) or join our [Discord community](https://discord.gg/your-server).*

**Current Version:** 1.0.0  
**Release Date:** January 8, 2025  
**Compatibility:** MFPS 1.9+, Unity 2020.3+