using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public enum ECharacter
    {
        Mario,
        Luigi,
        Count
    }

    [SerializeField]
    private GameObject[] models;
    [SerializeField]
    private GameObject[] karts;

    [SerializeField]
    private Text model_txt;
    [SerializeField]
    private Text kart_txt;

    int activeModel = 0;
    int activeKart = 0;

    private void Start()
    {
        // 기본 캐릭터 및 카트 설정
        model_txt.text = ECharacter.Mario.ToString();
        kart_txt.text = ECharacter.Mario.ToString();

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
        SetCharacter();
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
        SetCharacter();
    }

    private void SetCharacter()
    {
        model_txt.text = models[activeModel].name;
        kart_txt.text = karts[activeKart].name;
    }
}
