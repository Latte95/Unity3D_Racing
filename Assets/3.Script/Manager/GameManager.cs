using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Test()
    {
        charName = "MairoModel";
        kartName = "MarioKart";
    }
}
