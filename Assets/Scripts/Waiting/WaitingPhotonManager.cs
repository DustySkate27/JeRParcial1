using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class WaitingPhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private WaitManager wm;

    public int PlayerCount => AmountOfPlayers();


    private bool isMaster;

    private void Start()
    {
        InitializeScene();
    }

    private void InitializeScene()
    {
        string roomName = PhotonNetwork.CurrentRoom.Name;
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        isMaster = PhotonNetwork.IsMasterClient;

        if (playerCount < 4)
        {
            wm.SpawnPlayer(playerCount);
        }
        else
        {
            Application.Quit();
        }
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
}
