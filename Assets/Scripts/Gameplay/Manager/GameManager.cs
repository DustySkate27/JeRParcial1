using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviourPun, IPunObservable
{
    public GameplayPhotonManager phMan;

    [Header("Camera Related")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform orignalPosition;

    [Header("Player Related")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<GameObject> playerSpawners;

    [Header("WinCondition")]
    [SerializeField] private float matchDuration;
    public float currentTime;
    private bool matchStarted;

    [Header("Canvas")]
    [SerializeField] public GameObject switchingMasterCanvas;

    [Header("Crown Related")]
    [SerializeField] private GameObject crownPrefab;
    [SerializeField] private GameObject crownSpawner;
    public CrownController crownController;

    [Header("Powers")]
    [SerializeField] private GameObject speedPrefab;
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private GameObject shieldSpawner;
    [SerializeField] private GameObject speedSpawner;
    private float spawnCooldown = 15f;
    private float timeToSpawnPowers = 0;

    public Dictionary<int, PlayerController> playersInMatch = new Dictionary<int, PlayerController>();

    private void Start()
    {
        SpawnPowers();
        timeToSpawnPowers = 0;
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (matchStarted)
        {
            currentTime += Time.deltaTime;
            EndCondition();
        }

        if(timeToSpawnPowers < spawnCooldown)
        {
            timeToSpawnPowers += Time.deltaTime;
        }
        else
        {
            SpawnPowers();
            timeToSpawnPowers = 0;
        }

    }

    public void StartMatch()
    {
        var crown = phMan.ReturnSpawnedRoomObject(crownPrefab.name, crownSpawner.transform.position, Quaternion.identity);
        int crownViewID = crown.GetComponent<PhotonView>().ViewID;

        photonView.RPC(nameof(RegisterCrown), RpcTarget.All, crownViewID);
        matchStarted = true;
    }

    private void PauseGame()
    {
        Time.timeScale = 0;
        switchingMasterCanvas.SetActive(true);
    }

    private void UnpauseGame()
    {
        Time.timeScale = 1;
        switchingMasterCanvas.SetActive(false);
    }

    private void SpawnPowers()
    {
        phMan.ReturnSpawnedRoomObject(speedPrefab.name, speedSpawner.transform.position, Quaternion.identity);
        phMan.ReturnSpawnedRoomObject(shieldPrefab.name, shieldSpawner.transform.position, Quaternion.identity);
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentTime);
            stream.SendNext(matchStarted);
            stream.SendNext(timeToSpawnPowers);
        }
        else
        {
            currentTime = (float)stream.ReceiveNext();
            matchStarted = (bool)stream.ReceiveNext();
            timeToSpawnPowers = (float)stream.ReceiveNext();
        }
    }

    #region RPCMethods

    [PunRPC]
    public void RegisterCrown(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv == null) return;

        crownController = pv.GetComponent<CrownController>();
    }

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

        foreach(var player in playersInMatch)
        {
            player.Value.canMove = false;
            player.Value.CallQuitCrown();

            if (player.Value.points > highestPoints)
            {
                highestPoints = player.Value.points;
                currentWinner = player.Key;
            }
        }

        if (currentWinner == -1) return;

        PlayerController winner = playersInMatch[currentWinner];
        winner.transform.position = crownSpawner.transform.position;
        crownController.isCrownTaken = false;
        
        Vector3 position = new Vector3(winner.cameraWinTransform.position.x, winner.cameraWinTransform.position.y, mainCamera.transform.position.z);
        winner.speed = 0;
        mainCamera.transform.position = position;
    }

    [PunRPC]
    public void PauseGameRPC()
    {
        PauseGame();
    }

    [PunRPC]
    public void UnpauseGameRPC()
    {
        UnpauseGame();
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
