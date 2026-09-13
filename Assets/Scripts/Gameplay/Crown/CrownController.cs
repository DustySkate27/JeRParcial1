using Photon.Pun;
using UnityEngine;

public class CrownController : MonoBehaviourPun
{
    [Header("Crown configuration")]
    [SerializeField] private float afterDropCD;
    private float currentTime = 0;
    private bool canPickDroppedCrown = true;
    private bool isCrownTaken = false;

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
        if (isCrownTaken)
        {
            transform.position = currentPlayer.crownPosition.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("colisione con algo");

        // Comprobación correcta con bitmask
        if ((playerDetectionLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            if (isCrownTaken) return;
            if (!canPickDroppedCrown) return;

            Debug.Log("El player me toco");
            canPickDroppedCrown = false;
            isCrownTaken = true;
            currentPlayer = other.gameObject.GetComponent<PlayerController>();
            currentPlayer.CallAddCrown();
        }

        if (currentPlayer != null && (bulletDetectionLayer.value & (1<<other.gameObject.layer)) != 0)
        {
            if (currentPlayer.haveShield) return;

            canPickDroppedCrown = false;
            isCrownTaken = false;
            currentPlayer.CallQuitCrown();
        }
    }
}
