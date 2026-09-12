using UnityEngine;
using Photon;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Unity.VisualScripting;
public class RoomCreator : MonoBehaviourPun
{
    [SerializeField] private MainMenuPhotonManager phMan;
    [SerializeField] private GameObject createCanvas;
    [SerializeField] private TMP_InputField roomName;
    [SerializeField] private TMP_InputField roomPassword;

    public void RoomCreation()
    {
        bool hasPassword = !string.IsNullOrEmpty(roomPassword.text);

        Hashtable customProps = null;

        customProps = new Hashtable
        {
            { "isPrivate", hasPassword },
            { "password", roomPassword.text }
        };

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 4,
            CustomRoomProperties = customProps,
            CustomRoomPropertiesForLobby = new string[] { "isPrivate" }
        };

        
        phMan.CreateRoom(roomName.text, options, createCanvas);
    }
}
