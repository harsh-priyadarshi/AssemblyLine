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
        [SerializeField]
        private GameObject mainMenu;
       

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
            Coordinator.instance.appManager.OnLoginToggle(true);
        }

        public void Logout()
        {
            loginScreen.SetActive(true);
            Coordinator.instance.desktopScreenController.ResetLogoutControl();
            usernameText.text = "";
            LoggedIn = false;
            Coordinator.instance.appManager.OnLoginToggle(false);
        }
    }
}
