using Photon.Pun;
using Photon.Realtime;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class RoomJoiner : MonoBehaviourPun
{
    [SerializeField] private MainMenuPhotonManager phMan;
    [SerializeField] private GameObject joinCanvas;
    [SerializeField] private TMP_InputField roomName;
    [SerializeField] private TMP_InputField roomPassword;


    public void PrivateRoomJoin()
    {
        phMan.LoadRoom(roomName.text, roomPassword.text, joinCanvas);
    }
}
