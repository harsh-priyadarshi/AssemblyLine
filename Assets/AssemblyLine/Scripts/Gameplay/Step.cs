using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AL.Gameplay
{
    public enum StepStatus
    {
        NOT_STARTED,
        ON_GOING,
        COMPLETE
    }

    public enum MistakeLevel
    {
        TOOL,
        PART,
        LOCATION
    }

    public class Step
    {
        public const string highlightShaderPath = "Ciconia Studio/Effects/Highlight/Opaque";
        public const string blinkHighlightShaderPath = "Ciconia Studio/Effects/Highlight/Blink";
        public const string normalShaderPath = "Ciconia Studio/Effects/Highlight/Opaque";

        [SerializeField]
        GameObject correctTool, correctPart;

        [SerializeField]
        private string narration;

        private int wrongAttemptCount = 0;
        private StepStatus status = StepStatus.NOT_STARTED;
        private Color redHighlight = new Color(.45f, 0, 0, 4) / 4;
        private Color greenHighlight = new Color(0, .45f, 0, 4) / 4;

        /// <summary>
        /// For indicating object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="val"></param>
        private void BlinkHighlight(GameObject obj)
        {
            ApplyShader(true, blinkHighlightShaderPath, obj, Coordinator.instance.appTheme.SelectedTheme.objectHighlightColor);
        }

        /// <summary>
        /// For showing action correctness
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="correct"></param>
        private void Highlight(GameObject obj, bool correct)
        {
            ApplyShader(true, highlightShaderPath, obj, correct ? greenHighlight : redHighlight);
        }

        private void Unhighlight(GameObject obj)
        {
            ApplyShader(false, normalShaderPath, obj, Coordinator.instance.appTheme.SelectedTheme.objectHighlightColor);
        }

        public void ApplyShader(bool highlight, string shaderPath, GameObject obj, Color highlightColor)
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                foreach (Material mat in renderer.materials)
                {

                    try
                    {
                        //metallic = mat.GetFloat("_Metallic");
                        //smoothness = mat.GetFloat("_Glossiness");
                        //mat.SetFloat("_Glossiness", smoothness);
                        //mat.SetFloat("_Metallic", metallic);
                    }
                    catch { }
                    finally
                    {
                        mat.shader = Shader.Find(shaderPath);
                        if (highlight)
                        {
                            mat.SetFloat("_FresnelSpread", 2.0f);
                            mat.SetFloat("_FresnelStrength", 8);
                            mat.SetColor("_HighlightColor", highlightColor);
                        }
                    }
                }
            }

            foreach (Transform child in obj.transform)
                ApplyShader(highlight, shaderPath, child.gameObject, highlightColor);

        }

        private void InstructForStep()
        {
            BlinkHighlight(correctTool);
            BlinkHighlight(correctPart);
            Coordinator.instance.audioManager.Play(narration);
        }

        public void InitiateStep()
        {
            status = StepStatus.ON_GOING;
            InstructForStep();
        }

        public void CompleteStep()
        {
            status = StepStatus.COMPLETE;
            Unhighlight(correctPart);
            Unhighlight(correctTool);
        }

        public void OnWrongAttempt(MistakeLevel mistakeLevel)
        {
            var selectedNarration = Coordinator.instance.appManager.RetrieveNarration(mistakeLevel);
            Coordinator.instance.audioManager.Play(selectedNarration);
            wrongAttemptCount++;
        }
    }
}
