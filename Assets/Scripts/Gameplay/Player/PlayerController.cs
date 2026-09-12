using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class PlayerController : MonoBehaviourPun
{
    private GameplayPhotonManager phMan;
    private WaitingPhotonManager phWait;
    private GameManager gm;
    private float rtPoints;
    private int points;
    private int ID;

    [Header("Movement configuration")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpForces;
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private Transform checkFloor;
    [SerializeField] private float checkFloorDistance;
    private float moveDirection;
    private bool isFacingRight = true;
    private bool canJump = false;
    private Rigidbody rb;

    [Header("Render")]
    [SerializeField] private Renderer render;
    private Material material;


    [Header("Ray")]
    [SerializeField] private GameObject ray;
    [SerializeField] private GameObject raySpawn;

    [Header("Ray cooldown")]
    [SerializeField] private float attackCD;
    private float counterCD;

    [Header("Crown")]
    public Transform crownPosition;
    private bool haveCrown;


    public PlayerController SpawnAndWait(WaitingPhotonManager ph)
    {
        rb = gameObject.GetComponent<Rigidbody>();

        phWait = ph;

        return this;
    }

    public void Initialize(GameplayPhotonManager phMan, GameManager gm, int ID)
    {
        rb = gameObject.GetComponent<Rigidbody>();

        this.phMan = phMan;
        this.gm = gm;
        this.ID = ID;

        material = gm.PlayerMaterials[ID];
        render.material = gm.PlayerMaterials[ID];

        Debug.Log(gm);
        Debug.Log(gameObject.name + " has join the party");
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        if (haveCrown)
        {
            AddPoint();
        }

        moveDirection = Input.GetAxisRaw("Horizontal");
        counterCD += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.W))
        { 
            Attack();
        }

        if (moveDirection > 0f && !isFacingRight)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f); // mirando a la derecha
            isFacingRight = true;
        }
        else if (moveDirection < 0f && isFacingRight)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f); // mirando a la izquierda
            isFacingRight = false;
        }

        canJump = Physics.CheckSphere(checkFloor.position, checkFloorDistance, floorLayer);
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        if (rb == null) return;

        Move();
        Jump();
    }

    public void Move()
    {
        Vector3 movement = new Vector3(moveDirection, 0f, 0f) * speed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, rb.linearVelocity.z);
    }

    public void Jump()
    {
        if (canJump)
        {
            rb.AddForce(Vector3.up * jumpForces, ForceMode.Impulse);
        }
    }

    public void Attack()
    {
        if (Input.GetMouseButtonDown(0) && counterCD > attackCD)
        {
            counterCD = 0;
            RayMovement attack = phMan.ReturnSpawnedObject(ray.name, raySpawn.transform.position, raySpawn.transform.rotation).GetComponent<RayMovement>();
            attack.owner = this;
        }
    }

    private void AddPoint()
    {
        rtPoints += Time.deltaTime;
        points = Convert.ToInt32(rtPoints);

        if(points >= gm.WinningPoints)
        {
            gm.WinCondition(points, this);
        }
    }

    public void CallAddCrown()
    {
        photonView.RPC(nameof(AddCrown), RpcTarget.All);
    }

    public void CallQuitCrown()
    {
        photonView.RPC(nameof(QuitCrown), RpcTarget.All);
    }

    private void OnDrawGizmosSelected()
    {
        if (checkFloor == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(checkFloor.position, checkFloorDistance);
    }

    #region RPCMethods
    [PunRPC]
    private void AddCrown()
    {
        haveCrown = true;
    }

    [PunRPC]
    private void QuitCrown()
    {
        haveCrown = false;
    }

    private void OnApplicationQuit()
    {
        photonView.RPC(nameof(gm.PlayerQuitParty), RpcTarget.All, this);
    }
    #endregion
}
