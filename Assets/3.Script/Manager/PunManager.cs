using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PunManager : MonoBehaviourPunCallbacks // 기본 유니티 콜백 + 포톤 콜백
{
    [Header("Server Setting")]
    // 서버 접속(Master 서버 -> Lobby -> Room)
    private readonly string gameVersion = "1";
    public ServerSettings setting = null;

    [Header("Player Prefab")]
    // 플레이어 프리팹
    public GameObject playerPrefabs;

    private void Start()
    {
        Connect();
    }
    private void OnApplicationQuit()
    {
        Disconnect();
    }

    #region 서버 관련 콜백 함수
    // connecttomaser
    public void Connect()
    {
        PhotonNetwork.GameVersion = gameVersion;

        // 마스터 서버 연결
        PhotonNetwork.ConnectToMaster(setting.AppSettings.Server, setting.AppSettings.Port, "");    // 포톤 서버에 접속하는거라 앱 ID 없음

        Debug.Log("Connect to Master Server");
    }
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    // 콜백
    public void JoinRandomRoomOrCreateRoom()
    {
        
    }
    public void CancelMatching()
    {
        Debug.Log("매칭 취소");
        Debug.Log("방을 떠남");
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
        Debug.Log($"{newPlayer.NickName} 참가");
        UpdatePlayer();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log($"{otherPlayer.NickName} 탈주");
        UpdatePlayer();
    }

    public void UpdatePlayer()
    {
    }
    #endregion 서버 관련 콜백 함수
}
