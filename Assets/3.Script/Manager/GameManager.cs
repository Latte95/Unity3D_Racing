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
            // pathCheck�� �ϳ��� false�� false
            foreach (bool pc in pathCheck)
            {
                if(!pc)
                {
                    return false;
                }
            }
            // pathCheck�� ��� true��� true
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
    #region �̱���
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
    #endregion �̱���

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
            totalLap = 1;
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
                // ��� ������ ����ģ �� �ٽ� ù ����(=���� ����)�� �������� ��� Lap�� ����
                if(pathIndex == 0 && characters[i].allCheck)
                {
                    int length = paths.Length;
                    for (int j = 0; j < length;j++)
                    {
                        characters[i].pathCheck[j] = false;
                    }

                    CharacterControl c = characters[i].character.GetComponent<CharacterControl>();
                    c.currentLapCount++;
                    if(c.currentLapCount > totalLap)
                    {
                        isPlay = false;
                    }

                    c.LapIncrease();
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
