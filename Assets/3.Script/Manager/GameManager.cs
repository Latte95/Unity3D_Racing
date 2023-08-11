using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

[System.Serializable]
public class Character
{
    public GameObject character_object;
    public bool[] pathCheck;
    public bool allCheck
    {
        get
        {
            // pathCheck가 하나라도 false면 false
            foreach (bool pc in pathCheck)
            {
                if (!pc)
                {
                    return false;
                }
            }
            // pathCheck가 모두 true라면 true
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
            return c + (character_object.GetComponent<CharacterControl>().currentLapCount - 1) * pathCheck.Length;
        }
    }
    public string name
    {
        get
        {
            return character_object.GetComponent<PlayerControl>().myName;
        }
    }

    public Character(GameObject c, bool[] pc)
    {
        character_object = c;
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

    public Character[] characters;
    public int myIndex;
    public string[] charName = new string[8];
    public string[] kartName = new string[8];
    public string userName;
    public int totalCharacters { get; private set; }
    public bool isTitle = false;
    public bool isPlay = false;
    public bool isLogin = false;

    public int totalLap;
    [SerializeField]
    private Text totalLap_txt;

    [HideInInspector]
    public GameObject[] paths;

    public bool isMobile = false;
    public GameObject pcUI;
    public GameObject mobileUI;
    public Animator countAnim;

    public void SetName(string name)
    {
        for (int i = 1; i < charName.Length; i++)
        {
            if (charName[i] == null)
            {
                charName[i] = name;
                break;
            }
        }
    }

    public void Init()
    {
        SetTotalLap();
        if (!isTitle)
        {
            Cursor.visible = false;
            GameObject countObject = GameObject.FindGameObjectWithTag("Count");
            countObject.TryGetComponent(out countAnim);
            GameObject Lap = GameObject.FindGameObjectWithTag("Lap");
            Lap.TryGetComponent(out totalLap_txt);
            totalLap_txt.text = "/" + totalLap.ToString();

            SetPath();
            SetUI();

            StartCoroutine(CountDown_co(5));
        }
        else
        {
            Cursor.visible = true;
            isPlay = false;
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
        foreach (GameObject p in players)
        {
            if (p.name.Contains("Clone"))
            {
                p.GetComponent<PlayerControl>().enabled = false;
            }
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
            totalLap = 1;
        }
        else
        {
            totalLap = 2;
        }
        isTitle = false;
    }

    /// <summary>
    /// 캐릭터가 정상 경로로 경주하여 한 바퀴를 돌았는지 판단하고 완주한 캐릭터가 있을 시 게임을 종료하는 메소드.
    /// </summary>
    public void SetPathCheck(GameObject character, int pathIndex)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i].character_object == character)
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
                    for (int j = 0; j < length; j++)
                    {
                        characters[i].pathCheck[j] = false;
                    }

                    // lapCount 증가
                    CharacterControl c = characters[i].character_object.GetComponent<CharacterControl>();
                    c.LapIncrease();
                    if (c.currentLapCount < totalLap)
                    {
                        c.currentLapCount++;
                    }
                    else
                    {
                        // 완주했을 경우 게임 종료
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
    /// 카운트 다운이 끝나면 플레이어가 이동 가능하도록 제어하는 코루틴
    /// </summary>
    /// <param name="time">카운트다운 시간</param>
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

        // 카운트 다운 시작
        yield return new WaitForSeconds(preTime);
        SetChar();
        countAnim.SetTrigger("Timer");

        // 카운트다운이 끝나기 전에 미리 Freeze를 통한 이동제한 해제
        yield return new WaitForSeconds(time - preTime - 1);
        for (int i = 0; i < characters.Length; i++)
        {
            CharacterControl cc = characters[i].character_object.GetComponent<CharacterControl>();

            cc.rigid.constraints = RigidbodyConstraints.None;
        }

        // 카운트 다운이 끝나면 이동 가능한 상태로 전환
        yield return new WaitForSeconds(1);
        isPlay = true;
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i].character_object.CompareTag("Player"))
            {
                PlayerControl pc = characters[i].character_object.GetComponent<PlayerControl>();
                pc.SetState(pc.nomalState);
            }
        }

    }

    private void Test()
    {
        charName[0] = ECharacter.Mario.ToString();
        kartName[0] = ECharacter.Mario.ToString();
    }

    //test
    public SceneChange a;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            a.ChangeScene("Title");
        }
    }
}
