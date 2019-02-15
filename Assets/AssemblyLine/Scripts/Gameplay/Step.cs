﻿using System.Collections;
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
        public static IAssemblyItem pickedupAssemblyItem;
        private float startTime, endTime;

        private static GameObject pickedUpTool = null;

        public StepType StepType { get { return type; } }

        public StepStatus Status { get { return status; } }

        public float TimeTaken { get { return endTime - startTime; } }

        public int WrongAttemptCount { get { return wrongAttemptCount; } }

        public GameObject CorrectPart {
            get { return correctPart.gameObject; }
            set
            {
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
            correctAssemblyItemSample.ShowUpForAssembly(type);
            correctPart.ShowUpForAssembly(type);
            if (!string.IsNullOrEmpty(narration))
                Coordinator.instance.audioManager.Play(narration);
        }

       

        public void InitiateStep()
        {
            Debug.Log("InitiateStep: "+ name);
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
                    if (obj.Equals(correctPickSample))
                        correctAssemblyItemSample.Highlight(HighlightType.NONE);
                    return true;
                }
            }
            else
            {
                if (obj.tag.Equals(correctPickSample.tag))
                {
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
