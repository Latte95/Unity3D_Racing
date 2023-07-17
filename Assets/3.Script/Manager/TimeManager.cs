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
        // ���� ����(ī��Ʈ�ٿ� ����)�ϸ� ��� �ð� ǥ��
        if (gameManager.isPlay)
        {
            currentTime += Time.deltaTime;
            UpdateTime();
        }
    }

    private void UpdateTime()
    {
        timeSpan = TimeSpan.FromSeconds(currentTime);
        // ��� �ð� ǥ��
        if (timeSpan.Hours < 1)
        {
            // D2 => �ּ� 2�ڸ� ǥ��, F => ���̽� ���ӿ��� Ÿ�Ӿ����� �߿��ϱ� ������ ���е��� �ø��� ���ؼ� ������ ���
            currnetTime_txt.text = string.Format($"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}:{(int)(timeSpan.Milliseconds*0.1F):D2}");
        }
        // 1�ð��� ����ϸ� 59:59:99�� ����
        else
        {
            currnetTime_txt.text = "59:59:99";
        }
    }

    //public 
}
