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
    #endregion �̱���

    [Header("Server Setting")]
    // ���� ����(Master ���� -> Lobby -> Room)
    private readonly string gameVersion = "1";
    public ServerSettings setting = null;

    private const int MAX_PLAYER = 4;
    private const float MAX_TIME = 3f;
    private float matchingStartTime;
    private Button btn;

    [Header("Player Prefab")]
    // �÷��̾� ������
    public GameObject playerPrefabs;

    private GameManager gameManager;
    private TitleManager titleManager;

    private new PhotonView photonView;

    private void Start()
    {
        titleManager = GameObject.FindGameObjectWithTag("TitleManager").GetComponent<TitleManager>();
        gameManager = GameManager.Instance;
        photonView = GetComponent<PhotonView>();
        Connect();
    }
    private void OnApplicationQuit()
    {
        Disconnect();
    }
    private new void OnDisable()
    {
        base.OnDisable();
        Disconnect();
    }

    #region ���� ���� �ݹ� �Լ�
    // connecttomaser
    public void Connect()
    {
        btn = GameObject.FindGameObjectWithTag("Match").GetComponent<Button>();
        if (!gameManager.isLogin)
        {
            btn.transform.parent.gameObject.SetActive(false);
        }
        PhotonNetwork.GameVersion = gameVersion;

        // ������ ���� ����
        // ���� ������ �����ϴ°Ŷ� �� ID ����
        PhotonNetwork.ConnectToMaster(setting.AppSettings.Server, setting.AppSettings.Port, "");
    }
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    // �ݹ�
    public void JoinRandomRoomOrCreateRoom()
    {
        RoomOptions option = new RoomOptions();

        option.MaxPlayers = MAX_PLAYER;
        // IsGameStarted ������Ƽ ��ü ����
        // ������ �̹� ���۵� ��� �������� �ʰ� �ϴ� ����
        option.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            { "IsGameStarted", false },
            { "LoadedCount", 0 }
        };
        option.CustomRoomPropertiesForLobby = new string[] { "IsGameStarted" };
        StartMatchmakingTimer();

        // �� ���� �õ� �� ���н� �� ����
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

        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        btn.onClick.AddListener(JoinRandomRoomOrCreateRoom);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        PhotonNetwork.LocalPlayer.NickName = titleManager.model_txt.text;

        // Ŀ���� ������Ƽ ����
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
        {
            { "ModelName", titleManager.model_txt.text },
            { "KartName", titleManager.kart_txt.text }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

        StartCoroutine(GameStart_co());
    }

    public IEnumerator GameStart_co()
    {
        while (true)
        {
            yield return null;

            float elapsedTime = Time.time - matchingStartTime;
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;

            if (playerCount == maxPlayers || elapsedTime > MAX_TIME)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "IsGameStarted", true } });
                    yield return new WaitForSeconds(1f);
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


    public bool isReady = false;
    public int loadedCount = 0;
    int num;
    [PunRPC]
    public void GameStart()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            ExitGames.Client.Photon.Hashtable properties = player.CustomProperties;

            gameManager.charName[player.ActorNumber - 1] = properties["ModelName"] as string;
            gameManager.kartName[player.ActorNumber - 1] = properties["KartName"] as string;
        }
        num = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        SceneManager.sceneLoaded += OnSceneLoaded;
        PhotonNetwork.AutomaticallySyncScene = true;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(titleManager.map_txt.text);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        MakePlayer();
        PhotonNetwork.AutomaticallySyncScene = false;
        photonView.RPC("CountLoadedScene", RpcTarget.MasterClient);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    [PunRPC]
    private void CountLoadedScene()
    {
        ExitGames.Client.Photon.Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (roomProperties.ContainsKey("LoadedCount"))
        {
            loadedCount = (int)roomProperties["LoadedCount"];
        }
        loadedCount++;
        roomProperties["LoadedCount"] = loadedCount;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

        // ��� �÷��̾ ���ε� �Ϸ�� �� ���� ����
        if (loadedCount == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            photonView.RPC("StartGame", RpcTarget.All);
        }
    }

    [PunRPC]
    private void StartGame()
    {
        gameManager.Init();
    }

    private void MakePlayer()
    {
        Vector3 position = new Vector3(-317.22f + num * 7.78f, 83.1f, -26.26f + num * 10.36f);
        Quaternion rotation = Quaternion.Euler(0, 180, 0);
        GameObject player = PhotonNetwork.Instantiate(playerPrefabs.name, position, rotation);

        player.name = "Player";
        PlayerControl p = player.GetComponent<PlayerControl>();
        p.GetComponent<PhotonView>().RPC("SetMyIndex", RpcTarget.AllBuffered, num, gameManager.userName);

        p.enabled = true;
    }
}
