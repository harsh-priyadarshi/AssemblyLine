using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AL.Audio;
using AL.Gameplay;
using System;
using UnityEngine.Events;
using System.Linq;
using DG.Tweening;

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
        public const string assemblyInitiationTriggerString = "InitiateAssembly";


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
        private GameObject trainingEnvironment, assessmentEnvironment, assemblyObjects, dynamicAssemblyObjects, BIW, doorHanger;
        [SerializeField]
        NarrationSet narrationSet;
        [SerializeField]
        List<AssemblyComponent> assemblyComponents;
        [SerializeField]
        List<RawComponent> rawComponents;
        [SerializeField]
        List<DynamicObject> resetttableTransorms;
        [SerializeField]
        List<AssemblyTool> grabbableTools;

        [Header("Anchors")]
        [SerializeField]
        private Transform BIWAnchor1;
      
        [SerializeField]
        private Transform hangerAnchor1;
        [SerializeField]
        private Transform hangerAnchor2;

        private GameObject gameplayEnvironment;
        private GameObject gameplayHomeMenu;
        private  static State currentState = State.NONE;
        private bool atHome = true;
        private CustomTransform homePlayerPosition, gameplayPlayerPosition;
        private IEnumerator toggleHomeCoroutine = null;
        private bool gameplayStarted = false;
        private int currentStepIndex = 0;
        private bool resultReady = false;
        private bool introDone = false;
        private Tweener assemblyInitiator;

        public static State CurrentState { get { return currentState; } }
        public bool AtHome { get { return atHome; } }

        private void Start()
        {
            Init();
        }

        private void Update()
        {
            if (Coordinator.instance.settings.SelectedPreferences.mainMenuKey.GetDown())
                ToggleGameplay();

            if (Coordinator.instance.settings.SelectedPreferences.gameplayStartKey.GetDown() && currentState != State.NONE && !gameplayStarted && !atHome)
            {
                InitiateAssembly();
            }


            if (Coordinator.instance.settings.SelectedPreferences.handMenuKey.GetDown() && resultReady)
                ToggleResultPanel();

            //if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
            //{
            //    if (Coordinator.instance.modalWindow.gameObject.activeSelf)
            //    {
            //        Coordinator.instance.modalWindow.Close();
            //    }
            //    else
            //    {
            //        Coordinator.instance.modalWindow.Show(UI.WindowType.ERROR, "Hi, this is dummy result");

            //    }
            //}

        }

        private void Init()
        {
            if (Coordinator.instance == null)
            {
                print("Coordinator null");
            }
            Coordinator.instance.ovrPlayerController.SetHaltUpdateMovement(true);
            homePlayerPosition.Extract(Coordinator.instance.ovrPlayerController.transform);
            gameplayPlayerPosition.Extract(Coordinator.instance.ovrPlayerController.transform);

            foreach (var item in grabbableTools)
                item.Init();
            foreach (var item in resetttableTransorms)
                item.Init();
            foreach (var item in rawComponents)
                item.Init();
            foreach (var item in assemblyComponents)
                item.Init();
        }

        private void OnReset()
        {
            if (assemblyInitiator != null)
            {
                assemblyInitiator.Kill();
                assemblyInitiator = null;
            }

            resultReady = false;
            BIW.SetActive(false);
            doorHanger.SetActive(false);
            dynamicAssemblyObjects.SetActive(false);
            gameplayStarted = false;
            currentStepIndex = 0;
            homePlayerPosition.Apply(Coordinator.instance.ovrPlayerController.transform);
            gameplayPlayerPosition = homePlayerPosition;
            assemblyObjects.SetActive(false);
            Coordinator.instance.audioManager.OnReset();
            Coordinator.instance.modalWindow.OnReset();
            ComponentReset();
           
            foreach (var item in resetttableTransorms)
                item.OnReset();
            foreach (var item in grabbableTools)
                item.OnReset();

            Coordinator.instance.databaseManager.SaveUserActivity(assemblySteps);
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
            gameplayHomeMenu = trainingHomeMenu;
            gameplayEnvironment = trainingEnvironment;
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
            OnReset();

            currentState = State.NONE;
            gameplayHomeMenu.SetActive(false);
            mainHomeMenu.SetActive(true);
        }

        public void TrainingReset()
        {
            OnReset();
            ToggleHome(null);
        }

        public void AssessmentReset()
        {
            OnReset();
            ToggleHome(null);
        }

        public void ComponentReset()
        {
            foreach (var item in assemblyComponents)
                item.OnReset();
            foreach (var item in rawComponents)
                item.OnReset();
          
        }

        public void OnLoginToggle(bool val)
        {
            if (!val)
            {
                ToggleHome(() => {
                    GameplayQuit();
                    vrTitleText.SetActive(val);
                    vrLoginInstructionText.SetActive(!val);
                    mainHomeMenu.SetActive(val);
                    introDone = false;
                });
            }
            else
            {
                vrTitleText.SetActive(val);
                vrLoginInstructionText.SetActive(!val);
                if (currentState == State.NONE)
                    mainHomeMenu.SetActive(val);
                else
                    gameplayHomeMenu.SetActive(val);
            }
        }

        #region GAMEPLAY
        private void InitiateNextStep()
        {
            print("InitiateNextStep");
            if (assemblySteps.Count > currentStepIndex)
                assemblySteps[currentStepIndex].InitiateStep();
        }

        private void GiveIntro()
        {
            if (!introDone && !gameplayStarted && currentState == State.TRAINING)
            {
                Coordinator.instance.audioManager.Queue(currentState == State.TRAINING ? "training_intro" : "assessment_intro");
                introDone = true;
            }
        }

        private void InitiateAssembly()
        {
            dynamicAssemblyObjects.SetActive(true);
            gameplayStarted = true;
            BIW.SetActive(true);
            assemblyInitiator = BIW.transform.DOMove(BIWAnchor1.position, 11f).OnComplete(
                () =>
                {
                    doorHanger.SetActive(true);
                    assemblyInitiator = doorHanger.transform.DOMove(hangerAnchor1.position, 5).OnComplete(
                    () =>
                    {
                        assemblyInitiator = doorHanger.transform.DOMove(hangerAnchor2.position, 3).OnComplete(() =>
                        {
                            assemblyInitiator = null;
                            InitiateNextStep();
                        }
                        );
                    }
                    );
                }
                );

            Coordinator.instance.audioManager.Play(AudioManager.machineAudio);
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
            if (assemblyInitiator != null)
                assemblyInitiator.TogglePause();

            if (atHome)
                Coordinator.instance.audioManager.Resume(AudioManager.homeBackgroundMusic, ovrScreenFade.fadeTime);
            else
            {
                Coordinator.instance.audioManager.Resume(currentState == State.TRAINING ? AudioManager.trainingBackgroundMusic : AudioManager.assessmentBackgroundMusic);
                Coordinator.instance.audioManager.Resume(ovrScreenFade.fadeTime);
            }
            gameplayHomeMenu.SetActive(atHome);
            gameplayEnvironment.SetActive(!atHome);
            assemblyObjects.SetActive(!atHome);

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

        private bool InterchangeTarget(GameObject hoveredObj)
        {
            var stepInvolvingHoveredObj = assemblySteps.Find(item => item.CorrectPart.Equals(hoveredObj) && item.StepType == assemblySteps[currentStepIndex].StepType);

            if (stepInvolvingHoveredObj == null || stepInvolvingHoveredObj.Status != StepStatus.NOT_STARTED)
                return false;
            else
            {
                stepInvolvingHoveredObj.CorrectPart = assemblySteps[currentStepIndex].CorrectPart;
                assemblySteps[currentStepIndex].CorrectPart = hoveredObj;
                return true;
            }
        }

        private void ToggleResultPanel()
        {
            if (Coordinator.instance.modalWindow.gameObject.activeSelf)
                Coordinator.instance.modalWindow.Close();
            else
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

        public bool OnObjectGrab(GameObject obj)
        {
            //print("OnObjectGrab");
            if (currentStepIndex < assemblySteps.Count && assemblySteps[currentStepIndex].StepType == StepType.PART_PLACEMENT)
                return assemblySteps[currentStepIndex].ValidatePickup(obj);
            else
                return false;
        }

        public bool ValidateHover(StepType relatedStepType, GameObject obj, GameObject hoveredObject)
        {
            if (currentStepIndex >= assemblySteps.Count)
                return false;

            if (!obj.tag.Equals(hoveredObject.tag) || relatedStepType != assemblySteps[currentStepIndex].StepType)
                return false;
            else if (assemblySteps[currentStepIndex].ValidateHover(hoveredObject))
                return true;
            else
                return InterchangeTarget(hoveredObject);
        }

        public bool OnToolGrab(GameObject obj)
        {
            if (currentStepIndex < assemblySteps.Count && assemblySteps[currentStepIndex].StepType == StepType.PART_INSTALLATION)
                return assemblySteps[currentStepIndex].ValidatePickup(obj);
            else
                return false;
        }

        public void Finish()
        {
            resultReady = true;
            var finishNarration = RetrieveNarration(MultipleNarrationType.ASSEMBLY_FINISH);
            Coordinator.instance.audioManager.Queue(finishNarration);
        }

        public void OnPlacement(IAssemblyItem pickedUpItem, bool correct)
        {
            if (currentStepIndex < assemblySteps.Count)
            {
                assemblySteps[currentStepIndex].OnPlacement(pickedUpItem, correct);
                if (correct)
                {
                    assemblySteps[currentStepIndex].CompleteStep(pickedUpItem);
                    currentStepIndex++;
                    if (currentStepIndex == assemblySteps.Count)
                        Finish();
                    else
                        Invoke("InitiateNextStep", 2 * Coordinator.instance.settings.SelectedPreferences.assemblyTweenLength);
                }
                else
                    assemblySteps[currentStepIndex].OnWrongAttempt(Mistake.LOCATION);
            }
        }

        public void OnToolRun(IAssemblyItem pickedUpItem, bool correct)
        {
            if (currentStepIndex < assemblySteps.Count)
            {
                assemblySteps[currentStepIndex].OnToolRun(pickedUpItem, correct);
                if (correct)
                {
                    assemblySteps[currentStepIndex].CompleteStep(pickedUpItem);
                    currentStepIndex++;
                    if (currentStepIndex == assemblySteps.Count)
                        Finish();
                    else
                        Invoke("InitiateNextStep", Coordinator.instance.settings.SelectedPreferences.assemblyTweenLength);
                }
                else
                    assemblySteps[currentStepIndex].OnWrongAttempt(Mistake.LOCATION);
            }
        }

        #endregion
    }
}
