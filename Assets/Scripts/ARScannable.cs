using UnityEngine;

/// <summary>
/// ARScannable
/// This script marks an object as "scannable" by the player's scanner.
/// It stores all the information that will be revealed during scanning.
/// </summary>
public class ARScannable : MonoBehaviour
{
    // =========================
    // BASIC INFO (what the player sees)
    // =========================

    [Header("Scan Information")]

    [Tooltip("Name shown on the scanner UI")]
    public string displayName = "Unknown Object";

    [Tooltip("Description shown after scan completes")]
    [TextArea]
    public string description = "No data available.";

    // =========================
    // VISUAL ELEMENTS (optional)
    // =========================

    [Header("Visual Feedback")]

    [Tooltip("Prefab that appears above the object when scanned (e.g. hologram)")]
    public GameObject overlayPrefab;

    [Tooltip("Optional position for the overlay (if null, uses object position)")]
    public Transform overlayAnchor;

    [Tooltip("Hidden object that becomes visible during scan (e.g. X-ray mesh)")]
    public GameObject hiddenXRayObject;

    // =========================
    // SCAN SETTINGS
    // =========================

    [Header("Scan Settings")]

    [Tooltip("How long the player must scan this object (seconds)")]
    public float requiredScanTime = 1.5f;

    // =========================
    // INTERNAL STATE (not visible in Inspector)
    // =========================

    [HideInInspector]
    public GameObject spawnedOverlayInstance;

    // =========================
    // FUNCTIONS CALLED BY SCANNER
    // =========================

    /// <summary>
    /// Called when scan completes successfully.
    /// Shows any hidden visuals.
    /// </summary>
    public void ShowScanVisuals()
    {
        // Turn on hidden "X-ray" object if assigned
        if (hiddenXRayObject != null)
        {
            hiddenXRayObject.SetActive(true);
        }

        // Spawn hologram overlay if assigned and not already spawned
        if (overlayPrefab != null && spawnedOverlayInstance == null)
        {
            // Use anchor if available, otherwise use object position
            Transform anchor = overlayAnchor != null ? overlayAnchor : transform;

            spawnedOverlayInstance = Instantiate(
                overlayPrefab,
                anchor.position,
                anchor.rotation,
                anchor
            );
        }
    }

    /// <summary>
    /// Called when scanning stops or resets.
    /// Cleans up visuals.
    /// </summary>
    public void HideScanVisuals()
    {
        // Turn off X-ray object
        if (hiddenXRayObject != null)
        {
            hiddenXRayObject.SetActive(false);
        }

        // Destroy overlay instance
        if (spawnedOverlayInstance != null)
        {
            Destroy(spawnedOverlayInstance);
        }
    }
}