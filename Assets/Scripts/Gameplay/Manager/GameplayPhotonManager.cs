using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayPhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameManager gm;
    [SerializeField] private string roomName;

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

        if (playerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                gm.SpawnPlayer(0);
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

        gm.CrownSpawn();
        gm.currentTime = 0;
    }

    private int GetPlayerID()
    {
        return PhotonNetwork.LocalPlayer.ActorNumber - 1;
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

        if (PhotonNetwork.IsMasterClient)
        {
            //Pause for 5 seconds.
            //If it doesn't come back, PhotonNetwork.SetMasterClient.
            //Catch currentTime and send it to the new MasterClient.
        }

        if(gm.crownController == null)
        {
            gm.CrownSpawn();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
    }
}
