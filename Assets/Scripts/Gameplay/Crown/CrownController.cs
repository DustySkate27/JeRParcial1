using Photon.Pun;
using UnityEngine;

public class CrownController : MonoBehaviourPun
{
    [Header("Crown configuration")]
    [SerializeField] private float afterDropCD;
    private float currentTime;
    private bool canPickDroppedCrown = true;
    private bool isCrownTaken = false;

    //[SerializeField] private LayerMask playerDetectionLayer;
    [SerializeField] private LayerMask bulletDetectionLayer;

    private PlayerController currentPlayer;

    // Update is called once per frame
    void Update()
    {
        if (!canPickDroppedCrown)
        {
            currentTime += Time.deltaTime;

            if (currentTime >= afterDropCD)
            {
                canPickDroppedCrown = true;
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
        if (!canPickDroppedCrown) return;

        Debug.Log("colisione con algo");

        // Comprobación correcta con bitmask
        if ((playerDetectionLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            Debug.Log("El player me toco");
            canPickDroppedCrown = false;
            currentPlayer = other.gameObject.GetComponent<PlayerController>();
            currentPlayer.CallAddCrown();
        }

        if ((bulletDetectionLayer.value & (1<<other.gameObject.layer)) != 0)
        {
            canPickDroppedCrown = false;
            currentPlayer.CallQuitCrown();
        }
    }
}
