using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AL.Audio;
using AL.Gameplay;
using System;
using UnityEngine.Events;

namespace AL
{
    [System.Serializable]
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

    [Serializable]
    public class NarrationSet
    {
        [SerializeField]
        private List<string> narrations;
        public string RetrieveNarration()
        {
            if (narrations.Count == 0)
                return string.Empty;

            return narrations[(int)UnityEngine.Random.Range(0, narrations.Count)];
        }
    }

    public class AppManager : MonoBehaviour {

        public const string idleHighlightShaderPath = "Ciconia Studio/Effects/Highlight/Opaque";
        public const string blinkHighlightShaderPath = "Ciconia Studio/Effects/Highlight/BlinkHighlight";
        public const string transparentHighlightShaderPath = "Unlit/AL/TransparentHiglight";
        public const string normalShaderPath = "Standard";

        [Header("Gameplay")]
        [SerializeField]
        private List<Step> assemblySteps;

        [Header("Others")]
        [SerializeField]
        private Material homeSkybox;
        [SerializeField]
        private Material gameplaySkybox;
        [SerializeField]
        private GameObject homeCanvas;
        [SerializeField]
        private GameObject mainHomeMenu, trainingHomeMenu, assessmentHomeMenu;
        [SerializeField]
        private GameObject vrTitleText, vrLoginInstructionText;
        [SerializeField]
        private OVRScreenFade ovrScreenFade;
        [SerializeField]
        private GameObject trainingEnvironment, assessmentEnvironment, assemblyObjects;
        [SerializeField]
        NarrationSet wrongToolNarrationSet, wrongLocationNarrationSet, wrongPartNarrationSet, correctStepNarrationSet;

        private GameObject gameplayEnvironment;
        private GameObject gameplayHomeMenu;
        private State currentState = State.NONE;
        private bool atHome = true;
        private CustomTransform homePlayerPosition, gameplayPlayerPosition;
        private IEnumerator toggleHomeCoroutine = null;

        private bool introDone = false;

        #region RESETTABLE_VARIABLES
        private bool gameplayStarted = false;
        private int currentStepIndex = 0;
        #endregion

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

            if (Coordinator.instance.settings.SelectedPreferences.gameplayStartKey.GetDown() && currentState != State.NONE && !gameplayStarted && !atHome)
            {
                assemblyObjects.SetActive(true);
                InitiateNextStep();
                gameplayStarted = true;
            }
        }

        public void ApplyShader(bool CiconiaHighlight, string shaderPath, GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                foreach (Material mat in renderer.materials)
                {

                    try
                    {
                        mat.shader = Shader.Find(shaderPath);
                        if (CiconiaHighlight)
                        {
                            mat.SetFloat("_FresnelSpread", 2.0f);
                            mat.SetFloat("_FresnelStrength", 8);
                            mat.SetColor("_HighlightColor", color);
                        }
                    }
                    catch { }
                    finally
                    {

                    }
                }
            }

