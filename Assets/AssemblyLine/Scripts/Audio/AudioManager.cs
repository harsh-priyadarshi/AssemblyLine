using UnityEngine;
using System;
using DG.Tweening;
using System.Collections;

namespace AL.Audio
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0.0f, 1.0f)]
        public float volume;
        [Range(0.0f, 3.0f)]
        public float pitch;
        public bool loop = false;

        [HideInInspector]
        public AudioSource audioSource;

       
    }

    public class AudioManager : MonoBehaviour
    {
        public const string backgroundMusic = "background_music";
        public const string buttonClick = "button_click";

        public Sound[] sounds;
        private void Awake()
        {
            foreach (var item in sounds)
            {
                item.audioSource = gameObject.AddComponent<AudioSource>();
                item.audioSource.playOnAwake = false;
                item.audioSource.clip = item.clip;
                item.audioSource.volume = item.volume;
                item.audioSource.pitch = item.pitch;
                item.audioSource.loop = item.loop;
            }
        }

        private IEnumerator FadeAudioToggle(AudioSource audioSource, bool play, float fadeDuration)
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

        public void Play(string name)
        {
            //if (VRSetup.CurrentPlatform == Enums.ViewPlatform.Desktop)
            //    return;

            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogError("sound: " + name + " not fouond");
                return;
            }
            if (s.audioSource.isPlaying)
                s.audioSource.Stop();
            s.audioSource.Play();
        }

        public void Resume(string name)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogError("sound: " + name + " not fouond");
                return;
            }
            s.audioSource.Play();
        }

        public void FadePause(string name, float duration)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogError("sound: " + name + " not fouond");
                return;
            }
            StartCoroutine(FadeAudioToggle(s.audioSource, false, duration));
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
            s.audioSource.Pause();
        }

        public AudioClip HookedUpClip(string name)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
                return null;
            return s.audioSource.clip;
        }
    }
}
