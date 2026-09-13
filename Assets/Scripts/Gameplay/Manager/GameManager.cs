using Photon.Pun;
using System.Collections;
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
    public float currentTime;
    private bool matchStarted;


    [Header("Crown Related")]
    [SerializeField] private GameObject crownPrefab;
    [SerializeField] private GameObject crownSpawners;
    public CrownController crownController;

    public List<PlayerController> playersInMatch;

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (matchStarted)
        {
            currentTime += Time.deltaTime;
            EndCondition();
        }

    }

    public void StartMatch()
    {
        var crown = phMan.ReturnSpawnedRoomObject(crownPrefab.name, crownSpawners.transform.position, Quaternion.identity);
        crownController = crown.GetComponent<CrownController>();
        matchStarted = true;
    }

    public void SpawnPlayer(int ID)
    {
        GameObject currentPlayer = phMan.ReturnSpawnedObject(playerPrefab.name, playerSpawners[ID].transform.position, Quaternion.identity);
        PlayerController player = currentPlayer.GetComponent<PlayerController>();
        player.InitializeGame(phMan, this, ID);
    }

    public void EndCondition()
    {
        if (currentTime >= matchDuration)
        {
            photonView.RPC(nameof(EndGame), RpcTarget.All);
            StartCoroutine(ReturningToWaitingScene());
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
                Debug.Log(currentWinner);
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

    private IEnumerator ReturningToWaitingScene()
    {
        yield return new WaitForSeconds(10f);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LoadLevel("WaitingScene");
    }
}
