using UnityEngine;
using UnityEngine.UI;
using MFPS.InputManager;

#if ADVANCED_MOVEMENT_SLOPE

/// <summary>
/// Jetpack system for slope/skiing mode
/// Uses unified MFPS input system via bl_GameInput
/// Requires ADVANCED_MOVEMENT_SLOPE define to be enabled
/// </summary>
[RequireComponent(typeof(bl_FirstPersonController))]
public class SlopeJetpack : MonoBehaviour
{
    [Header("Jetpack Settings")]
    [Tooltip("Upward acceleration force applied by the jetpack.")]
    public float jetpackForce = 25f;

    [Tooltip("Maximum allowed upward vertical speed.")]
    public float maxVerticalSpeed = 12f;

    [Header("Fuel Settings")]
    [Tooltip("Maximum jetpack fuel.")]
    public float maxFuel = 100f;

    [Tooltip("Fuel consumed per second while using the jetpack.")]
    public float fuelConsumeRate = 20f;

    [Tooltip("Fuel refilled per second after delay.")]
    public float fuelRefillRate = 15f;

    [Tooltip("Delay before fuel starts refilling after last use.")]
    public float fuelRefillDelay = 2f;

    [Header("UI (Optional)")]
    [Tooltip("UI image used as a fuel bar.")]
    public Image fuelBar;

    private float currentFuel;
    public float CurrentFuel => currentFuel;
    private bool isUsingJetpack;
    private float lastUseTime;

    private bl_FirstPersonController controller;

    void Start()
    {
        currentFuel = maxFuel;
        controller = GetComponent<bl_FirstPersonController>();
    }

    void Update()
    {
        HandleInput();
        HandleFuel();
        UpdateUI();
    }

    /// <summary>
    /// Handle input using unified MFPS input system
    /// </summary>
    private void HandleInput()
    {
        // Jetpack activates while jump key is held and fuel is available
        // Uses bl_GameInput for platform-consistent input
        if (bl_GameInput.Jump() && currentFuel > 0f)
        {
            isUsingJetpack = true;
            lastUseTime = Time.time;
        }
        else
        {
            isUsingJetpack = false;
        }
    }

    /// <summary>
    /// Calculates the vertical jetpack force.
    /// Called externally by SlopeMovement.
    /// </summary>
    public Vector3 CalculateJetpackForce()
    {
        if (!isUsingJetpack || currentFuel <= 0f)
            return Vector3.zero;

        // Consume fuel
        currentFuel -= fuelConsumeRate * Time.deltaTime;
        currentFuel = Mathf.Max(0f, currentFuel);

        float verticalSpeed = controller.Velocity.y;

        // Prevent exceeding max vertical speed
        if (verticalSpeed >= maxVerticalSpeed)
            return Vector3.zero;

        // Reduce force as we approach max vertical speed
        float speedRatio = Mathf.Clamp01(verticalSpeed / maxVerticalSpeed);
        float adjustedForce = jetpackForce * (1f - speedRatio * 0.5f);

        return Vector3.up * adjustedForce;
    }

    /// <summary>
    /// Handle fuel refill
    /// </summary>
    private void HandleFuel()
    {
        // Refill fuel after delay when not using the jetpack
        if (!isUsingJetpack && Time.time > lastUseTime + fuelRefillDelay)
        {
            currentFuel += fuelRefillRate * Time.deltaTime;
            currentFuel = Mathf.Min(currentFuel, maxFuel);
        }
    }

    /// <summary>
    /// Update fuel bar UI
    /// </summary>
    private void UpdateUI()
    {
        if (fuelBar != null)
        {
            fuelBar.fillAmount = currentFuel / maxFuel;
        }
    }

    /// <summary>
    /// Indicates whether the jetpack is currently active.
    /// </summary>
    public bool IsActive => isUsingJetpack;
}

#endif
