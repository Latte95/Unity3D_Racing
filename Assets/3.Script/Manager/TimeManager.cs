using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    [SerializeField]
    private Text lapPlusTime_txt;
    [SerializeField]
    private Text currnetTime_txt;
    [SerializeField]
    private Text bestTime_txt;

    private float currentTime = 0f;
    private TimeSpan timeSpan;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    private void Update()
    {
        // 게임 시작(카운트다운 이후)하면 경과 시간 표시
        if (gameManager.isStart)
        {
            currentTime += Time.deltaTime;
            UpdateTime();
        }
    }

    public void UpdateTime()
    {
        timeSpan = TimeSpan.FromSeconds(currentTime);
        // 경과 시간 표시
        if (timeSpan.Hours < 1)
        {
            // D2 => 최소 2자리 표시, F => 레이싱 게임에서 타임어택이 중요하기 때문에 정밀도를 올리기 위해서 더블형 사용
            currnetTime_txt.text = string.Format($"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}:{(int)(timeSpan.Milliseconds*0.1F):D2}");
        }
        // 1시간이 경과하면 59:59:99로 고정
        else
        {
            currnetTime_txt.text = "59:59:99";
        }
    }
}
