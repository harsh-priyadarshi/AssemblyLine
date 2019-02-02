using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace AL.Web
{
    public class Authentication : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField username, password;
        [SerializeField]
        private GameObject loginScreen;
        [SerializeField]
        private TextMeshProUGUI usernameText;

        public void Login()
        {
            loginScreen.SetActive(false);
            usernameText.text = username.text;
            username.text = "";
            password.text = "";
        }

        public void Logout()
        {
            loginScreen.SetActive(true);
            Coordinator.instance.desktopScreenController.ResetLogoutControl();
            usernameText.text = "";
        }
    }
}
