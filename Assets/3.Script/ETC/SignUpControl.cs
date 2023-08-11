using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SignUpControl : MonoBehaviour
{
    private string serverUrl = "http://3.19.19.98:5000/signin";

    public InputField ID_i;
    public InputField Password_i;

    public Text log;

    public GameObject logIn;

    private void OnEnable()
    {
        // 회원가입을 여러번 할 경우를 대비한 기존 입력 데이터 초기화
        ID_i.text = "";
        Password_i.text = "";
        log.text = "";
    }

    private void SignUp()
    {
        logIn.SetActive(true);
    }

    public void SignUp_btn()
    {
        // 필수 입력 값 없을 경우
        if (ID_i.text.Equals(string.Empty) || Password_i.text.Equals(string.Empty))
        {
            log.text = "아이디 혹은 비밀번호를 입력해주세요.";
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

        // 서버에게 DB에 회원정보 등록 요청
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, form))
        {
            // 응답 대기
            yield return www.SendWebRequest();

            // 서버와 연결 실패
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                log.text = "회원가입 실패";
            }
            else
            {
                var response = JSON.Parse(www.downloadHandler.text);

                // 일치하는 회원이 없을 경우 회원가입 성공
                if (response["message"] == "Signin success.")
                {
                    log.text = "회원가입 성공";
                    yield return new WaitForSeconds(2);
                    transform.gameObject.SetActive(false);
                    SignUp();
                }
                // 이미 존재하는 ID로 회원가입 시도할 경우
                else if (response["message"] == "ID that exists.")
                {
                    log.text = "이미 존재하는 ID입니다.";
                }
            }
        }
    }
}
