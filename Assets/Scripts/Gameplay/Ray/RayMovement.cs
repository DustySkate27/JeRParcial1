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
        currentTime += Time.deltaTime;

        if (currentTime > timeLimit)
        {
            photonView.RPC(nameof(DestroyBullet), RpcTarget.All);
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if ((wallDetectionLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            Debug.Log(other.gameObject.name);
        }
        else
        {
            var collision = other.gameObject.GetComponent<PhotonView>();

            if (collision != null && !collision.IsMine)
            {
                if ((playerDetectionLayer.value & (1 << other.gameObject.layer)) != 0)
                {
                    PlayerController player = other.gameObject.GetComponent<PlayerController>();
                    Debug.Log(other.gameObject.name + "Ray collision");
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
