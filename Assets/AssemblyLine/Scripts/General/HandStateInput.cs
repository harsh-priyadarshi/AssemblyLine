using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL
{
    public class HandStateInput : MonoBehaviour
    {
        public HandStateInputConfiguration openStateInput, pointingStateInput, scrueGrabbingStateInput, ballGrabbingStateInput;

        public bool GetDown(HandState state, OVRInput.Controller controller)
        {
            if (state == ballGrabbingStateInput.HandState)
                return ballGrabbingStateInput.GetDown(controller);
            else if (state == scrueGrabbingStateInput.HandState)
                return scrueGrabbingStateInput.GetDown(controller);
            else if (state == openStateInput.HandState)
                return openStateInput.GetDown(controller);
            else if (state == pointingStateInput.HandState)
                return pointingStateInput.GetDown(controller);
            return false;
        }

        public bool GetUp(HandState state, OVRInput.Controller controller)
        {
            if (state == ballGrabbingStateInput.HandState)
                return ballGrabbingStateInput.GetUp(controller);
            else if (state == scrueGrabbingStateInput.HandState)
                return scrueGrabbingStateInput.GetUp(controller);
            else if (state == openStateInput.HandState)
                return openStateInput.GetUp(controller);
            else if (state == pointingStateInput.HandState)
                return pointingStateInput.GetUp(controller);
            return false;
        }

        public HandState CheckForNewInput(out string boolianString, OVRInput.Controller controller)
        {
            if (ballGrabbingStateInput.GetDown(controller))
            {
                boolianString = CustomHand.ballGrabbingBoolString;
                return HandState.BALL_GRABBING;
            }
            else if (scrueGrabbingStateInput.GetDown(controller))
            {
                boolianString = CustomHand.scrueGrabbingBoolString;
                return HandState.SCRUE_GRABBING;
            }
            else if (pointingStateInput.GetDown(controller))
            {
                boolianString = CustomHand.handPointingBoolString;
                return HandState.POINTING;
            }
            else if (openStateInput.GetDown(controller))
            {
                boolianString = CustomHand.handOpenBoolString;
                return HandState.OPEN;
            }
            boolianString = string.Empty;
            return HandState.NONE;
        }
    }
}
