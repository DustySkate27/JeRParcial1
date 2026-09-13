using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;

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

        if (playerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                wm.SpawnPlayer(0);
            }
            else
            {
                wm.SpawnPlayer(GetPlayerID());
            }
        }
        else
        {
            Application.Quit();
        }
    }

    private int GetPlayerID()
    {
        return PhotonNetwork.LocalPlayer.ActorNumber - 1;
    }

    public void GameStartConfirmed()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }

    public GameObject ReturnSpawnedObject(string name, Vector3 position, Quaternion rotation)
    {
        return PhotonNetwork.Instantiate(name, position, rotation, group: 0);
    }

    public void DestroyObject(GameObject obj)
    {
        PhotonNetwork.Destroy(obj);
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
