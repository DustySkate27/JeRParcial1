using Photon.Pun;
using UnityEngine;

public class RayMovement : MonoBehaviourPun
{
    public PlayerController owner;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed;
    [SerializeField] private float timeLimit = 2;
    private float currentTime = 0;

    [SerializeField] private LayerMask playerDetectionLayer;
    [SerializeField] private LayerMask wallDetectionLayer;

    void Update()
    {
        rb.linearVelocity = transform.forward * speed;
        currentTime += Time.deltaTime;

        if (currentTime > timeLimit)
        {
            photonView.RPC(nameof(DestroyBullet), RpcTarget.All);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;

        if (collision.gameObject.layer == wallDetectionLayer)
        {
            Debug.Log(collision.gameObject.name);
        }
        else
        {
            var other = collision.gameObject.GetComponent<PhotonView>();
            if (!other.IsMine)
            {
                if (collision.gameObject.layer == playerDetectionLayer)
                {
                    PlayerController player = collision.gameObject.GetComponent<PlayerController>();
                    Debug.Log(collision.gameObject.name + "Ray collision");
                    player.CallQuitCrown();
                }
            }
        }

        photonView.RPC(nameof(DestroyBullet), RpcTarget.All);
    }

    [PunRPC]
    public void DestroyBullet()
    {
        owner.gm.pm.DestroyObject(gameObject);
    }
}
