using Photon.Pun;
using UnityEngine;

public class CrownController : MonoBehaviourPun
{
    [Header("Crown configuration")]
    [SerializeField] private float afterDropCD;
    private float currentTime;
    private bool canTakeCrown = true;

    [SerializeField] private LayerMask playerDetectionLayer;

    public void ResetCrown()
    {
        canTakeCrown = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canTakeCrown)
        {
            if(currentTime > afterDropCD)
            {
                currentTime += Time.deltaTime;
            }
            else
            {
                canTakeCrown = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerDetectionLayer)
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            player.CallAddCrown();
            photonView.RPC(nameof(DestroyCrown), RpcTarget.All);
        }
    }

    [PunRPC]
    private void DestroyCrown()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
