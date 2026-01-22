using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// EDITOR XR INTERACTION SIMULATOR (UNITY 6 SAFE)
/// ============================================
///
/// PURPOSE
/// -------
/// Allows XR Interactables to be tested in the Unity Editor
/// WITHOUT a VR headset, using keyboard + mouse.
///
/// FEATURES
/// --------
/// • Simulates XR "poke" and "grab" via raycasting
/// • Creates ONE hidden XRDirectInteractor at runtime
/// • Fully satisfies XR collider requirements (no warnings)
/// • Uses modern interface-based XR API
/// • Automatically disables itself when real XR is active
///
/// INTENDED USE
/// ------------
/// • Desktop / Editor testing
/// • Teaching XR interaction concepts
/// • Fallback when no headset is connected
///
/// NOT USED IN
/// -----------
/// • Real VR gameplay (auto-disabled)
/// </summary>
public class EditorXRInteractionSimulator : MonoBehaviour
{
    // ======================================================
    // INSPECTOR CONTROLS (STUDENT-FRIENDLY)
    // ======================================================

    [Header("Editor XR Simulation")]
    [Tooltip("Master toggle for editor-based XR simulation")]
    public bool enableEditorSimulation = true;

    [Tooltip("Camera used for raycasting (Non_VR_Camera)")]
    public Camera raycastCamera;

    [Header("Input Keys")]
    public KeyCode pokeKey = KeyCode.E;
    public KeyCode grabKey = KeyCode.G;

    [Header("Interaction Settings")]
    [Tooltip("Maximum interaction distance")]
    public float maxDistance = 3f;

    // ======================================================
    // XR REFERENCES (RUNTIME ONLY)
    // ======================================================

    XRInteractionManager interactionManager;
    XRDirectInteractor dummyInteractor;

    // Grab state
    IXRSelectInteractable grabbedInteractable;
    Rigidbody grabbedRigidbody;

    // ======================================================
    // UNITY LIFECYCLE
    // ======================================================

    void Start()
    {
        // --------------------------------------------------
        // 1. Disable if XR is actually running
        // --------------------------------------------------
        if (IsXRActive())
        {
            enabled = false;
            return;
        }

        if (!enableEditorSimulation)
        {
            enabled = false;
            return;
        }

        // --------------------------------------------------
        // 2. Find XR Interaction Manager (Unity 6 safe)
        // --------------------------------------------------
        interactionManager = FindAnyObjectByType<XRInteractionManager>();

        if (interactionManager == null)
        {
            Debug.LogWarning(
                "[EditorXRInteractionSimulator] No XRInteractionManager found. Simulation disabled."
            );
            enabled = false;
            return;
        }

        // --------------------------------------------------
        // 3. Create ONE dummy interactor (correct order)
        // --------------------------------------------------
        CreateDummyInteractor();
    }

    void Update()
    {
        if (!enableEditorSimulation) return;

        if (Input.GetKeyDown(pokeKey))
        {
            SimulatePoke();
        }

        if (Input.GetKeyDown(grabKey))
        {
            if (grabbedInteractable == null)
                SimulateGrab();
            else
                SimulateRelease();
        }

        // Smoothly pull grabbed object toward camera
        if (grabbedInteractable != null && grabbedRigidbody != null)
        {
            Vector3 targetPosition =
                raycastCamera.transform.position +
                raycastCamera.transform.forward * 0.6f;

            grabbedRigidbody.linearVelocity =
                (targetPosition - grabbedRigidbody.position) * 10f;
        }
    }

    // ======================================================
    // DUMMY INTERACTOR CREATION (ORDER MATTERS)
    // ======================================================

    void CreateDummyInteractor()
    {
        GameObject interactorGO =
            new GameObject("Editor_Dummy_XRDirectInteractor");

        // --------------------------------------------------
        // IMPORTANT:
        // Collider + Rigidbody MUST exist BEFORE
        // XRDirectInteractor is added or enabled.
        // --------------------------------------------------

        // 1️⃣ Trigger collider (required by XRDirectInteractor)
        SphereCollider trigger = interactorGO.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 0.08f;

        // 2️⃣ Rigidbody (required for trigger detection)
        Rigidbody rb = interactorGO.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // 3️⃣ XRDirectInteractor (now passes validation)
        dummyInteractor = interactorGO.AddComponent<XRDirectInteractor>();
        dummyInteractor.interactionManager = interactionManager;

        // Hide from hierarchy (editor-only helper)
        interactorGO.hideFlags = HideFlags.HideInHierarchy;
    }

    // ======================================================
    // INTERACTION SIMULATION
    // ======================================================

    void SimulatePoke()
    {
        if (!RaycastForInteractable(out XRBaseInteractable interactable))
            return;

        interactionManager.SelectEnter(
            (IXRSelectInteractor)dummyInteractor,
            (IXRSelectInteractable)interactable
        );

        interactionManager.SelectExit(
            (IXRSelectInteractor)dummyInteractor,
            (IXRSelectInteractable)interactable
        );

        Debug.Log($"[Editor XR Poke] {interactable.name}");
    }

    void SimulateGrab()
    {
        if (!RaycastForInteractable(out XRBaseInteractable interactable))
            return;

        interactionManager.SelectEnter(
            (IXRSelectInteractor)dummyInteractor,
            (IXRSelectInteractable)interactable
        );

        grabbedInteractable = interactable;
        grabbedRigidbody = interactable.GetComponent<Rigidbody>();

        if (grabbedRigidbody != null)
        {
            grabbedRigidbody.isKinematic = false;
            grabbedRigidbody.useGravity = true;
        }

        Debug.Log($"[Editor XR Grab] {interactable.name}");
    }

    void SimulateRelease()
    {
        if (grabbedInteractable == null) return;

        interactionManager.SelectExit(
            (IXRSelectInteractor)dummyInteractor,
            (IXRSelectInteractable)grabbedInteractable
        );

        grabbedInteractable = null;
        grabbedRigidbody = null;

        Debug.Log("[Editor XR Release]");
    }

    // ======================================================
    // RAYCAST HELPER
    // ======================================================

    bool RaycastForInteractable(out XRBaseInteractable interactable)
    {
        interactable = null;

        if (raycastCamera == null) return false;

        Ray ray = new Ray(
            raycastCamera.transform.position,
            raycastCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            interactable = hit.collider.GetComponent<XRBaseInteractable>();
            return interactable != null;
        }

        return false;
    }

    // ======================================================
    // XR MODE DETECTION
    // ======================================================

    bool IsXRActive()
    {
#if UNITY_XR_MANAGEMENT
        return UnityEngine.XR.Management.XRGeneralSettings.Instance != null &&
               UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager != null &&
               UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.isInitializationComplete;
#else
        return false;
#endif
    }
}
