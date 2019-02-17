using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL.Gameplay
{
    public class AssemblyComponent : MonoBehaviour, IResettable, IAssemblyItem
    {
        [SerializeField]
        private List<MeshRenderer> meshRenderer;
        [SerializeField]
        [Range(1, 10)]
        private float onHighlightScaleFactor = 1;

        private bool highlighted = false;
        private StepType stepType = StepType.PART_PLACEMENT;
        private IEnumerator assemblyCompleteEnumerator;
        private Vector3 originalScale;
        private bool watchingForAssembly = false;

        private IEnumerator AssemblyCompleteEnumerator(float tweenLength)
        {
            yield return new WaitForSeconds(tweenLength);
            AssemblyComplete();
        }

        public void Init()
        {
            originalScale = transform.localScale;
        }

        private void AssemblyComplete()
        {
            Highlight(HighlightType.NONE);
            if (stepType == StepType.PART_PLACEMENT)
            {
                foreach (var item in meshRenderer)
                    item.enabled = true;
            }
        }

        public void StopWatchingForAssembly()
        {
            if (watchingForAssembly)
            {
                if (stepType == StepType.PART_PLACEMENT)
                    foreach (var item in meshRenderer)
                        item.enabled = false;
                Highlight(HighlightType.NONE);
            }
        }

        public void ShowUpForAssembly(StepType type)
        {
            stepType = type;
            watchingForAssembly = true;

            if (AppManager.CurrentState == State.TRAINING)
            {
                foreach (var item in meshRenderer)
                    item.enabled = true;
            }
            
            if (type == StepType.PART_PLACEMENT)
                Highlight(HighlightType.TRANSPARENT);
            else
                Highlight(HighlightType.BLILNK);
        }

        public void AssemblyComplete(float tweenLength)
        {
            if (assemblyCompleteEnumerator != null)
                StopCoroutine(assemblyCompleteEnumerator);
            if (stepType == StepType.PART_PLACEMENT)
                foreach (var item in meshRenderer)
                    item.enabled = false;
            assemblyCompleteEnumerator = AssemblyCompleteEnumerator(tweenLength);
            StartCoroutine(assemblyCompleteEnumerator);
            watchingForAssembly = false;
        }

        public void OnReset()
        {
            foreach (var item in meshRenderer)
                item.enabled = false;
            watchingForAssembly = false;
            Highlight(HighlightType.NONE);
        }

        public void Highlight(HighlightType type)
        {
            if (AppManager.CurrentState == State.ASSESSMENT)
                return;

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

            //if (type != HighlightType.NONE)
            //    transform.localScale = originalScale * onHighlightScaleFactor;
            //else
            //    transform.localScale = originalScale;
        }
    }
}