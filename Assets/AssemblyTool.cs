using AL.Audio;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL.Gameplay
{
    /// <summary>
    /// Tool run is hard coded for spanner functionality
    /// </summary>
    public class AssemblyTool : OVRGrabbable, IResettable, IAssemblyItem
    {
        private GameObject hoveredObject;
        private bool pickedUpCorrectly = false;
        private bool hoveringOverCorrectTarget = false;
        CustomTransform originalTransform;
        HighlightType highlightedType = HighlightType.NONE;
        private bool assemblyShowUp = false;

        [SerializeField]
        private Transform fastener;


        private IEnumerator onCompleteEnumerator, onGrabEnumerator, toolEnumerator;

        void Update()
        {
            if (pickedUpCorrectly && hoveredObject != null && hoveredObject.layer == 13 && Coordinator.instance.settings.SelectedPreferences.toolStartKey.GetDown())
            {
                Coordinator.instance.appManager.OnToolRun(this, hoveringOverCorrectTarget);
                Coordinator.instance.audioManager.Play(AudioManager.wrench);
            }

            //if (Coordinator.instance.settings.SelectedPreferences.toolStartKey.GetDown())
            //{
            //    RunTool();
            //    Coordinator.instance.audioManager.Play(AudioManager.wrench);
            //}
        }

        private void RunTool()
        {
            if (fastener == null)
                return;

            if (toolEnumerator != null)
                StopCoroutine(toolEnumerator);

            toolEnumerator = RotateSpanner();
            StartCoroutine(toolEnumerator);
        }

        private IEnumerator RotateSpanner()
        {
            float elapsedTime = 0.0f;
            var tweenLength = 1.0f;

            while (elapsedTime < tweenLength)
            {
                elapsedTime += Time.deltaTime;
                fastener.Rotate(Vector3.up, 3000 * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator OnCompleteEnumerator(float tweenLength)
        {
            yield return new WaitForSeconds(tweenLength);
            onCompleteEnumerator = null;
            AssemblyComplete();
        }

        private void AssemblyComplete()
        {
            if (assemblyShowUp)
                ShowUpForAssembly(StepType.PART_INSTALLATION);
            else
            {
                hoveredObject = null;
                originalTransform.Apply(transform);
                gameObject.SetActive(false);
                pickedUpCorrectly = false;
                hoveringOverCorrectTarget = false;
                Highlight(highlightedType);
            }
        }

        private IEnumerator OnGrabEnumerator()
        {
            Highlight(pickedUpCorrectly ? HighlightType.GREEN: HighlightType.YELLOW);
            if (!pickedUpCorrectly && AppManager.CurrentState == State.TRAINING)
                Coordinator.instance.modalWindow.Show(UI.WindowType.WARNING, "Wrong Object!");
            yield return new WaitForSeconds(Coordinator.instance.settings.SelectedPreferences.assemblyTweenLength);
            Highlight(HighlightType.NONE);
        }

        public void Init()
        {
            originalTransform.Extract(transform);
        }

        public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
        {
            base.GrabBegin(hand, grabPoint);
            pickedUpCorrectly = Coordinator.instance.appManager.OnToolGrab(gameObject);
          
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
            //print("OnTriggerEnter: " + other.name);

            hoveredObject = other.gameObject;
            if (hoveredObject.layer == 13 && pickedUpCorrectly)
            {
                hoveringOverCorrectTarget = Coordinator.instance.appManager.ValidateHover(StepType.PART_INSTALLATION, gameObject, hoveredObject);
                Highlight(hoveringOverCorrectTarget ? HighlightType.GREEN : HighlightType.RED);
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
            //Debug.Log("ShowUpForAssembly");
            if (onCompleteEnumerator == null)
            {
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
            
        }

    }
}
