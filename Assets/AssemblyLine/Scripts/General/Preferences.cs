﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL
{
    [Serializable]
    public struct Key
    {
        [SerializeField]
        OVRInput.Button button;
        [SerializeField]
        OVRInput.Controller controller;

        [SerializeField]
        public Key(OVRInput.Button _button, OVRInput.Controller _controller)
        {
            button = _button;
            controller = _controller;
        }

        public bool GetDown()
        {
            return OVRInput.GetDown(button, controller);
        }
    }

    [CreateAssetMenu(fileName = "NewALSetting", menuName = "AL Settings")]
    public class Preferences : ScriptableObject
    {
        [Header("Key Configuration")]
        public Key mainMenuKey = new Key(OVRInput.Button.One, OVRInput.Controller.LTouch);
        public Key gameplayStartKey = new Key(OVRInput.Button.One, OVRInput.Controller.RTouch);

        [Header("Button Interaction")]
        [Range(0,1)]
        public float buttonClickAnimationSpeed = .1f;
        [Range(0, 1)]
        public float buttonClickAnimationImpact = .1f;
        [Range(0, 1)]
        public float environmentSwitchDuration = .2f;

        [Header("Assembly")]
        [Range(0, 1)]
        public float assemblyTweenLength = .4f;

        [Header("Audio")]
        public float narrationMinimumGap = .4f;
    }
}
