using Photon.Pun;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class WaitManager : MonoBehaviourPun
{
    [SerializeField] private WaitingPhotonManager phWait;

    [Header("Master Texts")]
    [SerializeField] private GameObject waitText;
    [SerializeField] private GameObject readyText;

    [Header("Player Related")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<GameObject> playerSpawners;
    [SerializeField] private List<Material> playerMaterials;
    public List<Material> PlayerMaterials => playerMaterials;

    public bool transitionReady = false;

    public PlayerController SpawnPlayer(int ID)
    {
        GameObject currentPlayer = phWait.ReturnSpawnedObject(playerPrefab.name, playerSpawners[ID].transform.position, Quaternion.identity);
        PlayerController player = currentPlayer.GetComponent<PlayerController>();
        player.InitializeWait(phWait, this, ID);
        return player;
    }

    public void RoomReady()
    {
        waitText.SetActive(false);
        readyText.SetActive(true);
    }

    public void RoomNotReady()
    {
        waitText.SetActive(true);
        readyText.SetActive(false);
    }

    public void PlayerEnteredParty()
    {
        if (phWait.PlayerCount < 2)
        {
            transitionReady = false;
            RoomNotReady();
        }
        else
        {
            transitionReady = true;
            RoomReady();
        }
        
    }
}
