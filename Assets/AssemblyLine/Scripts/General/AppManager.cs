using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AL.Audio;

namespace AL
{
    /// <summary>
    /// Stores local values
    /// </summary>
    public struct CustomTransform
    {
        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;

        public Vector3 Position { get { return position; } }
        public Quaternion Rotation { get { return rotation; } }
        public Vector3 Scale { get { return scale; } }

        public CustomTransform(Vector3 _position, Quaternion _rotation)
        {
            position = _position;
            rotation = _rotation;
            scale = Vector3.one;
        }

        public void Extract(Transform transform)
        {
            position = transform.localPosition;
            rotation = transform.localRotation;
            scale = transform.localScale;
        }

        public void Apply(Transform transform)
        {
            transform.localPosition = position;
            transform.localRotation = rotation;
            transform.localScale = scale;
        }
    }

    public enum State
    {
        NONE,
        TRAINING,
        ASSESSMENT
    }

    public class AppManager : MonoBehaviour {
        [SerializeField]
        private Material homeSkybox, gameplaySkybox;
        [SerializeField]
        private GameObject homeCanvas;
        [SerializeField]
        private GameObject mainHomeMenu, trainingHomeMenu, assessmentHomeMenu;
        [SerializeField]
        private GameObject vrTitleText, vrLoginInstructionText;
        [SerializeField]
        private OVRScreenFade ovrScreenFade;

        private GameObject gameplayHomeMenu;
        private State currentState = State.NONE;
        private bool atHome = true;
        private CustomTransform homePlayerPosition, gameplayPlayerPosition;
        private IEnumerator toggleHomeCoroutine = null;

        public State CurrentState { get { return currentState; } }

        private void Awake()
        {
            Coordinator.instance.ovrPlayerController.SetHaltUpdateMovement(true);
            homePlayerPosition.Extract(Coordinator.instance.ovrPlayerController.transform);
            gameplayPlayerPosition.Extract(Coordinator.instance.ovrPlayerController.transform);
        }

        private void Update()
        {
            if (Coordinator.instance.settings.SelectedPreferences.mainMenuKey.GetDown())
                ToggleGameplay();
        }

        private IEnumerator ToggleHomeCoroutine()
        {
            atHome = !atHome;
            ovrScreenFade.FadeOut();

            yield return new WaitForSeconds(ovrScreenFade.fadeTime);

            if (atHome)
            {
                gameplayPlayerPosition.Extract(Coordinator.instance.ovrPlayerController.transform);
                homePlayerPosition.Apply(Coordinator.instance.ovrPlayerController.transform);
            }
            else
            {
                gameplayPlayerPosition.Apply(Coordinator.instance.ovrPlayerController.transform);
            }

            RenderSettings.skybox = atHome ? homeSkybox : gameplaySkybox;
            homeCanvas.SetActive(atHome);
            if (atHome)
                Coordinator.instance.audioManager.Resume(AudioManager.backgroundMusic);
            else
                Coordinator.instance.audioManager.FadePause(AudioManager.backgroundMusic, ovrScreenFade.fadeTime);
            gameplayHomeMenu.SetActive(atHome);
            Coordinator.instance.ovrPlayerController.SetHaltUpdateMovement(atHome);
            ovrScreenFade.FadeIn();

            yield return new WaitForSeconds(ovrScreenFade.fadeTime);

            toggleHomeCoroutine = null;
        }

        private void ToggleHome()
        {
            if (toggleHomeCoroutine == null)
            {
                toggleHomeCoroutine = ToggleHomeCoroutine();
                StartCoroutine(toggleHomeCoroutine);
            }
        }

        public void InitTraining()
        {
            currentState = State.TRAINING;
            gameplayHomeMenu = trainingHomeMenu;
            gameplayHomeMenu.SetActive(true);
            mainHomeMenu.SetActive(false);
            ToggleHome();
        }

        public void InitAssessment()
        {
            currentState = State.ASSESSMENT;
            gameplayHomeMenu = assessmentHomeMenu;
            gameplayHomeMenu.SetActive(true);
            mainHomeMenu.SetActive(false);
            ToggleHome();
        }

        public void ToggleGameplay()
        {
            //print("ToggleGameplay");
            if (currentState != State.NONE)
            {
                ToggleHome();
                
            }
        }

        public  void GameplayQuit()
        {
            //print("GameplayQuit");
            currentState = State.NONE;
            gameplayHomeMenu.SetActive(false);
            mainHomeMenu.SetActive(true);
        }

        public void TrainingReset()
        {
            //print("TrainingReset");
            ToggleHome();
        }

        public void AssessmentReset()
        {
            //print("AssessmentReset");
            ToggleHome();
        }

        public void OnLoginToggle(bool val)
        {
            vrTitleText.SetActive(val);
            vrLoginInstructionText.SetActive(!val);
            if (currentState == State.NONE)
                mainHomeMenu.SetActive(val);
            else
                gameplayHomeMenu.SetActive(val);

        }
    }
}
