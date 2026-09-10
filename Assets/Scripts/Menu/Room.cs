using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
    public TextMeshProUGUI roomName;
    public TextMeshProUGUI playerCount;
    public RoomJoiner joiner;

    public void JoinRoom()
    {
        joiner.JoinPublicRoom(roomName.text);
    }

}
