using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;

public class LoginControl : MonoBehaviour
{
    private string serverUrl = "http://18.218.154.6:5000/login";

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

    private void SignUp()
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
        WWWForm userData = new WWWForm();
        userData.AddField("id", userID);
        userData.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, userData))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                log.text = "��ġ�ϴ� ȸ���� �����ϴ�.";
            }
            else
            {
                //JSONNode response = JSON.Parse(www.downloadHandler.text);

                //if (response["message"] == "Logged in.")
                if (www.downloadHandler.text == "Logged in.")
                {
                    log.text = $"{userID}�� ȯ���մϴ�.";

                    yield return new WaitForSeconds(2);
                    gameObject.SetActive(false);
                    select.SetActive(true);
                    GameManager.Instance.isLogin = true;
                    GameManager.Instance.userName = userID;
                }
                else
                {
                    log.text = "�α��� ����";
                }
            }
        }
    }

    public void SignUp_btn()
    {
        transform.gameObject.SetActive(false);
        SignUp();
    }
}
