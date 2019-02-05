using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using AL.Audio;


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

        public override void ToggleOutlineHighlight(bool val)
        {
            if (val && selectable.interactable)
                buttonText.color = Coordinator.instance.appTheme.SelectedTheme.buttonHighlightTextColor;
            else
                buttonText.color = Coordinator.instance.appTheme.SelectedTheme.buttonNormalTextColor;
        }

        public override void ToggleBackgroundHighlight(bool val) { }
   

        public override void Reset()
        {
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
            targetRectTransform.DOSizeDelta(targetRectTransform.sizeDelta - new Vector2(1, 0) * buttonClickImpact, 1 * buttonClickSpeed).OnComplete(() => targetRectTransform.sizeDelta = targetRectTransform.sizeDelta + new Vector2(1, 0) * buttonClickImpact);
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
