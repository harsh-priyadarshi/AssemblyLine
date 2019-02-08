using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL.Gameplay
{
    public class RawComponent : OVRGrabbable, IHighlightable
    {
        private bool highlighted = false;
        private GameObject hovereObject;

        public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
        {
            base.GrabBegin(hand, grabPoint);
            Coordinator.instance.appManager.OnObjectGrab(gameObject);

        }

        public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
        {
            base.GrabEnd(linearVelocity, angularVelocity);
            if (hovereObject != null && hovereObject.layer == 13)
                Coordinator.instance.appManager.OnPlacement(hovereObject.tag.Equals(gameObject.tag));
        }

        public void OnTriggerEnter(Collider other)
        {
            hovereObject = other.gameObject;
            if (hovereObject.layer == 13)
                Highlight(hovereObject.tag.Equals(gameObject.tag) ? HighlightType.GREEN : HighlightType.RED);
        }

        public void OnTriggerExit(Collider other)
        {
            hovereObject = null;
            Highlight(HighlightType.NONE);
        }

        public void Highlight(HighlightType type)
        {
            switch (type)
            {
                case HighlightType.NONE:
                    if (highlighted)
                    {
                        Coordinator.instance.appManager.ApplyShader(AppManager.normalShaderPath, gameObject);
                        highlighted = false;
                    }
                    break;
                case HighlightType.BLILNK:
                    Coordinator.instance.appManager.ApplyShader(true, AppManager.blinkHighlightShaderPath, gameObject, Coordinator.instance.appTheme.SelectedTheme.objectHighlightColor);
                    highlighted = true;
                    break;
                case HighlightType.RED:
                    Coordinator.instance.appManager.ApplyShader(true, AppManager.idleHighlightShaderPath, gameObject, Coordinator.instance.appTheme.SelectedTheme.redHighlight);
                    highlighted = true;
                    break;
                case HighlightType.GREEN:
                    Coordinator.instance.appManager.ApplyShader(true, AppManager.idleHighlightShaderPath, gameObject, Coordinator.instance.appTheme.SelectedTheme.greenHighlight);
                    highlighted = true;
                    break;
                case HighlightType.TRANSPARENT:
                    Coordinator.instance.appManager.ApplyShader(AppManager.transparentHighlightShaderPath, gameObject);
                    highlighted = true;
                    break;
                default:
                    break;
            }
        }

    }
}
