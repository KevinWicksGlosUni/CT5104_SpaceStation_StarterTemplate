using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// SIMULATE POKE (NON-VR / EDITOR MODE)
/// ----------------------------------
/// Allows keyboard-based "poke" interaction with XR Interactables
/// while testing the scene without a VR headset.
///
/// This exists ONLY for editor convenience.
/// In a real headset, XRDirectInteractor would be driven by hand tracking.
/// </summary>
public class SimulatePoke3D : MonoBehaviour
{
    [Header("Input")]
    public KeyCode pokeKey = KeyCode.E;
    public float maxDistance = 3f;

    [Header("XR References")]
    public XRInteractionManager interactionManager;
    public XRDirectInteractor dummyInteractor;

    void Start()
    {
        // ------------------------------------------------------
        // Auto-find XR Interaction Manager
        // ------------------------------------------------------
        if (interactionManager == null)
        {
            interactionManager = FindAnyObjectByType<XRInteractionManager>();
        }

        // ------------------------------------------------------
        // Create an invisible, compliant XR Direct Interactor
        // ------------------------------------------------------
        if (dummyInteractor == null)
        {
            GameObject interactorObj = new GameObject("Editor_Dummy_PokeInteractor");

            dummyInteractor = interactorObj.AddComponent<XRDirectInteractor>();
            dummyInteractor.interactionManager = interactionManager;

            // XRDirectInteractor REQUIRES a trigger collider
            SphereCollider trigger = interactorObj.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.05f;

            // Rigidbody required for trigger processing
            Rigidbody rb = interactorObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Keep hierarchy tidy
            interactorObj.hideFlags = HideFlags.HideInHierarchy;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(pokeKey))
        {
            SimulatePoke();
        }
    }

    void SimulatePoke()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            XRBaseInteractable interactable =
                hit.collider.GetComponent<XRBaseInteractable>();

            if (interactable == null) return;

            // --------------------------------------------------
            // UPDATED XR API (INTERFACE-BASED)
            // --------------------------------------------------
            interactionManager.SelectEnter(
                (IXRSelectInteractor)dummyInteractor,
                (IXRSelectInteractable)interactable
            );

            interactionManager.SelectExit(
                (IXRSelectInteractor)dummyInteractor,
                (IXRSelectInteractable)interactable
            );

            Debug.Log($"[Editor Poke] {interactable.name}");
        }
    }
}
