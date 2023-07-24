using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SignInControl : MonoBehaviour
{
    private string serverUrl = "http://3.19.19.98:5000/signin";

    public InputField ID_i;
    public InputField Password_i;

    public Text log;

    public GameObject logIn;

    private void OnEnable()
    {
        ID_i.text = "";
        Password_i.text = "";
        log.text = "";
    }

    private void SignIn()
    {
        logIn.SetActive(true);
    }

    public void SignIn_btn()
    {
        if (ID_i.text.Equals(string.Empty) || Password_i.text.Equals(string.Empty))
        {
            log.text = "���̵� Ȥ�� ��й�ȣ�� �Է����ּ���.";
            return;
        }
        else
        {
            StartCoroutine(Signin_co(ID_i.text, Password_i.text));
        }
    }
    public IEnumerator Signin_co(string userID, string password)
    {

        WWWForm form = new WWWForm();
        form.AddField("id", userID);
        form.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                log.text = "ȸ������ ����";
            }
            else
            {
                var response = JSON.Parse(www.downloadHandler.text);

                if (response["message"] == "Signin success.")
                {
                    log.text = "ȸ������ ����";
                    yield return new WaitForSeconds(2);
                    transform.gameObject.SetActive(false);
                    SignIn();
                }
                else if (response["message"] == "ID that exists.")
                {
                    log.text = "�̹� �����ϴ� ID�Դϴ�.";
                }
            }
        }
    }
}
