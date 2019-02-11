using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AL.Audio;
using AL.Gameplay;
using System;
using UnityEngine.Events;
using System.Linq;

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
        private Transform parent;

        public Vector3 Position { get { return position; } }
        public Quaternion Rotation { get { return rotation; } }
        public Vector3 Scale { get { return scale; } }

        public CustomTransform(Vector3 _position, Quaternion _rotation, Transform _parent)
        {
            position = _position;
            rotation = _rotation;
            scale = Vector3.one;
            parent = _parent;
        }

        public void Extract(Transform transform)
        {
            position = transform.localPosition;
            rotation = transform.localRotation;
            scale = transform.localScale;
            parent = transform.parent;
        }

        public void Apply(Transform transform)
        {
            transform.SetParent(parent);
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
    public struct Narration
    {
        public string name;
        public MultipleNarrationType narrationType;
    }

    [Serializable]
    public class NarrationSet
    {
        [SerializeField]
        private List<Narration> narrations;

        public string RetrieveNarration(MultipleNarrationType type)
        {
            if (narrations.Count == 0)
                return string.Empty;

            var requestedNarrationList = narrations.Where(item => item.narrationType == type).ToList();

            if (requestedNarrationList.Count == 0)
                return string.Empty;

            return requestedNarrationList[(int)UnityEngine.Random.Range(0, requestedNarrationList.Count)].name;
        }
    }

    public enum MultipleNarrationType
    {
        ASSEMBLY_FINISH,
        WRONG_TOOL,
        WRONG_LOCATION,
        WRONG_PART,
        CORRECT_STEP
    }

    public class AppManager : MonoBehaviour {

        public const string idleHighlightShaderPath = "Ciconia Studio/Effects/Highlight/Opaque";
        public const string blinkHighlightShaderPath = "Ciconia Studio/Effects/Highlight/BlinkHighlight";
        public const string transparentHighlightShaderPath = "Unlit/AL/TransparentHiglight";
        public const string correctTextColorStyle = "Correct";
        public const string errorTextColorStyle = "Error";
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
        NarrationSet narrationSet;
        [SerializeField]
        List<AssemblyComponent> assemblyComponents;
        [SerializeField]
        List<RawComponent> rawComponents;

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
        public bool AtHome { get { return atHome; } }

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

            if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
            {
                if (Coordinator.instance.modalWindow.gameObject.activeSelf)
                {
                    Coordinator.instance.modalWindow.Close();
                }
                else
                {
                    Coordinator.instance.modalWindow.Show(UI.WindowType.ERROR, "Hi, this is dummy result");

                }
            }

        }

        private void Reset()
        {
            gameplayStarted = false;
            currentStepIndex = 0;
            homePlayerPosition.Apply(Coordinator.instance.ovrPlayerController.transform);
            gameplayPlayerPosition = homePlayerPosition;
            assemblyObjects.SetActive(false);
            Coordinator.instance.audioManager.Reset();
            Coordinator.instance.modalWindow.Reset();
            ComponentReset();
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
            Reset();

            currentState = State.NONE;
            gameplayHomeMenu.SetActive(false);
            mainHomeMenu.SetActive(true);
        }

        public void TrainingReset()
        {
            Reset();
            ToggleHome(null);
        }
        
        public void AssessmentReset()
        {
            Reset();
            ToggleHome(null);
        }

        public void ComponentReset()
        {
            foreach (var item in assemblyComponents)
                item.Reset();
            foreach (var item in rawComponents)
                item.Reset();
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
                Coordinator.instance.audioManager.Queue(currentState == State.TRAINING ? "training_intro" : "assessment_intro");
                introDone = true;
            }
        }

        private IEnumerator ToggleHomeCoroutine(UnityAction onCompleteAction)
        {
;           atHome = !atHome;
            ovrScreenFade.FadeOut();

            yield return new WaitForSeconds(ovrScreenFade.fadeTime);

            if (atHome)
            {
                Coordinator.instance.audioManager.FadePause(currentState == State.TRAINING ? AudioManager.trainingBackgroundMusic : AudioManager.assessmentBackgroundMusic, ovrScreenFade.fadeTime);
                Coordinator.instance.audioManager.Pause(ovrScreenFade.fadeTime);
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
            Coordinator.instance.modalWindow.OnHomeToggle();

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
                    return narrationSet.RetrieveNarration(MultipleNarrationType.WRONG_TOOL);
                case Mistake.PART:
                    return narrationSet.RetrieveNarration(MultipleNarrationType.WRONG_PART);
                case Mistake.LOCATION:
                    return narrationSet.RetrieveNarration(MultipleNarrationType.WRONG_LOCATION);
                default:
                    return String.Empty;
            }
        }

        public string RetrieveNarration(MultipleNarrationType type)
        {
            return narrationSet.RetrieveNarration(type);
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
        }

        public void Finish()
        {
            float totalTimeTaken = 0;
            int totalNumberOfWrongAttemps = 0;

            foreach (var item in assemblySteps)
            {
                totalTimeTaken += item.TimeTaken;
                totalNumberOfWrongAttemps += item.WrongAttemptCount;
            }

            var result = "Time taken: " + (int)totalTimeTaken + " seconds\n" + "Wrong attempts: ".Style(errorTextColorStyle) + totalNumberOfWrongAttemps;
            Coordinator.instance.modalWindow.Show(UI.WindowType.RESULT, result);

            var finishNarration = RetrieveNarration(MultipleNarrationType.ASSEMBLY_FINISH);
            Coordinator.instance.audioManager.Queue(finishNarration);
        }

        public void OnPlacement(bool correct)
        {
            if (currentStepIndex < assemblySteps.Count && assemblySteps[currentStepIndex].StepType == StepType.PART_PLACEMENT)
            {
                assemblySteps[currentStepIndex].OnPlacement(correct);
                if (correct && assemblySteps[currentStepIndex].StepType == StepType.PART_PLACEMENT)
                {
                    assemblySteps[currentStepIndex].CompleteStep();
                    currentStepIndex++;
                    if (currentStepIndex == assemblySteps.Count)
                        Finish();
                }
            }
        }
       
        #endregion
    }
}
