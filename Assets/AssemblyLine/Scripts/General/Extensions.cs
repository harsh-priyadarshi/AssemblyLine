using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AL
{
    public static class Extensions
    {

        #region TransformExtensions

        /// <summary>
        /// It will copy local position, width, height, pivot and anchors from reference
        /// </summary>
        /// <param name="target"></param>
        /// <param name="reference"></param>
        public static void Copy(this RectTransform target, RectTransform reference)
        {
            target.sizeDelta = reference.sizeDelta;
            target.pivot = reference.pivot;
            target.anchorMax = reference.anchorMax;
            target.anchorMin = reference.anchorMin;
            target.localPosition = reference.localPosition;
        }

        #endregion
    }
}
