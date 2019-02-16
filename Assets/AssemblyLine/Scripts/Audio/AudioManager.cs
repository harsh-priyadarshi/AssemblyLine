using UnityEngine;
using System;
using DG.Tweening;
using System.Collections;
using UnityEngine.Events;
using AL.Gameplay;
using System.Collections.Generic;
using System.Reflection;

namespace AL.Audio
{
    [System.Serializable]
    public class Sound : IResettable
    {
        private UnityAction onCompleteAction;

        [SerializeField]
        private string name;

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
        private GameObject audioSourceObject;
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float spatialBlend = 0;

        [SerializeField]
        private bool isNarration = false;

        private bool initialized = false;
        private bool cancelOnCompleteActionOnInterruption = true;

        private AudioSource audioSource;
        private IEnumerator fadeCoroutine;
        private IEnumerator onCompleteActionEnumerator;

        private void ResetOnCompleteAction()
        {
            if (onCompleteActionEnumerator != null)
            {
                Coordinator.instance.StopCoroutine(onCompleteActionEnumerator);
                onCompleteActionEnumerator = null;
                onCompleteAction = null;
            }
        }

        private IEnumerator OnCompleteAction()
        {
            yield return new WaitForSeconds(RemainingLength);

            if (onCompleteAction != null)
                onCompleteAction.Invoke();
        }

        public bool IsNarration { get { return isNarration; } }

        public bool IsInitialized { get { return initialized; } }

        public string Name { get { return name; } }

        public bool IsPlaying { get { return audioSource ? audioSource.isPlaying : false; } }

        public float RemainingLength { get { return audioSource.clip.length - audioSource.time; } }

        public GameObject AUdioSourceObject { get { return audioSourceObject; } }


        public AudioSource Source { get { return audioSource; } }

        public AudioClip Clip { get { return clip; } }

        public void SetOnCompleteAction(UnityAction _action, bool cancelOnInterruption)
        {
            onCompleteAction = _action;
            if (onCompleteAction != null)
            {
                onCompleteActionEnumerator = OnCompleteAction();
                Coordinator.instance.StartCoroutine(onCompleteActionEnumerator);
            }
        }

        public void Play()
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                if (cancelOnCompleteActionOnInterruption)
                    ResetOnCompleteAction();
            }
            if (IsNarration)
            {
                audioSource.clip = clip;
                audioSource.volume = volume;
            }

