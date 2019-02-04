using UnityEngine;
using System;

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
        public const string backgroundMusic = "background-music";

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
            s.audioSource.Play();
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

        public void PlayBackgroundMusic()
        {

        }

        public void PauseBackgroundMusic()
        {

        }
    }
}
