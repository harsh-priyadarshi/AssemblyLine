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

        public string name
        {
            get
            {
                return button.ToString();
            }
        }

        public bool Get(OVRInput.Controller controller)
        {
            return OVRInput.Get(button, controller) == activationBool;
        }

        public bool GetOn(OVRInput.Controller controller)
        {
            if (activationBool && OVRInput.GetDown(button, controller))
            {
                //Debug.Log(button.ToString() + " get on true");
                return true;
            }
            else if (!activationBool && OVRInput.GetUp(button, controller))
            {
                //Debug.Log(button.ToString() + " get on true");
                return true;
            }
            //Debug.Log(button.ToString() + " get on false");
            
            return false;   
        }

        public bool GetOff(OVRInput.Controller controller)
        {
            if (activationBool && OVRInput.GetUp(button, controller))
            {
                //Debug.Log(button.ToString() + " get off true");
                return true;
            }
            else if (!activationBool && OVRInput.GetDown(button, controller))
            {
                //Debug.Log(button.ToString() + " get off true");
                return true;
            }

            //Debug.Log(button.ToString() + " get off false");
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

        public string name
        {
            get
            {
                return nearTouchKey.ToString();
            }
        }

        public bool Get(OVRInput.Controller controller)
        {
            return OVRInput.Get(nearTouchKey, controller) == activationBool;
        }

        public bool GetOn(OVRInput.Controller controller)
        {
            if (activationBool && OVRInput.GetDown(nearTouchKey, controller))
            {
                //Debug.Log("NearTouchBasedGestureInput: " + nearTouchKey.ToString() + " true");
                return true;
            }
            else if (!activationBool && OVRInput.GetUp(nearTouchKey, controller))
            {
                //Debug.Log("NearTouchBasedGestureInput: " + nearTouchKey.ToString() + " true");
                return true;
            }

            //Debug.Log("NearTouchBasedGestureInput: " + nearTouchKey.ToString() + " false");

            return false;
        }

        public bool GetOff(OVRInput.Controller controller)
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
        private string name = "";
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

        public bool GetOn(OVRInput.Controller controller)
        {
            bool anyKeyJustDown = false;
            
            foreach (var item in buttonBasedGestureInputs)
            {
                if (!item.Get(controller))
                {
                    //Debug.Log("HandStateInputConfiguration: " + name + " get on: false");
                    return false;
                }

                if (!anyKeyJustDown)
                    anyKeyJustDown = item.GetOn(controller);
            }

            foreach (var item in nearTouchBasedGestureInputs)
            {
                if (!item.Get(controller))
                {
                    //Debug.Log("HandStateInputConfiguration: " + name + " get on: false");
                    return false;
                }
                if (!anyKeyJustDown)
                    anyKeyJustDown = item.GetOn(controller);
            }

            //Debug.Log("HandStateInputConfiguration: " + name + " get on: " + anyKeyJustDown.ToString());

            return anyKeyJustDown;
        }

        public bool GetOff(OVRInput.Controller controller)
        {
            bool anyKeyJustUp = false;

            foreach (var item in buttonBasedGestureInputs)
            {
                if (!item.Get(controller) && !item.GetOff(controller))
                {
                    Debug.Log("HandStateInputConfiguration: " + name + " get off: false");
                    return false;
                }
                if (!anyKeyJustUp)
                    anyKeyJustUp = item.GetOff(controller);
            }

            foreach (var item in nearTouchBasedGestureInputs)
            {
                if (!item.Get(controller) && !item.GetOff(controller))
                {
                    Debug.Log("HandStateInputConfiguration: " + name + " get off: false");
                    return false;
                }
                if (!anyKeyJustUp)
                    anyKeyJustUp = item.GetOff(controller);
            }

            //Debug.Log("HandStateInputConfiguration: " + name + " get off: " + anyKeyJustUp.ToString());

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
        HOLDING,
        IDLE
    }

    public enum HandStateCategory
    {
        IDLE,
        INPUT_BASED,
        ACTION_BASED
    }

    public class CustomHand : MonoBehaviour
    {
        private const string handOpenBoolString = "IsOpen";
        private const string handPointingBoolString = "IsPointing";
        private const string scrueGrabbingBoolString = "IsScrueGrabbing";
        private const string ballGrabbingBoolString = "IsBallGrabbing";
        private const string handHoldingBoolString = "IsHolding";
        private const string holdToPointingTrigger = "HoldToPointing";
        private const string pointingToHoldTrigger = "PointingToHold";
        private static float stateTransitionDuration = .125f;

        [SerializeField]
        private OVRInput.Controller hand;
        [SerializeField]
        private Animator animator;
        [SerializeField]

        private HandState currentState = HandState.IDLE;
        private HandStateCategory currentHandStateType = HandStateCategory.IDLE;


        private void Update()
        {
            if (currentHandStateType != HandStateCategory.ACTION_BASED)
                UpdateHandState();
        }

        private void UpdateHandState()
        {
            var newState = Coordinator.instance.handStateInput.CheckForNewInput(hand);

            if (newState != HandState.NONE)
            {
                if (currentState != HandState.IDLE)
                    StartCoroutine(ResetCurrentBoolian(currentState));
                animator.SetBool(StateToBoolianString(newState), true);
                if (currentState == HandState.HOLDING && newState == HandState.POINTING)
                    animator.SetTrigger(holdToPointingTrigger);
                else if (currentState == HandState.POINTING && newState == HandState.HOLDING)
                    animator.SetTrigger(pointingToHoldTrigger);

                //animator.SetBool(StateToBoolianString(currentState), false);
                UpdateCurrentState(newState, HandStateCategory.INPUT_BASED);
            }
            else if (currentHandStateType != HandStateCategory.IDLE && Coordinator.instance.handStateInput.GetUp(currentState, hand))
            {
                animator.SetBool(StateToBoolianString(currentState), false);
                UpdateCurrentState(HandState.IDLE, HandStateCategory.IDLE);
            }
        }

        IEnumerator ResetCurrentBoolian(HandState state)
        {
            yield return new WaitForSeconds(stateTransitionDuration+.1f);
            animator.SetBool(StateToBoolianString(state), false);
        }


        private void UpdateCurrentState(HandState _currentState, HandStateCategory _currentStateType)
        {
            currentState = _currentState;
            currentHandStateType = _currentStateType;
        }

        private string StateToBoolianString(HandState state)
        {
            switch (state)
            {
                case HandState.OPEN:
                    return handOpenBoolString;
                case HandState.POINTING:
                    return handPointingBoolString;
                case HandState.SCRUE_GRABBING:
                    return scrueGrabbingBoolString;
                case HandState.BALL_GRABBING:
                    return ballGrabbingBoolString;
                case HandState.HOLDING:
                    return handHoldingBoolString;
                default:
                    return string.Empty;
            }
        }
    }
}
