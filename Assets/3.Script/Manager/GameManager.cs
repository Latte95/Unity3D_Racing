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

    public string charName { get; set; }
    public string kartName { get; set; }
    public bool isTitle = false;
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

    public Animator countAnim;

    private void Start()
    {
        Init();
        //SetTotalLap();
        //if (!isTitle)
        //{
        //    Init();
        //    SetPath();
        //    SetChar();

        //    StartCoroutine(CountDown_co(5));
        //}
    }

    public void Init()
    {
        SetTotalLap();
        if (!isTitle)
        {
            GameObject countObject = GameObject.FindGameObjectWithTag("Count");
            countObject.TryGetComponent(out countAnim);
            GameObject Lap = GameObject.FindGameObjectWithTag("Lap");
            Lap.TryGetComponent(out currentLap_txt);
            Lap.transform.GetChild(0).gameObject.TryGetComponent(out totalLap_txt);
            totalLap_txt.text = "/" + totalLap.ToString();

            SetPath();
            SetChar();

            StartCoroutine(CountDown_co(5));
        }
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
            totalLap = 1;
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
                    for (int j = 0; j < length; j++)
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
        countAnim.SetTrigger("Timer");

        // 카운트다운이 끝나기 전에 미리 Freeze를 통한 이동제한 해제
        yield return new WaitForSeconds(time - preTime - 1);
        for (int i = 0; i < characters.Length; i++)
        {
            CharacterControl cc = characters[i].character.GetComponent<CharacterControl>();

            cc.rigid.constraints = RigidbodyConstraints.None;
        }

        // 카운트 다운이 끝나면 이동 가능한 상태로 전환
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
