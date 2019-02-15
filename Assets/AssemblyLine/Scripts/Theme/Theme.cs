using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL.Theme
{
    [CreateAssetMenu(fileName = "New Theme", menuName = "VES Themes")]
    public class Theme : ScriptableObject
    {
        #region UI
        public Color colorMix2, colorMix3, panelInteractionOutline;
        public Color objectHighlightColor = new Color32(255, 99, 71, 255);
        public Color buttonHighlightTextColor = Color.white;
        public Color buttonNormalTextColor = Color.white;
        public Color panelInteractionBackground;
        public Texture2D gradientTex;
        public Color vrControlDisabledColor;
        public Material panelMat;
        public Color toolbarButtonBackgroundHighlight;
        public Color ToolbarColor = new Color32(37, 37, 41, 255);
        public Material background;
        public Color ErrorColor = new Color32(255, 0, 42, 255);
        public Color InputFieldSelection = new Color32(5, 5, 10, 255);
        public Color InputFieldNormalColor = new Color32(12, 12, 18, 255);
        public Color PanelColor = new Color32(20, 20, 26, 255);
        public Color ErrorPanelColor = new Color32(20, 20, 26, 255);
        public Color WarningPanelColor = new Color32(20, 20, 26, 255);
        public Color ResultPanelColor = new Color32(20, 20, 26, 255);
        #endregion

        #region ObjectControl
        public Gradient redGradient, greenGradient, blueGradient;
        public Color redHighlight = new Color(.45f, 0, 0, 4) / 4;
        public Color greenHighlight = new Color(0, .45f, 0, 4) / 4;
        public Color yellowHighlight = new Color(0, .45f, 0, 4) / 4;
        #endregion
    }
}
