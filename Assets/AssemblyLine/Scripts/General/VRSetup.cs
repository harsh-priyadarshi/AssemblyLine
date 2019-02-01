using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;


namespace AL
{
    public class VRSetup : MonoBehaviour
    {
        public Toggle platformToggle;
        public GameObject DesktopCam;
        public GameObject OculusCam;
        public List<GameObject> DesktopGO;
        public List<GameObject> OculusGO;
        public Toggle collabToggle;
        public GameObject headsetInstructionPanel;

        public GameObject meetingPanel;

        private void  Start()
        {
            SetDesktopMode();
        }

        //private void OnEnable()
        //{
        //    OVRManager.HMDMounted += HeadsetInstructionPanelOff;
        //    OVRManager.HMDUnmounted += HeadsetInstructionPanelOn;
        //}

        //private void OnDisable()
        //{
        //    OVRManager.HMDMounted -= HeadsetInstructionPanelOn;
        //    OVRManager.HMDUnmounted -= HeadsetInstructionPanelOff;
        //}
    

        void SetOculusMode()
        {
            collabToggle.interactable = true;
            UnityEngine.XR.XRSettings.enabled = true;
            XRSettings.LoadDeviceByName("Oculus");
            DesktopCam.SetActive(false);
            OculusCam.SetActive(true);
            for (int i = 0; i < DesktopGO.Count; i++)
            {
                DesktopGO[i].SetActive(false);
            }

            for (int i = 0; i < OculusGO.Count; i++)
            {
                OculusGO[i].SetActive(true);
            }
            headsetInstructionPanel.SetActive(true);
            //HapticHelper.instance.Init();
        }

        void SetDesktopMode()
        {
            meetingPanel.SetActive(false);
            UnityEngine.XR.XRSettings.enabled = false;
            DesktopCam.SetActive(true);
            OculusCam.SetActive(false);
            for (int i = 0; i < DesktopGO.Count; i++)
            {
                DesktopGO[i].SetActive(true);
            }

            for (int i = 0; i < OculusGO.Count; i++)
            {
                OculusGO[i].SetActive(false);
            }
          
            HeadsetInstructionPanelOff();
        }

        void HeadsetInstructionPanelOn()
        {
            headsetInstructionPanel.SetActive(true);
        }

        void HeadsetInstructionPanelOff()
        {
            headsetInstructionPanel.SetActive(false);
        }
     
    }

}