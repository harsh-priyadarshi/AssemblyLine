using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL.Gameplay
{
    public class AssemblyComponent : MonoBehaviour, IHighlightable
    {
        [SerializeField]
        private MeshRenderer meshRenderer;
        private bool trackHover = true;

        private bool highlighted = false;

        public void OnTriggerEnter(Collider other)
        {
           
        }

        public void OnTriggerExit(Collider other)
        {
            //print("OnTriggerEnter: " + name);
        }

        public void WatchForAssembly()
        {
            meshRenderer.enabled = true;
            Highlight(HighlightType.TRANSPARENT);
        }

        public void AssemblyComplete()
        {
            trackHover = false;
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