using Photon.Pun;
using Unity.Multiplayer.PlayMode;
using UnityEngine;

public class ShieldPower : MonoBehaviourPun
{
    [SerializeField] private float shieldDuration = 5f;
    private float currentTime = 0;
    private bool isDestroyed = false;

    [SerializeField] private LayerMask playerDetectionLayer;

    private void Update()
    {
        if (!photonView.IsMine) return;
        currentTime += Time.deltaTime;

        if (currentTime > shieldDuration)
        {
            DestroyShield();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        if ((playerDetectionLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            Debug.Log("Shield adquierido por" + other.name);
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            player.AddShield();
        }

        DestroyShield();
    }

    private void DestroyShield()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        PhotonNetwork.Destroy(gameObject);
    }

}
