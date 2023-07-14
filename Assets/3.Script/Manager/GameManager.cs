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
public class GameManager : MonoBehaviour
{
    #region ΩÃ±€≈Ê
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
    #endregion ΩÃ±€≈Ê

    public string charName { get; private set; }
    public string kartName { get; private set; }
    public bool isStart = false;

    public int totalLap;
    [SerializeField]
    private Text totalLap_txt;

    private void Start()
    {
        SetTotalLap();
    }

    private void SetTotalLap()
    {
        if(SceneManager.GetActiveScene().name.Equals("MooMooMeadows"))
        {
            totalLap = 2;
        }
        else
        {
            totalLap = 0;
        }
        totalLap_txt.text = "/" + totalLap.ToString();
    }

    private void Test()
    {
        charName = ECharacter.Mario.ToString();
        kartName = ECharacter.Mario.ToString();
    }
}
