using AL.Theme;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AL.Audio;
using AL.UI;
using AL.Database;

namespace AL
{
    public class Coordinator : MonoBehaviour {

        public static Coordinator instance;

        [Header("Scripts")]
        public AppTheme appTheme;
        public AudioManager audioManager;
        public DesktopScreenController desktopScreenController;
        public HandStateInput handStateInput;
        public Settings settings;
        public OVRPlayerController ovrPlayerController;
        public AppManager appManager;
        public ModalWindow modalWindow;
        public DatabaseManager databaseManager;

        [Header("Objects")]
        public CustomHand leftHand;
        public CustomHand rightHand;


        private void Awake()
        {
            instance = this;
        }

    }
}
