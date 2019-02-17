using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using AL.Database;

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
        [SerializeField]
        private TextMeshProUGUI loginErrorText;

        private List<Person> users = new List<Person>();

        private Person currentUser;

        public Person CurrentUser { get { return currentUser; } }

        private IEnumerator loginFailEnumerator;

        private void Start()
        {
            LoadUsers();
        }


        private void LoadUsers()
        {
            var ds = new DataService("Users.db");
            var savedUsers = ds.GetPersons();
            
            foreach (var people in savedUsers)
                users.Add(people);
        }

        private void OnLoginFail()
        {
            loginErrorText.gameObject.SetActive(true);
            if (loginFailEnumerator != null)
                StopCoroutine(loginFailEnumerator);
            loginFailEnumerator = LoginFail();
            StartCoroutine(loginFailEnumerator);
        }

        private IEnumerator LoginFail()
        {
            yield return new WaitForSeconds(2 * Coordinator.instance.settings.SelectedPreferences.assemblyTweenLength);
            loginErrorText.gameObject.SetActive(false);
        }

        public void OnInputEndEdit()
        {
            if (Input.GetKey(KeyCode.Return))
                Login();
        }

        public void Login()
        {
            currentUser = users.Find(item => item.UserName.Equals(username.text));

            if (currentUser == null || !currentUser.Password.Equals(password.text))
                OnLoginFail();
            else
            {
                usernameText.text = currentUser.Name;
                LoggedIn = true;
                loginScreen.SetActive(false);
                Coordinator.instance.appManager.OnLoginToggle(true);
            }

            username.text = "";
            password.text = "";
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
