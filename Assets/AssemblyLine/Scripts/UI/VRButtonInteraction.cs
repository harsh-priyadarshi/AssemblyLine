using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using AL.Audio;
using UnityEngine.UI;

namespace AL.UI.VR
{
    public class VRUIInteractionBase : UIInteractionBase
    {
        public override void ToggleOutlineHighlight(bool val)
        {
        }

        public override void ToggleBackgroundHighlight(bool val)
        {
        }
    }

    public class VRButtonInteraction : VRUIInteractionBase, IPointerClickHandler
    {
        [SerializeField]
        TextMeshProUGUI buttonText;

        [SerializeField]
        private bool homeUi = false;

        public override void ToggleOutlineHighlight(bool val)
        {
            if (!homeUi)
                return;

            if (val && selectable.interactable)
                buttonText.color = Coordinator.instance.appTheme.SelectedTheme.buttonHighlightTextColor;
            else
                buttonText.color = Coordinator.instance.appTheme.SelectedTheme.buttonNormalTextColor;
        }

        public override void ToggleBackgroundHighlight(bool val) { }
   
        public override void Reset()
        {
            print("Reset");
            if (pointerHovering)
                buttonText.color = Coordinator.instance.appTheme.SelectedTheme.buttonHighlightTextColor;
            else
                buttonText.color = Coordinator.instance.appTheme.SelectedTheme.buttonNormalTextColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Coordinator.instance.audioManager.Play(AudioManager.buttonClick);
            var targetRectTransform = selectable.targetGraphic.rectTransform;
            var buttonClickSpeed = Coordinator.instance.settings.SelectedPreferences.buttonClickAnimationSpeed;
            var buttonClickImpact = Coordinator.instance.settings.SelectedPreferences.buttonClickAnimationImpact;

            var scaleDownTweener = targetRectTransform.DOSizeDelta(targetRectTransform.sizeDelta - new Vector2(6, 0) * buttonClickImpact, 1 / (10 * buttonClickSpeed));
            Tweener scaleUpTweener = null;
            scaleDownTweener.OnComplete(() =>
            {
                scaleUpTweener = targetRectTransform.DOSizeDelta(targetRectTransform.sizeDelta + new Vector2(6, 0) * buttonClickImpact, 1 / (10 * buttonClickSpeed));
                scaleUpTweener.OnComplete(() =>
                {
                    selectable.interactable = true;
                    if (eventData == null)
                    {
                        var button = ((Button)selectable);
                        if (button.onClick != null)
                            button.onClick.Invoke();
                    }
                    //print("Scale comlete");
                });
            });
           

            selectable.interactable = false;

            //targetRectTransform.DOSizeDelta(targetRectTransform.sizeDelta - new Vector2(1, 0), 1).OnComplete(() => targetRectTransform.sizeDelta = targetRectTransform.sizeDelta + new Vector2(1, 0) * buttonClickImpact);
            EventSystem.current.SetSelectedGameObject(null);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (selectable.interactable)
                OnPointerClick(null);
        }

    }
}
