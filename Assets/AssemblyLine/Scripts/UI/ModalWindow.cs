using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AL.UI
{
    public enum WindowType
    {
        ERROR,
        WARNING,
        RESULT
    }

    public class ModalWindow : MonoBehaviour, IResettable
    {
        [SerializeField]
        Image image;

        [SerializeField]
        TextMeshProUGUI textContent;

        [SerializeField]
        private Transform anchor;

        public void Show(WindowType windowType, string content)
        {
            switch (windowType)
            {
                case WindowType.ERROR:
                    image.color = Coordinator.instance.appTheme.SelectedTheme.ErrorPanelColor;
                    break;
                case WindowType.WARNING:
                    image.color = Coordinator.instance.appTheme.SelectedTheme.WarningPanelColor;
                    break;
                case WindowType.RESULT:
                    image.color = Coordinator.instance.appTheme.SelectedTheme.ResultPanelColor;
                    break;
                default:
                    image.color = Coordinator.instance.appTheme.SelectedTheme.ResultPanelColor;
                    break;
            }

            gameObject.SetActive(true);
            transform.SetParent(Coordinator.instance.ovrPlayerController.transform.parent);
            textContent.text = content;
        }

        public void Close()
        {
            gameObject.SetActive(false);
            transform.SetParent(anchor);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            Reset();
        }
        
        public void Reset()
        {
            textContent.text = "";
        }
    }
}







