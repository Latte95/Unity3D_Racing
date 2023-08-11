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
        // 회원 가입 후 다시 로그인 화면으로 돌아왔을 경우를 대비한 입력 데이터 초기화
        ID_Field.text = "";
        Password_Field.text = "";
        log.text = "";

        // 이미 로그인 한 뒤 게임을 플레이하다 타이틀 화면으로 다시 돌아왔을 경우 로그인창 활성화 x
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
        // 필수 입력 값 없을 경우
        if (ID_Field.text.Equals(string.Empty) || Password_Field.text.Equals(string.Empty))
        {
            log.text = "아이디 혹은 비밀번호를 입력해주세요.";
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

        // 서버에게 DB에서 회원정보 조회 요청
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, userData))
        {
            // 응답 대기
            yield return www.SendWebRequest();

            // 서버 연결 실패 or 일치하는 회원 없을 경우 로그인 실패
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                log.text = "로그인 실패";
            }
            // 서버에서 로그인 성공 메세지 응답 받았으면 로그인 성공
            else
            {
                JSONNode response = JSON.Parse(www.downloadHandler.text);

                if (response["message"] == "Logged in.")
                {
                    log.text = $"{userID}님 환영합니다.";

                    yield return new WaitForSeconds(2);
                    gameObject.SetActive(false);
                    select.SetActive(true);
                    GameManager.Instance.isLogin = true;
                    GameManager.Instance.userName = userID;
                }
                else
                {
                    log.text = "로그인 실패";
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
