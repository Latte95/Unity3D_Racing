using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum ECharacter
{
    Mario,
    Luigi,
}

[System.Serializable]
public class Character
{
    public GameObject character;
    public bool[] pathCheck;
    public bool allCheck
    {
        get
        {
            // pathCheck가 하나라도 false면 false
            foreach (bool pc in pathCheck)
            {
                if(!pc)
                {
                    return false;
                }
            }
            // pathCheck가 모두 true라면 true
            return true;
        }
    }

    public Character(GameObject c, bool[] pc)
    {
        character = c;
        pathCheck = pc;
    }
}

public class GameManager : MonoBehaviour
{
    #region 싱글톤
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        Test();
    }
    #endregion 싱글톤

    public string charName { get; private set; }
    public string kartName { get; private set; }
    public bool isPlay = false;

    public int totalLap;
    [SerializeField]
    private Text currentLap_txt;
    [SerializeField]
    private Text totalLap_txt;

    [SerializeField]
    private GameObject[] paths;

    [SerializeField]
    private Character[] characters;

    private void Start()
    {
        SetTotalLap();
        SetPath();
        SetChar();
    }

    private void SetChar()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] ais = GameObject.FindGameObjectsWithTag("AI");

        int totalCharacters = players.Length + ais.Length;
        characters = new Character[totalCharacters];

        for (int i = 0; i < totalCharacters; i++)
        {
            GameObject characterGameObject;
            if (i < players.Length)
            {
                characterGameObject = players[i];
            }
            else
            {
                characterGameObject = ais[i - players.Length];
            }

            bool[] pathCheck = new bool[paths.Length];
            characters[i] = new Character(characterGameObject, pathCheck);
        }
    }

    private void SetPath()
    {
        GameObject parentObject = GameObject.Find("Path");

        int childCount = parentObject.transform.childCount;
        paths = new GameObject[childCount];

        for (int i = 0; i < childCount; i++)
        {
            paths[i] = parentObject.transform.GetChild(i).gameObject;
        }
    }

    private void SetTotalLap()
    {
        if (SceneManager.GetActiveScene().name.Equals("MooMooMeadows"))
        {
            totalLap = 2;
        }
        else
        {
            totalLap = 1;
        }
        totalLap_txt.text = "/" + totalLap.ToString();
    }

    public void SetPathCheck(GameObject character, int pathIndex)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i].character == character)
            {
                // 이전 포인트를 지나치지 않았을 경우(=역주행) pathCheck 실행 x
                if (pathIndex > 0 && !characters[i].pathCheck[pathIndex - 1])
                {
                    return;
                }

                // 모든 지점을 지나친 뒤 다시 첫 지점(=골인 지점)에 도착했을 경우 Lap수 증가
                if (pathIndex == 0 && characters[i].allCheck)
                {
                    // pathCheck 초기화
                    int length = paths.Length;
                    for (int j = 0; j < length;j++)
                    {
                        characters[i].pathCheck[j] = false;
                    }

                    // lapCount 증가
                    CharacterControl c = characters[i].character.GetComponent<CharacterControl>();
                    c.currentLapCount++;
                    c.LapIncrease();

                    // 완주했을 경우 게임 종료
                    if (c.currentLapCount > totalLap)
                    {
                        isPlay = false;
                    }
                }

                characters[i].pathCheck[pathIndex] = true;
                break;
            }
        }
    }

    private void Test()
    {
        charName = ECharacter.Mario.ToString();
        kartName = ECharacter.Mario.ToString();
    }
}
