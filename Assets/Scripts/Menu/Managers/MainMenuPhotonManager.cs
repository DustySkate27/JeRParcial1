using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using UnityEngine.Device;
public class MainMenuPhotonManager : MonoBehaviourPunCallbacks
{
    [Header("Scene Objects")]
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private GameObject errorCanvas;
    [SerializeField] private TextMeshProUGUI errorText;

    private Dictionary<string, RoomInfo> roomsDic = new Dictionary<string, RoomInfo>();

    private void Awake()
    {
        loadingCanvas.SetActive(true);
        mainCanvas.SetActive(false);
        errorCanvas.SetActive(false);
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();  
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        loadingCanvas.SetActive(false);
        mainCanvas.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        UpdateRoomsDic(roomList);
    }

    private void UpdateRoomsDic(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                roomsDic.Remove(info.Name);
            }
            else
            {
                roomsDic[info.Name] = info;
            }
        }
    }

    #region Project Methods
    /// <summary>
    /// Method used for the transitions.
    /// </summary>
    /// <param name="screen"></param>
    private void OnLoadingRoom(GameObject screen)
    {
        screen.SetActive(false);
        loadingCanvas.SetActive(true);
    }

    /// <summary>
    /// Creates a new room. Screen refers to the current canvas.
    /// To make a private room, configure the options, creating a CustomProperty named "password" with an associated string and put IsVisible = false.
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="options"></param>
    /// <param name="screen"></param>
    public void CreateRoom(string roomName, RoomOptions options, GameObject screen)
    {
        if (roomsDic.ContainsKey(roomName))
        {
            StartCoroutine(ErrorExistingRoom(screen));
        }
        else
        {
            OnLoadingRoom(screen);
            PhotonNetwork.CreateRoom(roomName, options);
        }
    }

    /// <summary>
    /// Checks if a room with this name exists. If it does, it checks if it was visible. Non-Visible rooms are the ones which have password. 
    /// To access Non-Visible rooms, add password as parameter. Screen refers to the current canvas.
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="screen"></param>
    public void LoadRoom(string roomName, GameObject screen)
    {
        Debug.Log(roomsDic[roomName].Name);
        if(roomsDic.TryGetValue(roomName, out RoomInfo info) && info.PlayerCount < info.MaxPlayers)
        {
            Debug.Log("Entré asi nomas");
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            StartCoroutine(ErrorFullOrInexistent(screen));
        }
    }

    /// <summary>
    /// Checks if a room with this name exists. If it does, compares password values. Screen refers to the current canvas.
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="password"></param>
    public void LoadRoom(string roomName, string password, GameObject screen)
    {
        if (roomsDic.TryGetValue(roomName, out RoomInfo info) && info.PlayerCount < info.MaxPlayers)
        {
            Debug.Log("Entré con password");
            if ((string)info.CustomProperties["password"] == password)
                PhotonNetwork.JoinRoom(roomName);
            else
                StartCoroutine(ErrorIncorrectPassword(screen));   
        }
        else
        {
            StartCoroutine(ErrorFullOrInexistent(screen));
        }
    }


    /// <summary>
    /// Error used in case a room creation input is the same as an already existing room. Screen refers to the current canvas.
    /// </summary>
    /// <param name="screen"></param>
    private IEnumerator ErrorExistingRoom(GameObject screen)
    {
        screen.SetActive(false);
        errorText.text = "This room already exists.";
        errorCanvas.SetActive(true);

        yield return new WaitForSeconds(2f);

        errorCanvas.SetActive(false);
        mainCanvas.SetActive(true);
    }

    /// <summary>
    /// Error used in case a room doesn't exist or it's full. Screen refers to the current canvas.
    /// </summary>
    /// <param name="screen"></param>
    private IEnumerator ErrorFullOrInexistent(GameObject screen)
    {
        screen.SetActive(false);
        errorText.text = "The room doesn't exist anymore or it's already full.";
        errorCanvas.SetActive(true);

        yield return new WaitForSeconds(2f);

        errorCanvas.SetActive(false);
        mainCanvas.SetActive(true);
    }

    /// <summary>
    /// Error used in case the password is incorrect. Screen refers to the current canvas.
    /// </summary>
    /// <param name="screen"></param>
    private IEnumerator ErrorIncorrectPassword(GameObject screen)
    {
        screen.SetActive(false);
        errorText.text = "Incorrect password.";
        errorCanvas.SetActive(true);

        yield return new WaitForSeconds(2f);

        errorCanvas.SetActive(false);
        mainCanvas.SetActive(true);
    }

    

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("sala creada");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        PhotonNetwork.LeaveLobby();
        Debug.Log("unido a sala");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);

        loadingCanvas.SetActive(false);
        errorText.text = "unexpected error : RoomCreationFailed";
        errorCanvas.SetActive(true);

        Debug.LogError($"Create Room Failed | Code: {returnCode} | {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);

        loadingCanvas.SetActive(false);
        errorText.text = "unexpected error : RoomJoinFailed";
        errorCanvas.SetActive(true);

        Debug.LogError($"Create Room Failed | Code: {returnCode} | {message}");
    }

    #endregion
}
