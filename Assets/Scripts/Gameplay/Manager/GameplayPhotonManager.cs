using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

public class GameplayPhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameManager gm;
    [SerializeField] private string roomName;

    private Action onRoom;
    private bool isMaster;

    private void Start()
    {
        gm.pm = this;

        onRoom += MasterGameStart;

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Connected to lobby");
        PhotonNetwork.JoinRandomOrCreateRoom(roomName: roomName);
    }

    public override void OnJoinedRoom()
    {
        onRoom?.Invoke();
    }

    private void MasterGameStart()
    {
        string roomName = PhotonNetwork.CurrentRoom.Name;
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        isMaster = PhotonNetwork.IsMasterClient;

        if (isMaster)
        {
            gm.InitializeGame();
        }

        if (playerCount < 4)
        {
            gm.SpawnPlayer(playerCount);
        }

        else
        {
            Application.Quit();
        }
    }

    public void SpawnObject(string name, Vector3 position, Quaternion rotation)
    {
        PhotonNetwork.Instantiate(name, position, rotation, group: 0);
    }

    public GameObject ReturnSpawnedObject(string name, Vector3 position, Quaternion rotation)
    {
        return PhotonNetwork.Instantiate(name, position, rotation, group: 0);
    }

    public void SpawnRoomObject(string name, Vector3 position, Quaternion rotation)
    {
        PhotonNetwork.InstantiateRoomObject(name, position, rotation, group: 0);
    }

    public void DestroyObject(GameObject obj)
    {
        PhotonNetwork.Destroy(obj);
    }


}
