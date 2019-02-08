using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    public class Step
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

        private IHighlightable correctHighlightableSample;

        private int wrongAttemptCount = 0;
        private StepStatus status = StepStatus.NOT_STARTED;
       
        private GameObject pickedUpObject;
        private IHighlightable pickedUpHighlightable;

        public StepType StepType { get { return type; } }

        private void InstructForStep()
        {
            correctHighlightableSample = correctPickSample.GetComponent<IHighlightable>();
            correctPart.WatchForAssembly();
            correctHighlightableSample.Highlight(HighlightType.BLILNK);
            if (!string.IsNullOrEmpty(narration))
                Coordinator.instance.audioManager.Play(narration);
        }

        public void InitiateStep()
        {
            status = StepStatus.ONGOING;
            InstructForStep();
        }

        public void CompleteStep()
        {
            correctPart.AssemblyComplete();
            pickedUpObject.SetActive(false);
            status = StepStatus.COMPLETE;
        }

        public void OnWrongAttempt(Mistake mistakeLevel)
        {
            var selectedNarration = Coordinator.instance.appManager.RetrieveNarration(mistakeLevel);
            Coordinator.instance.audioManager.Play(selectedNarration);
            wrongAttemptCount++;
        }

        public void OnPlacement(bool correctPlacement)
        {
            pickedUpHighlightable.Highlight(HighlightType.NONE);
            string placementNarration;
            if (correctPlacement)
                placementNarration = Coordinator.instance.appManager.RetrieveCorrectStepNarration();
            else
                placementNarration = Coordinator.instance.appManager.RetrieveNarration(Mistake.LOCATION);
            Coordinator.instance.audioManager.Play(placementNarration);
            if (correctPlacement && type == StepType.PART_PLACEMENT)
                CompleteStep();
        }

        public bool ValidatePickup(GameObject obj)
        {
            if (obj.tag.Equals(correctPickSample.tag))
            {
                if (obj.Equals(correctPickSample))
                    correctHighlightableSample.Highlight(HighlightType.NONE);
                pickedUpObject = obj;
                pickedUpHighlightable = pickedUpObject.GetComponent<IHighlightable>();
                return true;
            }
            OnWrongAttempt(type == StepType.PART_PLACEMENT ? Mistake.PART : Mistake.TOOL);
            return false;
        }

    }
}
