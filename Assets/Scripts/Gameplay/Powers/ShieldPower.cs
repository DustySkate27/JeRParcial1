using Photon.Pun;
using Unity.Multiplayer.PlayMode;
using UnityEngine;

public class ShieldPower : MonoBehaviour
{
    [SerializeField] private float shieldDuration = 5f;
    private float currentTime = 0;

    [SerializeField] private LayerMask playerDetectionLayer;

    private void Update

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("colisione con algo");

        // Comprobación correcta con bitmask
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
        PhotonNetwork.Destroy(gameObject);
    }

}
