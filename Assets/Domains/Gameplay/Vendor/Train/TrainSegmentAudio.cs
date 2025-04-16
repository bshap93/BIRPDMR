using DG.Tweening;
using UnityEngine;

namespace Domains.Gameplay.Vendor.Train
{
    public class TrainSegmentAudio : MonoBehaviour
    {
        [Header("Audio Sources")] [SerializeField]
        private AudioSource engineSound;

        [SerializeField] private AudioSource startupSound;
        [SerializeField] private AudioSource shutdownSound;

        [Header("Audio Settings")] [SerializeField]
        private float fadeInDuration = 1.5f;

        [SerializeField] private float fadeOutDuration = 3.0f;
        [SerializeField] private float pitchVariation = 0.1f;

        private TrainSegmentController segmentController;
        private Tweener volumeTween;

        private void Awake()
        {
            segmentController = GetComponentInParent<TrainSegmentController>();

            // Initialize engine sound
            if (engineSound != null)
            {
                engineSound.loop = true;
                engineSound.volume = 0f;
                engineSound.playOnAwake = false;
            }
        }

        private void OnEnable()
        {
            // Subscribe to segment controller events if you add them
        }

        private void OnDisable()
        {
            // Unsubscribe from events

            // Kill any active tweens
            if (volumeTween != null && volumeTween.IsActive())
                volumeTween.Kill();
        }

        public void PlayStartupSound()
        {
            // Play one-shot startup sound
            if (startupSound != null)
                startupSound.Play();

            // Start engine and fade in
            if (engineSound != null)
            {
                // Add slight pitch randomization for more natural sound
                engineSound.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);

                // Stop any active volume tweens
                if (volumeTween != null && volumeTween.IsActive())
                    volumeTween.Kill();

                // Start the sound at 0 volume
                engineSound.volume = 0f;
                engineSound.Play();

                // Fade in with DOTween
                volumeTween = engineSound.DOFade(1f, fadeInDuration)
                    .SetEase(Ease.OutQuad);
            }
        }

        public void PlayShutdownSound()
        {
            // Play one-shot shutdown sound
            if (shutdownSound != null)
                shutdownSound.Play();

            // Fade out engine sound
            if (engineSound != null && engineSound.isPlaying)
            {
                // Stop any active volume tweens
                if (volumeTween != null && volumeTween.IsActive())
                    volumeTween.Kill();

                // Fade out with DOTween
                volumeTween = engineSound.DOFade(0f, fadeOutDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => engineSound.Stop());
            }
        }
    }
}