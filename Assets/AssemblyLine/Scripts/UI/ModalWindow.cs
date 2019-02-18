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
        NONE,
        ERROR,
        WARNING,
        RESULT
    }

    public class ModalWindow : MonoBehaviour, IResettable
    {
        [SerializeField]
        Image headerImage, headerIcon;

        [SerializeField]
        private List<Image> buttonImages;

        [SerializeField]
        private TextMeshProUGUI textContent, titleText;

        [SerializeField]
        private RectTransform tweenHeaderOpenAnchor, tweenHeaaderCloseAnchor;

        [SerializeField]
        private GameObject mainPanel;

        [SerializeField]
        private Transform anchor;

      
        [SerializeField]
        private Button okButton;

        [Header("Sprites")]
        [SerializeField]
        private Sprite errorIcon;
        [SerializeField]
        private Sprite warningIcon;
        [SerializeField]
        private Sprite resultIcon;

        bool windowOn = false;

        private CustomHand leftHand, rightHand;

        private bool leftHandHovering = false;
        private bool rightHandHovering = false;
        private IEnumerator autoCloseEnumerator;
        private WindowType currentWindowType = WindowType.NONE;

        private void Start()
        {
            leftHand = Coordinator.instance.leftHand;
            rightHand = Coordinator.instance.rightHand;
        }

        private void Update()
        {
            if (currentWindowType == WindowType.RESULT)
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

        private IEnumerator AutoClose()
        {
            yield return new WaitForSeconds(Coordinator.instance.settings.SelectedPreferences.modalWindowAutoCloseTime);
            Close();
        }

        public void Show(WindowType windowType)
        {
            if (autoCloseEnumerator != null)
            {
                StopCoroutine(autoCloseEnumerator);
                autoCloseEnumerator = null;
            }

            var autoClose = false;

            windowOn = true;
            switch (windowType)
            {
                case WindowType.ERROR:
                    headerImage.color = Coordinator.instance.appTheme.SelectedTheme.ErrorPanelColor;
                    headerIcon.sprite = errorIcon;
                    okButton.gameObject.SetActive(false);
                    autoClose = true;
                    break;
                case WindowType.WARNING:
                    headerImage.color = Coordinator.instance.appTheme.SelectedTheme.WarningPanelColor;
                    headerIcon.sprite = warningIcon;
                    autoClose = true;
                    okButton.gameObject.SetActive(false);
                    break;
                case WindowType.RESULT:
                    headerImage.color = Coordinator.instance.appTheme.SelectedTheme.ResultPanelColor;
                    headerIcon.sprite = resultIcon;
                    break;
                default:
                    headerImage.color = Coordinator.instance.appTheme.SelectedTheme.ResultPanelColor;
                    break;
            }

            titleText.text = windowType.ToString();

            foreach (var item in buttonImages)
                item.color = headerImage.color;

            transform.SetParent(Coordinator.instance.ovrPlayerController.transform.parent);

            Open();

            if (autoClose)
            {
                autoCloseEnumerator = AutoClose();
                StartCoroutine(autoCloseEnumerator);
            }
        }

        public void Show(WindowType windowType, string content)
        {
            //print(windowType.ToString() + " " + content);
            if (autoCloseEnumerator != null)
            {
                StopCoroutine(autoCloseEnumerator);
                autoCloseEnumerator = null;
            }

            var autoClose = false;

            windowOn = true;
            switch (windowType)
            {
                case WindowType.ERROR:
                    headerImage.color = Coordinator.instance.appTheme.SelectedTheme.ErrorPanelColor;
                    headerIcon.sprite = errorIcon;
                    okButton.gameObject.SetActive(false);
                    autoClose = true;
                    break;
                case WindowType.WARNING:
                    headerImage.color = Coordinator.instance.appTheme.SelectedTheme.WarningPanelColor;
                    headerIcon.sprite = warningIcon;
                    autoClose = true;
                    okButton.gameObject.SetActive(false);
                    break;
                case WindowType.RESULT:
                    headerImage.color = Coordinator.instance.appTheme.SelectedTheme.ResultPanelColor;
                    headerIcon.sprite = resultIcon;
                    break;
                default:
                    headerImage.color = Coordinator.instance.appTheme.SelectedTheme.ResultPanelColor;
                    break;
            }

            titleText.text = windowType.ToString();

            foreach (var item in buttonImages)
                item.color = headerImage.color;

            transform.SetParent(Coordinator.instance.ovrPlayerController.transform.parent);

            if (textContent != null)
                textContent.text = content;

            Open();

            if (autoClose)
            {
                autoCloseEnumerator = AutoClose();
                StartCoroutine(autoCloseEnumerator);
            }
        }

        public void Close()
        {
            currentWindowType = WindowType.NONE;
            windowOn = false;
            transform.SetParent(anchor);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            okButton.gameObject.SetActive(true);
            mainPanel.SetActive(false);
            headerImage.gameObject.SetActive(true);
            headerImage.rectTransform.DOSizeDelta(tweenHeaaderCloseAnchor.sizeDelta, Coordinator.instance.settings.SelectedPreferences.assemblyTweenLength).OnComplete(() => gameObject.SetActive(false));

            OnReset();
        }
        
        public void OnReset()
        {
            if (textContent != null)
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







