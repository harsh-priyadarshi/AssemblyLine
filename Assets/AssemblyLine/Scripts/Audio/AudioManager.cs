using UnityEngine;
using System;
using DG.Tweening;
using System.Collections;
using UnityEngine.Events;

namespace AL.Audio
{
    [System.Serializable]
    public class Sound
    {
        private UnityAction onCompleteAction;

        public string name;
        [SerializeField]
        private AudioClip clip;
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float volume;
        [SerializeField]
        [Range(0.0f, 3.0f)]
        private float pitch;
        [SerializeField]
        private bool loop = false;

        [SerializeField]
        private bool isNarration = false;

        private AudioSource audioSource;
        private IEnumerator fadeCoroutine;

        public bool IsNarration { get { return isNarration; } }

        public bool IsPlaying { get { return audioSource ? audioSource.isPlaying : false; } }

        public AudioSource Source { get { return audioSource; } }

        public AudioClip Clip { get { return clip; } }

        public void SetOnCompleteAction(UnityAction _action)
        {
            onCompleteAction = _action;
        }

        public void Play()
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            if (IsNarration)
                audioSource.clip = clip;

            audioSource.Play();
        }

        public void Resume()
        {
            audioSource.Play();
        }

        public void Stop()
        {
            audioSource.Stop();
        }

        public void Pause()
        {
            audioSource.Pause();
        }

        public void Init(AudioSource _audioSource)
        {
            audioSource = _audioSource;
            audioSource.playOnAwake = false;
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = loop;
        }

        public void FadeAudioToggle(bool play, float fadeDuration)
        {
            if (fadeCoroutine != null)
                Coordinator.instance.audioManager.StopCoroutine(fadeCoroutine);
            fadeCoroutine = FadeAudioToggleCoroutine(play, fadeDuration);
            Coordinator.instance.audioManager.StartCoroutine(fadeCoroutine);
        }

        private IEnumerator FadeAudioToggleCoroutine(bool play, float fadeDuration)
        {
            float initialVolume = audioSource.volume;
            float elapsedTime = 0.0f;
            while (elapsedTime < fadeDuration)
            {

                elapsedTime += Time.deltaTime;
                float currentVolume = Mathf.Lerp(play ? 0 : initialVolume, play ? initialVolume : 0, Mathf.Clamp01(elapsedTime / fadeDuration));
                audioSource.volume = currentVolume;
                yield return new WaitForEndOfFrame();
            }

            if (play)
                audioSource.Play();
            else
            {
                audioSource.Pause();
                audioSource.volume = initialVolume;
            }
        }
    }

    public class AudioManager : MonoBehaviour
    {
        public const string homeBackgroundMusic = "home_background_music";
        public const string buttonClick = "button_click";
        public const string trainingBackgroundMusic = "training_background_music";
        public const string assessmentBackgroundMusic = "assessment_background_music";

        public Sound[] sounds;

        private AudioSource narrationSource;
        private Sound currentNarration;

        private void Awake()
        {
            foreach (var item in sounds)
            {
                if (item.IsNarration && narrationSource == null)
                {
                    item.Init(gameObject.AddComponent<AudioSource>());
                    narrationSource = item.Source;
                }
                else if (item.IsNarration)
                    item.Init(narrationSource);
                else
                    item.Init(gameObject.AddComponent<AudioSource>());
            }
        }

        public Sound Play(string name)
        {
            //if (VRSetup.CurrentPlatform == Enums.ViewPlatform.Desktop)
            //    return;

            Sound s = Array.Find(sounds, sound => sound.name == name);

            if (s == null)
            {
                Debug.LogError("sound: " + name + " not found");
                return null;
            }

            s.Play();
            if (s.IsNarration)
                currentNarration = s;

            return s;
        }

        public void Resume(string name)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogError("sound: " + name + " not fouond");
                return;
            }
            s.Play();
        }

        public void Resume(string name, float resumeDuration)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogError("sound: " + name + " not fouond");
                return;
            }
            s.FadeAudioToggle(true, resumeDuration);
        }

        public void FadePause(string name, float duration)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogError("sound: " + name + " not fouond");
                return;
            }
            s.FadeAudioToggle(false, duration);
        }

        public void FadePause(Sound s, float duration)
        {
            if (s == null)
            {
                Debug.LogError("sound: " + name + " not fouond");
                return;
            }
            s.FadeAudioToggle(false, duration);
        }

        public void FadePause(float narrationFadeDuration)
        {
            if (currentNarration != null)
                currentNarration.FadeAudioToggle(false, narrationFadeDuration);
        }

        public void Resume(float narrationResumeDuration)
        {
            if (currentNarration != null)
                currentNarration.FadeAudioToggle(true, narrationResumeDuration);
        }

        public void Pause(string name)
        {
            //if (VRSetup.CurrentPlatform == Enums.ViewPlatform.Desktop)
            //    return;

            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogError("sound: " + name + " not fouond");
                return;
            }
            s.Pause();
        }

        public AudioClip HookedUpClip(string name)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
                return null;
            return s.Clip;
        }
    }
}
