using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL.Gameplay
{
    public class RawComponent : OVRGrabbable, IResettable, IAssemblyItem
    {
        private GameObject hoveredObject;
        private bool pickedUpCorrectly = false;
        private bool hoveringOverCorrectTarget = false;
        private bool assemblyShowUp = false;
        CustomTransform originalTransform;
        private HighlightType highlightedType = HighlightType.NONE;

        private IEnumerator onCompleteEnumerator, onGrabEnumerator;

        public void Init()
        {
            originalTransform.Extract(transform);
        }

        private IEnumerator OnCompleteEnumerator(float tweenLength)
        {
            yield return new WaitForSeconds(tweenLength);
            onCompleteEnumerator = null;
            AssemblyComplete();
        }

        private IEnumerator OnGrabEnumerator()
        {
            Highlight(pickedUpCorrectly ? HighlightType.GREEN : HighlightType.YELLOW);
            if (!pickedUpCorrectly && AppManager.CurrentState == State.TRAINING)
                Coordinator.instance.modalWindow.Show(UI.WindowType.WARNING, "Wrong Object!");
            yield return new WaitForSeconds(Coordinator.instance.settings.SelectedPreferences.assemblyTweenLength);
            Highlight(HighlightType.NONE);
        }

        private void AssemblyComplete()
        {
            if (assemblyShowUp)
                ShowUpForAssembly(StepType.PART_PLACEMENT);
            else
            {
                hoveredObject = null;
                originalTransform.Apply(transform);
                gameObject.SetActive(false);
                pickedUpCorrectly = false;
                hoveringOverCorrectTarget = false;
                Highlight(HighlightType.NONE);
            }
        }

        public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
        {
            base.GrabBegin(hand, grabPoint);
            pickedUpCorrectly = Coordinator.instance.appManager.OnObjectGrab(gameObject);

            if (onGrabEnumerator != null)
                StopCoroutine(onGrabEnumerator);
            onGrabEnumerator = OnGrabEnumerator();
            StartCoroutine(onGrabEnumerator);

            if (pickedUpCorrectly)
                Step.pickedupAssemblyItem = this;
        }

        public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
        {
            base.GrabEnd(linearVelocity, angularVelocity);
            Step.pickedupAssemblyItem = null;
            if (pickedUpCorrectly && hoveredObject != null && hoveredObject.layer == 13)
                Coordinator.instance.appManager.OnPlacement(this, pickedUpCorrectly && hoveringOverCorrectTarget);
            pickedUpCorrectly = false;
        }

        public void OnTriggerEnter(Collider other)
        {
            print("OnTriggerEnter: " + other.name);

            hoveredObject = other.gameObject;
            if (hoveredObject.layer == 13 && pickedUpCorrectly)
            {
                hoveringOverCorrectTarget = Coordinator.instance.appManager.ValidateHover(StepType.PART_PLACEMENT, gameObject, hoveredObject);
                Highlight( hoveringOverCorrectTarget ? HighlightType.GREEN : HighlightType.RED);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            hoveredObject = null;
            if (highlightedType != HighlightType.BLILNK)
                Highlight(HighlightType.NONE);
        }

        public void Highlight(HighlightType type)
        {
            if (AppManager.CurrentState == State.ASSESSMENT)
                return;

            //Debug.Log("Highlight: " + type.ToString());

            switch (type)
            {
                case HighlightType.NONE:
                    if (highlightedType != HighlightType.NONE)
                        Coordinator.instance.appManager.ApplyShader(AppManager.normalShaderPath, gameObject);
                    break;
                case HighlightType.BLILNK:
                    Coordinator.instance.appManager.ApplyShader(true, AppManager.blinkHighlightShaderPath, gameObject, Coordinator.instance.appTheme.SelectedTheme.objectHighlightColor);
                    break;
                case HighlightType.RED:
                    Coordinator.instance.appManager.ApplyShader(true, AppManager.idleHighlightShaderPath, gameObject, Coordinator.instance.appTheme.SelectedTheme.redHighlight);
                    break;
                case HighlightType.GREEN:
                    Coordinator.instance.appManager.ApplyShader(true, AppManager.idleHighlightShaderPath, gameObject, Coordinator.instance.appTheme.SelectedTheme.greenHighlight);
                    break;
                case HighlightType.YELLOW:
                    Coordinator.instance.appManager.ApplyShader(true, AppManager.idleHighlightShaderPath, gameObject, Coordinator.instance.appTheme.SelectedTheme.yellowHighlight);
                    break;
                case HighlightType.TRANSPARENT:
                    Coordinator.instance.appManager.ApplyShader(AppManager.transparentHighlightShaderPath, gameObject);
                    break;
                default:
                    break;
            }
            highlightedType = type;
        }

        public void OnReset()
        {
            assemblyShowUp = false;
            hoveredObject = null;
            Highlight(HighlightType.NONE);
            originalTransform.Apply(transform);
            gameObject.SetActive(true);
            pickedUpCorrectly = false;
            hoveringOverCorrectTarget = false;
            if (onCompleteEnumerator != null)
                StopCoroutine(onCompleteEnumerator);
            if (onGrabEnumerator != null)
                StopCoroutine(onGrabEnumerator);
        }

        public void ShowUpForAssembly(StepType type)
        {
            if (onCompleteEnumerator == null)
            {
                //Debug.Log("ShowUpForAssembly: oncompleteEnumerator is null");
                hoveredObject = null;
                assemblyShowUp = true;
                Highlight(HighlightType.BLILNK);
                originalTransform.Apply(transform);
                gameObject.SetActive(true);
                pickedUpCorrectly = false;
                hoveringOverCorrectTarget = false;
                  
                if (onGrabEnumerator != null)
                    StopCoroutine(onGrabEnumerator);

                assemblyShowUp = false;
            }
            else
                assemblyShowUp = true;
        }

        public void AssemblyComplete(float tweenLength)
        {
            if (onCompleteEnumerator != null)
                StopCoroutine(onCompleteEnumerator);
            onCompleteEnumerator = OnCompleteEnumerator(tweenLength);
            StartCoroutine(onCompleteEnumerator);
            transform.DOMove(hoveredObject.transform.position, tweenLength);
            transform.DORotate(hoveredObject.transform.eulerAngles, tweenLength);
        }
    }
}
