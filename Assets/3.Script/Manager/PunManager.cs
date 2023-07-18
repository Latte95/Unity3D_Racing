using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PunManager : MonoBehaviourPunCallbacks // �⺻ ����Ƽ �ݹ� + ���� �ݹ�
{
    [Header("Server Setting")]
    // ���� ����(Master ���� -> Lobby -> Room)
    private readonly string gameVersion = "1";
    public ServerSettings setting = null;

    [Header("Player Prefab")]
    // �÷��̾� ������
    public GameObject playerPrefabs;

    private void Start()
    {
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
        PhotonNetwork.ConnectToMaster(setting.AppSettings.Server, setting.AppSettings.Port, "");    // ���� ������ �����ϴ°Ŷ� �� ID ����

        Debug.Log("Connect to Master Server");
    }
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    // �ݹ�
    public void JoinRandomRoomOrCreateRoom()
    {
        
    }
    public void CancelMatching()
    {
        Debug.Log("��Ī ���");
        Debug.Log("���� ����");
        PhotonNetwork.LeaveRoom();
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
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Entered room");
        UpdatePlayer();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log($"{newPlayer.NickName} ����");
        UpdatePlayer();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log($"{otherPlayer.NickName} Ż��");
        UpdatePlayer();
    }

    public void UpdatePlayer()
    {
    }
    #endregion ���� ���� �ݹ� �Լ�
}
