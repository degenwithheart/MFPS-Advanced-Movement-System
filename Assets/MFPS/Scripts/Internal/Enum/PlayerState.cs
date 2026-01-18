public enum PlayerState : byte
{
    Idle = 0,
    Walking = 1,
    Running = 2,
    Crouching = 3,
    Jumping = 4,
    Climbing = 5,
    Sliding = 6,
    Dropping = 7,
    Gliding = 8,
    InVehicle = 9,
    Stealth = 10,
    Skiing = 11,
    VaultOver = 12,
    VaultOn = 13,
    MediumStepUp = 14,
    StepUp = 15,
    ClimbUp = 16,
    MediumStepUpM = 17,
    LandFromFall = 18,
    LandAndStepForward = 19,
    LandOnSpot = 20,
    FallingToRoll = 21,
    FreeHangClimb = 22,
    BracedHangClimb = 23,
    JumpDown = 24,
    JumpFromHang = 25,
    JumpFromFreeHang = 26,
    BracedHangTryJumpUp = 27,
    WallRun = 28
}

public enum PlayerFPState : byte
{
    Idle = 0,
    Firing = 1,
    Reloading = 2,
    Aiming = 3,
    Running = 4,
    FireAiming = 9,
}

public enum PlayerRunToAimBehave
{
    BlockAim = 0,
    StopRunning = 1,
    AimWhileRunning = 2,
}

public enum PlayerAnimationCommands
{
    PlayDoubleJump,
    OnFlashbang,
    OnUnFlashed,
    QuickMelee,
    QuickGrenade,
}