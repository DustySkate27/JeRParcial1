using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviourPun
{
    public GameplayPhotonManager pm;

    [Header("Camera Related")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform orignalPosition;

    [Header("Player Related")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<GameObject> playerSpawners;

    private Dictionary<PlayerController, int> playersInParty = new Dictionary<PlayerController, int>();

    [Header("WinCondition")]
    [SerializeField] private int winConditiion;

    [Header("Crown Related")]
    [SerializeField] private GameObject crownPrefab;
    [SerializeField] private GameObject crownSpawners;

    public void InitializeGame()
    {
        pm.SpawnObject(crownPrefab.name, crownSpawners.transform.position, Quaternion.identity);
        Debug.Log("The party has started");
    }

    public void SpawnPlayer(int ID)
    {
        GameObject currentPlayer = pm.ReturnSpawnedObject(playerPrefab.name, playerSpawners[ID].transform.position, Quaternion.identity);
        PlayerController player = currentPlayer.GetComponent<PlayerController>();
        playersInParty.Add(player, 0);
    }

    public void AddPoint(PlayerController player, int points)
    {
        playersInParty[player] = points;

        if (playersInParty[player] >= winConditiion)
        {
            photonView.RPC(nameof(WinGame), RpcTarget.All, player);
        }
    }

    [PunRPC]
    public void WinGame(PlayerController player)
    {
        Debug.Log(player.ToString() + " Win");
    }
}
