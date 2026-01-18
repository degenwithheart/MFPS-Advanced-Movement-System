using UnityEngine;

/// <summary>
/// Minimal compatibility shim for missing camera switcher component.
/// Some addons call GetComponent<bl_PlayerCameraSwitcher>() and enable/disable it.
/// Provide an empty MonoBehaviour so those calls compile and enabling/disabling works at runtime.
/// If a fuller implementation is needed, replace this with the original class.
/// </summary>
public class bl_PlayerCameraSwitcher : MonoBehaviour
{
    // Intentionally minimal: presence and enabled state is sufficient for compile and basic use.
}
