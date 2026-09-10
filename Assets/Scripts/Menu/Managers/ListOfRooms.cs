using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;

public class ListOfRooms : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject container;

    [SerializeField] private RoomJoiner joiner;

    [SerializeField] private Room RoomPrefab;

    public Dictionary<string,Room> displayingRooms = new Dictionary<string, Room>();

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                if (displayingRooms.TryGetValue(info.Name, out Room roomToRemove))
                {
                    Destroy(roomToRemove.gameObject);
                    displayingRooms.Remove(info.Name);
                }

                continue;
            }

            if (info.CustomProperties.ContainsKey("password"))
            {
                continue;
            }

            if (!displayingRooms.TryGetValue(info.Name, out Room room))
            {
                room = Instantiate(RoomPrefab, Vector3.zero, Quaternion.identity, container.transform);
                room.roomName.text = info.Name;
                room.playerCount.text = info.PlayerCount.ToString();
                room.joiner = joiner;

                displayingRooms[info.Name] = room;
            }       
        }
    }
}