            foreach (Transform child in obj.transform)
                ApplyShader(CiconiaHighlight, shaderPath, child.gameObject, color);

        }

        public void ApplyShader(string shaderPath, GameObject obj)
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                foreach (Material mat in renderer.materials)
                {

                    try
                    {
                        mat.shader = Shader.Find(shaderPath);
                    }
                    catch { }
                    finally
                    {

                    }
                }
            }

            foreach (Transform child in obj.transform)
                ApplyShader(shaderPath, child.gameObject);

        }

        public void InitTraining()
        {
            currentState = State.TRAINING;
            gameplayHomeMenu = trainingHomeMenu;
            gameplayEnvironment = trainingEnvironment;
            mainHomeMenu.SetActive(false);
            ToggleHome(() => GiveIntro());
        }

        public void InitAssessment()
        {
            currentState = State.ASSESSMENT;
            gameplayHomeMenu = assessmentHomeMenu;
            gameplayEnvironment = assessmentEnvironment;
            mainHomeMenu.SetActive(false);
            ToggleHome(() => GiveIntro());
        }

        public void ToggleGameplay()
        {
            //print("ToggleGameplay");
            if (currentState != State.NONE)
                ToggleHome(null);
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
            homePlayerPosition.Apply(Coordinator.instance.ovrPlayerController.transform);
            gameplayPlayerPosition = homePlayerPosition;
            ToggleHome(null);
        }

        public void AssessmentReset()
        {
            homePlayerPosition.Apply(Coordinator.instance.ovrPlayerController.transform);
            gameplayPlayerPosition = homePlayerPosition;
            ToggleHome(null);
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

        #region GAMEPLAY
        private void InitiateNextStep()
        {
            //print("InitiateNextStep");
            if (assemblySteps.Count > currentStepIndex)
                assemblySteps[currentStepIndex].InitiateStep();
        }

        private void GiveIntro()
        {
            if (!introDone)
            {
                Coordinator.instance.audioManager.Play(currentState == State.TRAINING ? "training_intro" : "assessment_intro");
                introDone = true;
            }
        }

        private IEnumerator ToggleHomeCoroutine(UnityAction onCompleteAction)
        {
            atHome = !atHome;
            ovrScreenFade.FadeOut();

            yield return new WaitForSeconds(ovrScreenFade.fadeTime);

            if (atHome)
            {
                Coordinator.instance.audioManager.FadePause(currentState == State.TRAINING ? AudioManager.trainingBackgroundMusic : AudioManager.assessmentBackgroundMusic, ovrScreenFade.fadeTime);
                Coordinator.instance.audioManager.FadePause(ovrScreenFade.fadeTime);
            }
            else
                Coordinator.instance.audioManager.FadePause(AudioManager.homeBackgroundMusic, ovrScreenFade.fadeTime);

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
                Coordinator.instance.audioManager.Resume(AudioManager.homeBackgroundMusic, ovrScreenFade.fadeTime);
            else
            {
                Coordinator.instance.audioManager.Resume(currentState == State.TRAINING ? AudioManager.trainingBackgroundMusic : AudioManager.assessmentBackgroundMusic);
                Coordinator.instance.audioManager.Resume(ovrScreenFade.fadeTime);
            }
            gameplayHomeMenu.SetActive(atHome);
            gameplayEnvironment.SetActive(!atHome);

            if (atHome)
                assemblyObjects.SetActive(false);
            else if (gameplayStarted)
                assemblyObjects.SetActive(true);

            Coordinator.instance.ovrPlayerController.SetHaltUpdateMovement(atHome);
            ovrScreenFade.FadeIn();

            yield return new WaitForSeconds(ovrScreenFade.fadeTime);

            toggleHomeCoroutine = null;

            if (onCompleteAction != null)
                onCompleteAction.Invoke();
            GiveIntro();
        }

        private void ToggleHome(UnityAction onCompleteAction)
        {
            if (toggleHomeCoroutine == null)
            {
                toggleHomeCoroutine = ToggleHomeCoroutine(onCompleteAction);
                StartCoroutine(toggleHomeCoroutine);
            }
        }

        public string RetrieveNarration(Mistake mistakeLevel)
        {
            switch (mistakeLevel)
            {
                case Mistake.TOOL:
                    return wrongToolNarrationSet.RetrieveNarration();
                case Mistake.PART:
                    return wrongPartNarrationSet.RetrieveNarration();
                case Mistake.LOCATION:
                    return wrongLocationNarrationSet.RetrieveNarration();
                default:
                    return String.Empty;
            }
        }

        public string RetrieveCorrectStepNarration()
        {
            return correctStepNarrationSet.RetrieveNarration();
        }

        public void OnObjectGrab(GameObject obj)
        {
            //print("OnObjectGrab");
            if (currentStepIndex < assemblySteps.Count && assemblySteps[currentStepIndex].StepType == StepType.PART_PLACEMENT)
            {
                assemblySteps[currentStepIndex].ValidatePickup(obj);
            }
        }

        public void OnToolGrab(GameObject obj)
        {
            print("OnToolGrab");
            if (currentStepIndex < assemblySteps.Count && assemblySteps[currentStepIndex].StepType == StepType.PART_INSTALLATION)
            {

            }
        }

        public void OnPlacement(bool correct)
        {
            if (currentStepIndex < assemblySteps.Count && assemblySteps[currentStepIndex].StepType == StepType.PART_PLACEMENT)
            {
                assemblySteps[currentStepIndex].OnPlacement(correct);
            }
        }
        #endregion
    }
}
