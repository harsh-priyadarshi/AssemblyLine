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
        private List<Image> buttonImages;

        [SerializeField]
        private TextMeshProUGUI textContent;

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

        public void Show(WindowType windowType, string content)
        {
            windowOn = true;
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

            foreach (var item in buttonImages)
                item.color = image.color;

            gameObject.SetActive(true);
            transform.SetParent(Coordinator.instance.ovrPlayerController.transform.parent);
            textContent.text = content;
        }

        public void Close()
        {
            windowOn = false;
            gameObject.SetActive(false);
            transform.SetParent(anchor);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            Reset();
        }
        
        public void Reset()
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







