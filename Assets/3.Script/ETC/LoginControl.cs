using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using MySql.Data.MySqlClient;
using SimpleJSON;

public class LoginControl : MonoBehaviour
{
    private string serverUrl = "http://3.19.19.98:5000/login";
    public IEnumerator Login_co(string userID, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", userID);
        form.AddField("password", password);

        Debug.Log(userID);
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("1 : " + www.error);
            }
            else if (www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("2 : " + www.error);
            }
            else
            {
                var response = JSON.Parse(www.downloadHandler.text);

                if (response["message"] == "Logged in.")
                {
                    Debug.Log("Login successful");
                }
                else
                {
                    Debug.Log("Login failed: " + response["message"]);
                }
            }
        }
    }

    public InputField ID_i;
    public InputField Password_i;

    public Text log;

    public GameObject signIn;
    public GameObject select;

    private void OnEnable()
    {
        ID_i.text = "";
        Password_i.text = "";
        log.text = "";
    }

    private void SignIn()
    {
        signIn.SetActive(true);
    }

    public void Login_btn()
    {
        if (ID_i.text.Equals(string.Empty) || Password_i.text.Equals(string.Empty))
        {
            log.text = "아이디 혹은 비밀번호를 입력해주세요.";
            return;
        }
        else
        {
            StartCoroutine(Login_co(ID_i.text, Password_i.text));
            //test
            //bool isLoggedIn = Login(ID_i.text, Password_i.text);
            //if (isLoggedIn)
            //{
            //    Debug.Log("Login successful.");
            //}
            //else
            //{
            //    Debug.Log("Login failed.");
            //}
        }
    }

    public void SignIn_btn()
    {
        transform.gameObject.SetActive(false);
        SignIn();
    }
}
