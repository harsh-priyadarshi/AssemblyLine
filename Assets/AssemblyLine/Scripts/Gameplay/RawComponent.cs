using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL.Gameplay
{
    public class RawComponent : OVRGrabbable, IResettable, IAssemblyItem
    {
        private bool highlighted = false;
        private GameObject hovereObject;

        CustomTransform originalTransform;

        private IEnumerator onCompleteEnumerator;

        protected override void Start()
        {
            base.Start();
            Init();
        }

        private void Init()
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

        public void OnReset()
        {
            hovereObject = null;
            Highlight(HighlightType.NONE);
            originalTransform.Apply(transform);
            gameObject.SetActive(true);
        }

        public void AssemblyComplete(float tweenLength)
        {
            if (onCompleteEnumerator != null)
                StopCoroutine(onCompleteEnumerator);
            onCompleteEnumerator = OnCompleteEnumerator(tweenLength);
            StartCoroutine(onCompleteEnumerator);
            transform.DOMove(hovereObject.transform.position, tweenLength);
            transform.DORotate(hovereObject.transform.eulerAngles, tweenLength);
        }
    }
}
