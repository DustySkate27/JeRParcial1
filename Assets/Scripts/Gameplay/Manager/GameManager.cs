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

    public Dictionary<int, PlayerController> playersInMatch = new Dictionary<int, PlayerController>();

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
        photonView.RPC(nameof(RegisterPlayer), RpcTarget.All, ID, player.photonView.ViewID);
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
    public void RegisterPlayer(int ID, int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv == null) return;

        playersInMatch[ID] = pv.GetComponent<PlayerController>();
    }

    [PunRPC]
    public void UnregisterPlayer(int ID)
    {
        playersInMatch.Remove(ID);
    }

    [PunRPC]
    public void EndGame()
    {
        int currentWinner = -1;
        int highestPoints = -1;

        foreach (var players in playersInMatch)
        {
            if (players.Value.points > highestPoints)
            {
                highestPoints = players.Value.points;
                currentWinner = players.Key;
            }
        }

        if (currentWinner == -1) return;

        PlayerController winner = playersInMatch[currentWinner];
        winner.canMove = false;
        crownController.isCrownTaken = false;

        Vector3 position = new Vector3(winner.cameraWinTransform.position.x, winner.cameraWinTransform.position.y, mainCamera.transform.position.z);
        mainCamera.transform.position = position;
    }

    [PunRPC]
    public void CancelGame()
    {
        Debug.Log(" Canceled game. Disconneting...");
    }
    #endregion

    private IEnumerator ReturningToWaitingScene()
    {
        matchStarted = false;
        yield return new WaitForSeconds(10f);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LoadLevel("WaitingScene");
    }
}
