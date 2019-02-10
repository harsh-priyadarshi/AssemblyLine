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

        private IAssemblyItem correctAssemblyItemSample;
        private int wrongAttemptCount = 0;
        private StepStatus status = StepStatus.NOT_STARTED;
        private GameObject pickedUpObject;
        private IAssemblyItem pickedupAssemblyItem;
        private float startTime, endTime;

        public StepType StepType { get { return type; } }

        public float TimeTaken { get { return endTime - startTime; } }

        public int WrongAttemptCount { get { return wrongAttemptCount; } }


        private void InstructForStep()
        {
            correctAssemblyItemSample = correctPickSample.GetComponent<IAssemblyItem>();
            correctPart.WatchForAssembly(type);
            correctAssemblyItemSample.Highlight(HighlightType.BLILNK);
            if (!string.IsNullOrEmpty(narration))
                Coordinator.instance.audioManager.Play(narration);
        }

        public void InitiateStep()
        {
            status = StepStatus.ONGOING;
            InstructForStep();
            startTime = Time.time;
        }

        public void CompleteStep()
        {
            var tweenLength = Coordinator.instance.settings.SelectedPreferences.assemblyTweenLength;
            correctPart.AssemblyComplete(tweenLength);
            pickedupAssemblyItem.AssemblyComplete(tweenLength);
            status = StepStatus.COMPLETE;
            endTime = Time.time;
        }

        public void OnWrongAttempt(Mistake mistakeLevel)
        {
            var selectedNarration = Coordinator.instance.appManager.RetrieveNarration(mistakeLevel);
            Coordinator.instance.audioManager.Interrupt(selectedNarration);
            wrongAttemptCount++;
        }

        public void OnPlacement(bool correctPlacement)
        {
            pickedupAssemblyItem.Highlight(HighlightType.NONE);
            string placementNarration;
            if (correctPlacement)
                placementNarration = Coordinator.instance.appManager.RetrieveNarration(MultipleNarrationType.CORRECT_STEP);
            else
                placementNarration = Coordinator.instance.appManager.RetrieveNarration(Mistake.LOCATION);
            Coordinator.instance.audioManager.Interrupt(placementNarration);
        }

        public bool ValidatePickup(GameObject obj)
        {
            if (obj.tag.Equals(correctPickSample.tag))
            {
                if (obj.Equals(correctPickSample))
                    correctAssemblyItemSample.Highlight(HighlightType.NONE);
                pickedUpObject = obj;
                pickedupAssemblyItem = pickedUpObject.GetComponent<IAssemblyItem>();
                return true;
            }
            OnWrongAttempt(type == StepType.PART_PLACEMENT ? Mistake.PART : Mistake.TOOL);
            return false;
        }

        public void Reset()
        {
            wrongAttemptCount = 0;
            status = StepStatus.NOT_STARTED;
            pickedUpObject = null;
            pickedupAssemblyItem = null;
        }
    }
}
