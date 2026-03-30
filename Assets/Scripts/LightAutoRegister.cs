using UnityEngine;

/// <summary>
/// LIGHT AUTO REGISTER
/// -------------------
/// This script automatically registers a Light component
/// with the LightingManager at runtime.
///
/// WHY THIS EXISTS:
/// Instead of manually assigning hundreds of lights,
/// each light "announces itself" to the manager.
///
/// WHERE TO USE:
/// Attach this to any GameObject that has a Light component.
/// (Best placed on light prefabs so it propagates automatically)
///
/// EXECUTION TIMING:
/// Uses Awake() instead of Start() to ensure registration
/// happens BEFORE the LightingManager initialises its state.
///
/// TEACHING POINT:
/// This demonstrates a "self-registering system",
/// which is common in scalable game architecture.
/// </summary>

[RequireComponent(typeof(Light))] // Enforces that a Light component must exist
public class LightAutoRegister : MonoBehaviour
{
    // Cached reference to avoid repeated GetComponent calls
    private Light cachedLight;

    // Prevent accidental double registration
    private bool hasRegistered = false;

    // =========================================================
    // 🔹 AWAKE (runs VERY early in Unity lifecycle)
    // =========================================================
    private void Awake()
    {
        // Cache the Light component immediately
        cachedLight = GetComponent<Light>();

        if (cachedLight == null)
        {
            Debug.LogWarning($"[LightAutoRegister] No Light found on {gameObject.name}");
            return;
        }

        TryRegister();
    }

    // =========================================================
    // 🔹 SAFETY FALLBACK (in case manager isn't ready yet)
    // =========================================================
    private void Start()
    {
        // If registration failed in Awake (e.g. manager not yet present),
        // try again here as a fallback.
        if (!hasRegistered)
        {
            TryRegister();
        }
    }

    // =========================================================
    // 🔹 REGISTRATION LOGIC
    // =========================================================
    private void TryRegister()
    {
        // Ensure we only register once
        if (hasRegistered) return;

        // Check if LightingManager exists
        if (LightingManager.Instance != null)
        {
            LightingManager.Instance.RegisterLight(cachedLight);
            hasRegistered = true;

            // Optional debug (comment out once stable)
            // Debug.Log($"[LightAutoRegister] Registered: {gameObject.name}");
        }
        else
        {
            // Manager not ready yet — this can happen depending on script order
            Debug.LogWarning($"[LightAutoRegister] LightingManager not found yet for {gameObject.name}");
        }
    }

    // =========================================================
    // 🔹 OPTIONAL: HANDLE OBJECT ENABLE/DISABLE
    // =========================================================
    private void OnEnable()
    {
        // If object is re-enabled (e.g. pooled or toggled),
        // ensure it's still registered
        if (!hasRegistered)
        {
            TryRegister();
        }
    }

    // =========================================================
    // 🔹 OPTIONAL: CLEANUP (ADVANCED / NOT REQUIRED YET)
    // =========================================================
    private void OnDestroy()
    {
        // You could unregister here if needed:
        // LightingManager.Instance?.UnregisterLight(cachedLight);
        // Not required unless you dynamically destroy lights
    }
}