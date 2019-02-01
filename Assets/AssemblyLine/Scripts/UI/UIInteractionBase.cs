﻿using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

namespace AL.UI
{
    public class UIInteractionBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        protected Shadow shadow;

        [SerializeField]
        protected Image image;

        [SerializeField]
        protected Selectable selectable;

        protected bool mouseHovering = false;

        private void Start()
        {
            Init();
        }

        public virtual void OnEnable() { }
     

        public virtual void OnDisable()
        {
            ToggleOutlineHighlight(false);
            ToggleBackgroundHighlight(false);
        }

        public virtual void Init()
        {
            ToggleBackgroundHighlight(false);
            ToggleOutlineHighlight(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            mouseHovering = true;
            ToggleOutlineHighlight(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mouseHovering = false;
            ToggleOutlineHighlight(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ToggleBackgroundHighlight(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ToggleBackgroundHighlight(false);
        }

        public virtual void ToggleOutlineHighlight(bool val)
        {
            if (val && selectable.interactable)
                shadow.effectColor = Coordinator.instance.appTheme.SelectedTheme.colorMix2;
            else
                shadow.effectColor = Coordinator.instance.appTheme.SelectedTheme.panelInteractionOutline;
        }

        public virtual void ToggleBackgroundHighlight(bool val) { }

        public virtual void Reset() { }

        public void HighlightOutline(Color col)
        {
            shadow.effectColor = col;
        }

        public void HardSelect()
        {
            ToggleBackgroundHighlight(true);
            ToggleOutlineHighlight(true);
        }
    }
}