using Photon.Pun;
using Photon.Realtime;
using System;
using System.Security.Cryptography;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayPhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameManager gm;

    public int PlayerCount => AmountOfPlayers();

    private void Start()
    {
        gm.phMan = this;
        InitializeMatch();
    }

    private void InitializeMatch()
    {
        string roomName = PhotonNetwork.CurrentRoom.Name;
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        if (playerCount <= PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                int myID = GetPlayerID();
                gm.SpawnPlayer(myID);
                gm.StartMatch();
                gm.currentTime = 0;
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "matchStarted", true } });
            }
            else
            {
                gm.SpawnPlayer(GetPlayerID());
            }
        }
        else
        {
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene("MainMenuScene");
        }
    }

    private int GetPlayerID()
    {
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == PhotonNetwork.LocalPlayer)
                return i;
        }
        return 0; 
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

    public GameObject ReturnSpawnedRoomObject(string name, Vector3 position, Quaternion rotation)
    {
        return PhotonNetwork.InstantiateRoomObject(name, position, rotation, group: 0);
    }

    public void DestroyObject(GameObject obj)
    {
        PhotonNetwork.Destroy(obj);
    }

    private int AmountOfPlayers()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        if (otherPlayer == PhotonNetwork.MasterClient || PhotonNetwork.MasterClient == null)
        {
            gm.photonView.RPC(nameof(gm.PauseGameRPC), RpcTarget.All);
        }

        int leftID = otherPlayer.ActorNumber - 1;

        if (PhotonNetwork.IsMasterClient)
        {
            if (gm.playersInMatch.ContainsKey(leftID))
            {
                gm.photonView.RPC(nameof(gm.UnregisterPlayer), RpcTarget.All, leftID);
            }

            if (PlayerCount < 2)
            {
                PhotonNetwork.AutomaticallySyncScene = true;
                PhotonNetwork.LoadLevel("WaitingScene");
            }
        }

        if (gm.crownController == null)
        {
            gm.StartMatch();
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);

        gm.photonView.RPC(nameof(gm.UnpauseGameRPC), RpcTarget.All);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer); 
    }
}
