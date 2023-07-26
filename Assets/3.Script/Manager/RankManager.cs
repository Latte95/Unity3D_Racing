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

        // ����ģ path�� ����
        int[] pathCount = new int[length];
        for(int i = 0; i < length; i++)
        {
            pathCount[i] = gameManager.characters[i].pathCheckCount;
        }
        // �������� ����
        Array.Sort(pathCount);
        Array.Reverse(pathCount);

        // ���� ���� �н� ���� ������ �ִµ� �̹� ��ŷ�� ����� ĳ���� ��
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

            // same����ŭ ��ŷ�� ǥ������ �ʰ� �н��ϱ� ���� ����
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
