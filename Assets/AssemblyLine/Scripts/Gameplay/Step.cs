using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

namespace AL.Gameplay
{
    public enum StepStatus
    {
        NOT_STARTED,
        ONGOING,
        COMPLETE
    }

    public enum Mistake
    {
        TOOL,
        PART,
        LOCATION
    }

    public enum StepType
    {
        PART_PLACEMENT,
        PART_INSTALLATION
    }

    [Serializable]
    public class Step : IResettable
    {
        [SerializeField]
        private string name;
        [SerializeField]
        private AssemblyComponent correctPart;
        [SerializeField]
        private GameObject correctPickSample;
        
        [SerializeField]
        private StepType type;
        [SerializeField]
        private string narration;
        [SerializeField]
        private string instruction;

        private IAssemblyItem correctAssemblyItemSample;
        private int wrongAttemptCount = 0;
        private StepStatus status = StepStatus.NOT_STARTED;
        public static IAssemblyItem pickedupAssemblyItem;
        private float startTime, endTime;

        private static GameObject pickedUpTool = null;

        public StepType StepType { get { return type; } }
        public string Name { get { return name; } }
        public StepStatus Status { get { return status; } }
        public float TimeTaken { get { return endTime - startTime; } }
        public int WrongAttemptCount { get { return wrongAttemptCount; } }
        public string Instruction { get { return instruction; } }


        public GameObject CorrectPart {
            get { return correctPart.gameObject; }
            set
            {
                //Debug.Log("Replacing target: " + value.name + "for: " + name + " " + status.ToString());
                correctPart = value.GetComponent<AssemblyComponent>();
                if (status == StepStatus.ONGOING)
                    correctPart.ShowUpForAssembly(type);
                else
                    correctPart.StopWatchingForAssembly();
            }
        }

        private void InstructForStep()
        {
            //Debug.Log("InstructForStep: " + name);
            correctAssemblyItemSample = correctPickSample.GetComponent<IAssemblyItem>();
            if (pickedupAssemblyItem != null)
            {
                if (pickedupAssemblyItem is RawComponent && StepType == StepType.PART_PLACEMENT)
                {
                    RawComponent pickedUpRaw = (RawComponent)pickedupAssemblyItem;
                    if (!pickedUpRaw.tag.Equals(correctPart.tag))
                        correctAssemblyItemSample.ShowUpForAssembly(type);

                }
                else if (pickedupAssemblyItem is AssemblyTool && StepType == StepType.PART_INSTALLATION)
                {
                    AssemblyTool tool = (AssemblyTool)pickedupAssemblyItem;
                    if (!tool.tag.Equals(correctPart.tag))
                        correctAssemblyItemSample.ShowUpForAssembly(type);
                }
                else
                    correctAssemblyItemSample.ShowUpForAssembly(type);
            }
            else
            {
                correctAssemblyItemSample.ShowUpForAssembly(type);
            }

            correctPart.ShowUpForAssembly(type);
            if (!string.IsNullOrEmpty(narration) && AppManager.CurrentState != State.ASSESSMENT)
                Coordinator.instance.audioManager.Play(narration);
        }

       

        public void InitiateStep()
        {
            //Debug.Log("InitiateStep: "+ name);
            status = StepStatus.ONGOING;
            InstructForStep();
            startTime = Time.time;
        }

        public void CompleteStep(IAssemblyItem pickedUpItem)
        {
            var tweenLength = Coordinator.instance.settings.SelectedPreferences.assemblyTweenLength;
            correctPart.AssemblyComplete(tweenLength);
            pickedUpItem.AssemblyComplete(tweenLength);
            status = StepStatus.COMPLETE;
            endTime = Time.time;
        }

        public void OnWrongAttempt(Mistake mistakeLevel)
        {
            Coordinator.instance.modalWindow.Show(UI.WindowType.ERROR, "Wrong Attempt!");
            if (AppManager.CurrentState == State.TRAINING)
            {
                var selectedNarration = Coordinator.instance.appManager.RetrieveNarration(mistakeLevel);
                Coordinator.instance.audioManager.Interrupt(selectedNarration);
            }
            wrongAttemptCount++;
        }

        public void OnPlacement(IAssemblyItem pickedUpItem, bool correctPlacement)
        {
            pickedUpItem.Highlight(HighlightType.NONE);
            if(AppManager.CurrentState == State.TRAINING)
            { 
                string placementNarration;
                if (correctPlacement)
                    placementNarration = Coordinator.instance.appManager.RetrieveNarration(MultipleNarrationType.CORRECT_STEP);
                else
                    placementNarration = Coordinator.instance.appManager.RetrieveNarration(Mistake.LOCATION);
                Coordinator.instance.audioManager.Interrupt(placementNarration);
            }
            if (!correctPlacement)
                Coordinator.instance.modalWindow.Show(UI.WindowType.ERROR, "Wrong Attempt!");
        }

        public void OnToolRun(IAssemblyItem pickedUpItem, bool correctOperation)
        {
            pickedUpItem.Highlight(HighlightType.NONE);
            if (AppManager.CurrentState == State.TRAINING)
            {
                string toolRunNarration;
                if (correctOperation)
                    toolRunNarration = Coordinator.instance.appManager.RetrieveNarration(MultipleNarrationType.CORRECT_STEP);
                else
                {
                    toolRunNarration = Coordinator.instance.appManager.RetrieveNarration(Mistake.LOCATION);
                }
                Coordinator.instance.audioManager.Interrupt(toolRunNarration);
            }
            if (!correctOperation)
                Coordinator.instance.modalWindow.Show(UI.WindowType.ERROR, "Wrong Attempt!");
        }

        public bool ValidateHover(GameObject hoveredObject)
        {
            //Debug.Log("ValidateHover: " + hoveredObject.name + " " + correctPart.gameObject.name);
            return hoveredObject != null && hoveredObject.Equals(correctPart.gameObject);
        }

        public bool ValidatePickup(GameObject obj)
        {
            if (type == StepType.PART_PLACEMENT)
            {
                if (obj.tag.Equals(correctPickSample.tag))
                {
                    if (!obj.Equals(correctPickSample))
                        correctAssemblyItemSample.Highlight(HighlightType.NONE);
                    return true;
                }
            }
            else
            {
                if (obj.tag.Equals(correctPickSample.tag))
                {
                    if (!obj.Equals(correctPickSample))
                        correctAssemblyItemSample.Highlight(HighlightType.NONE);
                    return true;
                }
            }

            return false;
        }

        public void OnReset()
        {
            Debug.Log("OnReset: " + name);
            wrongAttemptCount = 0;
            status = StepStatus.NOT_STARTED;
        }
    }
}
