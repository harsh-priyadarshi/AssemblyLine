using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace AL.UI
{
    public class DesktopScreenController : MonoBehaviour
    {
        [SerializeField]
        GameObject logoutInput, pointerClickDetector;

        [SerializeField]
        RectTransform fulScreenVectorAnchor, smallScreenVectorAnchor, headsetInstructionVector, fulScreenVectorAnchorForTween, smallScreenVectorAnchorForTween;

        IEnumerator ToggleLogoutCoroutine = null;

        public void OpenLogoutControl()
        {
            if (ToggleLogoutCoroutine != null)
                StopCoroutine(ToggleLogoutCoroutine);
            ToggleLogoutCoroutine = ToggleLogoutControl(true);
            StartCoroutine(ToggleLogoutCoroutine);
        }

        IEnumerator ToggleLogoutControl(bool val)
        {
            logoutInput.SetActive(val);
            pointerClickDetector.SetActive(!val);

            var referenceAnchor = val ? fulScreenVectorAnchorForTween : smallScreenVectorAnchorForTween;
            headsetInstructionVector.Copy(referenceAnchor);

            headsetInstructionVector.DOSizeDelta(val ? smallScreenVectorAnchorForTween.sizeDelta : fulScreenVectorAnchorForTween.sizeDelta, .5f, false);

            yield return new WaitForSeconds(.6f);
            referenceAnchor = val ? smallScreenVectorAnchor : fulScreenVectorAnchor;
            headsetInstructionVector.Copy(referenceAnchor);

            if (val)
            {
                yield return new WaitForSeconds(5.0f);
                StartCoroutine(ToggleLogoutControl(false));
            }
        }

        public void ResetLogoutControl()
        {
            headsetInstructionVector.sizeDelta = fulScreenVectorAnchor.sizeDelta;
            pointerClickDetector.SetActive(true);
            logoutInput.SetActive(false);
        }

    }
}