            Resume();
        }

        public void Resume()
        {
            audioSource.Play();
            if (onCompleteAction != null)
            {
                onCompleteActionEnumerator = OnCompleteAction();
                Coordinator.instance.StartCoroutine(onCompleteActionEnumerator);
            }

        }

        public void Stop()
        {
            audioSource.Stop();
            if (cancelOnCompleteActionOnInterruption)
                ResetOnCompleteAction();
        }

        public void Pause()
        {
            audioSource.Pause();
            if (onCompleteActionEnumerator != null)
                Coordinator.instance.StopCoroutine(onCompleteActionEnumerator);
        }

        public void Init(AudioSource _audioSource)
        {
            initialized = true;
            audioSource = _audioSource;
            audioSource.playOnAwake = false;
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.loop = loop;
            audioSource.spatialBlend = spatialBlend;
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
           
            if(!play)
            {
                Pause();
                audioSource.volume = initialVolume;
            }
        }

        public void OnReset()
        {
            if (fadeCoroutine != null)
            {
                Coordinator.instance.audioManager.StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
        }
    }

    public class AudioManager : MonoBehaviour, IResettable
    {
        public const string homeBackgroundMusic = "home_background_music";
        public const string buttonClick = "button_click";
        public const string trainingBackgroundMusic = "training_background_music";
        public const string assessmentBackgroundMusic = "assessment_background_music";
        public const string machineAudio = "machine_audio";
        public const string wrench = "wrench";
        
        public Sound[] sounds;

        private AudioSource narrationSource;
        private Sound currentNarration = null;
        private List<Sound> narrationQueue = new List<Sound>();
        private IEnumerator narrationQueueProcessor = null;

        private bool narrationPaused = true;

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
                {
                    if (item.AUdioSourceObject != null)
                        item.Init(gameObject.AddComponent<AudioSource>());
                    else
                        item.Init(gameObject.AddComponent<AudioSource>());
                }
            }
        }

        public Sound Play(string name)
        {

            Sound s = Array.Find(sounds, sound => sound.Name == name);

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

        /// <summary>
        /// Onlyl for narration
        /// </summary>
        /// <param name="narrationName"></param>
        /// <returns></returns>
        public Sound Queue(string narrationName)
        {
            narrationPaused = false;
            //print("Queue: " + narrationName);
            Sound s = Array.Find(sounds, sound => sound.Name == narrationName);

            if (s == null)
            {
                Debug.LogError("sound: " + narrationName + " not found");
                return null;
            }

            narrationQueue.Add(s);
            //print("Adding: " + s.name + " to narration queue");

            if (narrationQueueProcessor == null)
            {
                narrationQueueProcessor = ProcessAudioQueue();
                StartCoroutine(narrationQueueProcessor);
            }

            return s;
        }

        public Sound Interrupt(string narrationName)
        {
            Sound s = Array.Find(sounds, sound => sound.Name == narrationName);

            if (s == null)
            {
                Debug.LogError("sound: " + narrationName + " not found");
                return null;
            }
            
            ResetNarrationProcessor();

            //print("interrupted by: " + narrationName);

            s.Play();

            return s;
        }

        private void ResetNarrationProcessor()
        {
            if (narrationQueueProcessor != null)
            {
                StopCoroutine(narrationQueueProcessor);
                narrationQueueProcessor = null;
                narrationPaused = false;
            }
        }

        private IEnumerator ProcessAudioQueue()
        {
            do
            {
                if (!narrationPaused)
                {
                    currentNarration = narrationQueue[0];
                    Play(currentNarration.Name);
                    //print("ProcessAudioQueue: " + currentNarration.Name);
                }
                else
                {
                    //print("ProcessAudioQueue: " + currentNarration.Name);
                    Resume(currentNarration.Name);
                }

                //yield return new WaitForSeconds(Coordinator.instance.settings.SelectedPreferences.narrationMinimumGap);

                //print("Removing: " + currentNarration.Name + " from queue");
                narrationQueue.Remove(currentNarration);

                yield return new WaitForSeconds(currentNarration.RemainingLength);
               
            }
            while (narrationQueue.Count > 0);
            narrationQueueProcessor = null;
            currentNarration = null;
        }

        public void Pause(float narrationFadeDuration)
        {
            narrationPaused = true;

            if (narrationQueueProcessor != null)
            {
                StopCoroutine(narrationQueueProcessor);
                narrationQueueProcessor = null;
            }

            if (currentNarration == null)
                return;

            if (currentNarration.IsInitialized)
            {
                //print(currentNarration.Name);
                currentNarration.FadeAudioToggle(false, narrationFadeDuration);
            }
        }

        public void Resume(float narrationResumeDuration)
        {
            if (currentNarration != null && currentNarration.IsInitialized)
            {
                //print("Resume");
                narrationQueueProcessor = ProcessAudioQueue();
                StartCoroutine(narrationQueueProcessor);
                currentNarration.FadeAudioToggle(true, narrationResumeDuration);
            }

            narrationPaused = false;
        }

        public void Resume(string name)
        {
            Sound s = Array.Find(sounds, sound => sound.Name == name);
            if (s == null)
            {
                Debug.LogError("sound: " + name + " not found");
                return;
            }
            s.Resume();
        }

        public void Resume(string name, float resumeDuration)
        {
            Sound s = Array.Find(sounds, sound => sound.Name == name);
            if (s == null)
            {
                Debug.LogError("sound: " + name + " not fouond");
                return;
            }

            s.Play();
            s.FadeAudioToggle(true, resumeDuration);
        }


        public void FadePause(string name, float duration)
        {
            Sound s = Array.Find(sounds, sound => sound.Name == name);
            if (s == null)
            {
                Debug.LogError("sound: " + name + " not fouond");
                return;
            }
          
            if (s.IsPlaying)
                s.FadeAudioToggle(false, duration);
        }

        public void FadePause(Sound s, float duration)
        {
            if (s == null)
            {
                Debug.LogError("sound: " + name + " not fouond");
                return;
            }

            if (s.IsPlaying)
                s.FadeAudioToggle(false, duration);
        }

        public void Pause(string name)
        {
            //if (VRSetup.CurrentPlatform == Enums.ViewPlatform.Desktop)
            //    return;

            Sound s = Array.Find(sounds, sound => sound.Name == name);
            if (s == null)
            {
                Debug.LogError("sound: " + name + " not fouond");
                return;
            }
            s.Pause();
        }

        public AudioClip HookedUpClip(string name)
        {
            Sound s = Array.Find(sounds, sound => sound.Name == name);
            if (s == null)
                return null;
            return s.Clip;
        }

        public void OnReset()
        {
            foreach (var item in sounds)
            {
                item.OnReset();
            }
            if (narrationQueueProcessor != null)
            {
                StopCoroutine(narrationQueueProcessor);
                narrationQueueProcessor = null;
            }
            narrationPaused = false;
            narrationQueue.Clear();
            currentNarration = null;
        }
    }
}
