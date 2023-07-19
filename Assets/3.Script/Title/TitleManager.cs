using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public enum ECharacter
{
    Mario,
    Luigi,
    Count
}

public class TitleManager : MonoBehaviour
{
    public enum EMap
    {
        MooMooMeadows,
        RainbowRoad,
        Count
    }

    [SerializeField]
    private GameObject[] models;
    [SerializeField]
    private GameObject[] karts;

    [SerializeField]
    public Text model_txt;
    [SerializeField]
    public Text kart_txt;
    [SerializeField]
    public Text map_txt;

    int activeModel = 0;
    int activeKart = 0;
    int activeMap = 0;

    private void Start()
    {
        // 기본 설정
        model_txt.text = ECharacter.Mario.ToString();
        kart_txt.text = ECharacter.Mario.ToString();
        map_txt.text = EMap.MooMooMeadows.ToString();

        foreach (GameObject m in models)
        {
            if (m.name.Equals(model_txt.text))
            {
                m.SetActive(true);
                break;
            }
        }
        foreach (GameObject k in karts)
        {
            if (k.name.Equals(model_txt.text))
            {
                k.SetActive(true);
                break;
            }
        }
    }

    public void ModelChange(bool right)
    {
        int lenth = models.Length;
        for (int i = 0; i < lenth; i++)
        {
            if (models[i].activeSelf)
            {
                models[i].SetActive(false);
                activeModel = i;
                break;
            }
        }

        if (right)
        {
            activeModel++;
        }
        else
        {
            activeModel--;
        }
        if (activeModel >= (int)ECharacter.Count)
        {
            activeModel = 0;
        }
        else if (activeModel <= 0)
        {
            activeModel = (int)ECharacter.Count - 1;
        }

        models[activeModel].SetActive(true);
        SetText();
    }
    public void KartChange(bool right)
    {
        int lenth = karts.Length;
        for (int i = 0; i < lenth; i++)
        {
            if (karts[i].activeSelf)
            {
                karts[i].SetActive(false);
                activeKart = i;
                break;
            }
        }

        if (right)
        {
            activeKart++;
        }
        else
        {
            activeKart--;
        }
        if (activeKart >= (int)ECharacter.Count)
        {
            activeKart = 0;
        }
        else if (activeKart <= 0)
        {
            activeKart = (int)ECharacter.Count - 1;
        }

        karts[activeKart].SetActive(true);
        SetText();
    }
    public void MapChange(bool right)
    {
        if (right)
        {
            activeMap++;
        }
        else
        {
            activeMap--;
        }
        if (activeMap >= (int)EMap.Count)
        {
            activeMap = 0;
        }
        else if (activeMap <= 0)
        {
            activeMap = (int)EMap.Count - 1;
        }
        map_txt.text = ((EMap)activeMap).ToString();

        SetText();
    }

    private void SetText()
    {
        model_txt.text = models[activeModel].name;
        kart_txt.text = karts[activeKart].name;
    }

    //public void GameStart()
    //{
    //    GameManager.Instance.charName[0] = model_txt.text;
    //    GameManager.Instance.kartName[0] = kart_txt.text;

    //    SceneManager.sceneLoaded += OnSceneLoaded;

    //    PhotonNetwork.LoadLevel(map_txt.text);
    //}
    //private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    PunManager.Instance.GameStart();
    //    GameManager.Instance.Init();

    //    SceneManager.sceneLoaded -= OnSceneLoaded;
    //}
}
