using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class PunManager : MonoBehaviourPunCallbacks // �⺻ ����Ƽ �ݹ� + ���� �ݹ�
{
    #region �̱���
    public static PunManager Instance { get; private set; }

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
    }
    #endregion �̱���

    [Header("Server Setting")]
    // ���� ����(Master ���� -> Lobby -> Room)
    private readonly string gameVersion = "1";
    public ServerSettings setting = null;

    private int maxPlayer = 8;
    private float maxTime = 5f;
    private float matchingStartTime;
    public Button btn;

    [Header("Player Prefab")]
    // �÷��̾� ������
    public GameObject playerPrefabs;

    public TitleManager titleManager;

    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        Connect();
    }
    private void OnApplicationQuit()
    {
        Disconnect();
    }

    #region ���� ���� �ݹ� �Լ�
    // connecttomaser
    public void Connect()
    {
        PhotonNetwork.GameVersion = gameVersion;

        // ������ ���� ����
        // ���� ������ �����ϴ°Ŷ� �� ID ����
        PhotonNetwork.ConnectToMaster(setting.AppSettings.Server, setting.AppSettings.Port, "");

        Debug.Log("Connect to Master Server");
    }
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    // �ݹ�
    public void JoinRandomRoomOrCreateRoom()
    {
        Debug.Log($"��Ī ����");
        PhotonNetwork.LocalPlayer.NickName = GameManager.Instance.charName;

        RoomOptions option = new RoomOptions();

        option.MaxPlayers = maxPlayer;
        // MaxTime ������Ƽ ��ü ����
        option.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            { "MaxTime", maxTime }
        };
        option.CustomRoomPropertiesForLobby = new string[] { "MaxTime" };
        StartMatchmakingTimer();

        // �� ���� �õ� �� ���н� �� ����
        PhotonNetwork.JoinRandomOrCreateRoom
        (
            expectedCustomRoomProperties:
            new ExitGames.Client.Photon.Hashtable() { { "MaxTime", option.CustomRoomProperties["MaxTime"] } },
            expectedMaxPlayers:
            (byte)option.MaxPlayers,
            roomOptions:
            option
        );
        btn.onClick.RemoveAllListeners();
        btn.transform.GetChild(0).GetComponent<Text>().text = "Cancel";
        btn.onClick.AddListener(CancelMatching);
    }
    public void CancelMatching()
    {
        PhotonNetwork.LeaveRoom();
        btn.onClick.RemoveAllListeners();
        btn.transform.GetChild(0).GetComponent<Text>().text = "Matching";
        btn.onClick.AddListener(JoinRandomRoomOrCreateRoom);
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connect Complete");

        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Entered Lobby");
        base.OnJoinedLobby();
        PhotonNetwork.JoinRandomRoom();

        btn.onClick.AddListener(JoinRandomRoomOrCreateRoom);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Entered room");
        StartCoroutine(GameStart_co());
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log($"{newPlayer.NickName} ����");
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log($"{otherPlayer.NickName} Ż��");
        base.OnPlayerLeftRoom(otherPlayer);
    }

    public IEnumerator GameStart_co()
    {
        while (true)
        {
            yield return null;

            float elapsedTime = Time.time - matchingStartTime;
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;

            if (playerCount == maxPlayers || elapsedTime > maxTime)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    yield return new WaitForSeconds(0.5f);
                    photonView.RPC("GameStart", RpcTarget.All);
                }
                break;
            }
        }
    }
    public void StartMatchmakingTimer()
    {
        matchingStartTime = Time.time;
    }
    #endregion ���� ���� �ݹ� �Լ�

    [PunRPC]
    public void GameStart()
    {
        GameManager.Instance.charName = titleManager.model_txt.text;
        GameManager.Instance.kartName = titleManager.kart_txt.text;

        SceneManager.sceneLoaded += OnSceneLoaded;

        PhotonNetwork.LoadLevel(titleManager.map_txt.text);
    }
    private void MakePlayer()
    {

        Vector3 position = new Vector3(-317.22f, 83.1f, -26.26f);
        Quaternion rotation = Quaternion.Euler(0, 180, 0);
        GameObject player = PhotonNetwork.Instantiate(playerPrefabs.name, position, rotation);

        player.name = "Player";
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        MakePlayer();
        GameManager.Instance.Init();

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}