using Photon.Pun;
using Unity.Multiplayer.PlayMode;
using UnityEngine;

public class ShieldPower : MonoBehaviour
{
    [SerializeField] private float shieldDuration = 5f;
    private float currentTime = 0;
    private bool isDestroyed = false;

    [SerializeField] private LayerMask playerDetectionLayer;

    private void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime > shieldDuration)
        {
            DestroyShield();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
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
