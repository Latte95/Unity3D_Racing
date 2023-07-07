using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ECharacter
{
    Mario,
    Luigi,
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
    public bool isStart = false;

    private void Test()
    {
        charName = ECharacter.Mario.ToString();
        kartName = ECharacter.Mario.ToString();
    }
}
