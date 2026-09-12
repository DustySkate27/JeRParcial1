using Photon.Pun;
using UnityEngine;

public class LaserMovement : MonoBehaviourPun
{
    public PlayerController owner;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed;
    [SerializeField] private float timeLimit = 2;
    private float currentTime = 0;

    [SerializeField] private LayerMask wallDetectionLayer;

    void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime > timeLimit)
        {
            DestroyLaser();
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

        DestroyLaser();
    }

    private void DestroyLaser()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
