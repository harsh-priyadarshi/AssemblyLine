using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL.Gameplay
{
    public class RawComponent : OVRGrabbable, IResettable, IAssemblyItem
    {
        private bool highlighted = false;
        private GameObject hoveredObject;
        private bool pickedUpCorrectly = false;
        private bool hoveringOverCorrectTarget = false;
        CustomTransform originalTransform;

        private IEnumerator onCompleteEnumerator;

        public void Init()
        {
            originalTransform.Extract(transform);
        }

        private IEnumerator OnCompleteEnumerator(float tweenLength)
        {
            yield return new WaitForSeconds(tweenLength);
            AssemblyComplete();
        }

        private void AssemblyComplete()
        {
            gameObject.SetActive(false);
        }

        public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
        {
            base.GrabBegin(hand, grabPoint);
            pickedUpCorrectly = Coordinator.instance.appManager.OnObjectGrab(gameObject);

        }

        public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
        {
            base.GrabEnd(linearVelocity, angularVelocity);
            if (hoveredObject != null && hoveredObject.layer == 13)
                Coordinator.instance.appManager.OnPlacement(hoveredObject.tag.Equals(gameObject.tag));
            pickedUpCorrectly = false;
        }

        public void OnTriggerEnter(Collider other)
        {
            //print("OnTriggerEnter: " + other.name);
          
            hoveredObject = other.gameObject;
            if (hoveredObject.layer == 13 && pickedUpCorrectly)
            {
                hoveringOverCorrectTarget = Coordinator.instance.appManager.ValidateHover(gameObject, hoveredObject);
                Highlight( hoveringOverCorrectTarget ? HighlightType.GREEN : HighlightType.RED);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            hoveredObject = null;
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

        public void OnReset()
        {
            hoveredObject = null;
            Highlight(HighlightType.NONE);
            originalTransform.Apply(transform);
            gameObject.SetActive(true);
            pickedUpCorrectly = false;
            hoveringOverCorrectTarget = false;
        }

        public void AssemblyComplete(float tweenLength)
        {
            if (onCompleteEnumerator != null)
                StopCoroutine(onCompleteEnumerator);
            onCompleteEnumerator = OnCompleteEnumerator(tweenLength);
            StartCoroutine(onCompleteEnumerator);
            transform.DOMove(hoveredObject.transform.position, tweenLength);
            transform.DORotate(hoveredObject.transform.eulerAngles, tweenLength);
            pickedUpCorrectly = false;
            hoveringOverCorrectTarget = false;
            hoveredObject = null;
        }
    }
}
