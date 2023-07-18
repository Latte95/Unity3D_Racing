using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

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
                if (!pc)
                {
                    return false;
                }
            }
            // pathCheck�� ��� true��� true
            return true;
        }
    }
    public int pathCheckCount
    {
        get
        {
            int c = 0;
            foreach (bool pc in pathCheck)
            {
                if (pc)
                {
                    c++;
                }
            }
            return c + (character.GetComponent<CharacterControl>().currentLapCount - 1) * pathCheck.Length;
        }
    }
    public string name
    {
        get
        {
            return character.GetComponent<CharacterControl>().charAnim.gameObject.name;
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

    [SerializeField]
    public Character[] characters;
    public string charName { get; set; }
    public string kartName { get; set; }
    public int totalCharacters { get; private set; }
    public bool isTitle = false;
    public bool isPlay = false;

    public int totalLap;
    [SerializeField]
    private Text totalLap_txt;

    [HideInInspector]
    public GameObject[] paths;

    public bool isMobile = false;
    public GameObject pcUI;
    public GameObject mobileUI;
    public Animator countAnim;


    private void Start()
    {
        Init();
    }

    public void Init()
    {
        SetTotalLap();
        if (!isTitle)
        {
            GameObject countObject = GameObject.FindGameObjectWithTag("Count");
            countObject.TryGetComponent(out countAnim);
            GameObject Lap = GameObject.FindGameObjectWithTag("Lap");
            Lap.TryGetComponent(out totalLap_txt);
            totalLap_txt.text = "/" + totalLap.ToString();

            SetPath();
            SetChar();
            SetUI();

            StartCoroutine(CountDown_co(5));
        }
    }

    private void SetUI()
    {
        pcUI = GameObject.FindGameObjectWithTag("PCUI");
        mobileUI = GameObject.FindGameObjectWithTag("MobileUI");
#if UNITY_STANDALONE
        pcUI.SetActive(true);
        mobileUI.SetActive(false);
        isMobile = false;
#endif
        //test
#if UNITY_ANDROID || UNITY_EDITOR
        pcUI.SetActive(false);
        mobileUI.SetActive(true);
        isMobile = true;
#endif
    }

    private void SetChar()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] ais = GameObject.FindGameObjectsWithTag("AI");

        totalCharacters = players.Length + ais.Length;
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
        if (SceneManager.GetActiveScene().name.Equals("Title"))
        {
            isTitle = true;
            return;
        }
        else if (SceneManager.GetActiveScene().name.Equals("MooMooMeadows"))
        {
            totalLap = 2;
        }
        else
        {
            totalLap = 2;
        }
        isTitle = false;
    }

    /// <summary>
    /// ĳ���Ͱ� ���� ��η� �����Ͽ� �� ������ ���Ҵ��� �Ǵ��ϰ� ������ ĳ���Ͱ� ���� �� ������ �����ϴ� �޼ҵ�.
    /// </summary>
    public void SetPathCheck(GameObject character, int pathIndex)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i].character == character)
            {
                // ���� ����Ʈ�� ����ġ�� �ʾ��� ���(=������) pathCheck ���� x
                if (pathIndex > 0 && !characters[i].pathCheck[pathIndex - 1])
                {
                    return;
                }

                // ��� ������ ����ģ �� �ٽ� ù ����(=���� ����)�� �������� ��� Lap�� ����
                if (pathIndex == 0 && characters[i].allCheck)
                {
                    // pathCheck �ʱ�ȭ
                    int length = paths.Length;
                    for (int j = 0; j < length; j++)
                    {
                        characters[i].pathCheck[j] = false;
                    }

                    // lapCount ����
                    CharacterControl c = characters[i].character.GetComponent<CharacterControl>();
                    c.currentLapCount++;
                    c.LapIncrease();

                    // �������� ��� ���� ����
                    if (c.currentLapCount > totalLap)
                    {
                        isPlay = false;
                        countAnim.SetTrigger("Finish");
                    }
                }

                characters[i].pathCheck[pathIndex] = true;
                break;
            }
        }
    }

    /// <summary>
    /// ī��Ʈ �ٿ��� ������ �÷��̾ �̵� �����ϵ��� �����ϴ� �ڷ�ƾ
    /// </summary>
    /// <param name="time">ī��Ʈ�ٿ� �ð�</param>
    /// <returns></returns>
    private IEnumerator CountDown_co(float time)
    {
        int min = 3;
        if (time < min)
        {
            time = min;
        }

        float preTime = time - min;
        if (preTime < 0)
        {
            preTime = 0;
        }


        // ī��Ʈ �ٿ� ����
        yield return new WaitForSeconds(preTime);
        countAnim.SetTrigger("Timer");

        // ī��Ʈ�ٿ��� ������ ���� �̸� Freeze�� ���� �̵����� ����
        yield return new WaitForSeconds(time - preTime - 1);
        for (int i = 0; i < characters.Length; i++)
        {
            CharacterControl cc = characters[i].character.GetComponent<CharacterControl>();

            cc.rigid.constraints = RigidbodyConstraints.None;
        }

        // ī��Ʈ �ٿ��� ������ �̵� ������ ���·� ��ȯ
        yield return new WaitForSeconds(1);
        isPlay = true;
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i].character.CompareTag("Player"))
            {
                PlayerControl pc = characters[i].character.GetComponent<PlayerControl>();
                pc.SetState(pc.nomalState);
            }
        }

    }

    private void Test()
    {
        charName = ECharacter.Mario.ToString();
        kartName = ECharacter.Mario.ToString();
    }
}
