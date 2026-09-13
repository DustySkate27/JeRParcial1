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

    public void SpawnPlayer(int ID)
    {
        GameObject currentPlayer = phWait.ReturnSpawnedObject(playerPrefab.name, playerSpawners[ID - 1].transform.position, Quaternion.identity);
        PlayerController player = currentPlayer.GetComponent<PlayerController>();
        player.InitializeWait(phWait, this, ID);
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

    [PunRPC]
    public void PlayerQuitParty()
    {
        if (phWait.PlayerCount < 2)
        {
            Debug.Log("No apto para iniciar");
        }
    }
}
