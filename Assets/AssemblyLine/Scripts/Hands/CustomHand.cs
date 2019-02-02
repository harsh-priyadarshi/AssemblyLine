using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL
{
    [Serializable]
    public struct ButtonBasedGestureInput
    {
        [SerializeField]
        private OVRInput.Button button;
        [SerializeField]
        private bool activationBool;

        public bool Get(OVRInput.Controller controller)
        {
            return OVRInput.Get(button, controller) == activationBool;
        }

        public bool GetDown(OVRInput.Controller controller)
        {
            if (activationBool && OVRInput.GetDown(button, controller))
                return true;
            else if (!activationBool && OVRInput.GetUp(button))
                return true;
            return false;   
        }

        public bool GetUp(OVRInput.Controller controller)
        {
            if (activationBool && OVRInput.GetUp(button, controller))
                return true;
            else if (!activationBool && OVRInput.GetDown(button, controller))
                return true;
            return false;
        }
    }

    [Serializable]
    public struct NearTouchBasedGestureInput
    {
        [SerializeField]
        private OVRInput.NearTouch nearTouchKey;
        [SerializeField]
        private bool activationBool;

        public bool Get(OVRInput.Controller controller)
        {
            return OVRInput.Get(nearTouchKey, controller) == activationBool;
        }

        public bool GetDown(OVRInput.Controller controller)
        {
            if (activationBool && OVRInput.GetDown(nearTouchKey, controller))
                return true;
            else if (!activationBool && OVRInput.GetUp(nearTouchKey, controller))
                return true;
            return false;
        }

        public bool GetUp(OVRInput.Controller controller)
        {
            if (activationBool && OVRInput.GetUp(nearTouchKey, controller))
                return true;
            else if (!activationBool && OVRInput.GetDown(nearTouchKey, controller))
                return true;
            return false;
        }
    }

    [Serializable]
    public class HandStateInputConfiguration
    {
        [SerializeField]
        private List<ButtonBasedGestureInput> buttonBasedGestureInputs;
        [SerializeField]
        public List<NearTouchBasedGestureInput> nearTouchBasedGestureInputs;
        [SerializeField]
        HandState handState;

        public HandState HandState
        {
            get
            {
                return handState;
            }
        }

        public bool GetDown(OVRInput.Controller controller)
        {
            bool anyKeyJustDown = false;
            
            foreach (var item in buttonBasedGestureInputs)
            {
                if (!item.Get(controller))
                    return false;
                anyKeyJustDown = item.GetDown(controller);
            }


            foreach (var item in nearTouchBasedGestureInputs)
            {
                if (item.Get(controller))
                    return false;
                anyKeyJustDown = item.GetDown(controller);
            }

            return anyKeyJustDown;
        }

        public bool GetUp(OVRInput.Controller controller)
        {
            bool anyKeyJustUp = false;

            foreach (var item in buttonBasedGestureInputs)
            {
                if (!item.Get(controller))
                    return false;
                anyKeyJustUp = item.GetUp(controller);
            }


            foreach (var item in nearTouchBasedGestureInputs)
            {
                if (item.Get(controller))
                    return false;
                anyKeyJustUp = item.GetUp(controller);
            }

            return anyKeyJustUp;
        }
    }

    public enum HandState
    {
        NONE,
        OPEN,
        POINTING,
        SCRUE_GRABBING,
        BALL_GRABBING,
        IDLE
    }

    public class CustomHand : MonoBehaviour
    {
        public const string handOpenBoolString = "IsOpen";
        public const string handPointingBoolString = "IsPointing";
        public const string scrueGrabbingBoolString = "IsScrueGrabbing";
        public const string ballGrabbingBoolString = "IsBallGrabbing";

        [SerializeField]
        private OVRInput.Controller hand;
        [SerializeField]
        private Animator animator;
        [SerializeField]

        private HandState currentState = HandState.IDLE;
        private string currentStateBoolString = "";


        private void Update()
        {
            UpdateHandState();
        }

        private void UpdateHandState()
        {
            var newStateBoolString = "";
            var newState = Coordinator.instance.handStateInput.CheckForNewInput(out newStateBoolString, hand);

            if (newState != HandState.NONE)
                animator.SetBool(newStateBoolString, true);
            else if (currentState != HandState.IDLE && Coordinator.instance.handStateInput.GetUp(newState, hand))
                animator.SetBool(currentStateBoolString, false);
        }
    }
}
