using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// ARScannerDevice (Auto-Bind Version)
/// If no InputAction is assigned, creates one automatically.
/// This avoids setup errors in class.
/// </summary>
public class ARScannerDevice : MonoBehaviour
{
    [Header("Input")]
    public InputAction scanAction;

    [Header("Scan Settings")]
    public Transform scanOrigin;
    public float scanDistance = 8f;
    public LayerMask scannableLayers;

    [Header("Scanner UI")]
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI targetNameText;
    public TextMeshProUGUI statusText;

    [Header("Visual Feedback")]
    public LineRenderer scanLine;
    public GameObject scannerGlow;

    private ARScannable currentTarget;
    private float scanTimer;

    // =========================
    // AUTO INPUT SETUP
    // =========================

    void Awake()
    {
        // If no InputAction assigned → create one automatically
        if (scanAction == null)
        {
            scanAction = new InputAction("Scan", InputActionType.Button);

            // Desktop fallback
            scanAction.AddBinding("<Mouse>/leftButton");

            // XR trigger (Right Hand)
            scanAction.AddBinding("<XRController>{RightHand}/trigger");

            Debug.Log("Auto-created Scan InputAction (Mouse + XR Trigger)");
        }
    }

    private void OnEnable()
    {
        scanAction.Enable();
    }

    private void OnDisable()
    {
        scanAction.Disable();
    }

    void Start()
    {
        ResetUI();

        if (scanLine != null)
            scanLine.enabled = false;

        if (scannerGlow != null)
            scannerGlow.SetActive(false);
    }

    void Update()
    {
        if (scanAction.IsPressed())
        {
            Scan();
        }
        else
        {
            StopScan();
        }
    }

    void Scan()
    {
        if (scannerGlow != null)
            scannerGlow.SetActive(true);

        Ray ray = new Ray(scanOrigin.position, scanOrigin.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, scanDistance, scannableLayers))
        {
            if (scanLine != null)
            {
                scanLine.enabled = true;
                scanLine.SetPosition(0, scanOrigin.position);
                scanLine.SetPosition(1, hit.point);
            }

            ARScannable target = hit.collider.GetComponentInParent<ARScannable>();

            if (target != null)
            {
                if (currentTarget != target)
                {
                    ClearTarget();
                    currentTarget = target;
                    scanTimer = 0f;
                }

                scanTimer += Time.deltaTime;

                headerText.text = "SCANNING";
                targetNameText.text = target.displayName;

                float progress = Mathf.Clamp01(scanTimer / target.requiredScanTime);
                statusText.text = "Progress: " + Mathf.RoundToInt(progress * 100f) + "%";

                if (scanTimer >= target.requiredScanTime)
                {
                    headerText.text = "SCAN COMPLETE";
                    targetNameText.text = target.displayName;
                    statusText.text = target.description;

                    target.ShowScanVisuals();
                }

                return;
            }
        }

        LoseTarget();
    }

    void StopScan()
    {
        DisableVisuals();
        ClearTarget();
        ResetUI();
    }

    void LoseTarget()
    {
        DisableVisuals();
        ClearTarget();

        headerText.text = "NO TARGET";
        targetNameText.text = "---";
        statusText.text = "Aim at a scannable object.";
    }

    void DisableVisuals()
    {
        if (scanLine != null)
            scanLine.enabled = false;

        if (scannerGlow != null)
            scannerGlow.SetActive(false);
    }

    void ClearTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.HideScanVisuals();
            currentTarget = null;
        }

        scanTimer = 0f;
    }

    void ResetUI()
    {
        if (headerText != null) headerText.text = "SCANNER READY";
        if (targetNameText != null) targetNameText.text = "---";
        if (statusText != null) statusText.text = "Hold trigger or click to scan.";
    }
}