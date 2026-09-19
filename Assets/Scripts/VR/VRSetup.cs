using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;

namespace DeadSignal.VR
{
    /// <summary>
    /// Configures the VR environment for Quest 3/3S.
    /// Sets up controller input, hand tracking, and XR rig.
    /// </summary>
    public class VRSetup : MonoBehaviour
    {
        [Header("XR Configuration")]
        [SerializeField] private XROrigin xrOrigin;
        [SerializeField] private float targetFrameRate = 90f;
        [SerializeField] private bool enableHandTracking = true;

        [Header("Locomotion")]
        [SerializeField] private float teleportDistance = 10f;
        [SerializeField] private bool enableSnapTurning = true;
        [SerializeField] private float snapTurnAngle = 45f;

        [Header("Comfort")]
        [SerializeField] private bool limitHeadMovement = false;
        [SerializeField] private float maxHeadHeight = 2.5f;
        [SerializeField] private float minHeadHeight = 0.8f;

        private XRRayInteractor leftRayInteractor;
        private XRRayInteractor rightRayInteractor;
        private ActionBasedContinuousMoveProvider moveProvider;
        private ActionBasedSnapTurnProvider snapTurnProvider;

        private void Awake()
        {
            SetTargetFrameRate();
            InitializeXRRig();
        }

        private void Start()
        {
            ConfigureLocomotion();
            ConfigureInteraction();
        }

        /// <summary>
        /// Sets the target frame rate for the device
        /// </summary>
        private void SetTargetFrameRate()
        {
            Application.targetFrameRate = (int)targetFrameRate;
            QualitySettings.vSyncCount = 0;
            Debug.Log($"Target frame rate set to: {targetFrameRate} FPS");
        }

        /// <summary>
        /// Initializes the XR origin and rig
        /// </summary>
        private void InitializeXRRig()
        {
            if (xrOrigin == null)
            {
                xrOrigin = FindObjectOfType<XROrigin>();
                if (xrOrigin == null)
                {
                    Debug.LogError("XROrigin not found in scene. Please create an XR Rig first.");
                    return;
                }
            }

            Debug.Log("XR Origin initialized");
        }

        /// <summary>
        /// Configures locomotion providers (teleport, snap turn)
        /// </summary>
        private void ConfigureLocomotion()
        {
            if (xrOrigin == null) return;

            moveProvider = xrOrigin.GetComponent<ActionBasedContinuousMoveProvider>();
            if (moveProvider != null)
            {
                // Teleportation is safer for comfort
                moveProvider.enabled = false;
                Debug.Log("Continuous movement disabled (using teleportation)");
            }

            snapTurnProvider = xrOrigin.GetComponent<ActionBasedSnapTurnProvider>();
            if (snapTurnProvider != null)
            {
                snapTurnProvider.enabled = enableSnapTurning;
                snapTurnProvider.turnAmount = snapTurnAngle;
                Debug.Log($"Snap turning configured: {snapTurnAngle}° increments");
            }
        }

        /// <summary>
        /// Configures hand tracking and interaction
        /// </summary>
        private void ConfigureInteraction()
        {
            if (xrOrigin == null) return;

            var interactionManager = xrOrigin.GetComponent<XRInteractionManager>();
            if (interactionManager == null)
            {
                interactionManager = xrOrigin.gameObject.AddComponent<XRInteractionManager>();
            }

            // Find or create ray interactors for controllers
            var rayInteractors = xrOrigin.GetComponentsInChildren<XRRayInteractor>();
            if (rayInteractors.Length >= 2)
            {
                leftRayInteractor = rayInteractors[0];
                rightRayInteractor = rayInteractors[1];
                Debug.Log("Ray interactors found and configured");
            }
            else
            {
                Debug.LogWarning("Expected 2 ray interactors, found " + rayInteractors.Length);
            }
        }

        /// <summary>
        /// Checks if headset is being worn (optional comfort feature)
        /// </summary>
        public bool IsHeadsetWorn()
        {
            // Implementation depends on Meta SDK
            // Placeholder for presence detection
            return true;
        }

        /// <summary>
        /// Gets the current frame rate
        /// </summary>
        public float GetCurrentFrameRate()
        {
            return 1f / Time.deltaTime;
        }

        /// <summary>
        /// Enables or disables snap turning
        /// </summary>
        public void SetSnapTurningEnabled(bool enabled)
        {
            if (snapTurnProvider != null)
            {
                snapTurnProvider.enabled = enabled;
            }
        }

        /// <summary>
        /// Resets player to standing position at origin
        /// </summary>
        public void ResetPlayerPosition()
        {
            if (xrOrigin != null)
            {
                xrOrigin.MoveCameraToWorldPosition(Vector3.zero);
                Debug.Log("Player position reset to origin");
            }
        }
    }
}
