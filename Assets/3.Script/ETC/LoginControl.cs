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
            log.text = "아이디 혹은 비밀번호를 입력해주세요.";
            return;
        }
        // todo
        // 로그인 성공
        //if (DBManager.instance.Login(ID_i.text, Password_i.text))
        //{
        //    transform.gameObject.SetActive(false);
        //    select.SetActive(true);
        //}
        // 로그인 실패
        //else
        //{
        //    log.text = "아이디 혹은 비밀번호를 확인해주세요.";
        //}
    }

    public void SignIn_btn()
    {
        transform.gameObject.SetActive(false);
        SignIn();
    }
}
