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
            log.text = "아이디 혹은 비밀번호를 입력해주세요.";
            return;
        }
        // todo
        // 이미 존재하는 id

        // 생성 성공
        // db 등록
        transform.gameObject.SetActive(false);
        SignIn();
    }
}
