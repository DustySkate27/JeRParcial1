using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using UnityEngine.Device;
public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance;

    [Header("Scene Objects")]
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private GameObject errorCanvas;
    [SerializeField] private TextMeshProUGUI errorText;

    private Dictionary<string, RoomInfo> roomsDic = new Dictionary<string, RoomInfo>();
    private int playerLimit = 4;

    private void Awake()
    {
        if(Instance != null & Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        loadingCanvas.SetActive(true);
        mainCanvas.SetActive(false);
        errorCanvas.SetActive(false);
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
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
        if(roomsDic.TryGetValue(roomName, out RoomInfo roomInfo) && roomInfo.PlayerCount < playerLimit)
        {
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
        if (roomsDic.TryGetValue(roomName, out RoomInfo roomInfo) && roomInfo.PlayerCount < playerLimit)
        {
            if ((string)roomInfo.CustomProperties["password"] == password)
                PhotonNetwork.JoinRoom(roomName);
            else
                StartCoroutine(ErrorIncorrectPassword(screen));   
        }
        else
        {
            StartCoroutine(ErrorFullOrInexistent(screen));
        }
    }

    private void OnLoadingRoom(GameObject screen)
    {
        screen.SetActive(false);
        loadingCanvas.SetActive(true);
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

    #endregion
}
