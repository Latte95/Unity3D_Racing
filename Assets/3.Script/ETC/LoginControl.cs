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
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.isLogin)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void SignIn()
    {
        signIn.SetActive(true);
    }

    public void Login_btn()
    {
        if (ID_i.text.Equals(string.Empty) || Password_i.text.Equals(string.Empty))
        {
            log.text = "���̵� Ȥ�� ��й�ȣ�� �Է����ּ���.";
            return;
        }
        else
        {
            StartCoroutine(Login_co(ID_i.text, Password_i.text));
        }
    }
    public IEnumerator Login_co(string userID, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", userID);
        form.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
                log.text = "�α��� ����";
            }
            else
            {
                var response = JSON.Parse(www.downloadHandler.text);

                if (response["message"] == "Logged in.")
                {
                    Debug.Log("�α��� ����");
                    log.text = $"{userID}�� ȯ���մϴ�.";

                    yield return new WaitForSeconds(2);
                    gameObject.SetActive(false);
                    select.SetActive(true);
                    GameManager.Instance.isLogin = true;
                    GameManager.Instance.userName = userID;
                }
                else
                {
                    Debug.Log("Login failed: " + response["message"]);
                    log.text = "�α��� ����";
                }
            }
        }
    }

    public void SignIn_btn()
    {
        transform.gameObject.SetActive(false);
        SignIn();
    }
}
