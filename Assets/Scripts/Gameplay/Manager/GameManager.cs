using Photon.Pun;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviourPun
{
    public GameplayPhotonManager phMan;

    [Header("Camera Related")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform orignalPosition;

    [Header("Player Related")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<GameObject> playerSpawners;
    [SerializeField] private List<Material> playerMaterials;
    public List<Material> PlayerMaterials => playerMaterials;

    [Header("WinCondition")]
    [SerializeField] private int winningPoints;
    public int WinningPoints => winningPoints;

    [Header("Crown Related")]
    [SerializeField] private GameObject crownPrefab;
    [SerializeField] private GameObject crownSpawners;
    private CrownController crownController;

    public void GameStart()
    {
        var crown = phMan.ReturnSpawnedRoomObject(crownPrefab.name, crownSpawners.transform.position, Quaternion.identity);
        crownController = crown.GetComponent<CrownController>();
        Debug.Log("The party has started");
    }

    public void SpawnPlayer(int ID)
    {
        GameObject currentPlayer = phMan.ReturnSpawnedObject(playerPrefab.name, playerSpawners[ID].transform.position, Quaternion.identity);
        PlayerController player = currentPlayer.GetComponent<PlayerController>();
        player.InitializeGame(phMan, this, ID);
    }

    public void WinCondition(int points, PlayerController player)
    {
        if (points >= winningPoints)
        {
            photonView.RPC(nameof(WinGame), RpcTarget.All, player);
        }
    }

    #region RPCMethods

    [PunRPC]
    public void WinGame(PlayerController player)
    {
        Debug.Log(player.name + " Wins");
    }

    [PunRPC]
    public void CancelGame()
    {
        Debug.Log(" Canceled game. Disconneting...");
    }

    [PunRPC]
    public void PlayerQuitParty()
    {
        if (phMan.PlayerCount < 2)
        {
            photonView.RPC(nameof(CancelGame), RpcTarget.All);
        }
    }
    #endregion
}
