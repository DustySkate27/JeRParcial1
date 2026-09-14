using Photon.Pun;
using UnityEngine;

public class CrownController : MonoBehaviourPun, IPunObservable
{
    [Header("Crown configuration")]
    [SerializeField] private float afterDropCD;
    private float currentTime = 0;
    private bool canPickDroppedCrown = true;
    public bool isCrownTaken = false;

    [SerializeField] private LayerMask playerDetectionLayer;
    [SerializeField] private LayerMask bulletDetectionLayer;

    private PlayerController currentPlayer;

    // Update is called once per frame
    void Update()
    {
        if (!canPickDroppedCrown && !isCrownTaken)
        {
            currentTime += Time.deltaTime;

            if (currentTime >= afterDropCD)
            {
                canPickDroppedCrown = true;
                currentTime = 0;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isCrownTaken && currentPlayer != null)
        {
            transform.position = currentPlayer.crownPosition.position;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isCrownTaken);
            stream.SendNext(canPickDroppedCrown);
        }
        else
        {
            isCrownTaken = (bool)stream.ReceiveNext();
            canPickDroppedCrown = (bool)stream.ReceiveNext();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if ((playerDetectionLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            if (isCrownTaken || !canPickDroppedCrown) return;

            photonView.RPC(nameof(CrownPicked), RpcTarget.All, other.gameObject.GetComponent<PhotonView>().ViewID);
            isCrownTaken = true;
            canPickDroppedCrown = false;
        }

        if (currentPlayer != null && (bulletDetectionLayer.value & (1<<other.gameObject.layer)) != 0)
        {
            if (currentPlayer.haveShield) return;

            if (!isCrownTaken) return;

            photonView.RPC(nameof(CrownDropped), RpcTarget.All);
            canPickDroppedCrown = false;
            isCrownTaken = false;
        }
    }

    [PunRPC]
    public void CrownPicked(int playerViewID)
    {
        PhotonView pv = PhotonView.Find(playerViewID);
        if (pv == null) return;

        currentPlayer = pv.GetComponent<PlayerController>();
        photonView.TransferOwnership(currentPlayer.photonView.Owner);
        currentPlayer.CallAddCrown();
    }

    [PunRPC]
    public void CrownDropped()
    {
        transform.position = currentPlayer.transform.position;
        currentPlayer.CallQuitCrown();
        photonView.TransferOwnership(PhotonNetwork.MasterClient);
        currentPlayer = null;
    }
}
