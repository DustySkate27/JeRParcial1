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

    public void JoinPrivateRoom()
    {
        if (!string.IsNullOrEmpty(roomPassword.text))
        {
            phMan.LoadRoom(roomName.text, roomPassword.text, joinCanvas);
        }
        else
        {
            phMan.LoadRoom(roomName.text, joinCanvas);
        }
    }

    public void JoinPublicRoom(string roomName)
    {
        Debug.Log("Hasta aca bien");
        phMan.LoadRoom(roomName, joinCanvas);
    }
}
