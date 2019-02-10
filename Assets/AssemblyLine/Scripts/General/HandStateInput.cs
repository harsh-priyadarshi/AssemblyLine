using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL
{
    public class HandStateInput : MonoBehaviour
    {
        /// <summary>
        /// State with lower preference needs to have higher index
        /// </summary>
        public List<HandStateInputConfiguration> handStateInputs;

        public bool GetDown(HandState state, OVRInput.Controller controller)
        {
            var stateInput = handStateInputs.Find(item => item.HandState == state);
            return stateInput.GetOn(controller);
        }

        public bool GetUp(HandState state, OVRInput.Controller controller)
        {
            var stateInput = handStateInputs.Find(item => item.HandState == state);
            if (stateInput == null)
                return false;
            return stateInput.GetOff(controller);
        }

        public HandState CheckForGestureInput(OVRInput.Controller controller, bool resumeMode)
        {
            foreach (var item in handStateInputs)
            {
                if(resumeMode && item.Get(controller))
                    return item.HandState;
                else if (!resumeMode && item.GetOn(controller))
                    return item.HandState;
            }
            return HandState.NONE;
        }
    }
}
