using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace DeadSignal.UI
{
    /// <summary>
    /// Displays subtitles for audio dialogue and sound effects.
    /// Ensures accessibility for deaf and hard-of-hearing players.
    /// </summary>
    public class SubtitleDisplay : MonoBehaviour
    {
        [Header("Subtitle UI")]
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform subtitlePanel;

        [Header("Display Settings")]
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private bool followSpeaker = false;

        [Header("Accessibility")]
        [SerializeField] private float fontSizeMin = 24f;
        [SerializeField] private float fontSizeMax = 48f;
        [SerializeField] private float fontSizeDefault = 36f;

        private Coroutine displayCoroutine;
        private AudioSource audioSource;

        private void Start()
        {
            if (subtitleText == null)
            {
                subtitleText = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            audioSource = GetComponent<AudioSource>();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
            }

            SetFontSize(fontSizeDefault);
        }

        /// <summary>
        /// Displays a subtitle for the specified duration
        /// </summary>
        public void DisplaySubtitle(string text, float duration = -1)
        {
            if (string.IsNullOrEmpty(text))
            {
                HideSubtitle();
                return;
            }

            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }

            if (duration < 0)
                duration = displayDuration;

            displayCoroutine = StartCoroutine(DisplaySubtitleCoroutine(text, duration));
        }

        /// <summary>
        /// Displays subtitle synced to audio playback
        /// </summary>
        public void DisplaySubtitleWithAudio(string text, AudioClip audioClip)
        {
            if (audioClip == null)
            {
                DisplaySubtitle(text);
                return;
            }

            float duration = audioClip.length + 0.5f; // Extra time after audio ends
            DisplaySubtitle(text, duration);
        }

        /// <summary>
        /// Immediately hides the subtitle
        /// </summary>
        public void HideSubtitle()
        {
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
                displayCoroutine = null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
            }
        }

        /// <summary>
        /// Sets the subtitle font size (for accessibility)
        /// </summary>
        public void SetFontSize(float size)
        {
            if (subtitleText != null)
            {
                subtitleText.fontSize = Mathf.Clamp(size, fontSizeMin, fontSizeMax);
            }
        }

        /// <summary>
        /// Increases font size for accessibility
        /// </summary>
        public void IncreaseFontSize()
        {
            if (subtitleText != null)
            {
                float newSize = subtitleText.fontSize + 4;
                SetFontSize(newSize);
            }
        }

        /// <summary>
        /// Decreases font size
        /// </summary>
        public void DecreaseFontSize()
        {
            if (subtitleText != null)
            {
                float newSize = subtitleText.fontSize - 4;
                SetFontSize(newSize);
            }
        }

        /// <summary>
        /// Sets subtitle background transparency (0-1)
        /// </summary>
        public void SetBackgroundOpacity(float opacity)
        {
            if (canvasGroup != null)
            {
                // Adjust alpha based on background panel
                var bgImage = subtitlePanel.GetComponent<Image>();
                if (bgImage != null)
                {
                    Color color = bgImage.color;
                    color.a = Mathf.Clamp01(opacity);
                    bgImage.color = color;
                }
            }
        }

        private IEnumerator DisplaySubtitleCoroutine(string text, float duration)
        {
            // Set text
            if (subtitleText != null)
            {
                subtitleText.text = text;
            }

            // Fade in
            yield return StartCoroutine(FadeIn());

            // Display
            yield return new WaitForSeconds(duration);

            // Fade out
            yield return StartCoroutine(FadeOut());
        }

        private IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;

            float elapsed = 0;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / fadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1;
        }

        private IEnumerator FadeOut()
        {
            if (canvasGroup == null) yield break;

            float elapsed = 0;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeOutDuration);
                yield return null;
            }

            canvasGroup.alpha = 0;
        }

        /// <summary>
        /// Checks if subtitles are currently visible
        /// </summary>
        public bool IsVisible => canvasGroup != null && canvasGroup.alpha > 0.5f;
    }
}
