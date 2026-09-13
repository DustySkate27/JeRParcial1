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
    [SerializeField] private float matchDuration;
    [SerializeField] private int winningPoints;
    public int WinningPoints => winningPoints;
    public float currentTime;
    private bool matchStarted;


    [Header("Crown Related")]
    [SerializeField] private GameObject crownPrefab;
    [SerializeField] private GameObject crownSpawners;
    public CrownController crownController;

    public Dictionary<int, PlayerController> playersInMatch;

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (matchStarted)
        {
            currentTime += Time.deltaTime;
            EndCondition();
        }
    }

    public void CrownSpawn()
    {
        var crown = phMan.ReturnSpawnedRoomObject(crownPrefab.name, crownSpawners.transform.position, Quaternion.identity);
        crownController = crown.GetComponent<CrownController>();
        Debug.Log("The party has started");
    }

    public void SpawnPlayer(int ID)
    {
        GameObject currentPlayer = phMan.ReturnSpawnedObject(playerPrefab.name, playerSpawners[ID - 1].transform.position, Quaternion.identity);
        PlayerController player = currentPlayer.GetComponent<PlayerController>();
        player.InitializeGame(phMan, this, ID);
    }

    public void EndCondition()
    {
        if (currentTime >= matchDuration)
        {
            photonView.RPC(nameof(EndGame), RpcTarget.All);
        }
    }

    #region RPCMethods

    [PunRPC]
    public void EndGame()
    {
        int currentWinner = 0;

        for (int i = 0; i < playersInMatch.Count; i++)
        {
            if (currentWinner != i && playersInMatch[currentWinner].points < playersInMatch[i].points)
            {
                currentWinner = i;
            }
        }

        mainCamera.transform.position = playersInMatch[currentWinner].cameraWinTransform.position;
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
