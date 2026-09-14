using Photon.Pun;
using UnityEngine;

public class SpeedPower : MonoBehaviourPun
{
    [SerializeField] private float speedBostTime = 1f;
    [SerializeField] private float speedBostDuration = 5f;
    private float currentTime = 0;
    private bool isDestroyed = false;

    [SerializeField] private LayerMask playerDetectionLayer;

    private void Update()
    {
        if (!photonView.IsMine) return;
        currentTime += Time.deltaTime;

        if (currentTime > speedBostDuration)
        {
            DestroySpeed();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        if ((playerDetectionLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            Debug.Log("SpeedBost adquierido por" + other.name);
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            player.AddSpeedBost(speedBostTime);
        }

        DestroySpeed();
    }

    private void DestroySpeed()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        PhotonNetwork.Destroy(gameObject);
    }
}
