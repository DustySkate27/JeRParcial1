using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomJoiner : MonoBehaviourPun
{
    [SerializeField] private GameObject joinCanvas;
    [SerializeField] private TMP_InputField roomName;
    [SerializeField] private TMP_InputField roomPassword;


    public void PrivateRoomJoin()
    {
        MainMenuPhotonManager.Instance.LoadRoom(roomName.text, roomPassword.text, joinCanvas);
    }
}
