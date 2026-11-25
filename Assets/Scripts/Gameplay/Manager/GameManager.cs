using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviourPun, IPunObservable
{
    public GameplayPhotonManager phMan;
    public string modeToPlay;

    [Header("Player Related")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<GameObject> parkourModeSpawners;
    [SerializeField] private List<GameObject> laserModeSpawners;
    [SerializeField] private List<GameObject> dodgeballModeSpawners;

    [Header("Target Lasers Mode")]
    [SerializeField] private float matchDuration;
    public float currentTime;
    private bool matchStarted;

    [Header("Parkour")]
    public bool parkourWinner = false;

    [Header("Dodgeball")]

    [Header("Canvas")]
    [SerializeField] public GameObject switchingMasterCanvas;
    [SerializeField] private TextMeshProUGUI winText;


    public Dictionary<int, PlayerController> playersInMatch = new Dictionary<int, PlayerController>();

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (matchStarted)
        {
            switch (modeToPlay)
            {
                case "targetLaserMode":
                    EndConditionTargetLasers();
                    break;

                case "parkourMode":
                    if (!parkourWinner) return;
                    EndConditionParkour();
                    break;

                case "dodgeballMode":
                    EndConditionDodgeball();
                    break;
            }
        }
    }

    public void StartMatch()
    {
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


    public void SpawnPlayer(int ID)
    {
        switch (modeToPlay)
        {
            case "targetLaserMode":
                PlayerSpawnMethod(ID, laserModeSpawners);
                break;

            case "parkourMode":
                PlayerSpawnMethod(ID, parkourModeSpawners);
                break;

            case "dodgeballMode":
                PlayerSpawnMethod(ID, dodgeballModeSpawners);
                break;
        }
    }

    public void PlayerSpawnMethod(int ID, List<GameObject> spawns)
    {
        GameObject currentPlayer = phMan.ReturnSpawnedObject(playerPrefab.name, spawns[ID].transform.position, Quaternion.identity);
        PlayerController player = currentPlayer.GetComponent<PlayerController>();
        player.InitializeGame(phMan, this, ID);
        photonView.RPC(nameof(RegisterPlayer), RpcTarget.All, ID, player.photonView.ViewID);
    }

    public void EndConditionTargetLasers()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= matchDuration)
        {
            photonView.RPC(nameof(EndGame), RpcTarget.All);
            StartCoroutine(ReturningToWaitingScene());
        }
    }

    public void EndConditionParkour()
    {
        photonView.RPC(nameof(EndGame), RpcTarget.All);
        StartCoroutine(ReturningToWaitingScene());
    }

    public void EndConditionDodgeball()
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
        }
        else
        {
            currentTime = (float)stream.ReceiveNext();
            matchStarted = (bool)stream.ReceiveNext();
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

        foreach (var player in playersInMatch)
        {
            player.Value.canMove = false;

            if (player.Value.points > highestPoints)
            {
                highestPoints = player.Value.points;
                currentWinner = player.Key;
            }
        }

        if (currentWinner == -1) return;

        PlayerController winner = playersInMatch[currentWinner];

        winner.speed = 0;

        winText.text = "Lil bro wins";
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
