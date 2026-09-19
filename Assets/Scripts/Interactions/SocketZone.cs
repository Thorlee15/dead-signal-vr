using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace DeadSignal.Interactions
{
    /// <summary>
    /// Defines a socket where a GrabbableObject can be placed.
    /// Handles snapping and validation.
    /// </summary>
    public class SocketZone : XRSocketInteractor
    {
        // Snap range and attach alignment come from XRSocketInteractor's own
        // collider and attachTransform - do not shadow them with extra fields.

        [Header("Feedback")]
        [SerializeField] private bool enableSocketGlow = true;
        [SerializeField] private Color socketGlowColor = Color.green;
        [SerializeField] private AudioClip snapSound;
        [SerializeField] private float snapHapticIntensity = 1.0f;

        private Material socketMaterial;
        private Color originalSocketColor;
        private bool hasObject = false;

        public System.Action OnObjectSocketed;
        public System.Action OnObjectRemoved;

        protected override void OnEnable()
        {
            base.OnEnable();
            selectEntered.AddListener(OnObjectEntered);
            selectExited.AddListener(OnObjectExited);
            hoverEntered.AddListener(OnHoverEntered);
            hoverExited.AddListener(OnHoverExited);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            selectEntered.RemoveListener(OnObjectEntered);
            selectExited.RemoveListener(OnObjectExited);
            hoverEntered.RemoveListener(OnHoverEntered);
            hoverExited.RemoveListener(OnHoverExited);
        }

        private void Start()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer && enableSocketGlow)
            {
                socketMaterial = renderer.material;
                originalSocketColor = socketMaterial.color;
            }
        }

        private void OnObjectEntered(SelectEnterEventArgs args)
        {
            hasObject = true;
            PlaySnapFeedback();
            OnObjectSocketed?.Invoke();
        }

        private void OnObjectExited(SelectExitEventArgs args)
        {
            hasObject = false;
            PlayRemoveFeedback();
            OnObjectRemoved?.Invoke();
        }

        private void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (!hasObject && enableSocketGlow)
                SetSocketGlow(true);
        }

        private void OnHoverExited(HoverExitEventArgs args)
        {
            if (!hasObject && enableSocketGlow)
                SetSocketGlow(false);
        }

        private void SetSocketGlow(bool enabled)
        {
            if (!socketMaterial) return;

            if (enabled)
                socketMaterial.color = socketGlowColor;
            else
                socketMaterial.color = originalSocketColor;
        }

        private void PlaySnapFeedback()
        {
            var audioSource = GetComponent<AudioSource>();
            if (snapSound && audioSource)
                audioSource.PlayOneShot(snapSound, 0.7f);

            SendHapticFeedback(snapHapticIntensity, 0.1f);
        }

        private void PlayRemoveFeedback()
        {
            // Optional: play sound when object is removed
        }

        // NOT IMPLEMENTED - see the same stub in GrabbableObject.
        private void SendHapticFeedback(float intensity, float duration)
        {
        }

        public bool HasObject => hasObject;
    }
}
