# API Reference - MFPS Advanced Movement System

**Version 1.0.0**

Complete API documentation for developers extending or integrating with the Advanced Movement System.

---

## Table of Contents

- [Parkour System](#parkour-system)
  - [IParkourCharacter Interface](#iparkourcharacter-interface)
  - [ParkourController](#parkourcontroller)
  - [ClimbController](#climbcontroller)
  - [MFPS_IntegrationHelper](#mfps_integrationhelper)
- [Skiing System](#skiing-system)
  - [SlopeMovement](#slopemovement)
  - [SlopeJetpack](#slopejetpack)
- [Modified Controllers](#modified-controllers)
  - [bl_FirstPersonController](#bl_firstpersoncontroller)
- [Usage Examples](#usage-examples)
- [Events & Callbacks](#events--callbacks)
- [Extension Guide](#extension-guide)

---

## Parkour System

### IParkourCharacter Interface

**Namespace:** `FC_ParkourSystem`

Interface that character controllers must implement to use the parkour system. Allows the system to work with any character controller.

#### Properties

```csharp
bool UseRootMotion { get; set; }
```
Should the animator use root motion during parkour actions?

```csharp
Vector3 MoveDir { get; }
```
Current movement direction input (normalized).

```csharp
bool IsGrounded { get; }
```
Is the character currently grounded?

```csharp
float Gravity { get; }
```
Gravity value affecting the character.

```csharp
Animator Animator { get; set; }
```
Reference to the character's animator.

```csharp
bool PreventParkourAction { get; }
```
Should parkour actions be prevented? (e.g., during combat, reloading, etc.)

#### Methods

```csharp
void OnStartParkourAction()
```
Called when a parkour action starts. Should disable normal character control and prepare for animation-driven movement.

```csharp
void OnEndParkourAction()
```
Called when a parkour action ends. Should re-enable normal character control.

```csharp
IEnumerator HandleVerticalJump()
```
Handle vertical jump (optional implementation).

#### Implementation Example

```csharp
public class CustomCharacter : MonoBehaviour, IParkourCharacter
{
    public bool UseRootMotion { get; set; }
    public Vector3 MoveDir => GetInputDirection();
    public bool IsGrounded => characterController.isGrounded;
    public float Gravity => Physics.gravity.y;
    public Animator Animator { get; set; }
    public bool PreventParkourAction => isReloading || isDead;

    public void OnStartParkourAction()
    {
        DisableMovement();
        Animator.applyRootMotion = true;
    }

    public void OnEndParkourAction()
    {
        EnableMovement();
        Animator.applyRootMotion = false;
    }

    public IEnumerator HandleVerticalJump()
    {
        Jump();
        yield break;
    }
}
```

---

### ParkourController

**Namespace:** `FC_ParkourSystem`

Controls parkour actions like vaulting, climbing, stepping up, and wall running.

#### Public Properties

```csharp
bool ControlledByParkour { get; }
```
Returns true if player is currently performing a parkour action.

```csharp
bool InAction { get; }
```
Returns true if a parkour action is in progress.

#### Public Methods

```csharp
bool CheckForParkour()
```
Check if any parkour action is possible and execute it.
- **Returns:** `true` if parkour action was initiated
- **Usage:** Call in Update() when jump button is pressed

```csharp
void VerticalJump()
```
Perform a vertical jump (no parkour action).
- **Conditions:** Player must be grounded

```csharp
void CancelParkourAction()
```
Cancel the current parkour action and return control to player.
- **Usage:** Emergency stop or forced cancellation

```csharp
ParkourAction GetCurrentAction()
```
Get the currently executing parkour action.
- **Returns:** Current `ParkourAction` enum value

#### Configuration Fields

```csharp
[SerializeField] private LayerMask obstacleLayer;
```
Layers that can be vaulted/climbed.

```csharp
[SerializeField] private float forwardRayLength = 1.5f;
```
Distance to check for obstacles ahead.

```csharp
[SerializeField] private float vaultHeight = 1.0f;
```
Maximum height that can be vaulted over.

```csharp
[SerializeField] private float minVaultSpeed = 3f;
```
Minimum speed required to vault.

```csharp
[SerializeField] private float minWallRunSpeed = 5f;
```
Minimum speed required to wall run.

#### ParkourAction Enum

```csharp
public enum ParkourAction
{
    None,           // No action
    StepUp,         // Small obstacle (0.6m)
    MediumStepUp,   // Medium obstacle (1.2m)
    VaultOver,      // Vault over obstacle (1m)
    VaultOn,        // Vault onto obstacle
    ClimbUp,        // Climb tall obstacle (2m)
    WallRun,        // Run on wall
    VerticalJump    // Simple jump
}
```

#### Usage Example

```csharp
ParkourController parkour = GetComponent<ParkourController>();

void Update()
{
    if (Input.GetKeyDown(KeyCode.Space))
    {
        if (!parkour.CheckForParkour())
        {
            // No parkour action possible, do normal jump
            parkour.VerticalJump();
        }
    }

    // Check current state
    if (parkour.InAction)
    {
        ParkourAction action = parkour.GetCurrentAction();
        Debug.Log($"Performing: {action}");
    }
}
```

---

### ClimbController

**Namespace:** `FC_ParkourSystem`

Controls climbing mechanics - hanging, ledge climbing, and wall climbing.

#### Public Properties

```csharp
bool IsHanging { get; }
```
Is the player currently hanging from a ledge?

```csharp
bool IsClimbing { get; }
```
Is the player currently climbing a wall?

```csharp
float CurrentStamina { get; }
```
Current stamina value (0 to MaxStamina).

```csharp
float MaxStamina { get; }
```
Maximum stamina value.

```csharp
EnvironmentScanner envScanner { get; }
```
Reference to the environment scanner for ledge detection.

#### Public Methods

```csharp
void StartHanging(ClimbPoint climbPoint)
```
Start hanging from a ledge.
- **Parameters:** `climbPoint` - The climb point to hang from

```csharp
void StopHanging()
```
Stop hanging and fall/land.

```csharp
void StartClimbing(Vector3 climbPosition)
```
Start climbing a wall.
- **Parameters:** `climbPosition` - Position to start climbing from

```csharp
void StopClimbing()
```
Stop climbing and fall.

```csharp
WallCheckData? CheckWall(ClimbPoint point)
```
Check if there's a wall behind the player (for braced hang vs free hang).
- **Returns:** Wall check data or null if no wall

```csharp
ClimbPoint GetNearestPoint(Transform ledge, Vector3 position, bool checkConnected = true, bool obstacleCheck = true)
```
Get nearest climb point on a ledge.
- **Returns:** Nearest ClimbPoint or creates temporary one

#### Configuration Fields

```csharp
[SerializeField] private float climbSpeed = 2f;
```
Vertical climbing speed.

```csharp
[SerializeField] private float horizontalClimbSpeed = 1.5f;
```
Horizontal climbing speed along ledges.

```csharp
[SerializeField] private float hangStamina = 10f;
```
Maximum hang time in seconds.

```csharp
[SerializeField] private float staminaDrainRate = 1f;
```
Stamina consumed per second while hanging.

#### Usage Example

```csharp
ClimbController climb = GetComponent<ClimbController>();

void Update()
{
    // Check for drop to hang
    if (Input.GetKeyDown(KeyCode.E))
    {
        if (climb.envScanner.DropLedgeCheck(transform.forward, out ClimbLedgeData data))
        {
            ClimbPoint point = climb.GetNearestPoint(data.ledgeHit.transform, data.hangPoint);
            climb.StartHanging(point);
        }
    }

    // Monitor stamina while hanging
    if (climb.IsHanging)
    {
        float staminaPercent = climb.CurrentStamina / climb.MaxStamina;
        UpdateStaminaUI(staminaPercent);
        
        if (staminaPercent <= 0)
        {
            Debug.Log("Out of stamina!");
        }
    }
}
```

---

### MFPS_IntegrationHelper

**Namespace:** `FC_ParkourSystem`

Integration layer between the parkour/climbing systems and MFPS.

#### Public Methods

```csharp
void ForceEndParkourAction()
```
Force end any active parkour action (for debugging or emergency stops).

```csharp
bool IsParkourActive()
```
Check if currently performing a parkour action.
- **Returns:** `true` if parkour is active

```csharp
bool IsClimbing()
```
Check if currently in climbing state.
- **Returns:** `true` if climbing

#### IParkourCharacter Implementation

Implements all IParkourCharacter interface methods for MFPS integration:
- `OnStartParkourAction()` - Disables FPC, enables root motion, hides weapons
- `OnEndParkourAction()` - Re-enables FPC, disables root motion, shows weapons
- `HandleVerticalJump()` - Delegates to MFPS jump system

#### Usage Example

```csharp
MFPS_IntegrationHelper helper = GetComponent<MFPS_IntegrationHelper>();

// Check state
if (helper.IsParkourActive())
{
    Debug.Log("Player is performing parkour");
}

// Force stop (emergency)
if (playerDied)
{
    helper.ForceEndParkourAction();
}
```

---

## Skiing System

### SlopeMovement

**Namespace:** Global

Controls Tribes-style skiing physics with momentum-based movement.

#### Public Properties

```csharp
bool IsSkiing { get; private set; }
```
Is the player currently skiing?

```csharp
float CurrentSpeed { get; private set; }
```
Current horizontal speed in m/s.

#### Public Methods

```csharp
Vector3 CalculateMovement(Vector2 input, float baseSpeed, bool jumpPressed, bool jumpHeld)
```
Calculate and return movement vector based on skiing physics.
- **Parameters:**
  - `input` - Movement input (x = horizontal, y = vertical)
  - `baseSpeed` - Base movement speed
  - `jumpPressed` - Was jump just pressed this frame?
  - `jumpHeld` - Is jump being held down?
- **Returns:** Movement velocity vector to apply to CharacterController

#### Configuration Fields

```csharp
[SerializeField] public float minSpeedToSki = 8f;
```
Minimum speed required to enter skiing mode.

```csharp
[SerializeField] public float maxSpeed = 35f;
```
Absolute maximum horizontal speed (0 = unlimited).

```csharp
[SerializeField] public float gravity = 18f;
```
Gravity force applied when airborne.

```csharp
[SerializeField] public float skiFriction = 0.3f;
```
Base friction applied while skiing.

```csharp
[SerializeField] public float downhillSpeedGain = 0.6f;
```
Speed multiplier when going downhill (0-2).

```csharp
[SerializeField] public float uphillMomentumRetention = 0.75f;
```
How much momentum is retained when going uphill (0-1).

```csharp
[SerializeField] public float turningFriction = 1.5f;
```
Extra friction applied when turning while skiing (0-5).

#### Usage Example

```csharp
SlopeMovement slope = GetComponent<SlopeMovement>();

void FixedUpdate()
{
    Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    bool jumpPressed = Input.GetButtonDown("Jump");
    bool jumpHeld = Input.GetButton("Jump");
    
    Vector3 movement = slope.CalculateMovement(input, walkSpeed, jumpPressed, jumpHeld);
    characterController.Move(movement * Time.deltaTime);
    
    // Check skiing state
    if (slope.IsSkiing)
    {
        Debug.Log($"Skiing at {slope.CurrentSpeed:F1} m/s");
    }
}
```

---

### SlopeJetpack

**Namespace:** Global

Optional jetpack system with fuel management.

#### Public Properties

```csharp
bool IsActive { get; }
```
Is the jetpack currently active?

#### Public Methods

```csharp
Vector3 CalculateJetpackForce()
```
Calculate the vertical jetpack force to apply.
- **Returns:** Upward force vector
- **Note:** Call this in your movement calculation when jetpack is active

#### Configuration Fields

```csharp
[SerializeField] public float jetpackForce = 25f;
```
Upward acceleration force applied by jetpack.

```csharp
[SerializeField] public float maxVerticalSpeed = 12f;
```
Maximum allowed upward vertical speed.

```csharp
[SerializeField] public KeyCode jetpackKey = KeyCode.Space;
```
Key used to activate the jetpack.

```csharp
[SerializeField] public float maxFuel = 100f;
```
Maximum jetpack fuel.

```csharp
[SerializeField] public float fuelConsumeRate = 20f;
```
Fuel consumed per second while using jetpack.

```csharp
[SerializeField] public float fuelRefillRate = 15f;
```
Fuel refilled per second after delay.

```csharp
[SerializeField] public float fuelRefillDelay = 2f;
```
Delay before fuel starts refilling after last use.

```csharp
[SerializeField] public Image fuelBar;
```
UI image used as a fuel bar (optional).

#### Usage Example

```csharp
SlopeJetpack jetpack = GetComponent<SlopeJetpack>();

void FixedUpdate()
{
    if (!characterController.isGrounded && jetpack.IsActive)
    {
        Vector3 jetpackForce = jetpack.CalculateJetpackForce();
        velocity += jetpackForce * Time.deltaTime;
    }
}

void OnGUI()
{
    if (jetpack.IsActive)
    {
        GUILayout.Label("JETPACK ACTIVE");
    }
}
```

---

## Modified Controllers

### bl_FirstPersonController

Modified MFPS first-person controller with skiing integration.

#### Key Changes

**Skiing Integration:**
```csharp
private SlopeMovement SlopeMovement;
private SlopeJetpack SlopeJetpack;
```
References to skiing components, detected on Awake().

**Jump Input Handling:**
```csharp
private bool jumpPressed; // Single press for jump
private bool jumpHeld;    // Continuous hold for jetpack
```
Differentiates between tap (jump) and hold (jetpack).

**Movement Delegation:**
```csharp
if (SlopeMovement != null)
{
    targetDirection = SlopeMovement.CalculateMovement(m_Input, speed, jumpPressed, isHoldingJump);
}
else
{
    // Normal MFPS movement
}
```

**Skiing State:**
```csharp
public enum PlayerState
{
    // ... existing states ...
    Skiing = 28  // NEW
}
```

#### Usage Notes

- Automatically detects `SlopeMovement` component
- If found, delegates all movement to skiing system
- Maintains compatibility with all MFPS features
- Seamless transitions between skiing and normal movement

---

## Usage Examples

### Example 1: Custom Parkour Action

Add a new parkour action (e.g., slide under):

```csharp
// 1. Add to ParkourAction enum
public enum ParkourAction
{
    // ... existing actions ...
    SlideUnder
}

// 2. Add detection method in ParkourController
private bool DetectSlideUnder(out RaycastHit hit)
{
    Vector3 origin = transform.position + Vector3.up * 0.5f;
    return Physics.Raycast(origin, transform.forward, out hit, 2f, obstacleLayer);
}

// 3. Add execution method
private void PerformSlideUnder(Vector3 targetPos)
{
    currentAction = ParkourAction.SlideUnder;
    targetPosition = targetPos + transform.forward * 2f;
    StartCoroutine(ExecuteParkourAction("SlideUnder", 0.2f, 0.8f));
}

// 4. Call in CheckForParkour()
if (DetectSlideUnder(out RaycastHit slideHit))
{
    PerformSlideUnder(slideHit.point);
    return true;
}
```

### Example 2: Custom Skiing Physics

Modify skiing behavior with custom terrain:

```csharp
public class CustomSlopeMovement : SlopeMovement
{
    [Header("Terrain Types")]
    public float iceFriction = 0.1f;
    public float snowFriction = 0.3f;
    public float grassFriction = 2.0f;

    private string currentTerrainType = "snow";

    void Update()
    {
        DetectTerrain();
        AdjustFriction();
    }

    void DetectTerrain()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            if (hit.collider.CompareTag("Ice")) currentTerrainType = "ice";
            else if (hit.collider.CompareTag("Grass")) currentTerrainType = "grass";
            else currentTerrainType = "snow";
        }
    }

    void AdjustFriction()
    {
        switch (currentTerrainType)
        {
            case "ice": skiFriction = iceFriction; break;
            case "grass": skiFriction = grassFriction; break;
            default: skiFriction = snowFriction; break;
        }
    }
}
```

### Example 3: Jetpack Boost Ability

Add a temporary jetpack boost:

```csharp
public class JetpackBooster : MonoBehaviour
{
    private SlopeJetpack jetpack;
    private float originalForce;
    private bool boosting = false;

    void Start()
    {
        jetpack = GetComponent<SlopeJetpack>();
        originalForce = jetpack.jetpackForce;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !boosting)
        {
            StartCoroutine(BoostJetpack());
        }
    }

    IEnumerator BoostJetpack()
    {
        boosting = true;
        jetpack.jetpackForce *= 2f; // Double thrust
        jetpack.maxFuel *= 1.5f;    // Extra fuel

        yield return new WaitForSeconds(5f);

        jetpack.jetpackForce = originalForce;
        jetpack.maxFuel /= 1.5f;
        boosting = false;
    }
}
```

### Example 4: Speed-Based Effects

Add visual effects based on speed:

```csharp
public class SpeedEffects : MonoBehaviour
{
    private SlopeMovement slope;
    
    [Header("Effects")]
    public ParticleSystem speedTrail;
    public AudioSource windSound;
    public PostProcessVolume motionBlur;

    [Header("Thresholds")]
    public float trailThreshold = 20f;
    public float soundThreshold = 15f;
    public float blurThreshold = 25f;

    void Start()
    {
        slope = GetComponent<SlopeMovement>();
    }

    void Update()
    {
        float speed = slope.CurrentSpeed;

        // Speed trail
        if (speed > trailThreshold)
        {
            if (!speedTrail.isPlaying) speedTrail.Play();
        }
        else if (speedTrail.isPlaying) speedTrail.Stop();

        // Wind sound
        windSound.volume = Mathf.Clamp01((speed - soundThreshold) / 20f);

        // Motion blur intensity
        float blurAmount = Mathf.Clamp01((speed - blurThreshold) / 15f);
        motionBlur.weight = blurAmount;
    }
}
```

---

## Events & Callbacks

### Custom Events

You can subscribe to movement events by extending the integration helper:

```csharp
public class MovementEvents : MonoBehaviour
{
    public delegate void OnParkourAction(ParkourAction action);
    public static event OnParkourAction OnParkourStart;
    public static event OnParkourAction OnParkourEnd;

    public delegate void OnSkiingStateChange(bool isSkiing, float speed);
    public static event OnSkiingStateChange OnSkiingChanged;

    // Call these in MFPS_IntegrationHelper
    public static void RaiseParkourStart(ParkourAction action)
    {
        OnParkourStart?.Invoke(action);
    }

    public static void RaiseParkourEnd(ParkourAction action)
    {
        OnParkourEnd?.Invoke(action);
    }

    public static void RaiseSkiingChanged(bool skiing, float speed)
    {
        OnSkiingChanged?.Invoke(skiing, speed);
    }
}

// Usage
void OnEnable()
{
    MovementEvents.OnParkourStart += HandleParkourStart;
    MovementEvents.OnSkiingChanged += HandleSkiingChanged;
}

void HandleParkourStart(ParkourAction action)
{
    Debug.Log($"Started: {action}");
}

void HandleSkiingChanged(bool skiing, float speed)
{
    if (skiing) Debug.Log($"Skiing at {speed} m/s");
}
```

---

## Extension Guide

### Creating Custom Character Integration

To use these systems with a non-MFPS character controller:

1. **Implement IParkourCharacter:**
```csharp
public class MyCharacter : MonoBehaviour, IParkourCharacter
{
    // Implement all interface members
    // (See IParkourCharacter section above)
}
```

2. **Attach Components:**
```csharp
gameObject.AddComponent<ParkourController>();
gameObject.AddComponent<ClimbController>();
// Don't add MFPS_IntegrationHelper - create your own
```

3. **Create Custom Integration Helper:**
```csharp
public class MyCharacterIntegration : MonoBehaviour, IParkourCharacter
{
    private ParkourController parkour;
    private ClimbController climb;
    private MyCharacterController controller;

    void Awake()
    {
        parkour = GetComponent<ParkourController>();
        climb = GetComponent<ClimbController>();
        controller = GetComponent<MyCharacterController>();
    }

    public void OnStartParkourAction()
    {
        controller.enabled = false;
        // Your custom logic
    }

    public void OnEndParkourAction()
    {
        controller.enabled = true;
        // Your custom logic
    }

    // Implement other interface members
}
```

### Thread Safety

**Important:** These systems are designed for single-threaded Unity usage. Do not call movement calculation methods from background threads.

```csharp
// ❌ WRONG - Don't do this
Thread thread = new Thread(() => {
    slope.CalculateMovement(input, speed, false, false); // UNSAFE!
});

// ✅ CORRECT - Always use in main Unity thread
void FixedUpdate()
{
    slope.CalculateMovement(input, speed, false, false); // SAFE
}
```

---

## Performance Considerations

### Optimization Tips

**1. Reduce Raycast Frequency:**
```csharp
private int frameCounter = 0;

void Update()
{
    frameCounter++;
    if (frameCounter % 3 == 0) // Check every 3 frames
    {
        parkour.CheckForParkour();
    }
}
```

**2. Object Pooling for ClimbPoints:**
```csharp
public class ClimbPointPool
{
    private Queue<ClimbPoint> pool = new Queue<ClimbPoint>();
    
    public ClimbPoint Get()
    {
        return pool.Count > 0 ? pool.Dequeue() : CreateNew();
    }
    
    public void Return(ClimbPoint point)
    {
        pool.Enqueue(point);
    }
}
```

**3. Disable Gizmos in Builds:**
```csharp
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Gizmo code only runs in editor
    }
#endif
```

---

## Debugging Tools

### Debug Visualization

Enable visual debugging in your scripts:

```csharp
public class MovementDebugger : MonoBehaviour
{
    private SlopeMovement slope;
    private ParkourController parkour;

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"Skiing: {slope.IsSkiing}");
        GUILayout.Label($"Speed: {slope.CurrentSpeed:F2} m/s");
        GUILayout.Label($"Parkour Active: {parkour.InAction}");
        GUILayout.Label($"Current Action: {parkour.GetCurrentAction()}");
        GUILayout.EndArea();
    }
}
```

---

## Version History

### v1.0.0
- Initial API release
- Complete parkour system
- Tribes-style skiing
- Jetpack integration
- MFPS compatibility layer

---

## Support

For API questions or integration help:
- **Documentation:** [Setup Guides](../Documentation/)
- **Community:** Discord / Forums
- **Issues:** GitHub Issues

---

*Last Updated: January 2025 | API Version 1.0.0*