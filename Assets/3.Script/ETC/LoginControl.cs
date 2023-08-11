using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;

public class LoginControl : MonoBehaviour
{
    private string serverUrl = "http://3.19.19.98:5000/login";

    public InputField ID_Field;
    public InputField Password_Field;

    public Text log;

    public GameObject signIn;
    public GameObject select;

    private void OnEnable()
    {
        // ȸ�� ���� �� �ٽ� �α��� ȭ������ ���ƿ��� ��츦 ����� �Է� ������ �ʱ�ȭ
        ID_Field.text = "";
        Password_Field.text = "";
        log.text = "";

        // �̹� �α��� �� �� ������ �÷����ϴ� Ÿ��Ʋ ȭ������ �ٽ� ���ƿ��� ��� �α���â Ȱ��ȭ x
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
        // �ʼ� �Է� �� ���� ���
        if (ID_Field.text.Equals(string.Empty) || Password_Field.text.Equals(string.Empty))
        {
            log.text = "���̵� Ȥ�� ��й�ȣ�� �Է����ּ���.";
            return;
        }
        else
        {
            StartCoroutine(Login_co(ID_Field.text, Password_Field.text));
        }
    }
    public IEnumerator Login_co(string userID, string password)
    {
        WWWForm userData = new WWWForm();
        userData.AddField("id", userID);
        userData.AddField("password", password);

        // �������� DB���� ȸ������ ��ȸ ��û
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, userData))
        {
            // ���� ���
            yield return www.SendWebRequest();

            // ���� ���� ���� or ��ġ�ϴ� ȸ�� ���� ��� �α��� ����
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                log.text = "�α��� ����";
            }
            // �������� �α��� ���� �޼��� ���� �޾����� �α��� ����
            else
            {
                JSONNode response = JSON.Parse(www.downloadHandler.text);

                if (response["message"] == "Logged in.")
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
