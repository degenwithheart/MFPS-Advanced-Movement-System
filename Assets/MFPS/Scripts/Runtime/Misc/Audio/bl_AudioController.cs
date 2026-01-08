using System.Collections;
using UnityEngine;

namespace MFPS.Audio
{
    public class bl_AudioController : MonoBehaviour
    {
        [Header("Audio Handler")]
        public bl_VirtualAudioController audioController;

        [Header("Scene Settings")]
        public float maxWeaponDistance = 75;
        public float maxExplosionDistance = 100;
        public float maxFootstepDistance = 30;
        public AudioRolloffMode audioRolloffMode = AudioRolloffMode.Logarithmic;

        [Header("Background")]
        [SerializeField] private AudioClip BackgroundClip;
        [Range(0.01f, 1)] public float MaxBackgroundVolume = 0.3f;
        public float backgroundFadeDuration = 1;
        public AudioSource backgroundSource;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            audioController.Initialized(this);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            MaxBackgroundVolume = bl_MFPS.MusicVolume;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void PlayClip(string clipName)
        {
            if (Instance == null) return;

            Instance.audioController.PlayClip(clipName);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void PlayClipAtPoint(string clipName, Vector3 position)
        {
            if (Instance == null) return;

            Instance.audioController.PlayClipAtPoint(clipName, position);
        }

        /// <summary>
        /// 
        /// </summary>
        public void PlayBackground(bool fromStart = false)
        {
            if (!fromStart && backgroundSource != null && backgroundSource.isPlaying) { return; }

            if (BackgroundClip == null) return;
            if (backgroundSource == null) { backgroundSource = gameObject.AddComponent<AudioSource>(); }

            backgroundSource.clip = BackgroundClip;
            backgroundSource.volume = 0;
            backgroundSource.playOnAwake = false;
            backgroundSource.loop = true;
            StartCoroutine(FadeAudio(backgroundSource, true, MaxBackgroundVolume, backgroundFadeDuration));
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopBackground()
        {
            if (backgroundSource == null) return;

            FadeAudio(backgroundSource, false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ForceStopAllFades()
        {
            StopAllCoroutines();
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerator FadeAudio(AudioSource source, bool up, float volume = 1, float fadeDuration = 0.5f)
        {
            float duration = 0;
            if (up)
            {
                source.Play();
                float originalVolume = source.volume;

                while (duration < 1)
                {
                    duration += Time.deltaTime / fadeDuration;
                    source.volume = Mathf.Lerp(originalVolume, volume, duration);
                    yield return null;
                }
            }
            else
            {
                float originalVolume = source.volume;

                while (duration < 1)
                {
                    duration += Time.deltaTime / fadeDuration;
                    source.volume = Mathf.Lerp(originalVolume, 0, duration);
                    yield return null;
                }
            }
        }

        public float BackgroundVolume
        {
            set
            {
                if (backgroundSource != null) { backgroundSource.volume = value; }
            }
        }


        private static bl_AudioController _instance;
        public static bl_AudioController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<bl_AudioController>();
                }
                return _instance;
            }
        }
    }
}