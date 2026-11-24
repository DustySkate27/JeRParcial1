using JetBrains.Annotations;
using Photon.Pun;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class WaitManager : MonoBehaviourPun
{
    [SerializeField] private WaitingPhotonManager phWait;
    //private List<string> modesToPlay = new List<string> { "targetLaserMode", "parkourMode", "dodgeballMode" };
    List<string> modesToPlay = new List<string> { "parkourMode" };

    [Header("Master Texts")]
    [SerializeField] private GameObject waitText;
    [SerializeField] private GameObject readyText;
    [SerializeField] public GameObject noMoreMinigames;

    [Header("Player Related")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<GameObject> playerSpawners;

    public bool transitionReady = false;

    public PlayerController SpawnPlayer(int ID)
    {
        GameObject currentPlayer = phWait.ReturnSpawnedObject(playerPrefab.name, playerSpawners[ID].transform.position, Quaternion.identity);
        PlayerController player = currentPlayer.GetComponent<PlayerController>();
        player.InitializeWait(phWait, this, ID);
        return player;
    }

    public void TransitionIsReady()
    {
        if(transitionReady && PhotonNetwork.IsMasterClient)
        {
            phWait.GameStartConfirmed();
        }
    }

    public string RandomSelection()
    {
        foreach(string s in modesToPlay) //Esta accion se repite siempre, porque limpia los modos ya jugados.
        {
            if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(s, out object value))
            {
                if ((bool)value)
                {
                    modesToPlay.Remove(s);
                }
            }
        }
        
        if (modesToPlay.Count == 0)
        {
            return null;
        }
        
        int index = Random.Range(0, modesToPlay.Count - 1);

        return modesToPlay[index];
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
