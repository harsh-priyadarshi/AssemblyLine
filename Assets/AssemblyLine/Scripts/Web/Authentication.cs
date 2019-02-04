using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace AL.Web
{
    public class Authentication : MonoBehaviour
    {
        public static bool LoggedIn = false;
        [SerializeField]
        private TMP_InputField username, password;
        [SerializeField]
        private GameObject loginScreen;
        [SerializeField]
        private TextMeshProUGUI usernameText;
        [SerializeField]
        private TextMeshProUGUI vrTitleText, vrLoginInstructionText;

        public void OnInputEndEdit()
        {
            if (Input.GetKey(KeyCode.Return))
                Login();
        }

        public void Login()
        {
            loginScreen.SetActive(false);
            usernameText.text = username.text;
            username.text = "";
            password.text = "";
            LoggedIn = true;
            ToggleHomeLoginState(true);
        }

        public void Logout()
        {
            loginScreen.SetActive(true);
            Coordinator.instance.desktopScreenController.ResetLogoutControl();
            usernameText.text = "";
            LoggedIn = false;
            ToggleHomeLoginState(false);
        }

        private void ToggleHomeLoginState(bool val)
        {
            vrTitleText.gameObject.SetActive(val);
            vrLoginInstructionText.gameObject.SetActive(!val);
        }

    }
}
