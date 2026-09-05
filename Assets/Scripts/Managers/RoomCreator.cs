using UnityEngine;
using Photon;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using ExitGames.Client.Photon;
public class RoomCreator : MonoBehaviourPun
{
    [SerializeField] private GameObject createCanvas;
    [SerializeField] private TMP_InputField roomName;
    [SerializeField] private TMP_InputField roomPassword;

    private const string password = "password";

    public void RoomCreation()
    {
        if (!string.IsNullOrEmpty(roomPassword.text))
        {
            RoomOptions options = new RoomOptions
            {
                CustomRoomProperties = new Hashtable
                {
                    { password, roomPassword.text}
                },
                IsVisible = false
            };

            PhotonManager.Instance.CreateRoom(roomName.text, options, createCanvas);
        }
        else
            PhotonManager.Instance.CreateRoom(roomName.text, default, createCanvas);
    }
}
