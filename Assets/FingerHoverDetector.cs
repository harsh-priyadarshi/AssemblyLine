using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AL.UI.VR
{
    public class FingerHoverDetector : MonoBehaviour
    {
        [SerializeField]
        private Selectable selectable;
        [SerializeField]
        private Collider selectableCollider, hoverDetectorCollider;

        public void OnTriggerEnter(Collider other)
        {
            hoverDetectorCollider.enabled = false;
            selectableCollider.enabled = true;
        }
    }
}
