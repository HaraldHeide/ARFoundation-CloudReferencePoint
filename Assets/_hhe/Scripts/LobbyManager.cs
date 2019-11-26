﻿using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TMP_Text Message;
    [SerializeField]
    private TMP_InputField playerName;

    void Start()
    {
        playerName.onEndEdit.AddListener(OnLoginButtonClicked);
    }

    public void OnLoginButtonClicked(string text)
    {
        if(!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnDisable()
    {
        playerName.onEndEdit.RemoveListener(OnLoginButtonClicked);
    }
        
    #region PUN2 CallBacks
    public override void OnConnected()  // Has reached internett
    {
        //base.OnConnected();
        Message.text = "Connected...";
    }

    public override void OnConnectedToMaster()
    {
        Message.text = "Connected to Master";
        //PhotonNetwork.JoinLobby(TypedLobby.Default);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.JoinOrCreateRoom("CommonCloudReferencePoint", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedLobby()
    {
        Message.text = "Joined Lobby...";
    }

    public override void OnJoinedRoom()
    {
        Message.text = "Joined Room...";
        if (PhotonNetwork.PlayerList.Length > 1)
        {
            PhotonNetwork.LocalPlayer.NickName = "Black";
            StartCoroutine(SpawnMyPlayer1());
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = "White";
            StartCoroutine(SpawnMyPlayer2());
        }

        PhotonNetwork.LocalPlayer.NickName = playerName.text;

        Message.text = PhotonNetwork.LocalPlayer.NickName + " Joined Room " + PhotonNetwork.CurrentRoom.Name +
                " now containing " + PhotonNetwork.CountOfPlayers.ToString();

        PhotonNetwork.LoadLevel(1);  //Index of scene in building list
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
    }

    #endregion

    IEnumerator SpawnMyPlayer1()
    {
        yield return new WaitForSeconds(1f);
    }
    IEnumerator SpawnMyPlayer2()
    {
        yield return new WaitForSeconds(1f);
    }
}