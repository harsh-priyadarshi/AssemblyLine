﻿using AL.Theme;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AL.Audio;
using AL.UI;

namespace AL
{
    public class Coordinator : MonoBehaviour {

        public static Coordinator instance;

        [Header("Scripts")]
        public AppTheme appTheme;
        public AudioManager audioManager;
        public DesktopScreenController desktopScreenController;
        public HandStateInput handStateInput;

        private void Awake()
        {
            instance = this;
        }

    }
}