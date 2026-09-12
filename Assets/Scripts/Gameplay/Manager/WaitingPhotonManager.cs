using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class WaitingPhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private PlayerController player;
    [SerializeField] private List<GameObject> spawners;

    private void Awake()
    {
        PhotonNetwork.Instantiate(player.SpawnAndWait(this).name, spawners[PhotonNetwork.CurrentRoom.PlayerCount - 1].transform.position, Quaternion.identity);
    }
}
