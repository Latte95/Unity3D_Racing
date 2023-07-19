using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class PunManager : MonoBehaviourPunCallbacks // 기본 유니티 콜백 + 포톤 콜백
{
    #region 싱글톤
    public static PunManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Destroy(Instance.photonView);
            }

            DestroyImmediate(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion 싱글톤

    [Header("Server Setting")]
    // 서버 접속(Master 서버 -> Lobby -> Room)
    private readonly string gameVersion = "1";
    public ServerSettings setting = null;

    private int maxPlayer = 8;
    private float maxTime = 3f;
    private float matchingStartTime;
    private Button btn;

    [Header("Player Prefab")]
    // 플레이어 프리팹
    public GameObject playerPrefabs;

    private TitleManager titleManager;

    private PhotonView photonView;

    private void Start()
    {
        btn = GameObject.FindGameObjectWithTag("Match").GetComponent<Button>();
        titleManager = GameObject.FindGameObjectWithTag("TitleManager").GetComponent<TitleManager>();
        photonView = GetComponent<PhotonView>();
        Connect();
    }
    private void OnApplicationQuit()
    {
        Disconnect();
    }
    private void OnDisable()
    {
        base.OnDisable();
        Disconnect();
    }

    #region 서버 관련 콜백 함수
    // connecttomaser
    public void Connect()
    {
        PhotonNetwork.GameVersion = gameVersion;

        // 마스터 서버 연결
        // 포톤 서버에 접속하는거라 앱 ID 없음
        PhotonNetwork.ConnectToMaster(setting.AppSettings.Server, setting.AppSettings.Port, "");

        Debug.Log("Connect to Master Server");
    }
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    // 콜백
    public void JoinRandomRoomOrCreateRoom()
    {
        Debug.Log($"매칭 시작");
                
        RoomOptions option = new RoomOptions();

        option.MaxPlayers = maxPlayer;
        // IsGameStarted 프로퍼티 객체 생성
        // 게임이 이미 시작된 경우 참가하지 않게 하는 역할
        option.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            { "IsGameStarted", false }
        };
        option.CustomRoomPropertiesForLobby = new string[] { "IsGameStarted" };
        StartMatchmakingTimer();

        // 방 참가 시도 및 실패시 방 생성
        PhotonNetwork.JoinRandomOrCreateRoom
        (
            expectedCustomRoomProperties:
            new ExitGames.Client.Photon.Hashtable() { { "IsGameStarted", false } },
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

        btn.onClick.AddListener(JoinRandomRoomOrCreateRoom);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Entered room");

        PhotonNetwork.LocalPlayer.NickName = titleManager.model_txt.text;

        // Set custom properties
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
        {
            { "ModelName", titleManager.model_txt.text },
            { "KartName", titleManager.kart_txt.text }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

        StartCoroutine(GameStart_co());
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        Debug.Log($"{newPlayer.NickName} 참가");
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log($"{otherPlayer.NickName} 탈주");
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

                    PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "IsGameStarted", true } });
                }
                break;
            }
        }
    }
    public void StartMatchmakingTimer()
    {
        matchingStartTime = Time.time;
    }
    #endregion 서버 관련 콜백 함수

    int num;
    [PunRPC]
    public void GameStart()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            ExitGames.Client.Photon.Hashtable properties = player.CustomProperties;

            charName[player.ActorNumber - 1] = properties["ModelName"] as string;
            kartName[player.ActorNumber - 1] = properties["KartName"] as string;
        }
        num = PhotonNetwork.LocalPlayer.ActorNumber -1;

        SceneManager.sceneLoaded += OnSceneLoaded;

        PhotonNetwork.LoadLevel(titleManager.map_txt.text);
    }
    private void MakePlayer()
    {
        Vector3 position = new Vector3(-317.22f + num* 7.78f, 83.1f, -26.26f + num * 10.36f);
        Quaternion rotation = Quaternion.Euler(0, 180, 0);
        GameObject player = PhotonNetwork.Instantiate(playerPrefabs.name, position, rotation);

        player.name = "Player";
        PlayerControl p = player.GetComponent<PlayerControl>();
        p.myIndex = num;
        p.enabled = true;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        MakePlayer();
        GameManager.Instance.Init();

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public string[] charName = new string[8];
    public string[] kartName = new string[8];
}
