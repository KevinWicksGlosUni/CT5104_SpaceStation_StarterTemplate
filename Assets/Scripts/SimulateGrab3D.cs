using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// SIMULATE GRAB (NON-VR / EDITOR MODE)
/// ----------------------------------
/// Keyboard-driven grab/release simulation for XR Interactables.
/// Designed for rapid iteration without a VR headset.
/// </summary>
public class SimulateGrab3D : MonoBehaviour
{
    [Header("Input")]
    public KeyCode grabKey = KeyCode.G;
    public float maxDistance = 3f;

    [Header("XR References")]
    public XRInteractionManager interactionManager;
    public XRDirectInteractor dummyInteractor;

    private IXRSelectInteractable grabbedObject;
    private Rigidbody grabbedRigidbody;
    private Transform originalParent;

    void Start()
    {
        if (interactionManager == null)
        {
            interactionManager = FindAnyObjectByType<XRInteractionManager>();
        }

        if (dummyInteractor == null)
        {
            GameObject interactorObj = new GameObject("Editor_Dummy_GrabInteractor");

            dummyInteractor = interactorObj.AddComponent<XRDirectInteractor>();
            dummyInteractor.interactionManager = interactionManager;

            SphereCollider trigger = interactorObj.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.1f;

            Rigidbody rb = interactorObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            interactorObj.hideFlags = HideFlags.HideInHierarchy;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(grabKey))
        {
            if (grabbedObject == null)
                SimulateGrab();
            else
                SimulateRelease();
        }

        // Pull grabbed object toward camera
        if (grabbedObject != null && grabbedRigidbody != null)
        {
            Vector3 target =
                transform.position + transform.forward * 0.6f;

            Vector3 smooth =
                Vector3.Lerp(grabbedRigidbody.position, target, Time.deltaTime * 10f);

            grabbedRigidbody.linearVelocity =
                (smooth - grabbedRigidbody.position) / Time.deltaTime;
        }
    }

    void SimulateGrab()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            XRBaseInteractable interactable =
                hit.collider.GetComponent<XRBaseInteractable>();

            if (interactable == null) return;

            // UPDATED XR API
            interactionManager.SelectEnter(
                (IXRSelectInteractor)dummyInteractor,
                (IXRSelectInteractable)interactable
            );

            grabbedObject = interactable;
            grabbedRigidbody = hit.collider.GetComponent<Rigidbody>();

            if (grabbedRigidbody != null)
            {
                grabbedRigidbody.isKinematic = false;
                grabbedRigidbody.useGravity = true;

                originalParent = grabbedObject.transform.parent;
                grabbedObject.transform.SetParent(null);
            }

            Debug.Log($"[Editor Grab] {interactable.name}");
        }
    }

    void SimulateRelease()
    {
        if (grabbedObject == null) return;

        interactionManager.SelectExit(
            (IXRSelectInteractor)dummyInteractor,
            (IXRSelectInteractable)grabbedObject
        );

        if (grabbedRigidbody != null)
        {
            grabbedRigidbody.useGravity = true;
            grabbedRigidbody.isKinematic = false;
        }

        grabbedObject.transform.SetParent(originalParent);

        grabbedObject = null;
        grabbedRigidbody = null;

        Debug.Log("[Editor Release]");
    }
}
