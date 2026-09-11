using UnityEngine;
using Photon;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using ExitGames.Client.Photon;
public class RoomCreator : MonoBehaviourPun
{
    [SerializeField] private MainMenuPhotonManager phMan;
    [SerializeField] private GameObject createCanvas;
    [SerializeField] private TMP_InputField roomName;
    [SerializeField] private TMP_InputField roomPassword;

    public void RoomCreation()
    {
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 4
        };

        if (!string.IsNullOrEmpty(roomPassword.text))
        {
            Debug.Log($"{roomName.text} tiene password");

            options.CustomRoomProperties = new Hashtable
            {
                { "password", roomPassword.text },
            };
            
            options.CustomRoomPropertiesForLobby = new[]
            {
                "password"
            };
        }
        phMan.CreateRoom(roomName.text, options, createCanvas);
    }
}
