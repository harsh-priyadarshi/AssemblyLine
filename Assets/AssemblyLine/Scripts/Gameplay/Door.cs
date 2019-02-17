using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL.Gameplay
{
    public class Door : MonoBehaviour {

        [SerializeField]
        private Animator doorAnimator;

        private const string RotateTrigger = "Rotate";

        OVRInput.Controller handHovering = OVRInput.Controller.None;

        private void Update()
        {
            if ((handHovering == OVRInput.Controller.RTouch || handHovering == OVRInput.Controller.LTouch) && OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, handHovering) && Coordinator.instance.appManager.AssemblyFinished)
            {
                doorAnimator.SetTrigger(RotateTrigger);
                Coordinator.instance.appManager.Finish();
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == 11)
            {
                var ovrGrabber = other.gameObject.GetComponent<OVRGrabber>();
                if (ovrGrabber != null)
                    handHovering = ovrGrabber.Controller;
            }
        }


        public void OnTriggerExit(Collider other)
        {
            handHovering = OVRInput.Controller.None;
        }

        public void OnReset()
        {
            
        }
    }
}
