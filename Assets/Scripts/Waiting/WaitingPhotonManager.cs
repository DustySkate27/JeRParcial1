using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class WaitingPhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private WaitManager wm;

    public int PlayerCount => AmountOfPlayers();

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        InitializeScene();
    }

    private void InitializeScene()
    {
        string roomName = PhotonNetwork.CurrentRoom.Name;
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        if (playerCount <= PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                wm.SpawnPlayer(GetPlayerID());
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "matchStarted", false } });
            }
            else
            {
                wm.SpawnPlayer(GetPlayerID());
            }
        }
        else
        {
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene("MainMenuScene");
        }
        wm.PlayerEnteredParty();

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

    public void GameStartConfirmed()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }

    public GameObject ReturnSpawnedObject(string name, Vector3 position, Quaternion rotation)
    {
        return PhotonNetwork.Instantiate(name, position, rotation, group: 0);
    }

    private int AmountOfPlayers()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        if (!PhotonNetwork.IsMasterClient) return;

        wm.PlayerEnteredParty();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        if (!PhotonNetwork.IsMasterClient) return;

        wm.PlayerEnteredParty();
    }
}
