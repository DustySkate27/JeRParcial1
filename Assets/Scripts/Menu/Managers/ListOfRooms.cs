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

            Debug.Log($"ROOM: {info.Name}");

            foreach (object key in info.CustomProperties.Keys)
            {
                Debug.Log($"PROPERTY: {key} = {info.CustomProperties[key]}");
            }

            if (info.RemovedFromList)
            {
                if (displayingRooms.TryGetValue(info.Name, out Room roomToRemove))
                {
                    Destroy(roomToRemove.gameObject);
                    displayingRooms.Remove(info.Name);
                }

                continue;
            }

            Debug.Log(info.CustomProperties.ContainsKey("password"));

            if (info.CustomProperties.ContainsKey("password"))
            {
                Debug.Log($"{info.Name} tiene {info.CustomProperties["password"]}");

                if (displayingRooms.TryGetValue(info.Name, out Room privateRoom))
                {
                    Destroy(privateRoom.gameObject);
                    displayingRooms.Remove(info.Name);
                }
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

