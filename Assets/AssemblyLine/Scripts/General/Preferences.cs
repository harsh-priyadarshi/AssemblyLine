using System;
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
        #region KeyConfiguration
        public Key mainMenuKey = new Key(OVRInput.Button.One, OVRInput.Controller.LTouch);
        #endregion
    }
}
