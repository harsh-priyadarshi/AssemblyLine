using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AL.Audio;

namespace AL
{
    public enum State
    {
        NONE,
        TRAINING,
        ASSESSMENT
    }
    public class AppManager : MonoBehaviour {
        [SerializeField]
        Material homeSkybox, gameplaySkybox;
        [SerializeField]
        GameObject homeCanvas;

        private State currentState = State.NONE;
        private bool atHome = true;


        private void Update()
        {
            if (Coordinator.instance.settings.SelectedPreferences.mainMenuKey.GetDown())
            {
                print("ToggleGameplay");
                ToggleGameplay();
            }
        }

        public void InitTraining()
        {
            currentState = State.TRAINING;
            ToggleHome();
        }

        public void InitAssessment()
        {
            currentState = State.ASSESSMENT;
            ToggleHome();
        }

        public void ToggleGameplay()
        {
            if (currentState != State.NONE)
            {
                ToggleHome();
            }
        }

        private void ToggleHome()
        {
            RenderSettings.skybox = atHome ? gameplaySkybox : homeSkybox;
            homeCanvas.SetActive(!atHome);
            if (atHome)
                Coordinator.instance.audioManager.Pause(AudioManager.backgroundMusic);
            else
                Coordinator.instance.audioManager.Play(AudioManager.backgroundMusic);
            Coordinator.instance.ovrPlayerController.SetHaltUpdateMovement(atHome);
            atHome = !atHome;
        }
    }
}
