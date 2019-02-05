using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using AL.Audio;


namespace AL
{
    public class VRSetup : MonoBehaviour
    {
        public List<GameObject> DesktopGO;
        public List<GameObject> OculusGO;
        public GameObject headsetInstructionPanel;

        [SerializeField]
        OVRInputModule ovrInputModule;
        [SerializeField]
        StandaloneInputModule standAloneInputModule;
        [SerializeField]
        GameObject desktoCanvas;

        //private void  Start()
        //{
        //    SetDesktopMode();
        //}

        private void OnEnable()
        {
            OVRManager.HMDMounted += OnHMDMount;
            OVRManager.HMDUnmounted += OnHMDUnmount;
        }

        private void OnDisable()
        {
            OVRManager.HMDMounted -= OnHMDUnmount;
            OVRManager.HMDUnmounted -= OnHMDMount;
        }

        void OnHMDUnmount()
        {
            headsetInstructionPanel.SetActive(true);
            if (Coordinator.instance.appManager.CurrentState == State.NONE)
                Coordinator.instance.audioManager.Pause(AudioManager.backgroundMusic);
            standAloneInputModule.enabled = true;
            desktoCanvas.SetActive(true);
        }

        void OnHMDMount()
        {
            headsetInstructionPanel.SetActive(false);
            if (Coordinator.instance.appManager.CurrentState == State.NONE)
                Coordinator.instance.audioManager.Resume(AudioManager.backgroundMusic);
            standAloneInputModule.enabled = false;
            desktoCanvas.SetActive(false);
        }

    }

}