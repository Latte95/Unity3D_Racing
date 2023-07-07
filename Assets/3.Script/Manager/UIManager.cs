using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Text currnetLap_txt;

    private float currentTime = 0;

    private GameManager gameManager;
    private TimeManager timeManager;

    private void Start()
    {
        gameManager = GameManager.Instance;
        timeManager = new TimeManager();
    }

    private void Update()
    {
        if (gameManager.isStart)
        {
            timeManager.UpdateTime();
        }
    }
}
