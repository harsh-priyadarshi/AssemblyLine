using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL.Gameplay
{
    public class AssemblyComponent : MonoBehaviour, IResettable, IAssemblyItem
    {
        [SerializeField]
        private MeshRenderer meshRenderer;
        private bool trackHover = true;

        private bool highlighted = false;
        private StepType stepType = StepType.PART_PLACEMENT;

        private IEnumerator assemblyCompleteEnumerator;

        private IEnumerator AssemblyCompleteEnumerator(float tweenLength)
        {
            yield return new WaitForSeconds(tweenLength);

            AssemblyComplete();
        }

        private void AssemblyComplete()
        {
            trackHover = false;
            if (stepType == StepType.PART_PLACEMENT)
            {
                Highlight(HighlightType.NONE);
                meshRenderer.enabled = true;
            }
        }

        public void WatchForAssembly(StepType type)
        {
            stepType = type;
            if (type == StepType.PART_PLACEMENT)
            {
                meshRenderer.enabled = true;
                Highlight(HighlightType.TRANSPARENT);
            }
        }

        public void AssemblyComplete(float tweenLength)
        {
            if (assemblyCompleteEnumerator != null)
                StopCoroutine(assemblyCompleteEnumerator);
            if (stepType == StepType.PART_PLACEMENT)
                meshRenderer.enabled = false;
            assemblyCompleteEnumerator = AssemblyCompleteEnumerator(tweenLength);
            StartCoroutine(assemblyCompleteEnumerator);
        }

        public void OnReset()
        {
            meshRenderer.enabled = false;
            trackHover = true;
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