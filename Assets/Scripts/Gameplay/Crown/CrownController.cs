using Photon.Pun;
using UnityEngine;

public class CrownController : MonoBehaviourPun
{
    [Header("Crown configuration")]
    [SerializeField] private float afterDropCD;
    private float currentTime;
    private bool canTakeCrown = true;

    [SerializeField] private LayerMask playerDetectionLayer;

    private void Start()
    {
        ResetCrown();
    }

    public void ResetCrown()
    {
        canTakeCrown = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canTakeCrown)
        {
            currentTime += Time.deltaTime;

            if (currentTime >= afterDropCD)
            {
                canTakeCrown = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("colisione con algo");

        // Comprobación correcta con bitmask
        if ((playerDetectionLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            if (!canTakeCrown) return;

            Debug.Log("El player me toco");
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
