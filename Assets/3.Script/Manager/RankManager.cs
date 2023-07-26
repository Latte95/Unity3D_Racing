using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankManager : MonoBehaviour
{
    private GameManager gameManager;

    public GameObject[] ranks;
    public Text[] rankNames;
    public GameObject panel;

    private void Awake()
    {
        gameManager = GameManager.Instance;
    }

    private void Start()
    {
        panel.SetActive(false);
    }

    public void SetRank()
    {
        panel.SetActive(true);
        Cursor.visible = true;
        for (int i = gameManager.totalCharacters; i < 8; i++)
        {
            ranks[i].SetActive(false);
        }

        int length = gameManager.totalCharacters;

        // 지나친 path수 저장
        int[] pathCount = new int[length];
        for(int i = 0; i < length; i++)
        {
            pathCount[i] = gameManager.characters[i].pathCheckCount;
        }
        // 내림차순 정렬
        Array.Sort(pathCount);
        Array.Reverse(pathCount);

        // 나와 같은 패스 수를 가지고 있는데 이미 랭킹에 저장된 캐릭터 수
        int same = 0;
        for (int i = 0; i < length; i++)
        {
            if(i > 0 && pathCount[i] == pathCount[i-1])
            {
                same++;
            }
            else
            {
                same = 0;
            }

            // same수만큼 랭킹에 표시하지 않고 패스하기 위한 변수
            int pass = same;
            foreach(Character c in gameManager.characters)
            {
                if(c.pathCheckCount == pathCount[i])
                {
                    if(pass > 0)
                    {
                        pass--;
                        continue;
                    }
                    rankNames[i].text = c.name;
                    break;
                }
            }
        }
    }
}
