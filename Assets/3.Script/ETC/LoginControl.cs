using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginControl : MonoBehaviour
{
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
            log.text = "���̵� Ȥ�� ��й�ȣ�� �Է����ּ���.";
            return;
        }
        // todo
        // �α��� ����
        //if (DBManager.instance.Login(ID_i.text, Password_i.text))
        //{
        //    transform.gameObject.SetActive(false);
        //    select.SetActive(true);
        //}
        // �α��� ����
        //else
        //{
        //    log.text = "���̵� Ȥ�� ��й�ȣ�� Ȯ�����ּ���.";
        //}
    }

    public void SignIn_btn()
    {
        transform.gameObject.SetActive(false);
        SignIn();
    }
}
