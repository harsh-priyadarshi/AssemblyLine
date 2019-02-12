using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
        Image headerImage;

        [SerializeField]
        private List<Image> buttonImages;

        [SerializeField]
        private TextMeshProUGUI textContent;

        [SerializeField]
        private RectTransform tweenHeaderOpenAnchor, tweenHeaaderCloseAnchor;

        [SerializeField]
        private GameObject mainPanel;

        [SerializeField]
        private Transform anchor;

        bool windowOn = false;

        private CustomHand leftHand, rightHand;

        private bool leftHandHovering = false;
        private bool rightHandHovering = false;

        private void Start()
        {
            leftHand = Coordinator.instance.leftHand;
            rightHand = Coordinator.instance.rightHand;
        }

        private void Update()
        {
            UpdateHandPose();
        }

        private void UpdateHandPose()
        {
            var leftHandDistance = Vector3.Distance(transform.position, leftHand.transform.position);
            var righthandDistance = Vector3.Distance(transform.position, rightHand.transform.position);

            //print(leftHandDistance + " " + righthandDistance);

            if (leftHandDistance < Coordinator.instance.settings.SelectedPreferences.handHoverRange && !leftHandHovering)
            {
                leftHand.SetCustomPose(HandState.POINTING);
                leftHandHovering = true;
            }
            else if (leftHandDistance > Coordinator.instance.settings.SelectedPreferences.handHoverEndDistance && leftHandHovering)
            {
                leftHand.ReleaseCustomPose();
                leftHandHovering = false;
            }

            if (righthandDistance < Coordinator.instance.settings.SelectedPreferences.handHoverRange && !rightHandHovering)
            {
                rightHand.SetCustomPose(HandState.POINTING);
                rightHandHovering = true;
            }
            else if (righthandDistance > Coordinator.instance.settings.SelectedPreferences.handHoverEndDistance && rightHandHovering)
            {
                rightHand.ReleaseCustomPose();
                rightHandHovering = false;
            }
        }

        private void Open()
        {
            gameObject.SetActive(true);
            headerImage.rectTransform.DOSizeDelta(tweenHeaderOpenAnchor.sizeDelta, Coordinator.instance.settings.SelectedPreferences.assemblyTweenLength).OnComplete(() => mainPanel.SetActive(true));
        }

        public void Show(WindowType windowType, string content)
        {
            windowOn = true;
            switch (windowType)
            {
                case WindowType.ERROR:
                    headerImage.color = Coordinator.instance.appTheme.SelectedTheme.ErrorPanelColor;
                    break;
                case WindowType.WARNING:
                    headerImage.color = Coordinator.instance.appTheme.SelectedTheme.WarningPanelColor;
                    break;
                case WindowType.RESULT:
                    headerImage.color = Coordinator.instance.appTheme.SelectedTheme.ResultPanelColor;
                    break;
                default:
                    headerImage.color = Coordinator.instance.appTheme.SelectedTheme.ResultPanelColor;
                    break;
            }

            foreach (var item in buttonImages)
                item.color = headerImage.color;

            transform.SetParent(Coordinator.instance.ovrPlayerController.transform.parent);
            textContent.text = content;

            Open();
        }

        public void Close()
        {
            windowOn = false;
            transform.SetParent(anchor);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            mainPanel.SetActive(false);
            headerImage.gameObject.SetActive(true);
            headerImage.rectTransform.DOSizeDelta(tweenHeaaderCloseAnchor.sizeDelta, Coordinator.instance.settings.SelectedPreferences.assemblyTweenLength).OnComplete(() => gameObject.SetActive(false));

            OnReset();
        }
        
        public void OnReset()
        {
            textContent.text = "";
            if (leftHandHovering)
            {
                leftHand.ReleaseCustomPose();
                leftHandHovering = false;
            }

            if (rightHandHovering)
            {
                rightHand.ReleaseCustomPose();
                rightHandHovering = false;
            }
        }

        public void OnHomeToggle()
        {
            if (!gameObject.activeSelf && windowOn)
                gameObject.SetActive(true);
            else if(gameObject.activeSelf)
                gameObject.SetActive(false);

        }
    }
}







