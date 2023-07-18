using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class SignInControl : MonoBehaviour
{
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
        // todo
        // �̹� �����ϴ� id

        // ���� ����
        // db ���
        transform.gameObject.SetActive(false);
        SignIn();
    }
}
