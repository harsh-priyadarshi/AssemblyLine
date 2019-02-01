using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Authentication : MonoBehaviour {

    TMP_InputField username, password;
    public GameObject loginScreen;

    public void Login()
    {
        loginScreen.SetActive(false);
    }

    public void Logout()
    {
        loginScreen.SetActive(true);
    }
}
