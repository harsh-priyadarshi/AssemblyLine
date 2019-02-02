using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;


namespace AL
{
    public class VRSetup : MonoBehaviour
    {
        public List<GameObject> DesktopGO;
        public List<GameObject> OculusGO;
        public GameObject headsetInstructionPanel;
        

        //private void  Start()
        //{
        //    SetDesktopMode();
        //}

        private void OnEnable()
        {
            OVRManager.HMDMounted += HeadsetInstructionPanelOff;
            OVRManager.HMDUnmounted += HeadsetInstructionPanelOn;
        }

        private void OnDisable()
        {
            OVRManager.HMDMounted -= HeadsetInstructionPanelOn;
            OVRManager.HMDUnmounted -= HeadsetInstructionPanelOff;
        }


        void SetOculusMode()
        {
            UnityEngine.XR.XRSettings.enabled = true;
            XRSettings.LoadDeviceByName("Oculus");
           
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
            UnityEngine.XR.XRSettings.enabled = false;
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
            print("HeadsetInstructionPanelOn");
            headsetInstructionPanel.SetActive(true);
            Coordinator.instance.audioManager.Pause("background");
        }

        void HeadsetInstructionPanelOff()
        {
            print("HeadsetInstructionPanelOff");
            headsetInstructionPanel.SetActive(false);
            Coordinator.instance.audioManager.Play("background");
        }

    }

}