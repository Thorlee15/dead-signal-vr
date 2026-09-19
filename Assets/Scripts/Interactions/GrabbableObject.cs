using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace DeadSignal.Interactions
{
    /// <summary>
    /// Makes an object grabable by the player's hands.
    /// Handles grab/release feedback and state management.
    /// </summary>
    public class GrabbableObject : XRGrabInteractable
    {
        [Header("Grab Feedback")]
        [SerializeField] private bool enableHapticFeedback = true;
        [SerializeField] private float grabHapticIntensity = 0.5f;
        [SerializeField] private float grabHapticDuration = 0.02f;

        [SerializeField] private AudioClip grabSound;
        [SerializeField] private float grabSoundVolume = 0.5f;

        [Header("Visual Feedback")]
        [SerializeField] private Color grabTintColor = Color.green;
        [SerializeField] private bool enableGrabbableHighlight = true;
        [SerializeField] private Color highlightColor = Color.green * 0.5f;

        private Material[] originalMaterials;
        private Color[] originalColors;
        private AudioSource audioSource;
        private bool isCurrentlyGrabbed = false;

        public System.Action OnGrabbed;
        public System.Action OnReleased;
        public System.Action<SocketZone> OnSocketed;

        protected override void OnEnable()
        {
            base.OnEnable();
            selectEntered.AddListener(OnSelectEntered);
            selectExited.AddListener(OnSelectExited);
            interactableHoverEntered.AddListener(OnHoverEntered);
            interactableHoverExited.AddListener(OnHoverExited);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            selectEntered.RemoveListener(OnSelectEntered);
            selectExited.RemoveListener(OnSelectExited);
            interactableHoverEntered.RemoveListener(OnHoverEntered);
            interactableHoverExited.RemoveListener(OnHoverExited);
        }

        private void Start()
        {
            CacheMaterials();
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void CacheMaterials()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            originalMaterials = new Material[renderers.Length];
            originalColors = new Color[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].material;
                originalColors[i] = originalMaterials[i].color;
            }
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            isCurrentlyGrabbed = true;
            PlayGrabFeedback();
            OnGrabbed?.Invoke();
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            isCurrentlyGrabbed = false;
            PlayReleaseFeedback();
            OnReleased?.Invoke();
        }

        private void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (!isCurrentlyGrabbed && enableGrabbableHighlight)
                SetHighlight(true);
        }

        private void OnHoverExited(HoverExitEventArgs args)
        {
            if (!isCurrentlyGrabbed && enableGrabbableHighlight)
                SetHighlight(false);
        }

        private void PlayGrabFeedback()
        {
            if (enableHapticFeedback)
                SendHapticFeedback(grabHapticIntensity, grabHapticDuration);

            if (grabSound && audioSource)
                audioSource.PlayOneShot(grabSound, grabSoundVolume);

            SetTint(grabTintColor);
        }

        private void PlayReleaseFeedback()
        {
            ResetTint();
        }

        private void SendHapticFeedback(float intensity, float duration)
        {
            // Implementation depends on input system
            // Placeholder for haptic feedback
        }

        private void SetHighlight(bool enabled)
        {
            if (enabled)
            {
                var renderers = GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    var mat = renderer.material;
                    mat.color = highlightColor;
                }
            }
            else
            {
                ResetMaterials();
            }
        }

        private void SetTint(Color tint)
        {
            var renderers = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (i < originalColors.Length)
                    renderers[i].material.color = originalColors[i] * tint;
            }
        }

        private void ResetTint()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (i < originalColors.Length)
                    renderers[i].material.color = originalColors[i];
            }
        }

        private void ResetMaterials()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (i < originalColors.Length)
                    renderers[i].material.color = originalColors[i];
            }
        }

        public bool IsGrabbed => isCurrentlyGrabbed;
    }
}
