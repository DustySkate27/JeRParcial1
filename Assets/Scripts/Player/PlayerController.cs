using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviourPun
{
    private GameplayPhotonManager phMan;
    private WaitingPhotonManager phWait;
    private GameManager gm;
    private WaitManager wm;
    private bool gameScene = false;

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
    private bool jump = false;
    private Rigidbody rb;

    [Header("Render")]
    [SerializeField] private Renderer render;

    [Header("Laser")]
    [SerializeField] private GameObject laser;
    [SerializeField] private GameObject laserSpawn;
    [SerializeField] private float attackCD;
    private float counterCD;

    [Header("Crown")]
    public Transform crownPosition;
    private bool haveCrown;


    public void InitializeWait(WaitingPhotonManager ph, WaitManager wm, int ID)
    {
        rb = gameObject.GetComponent<Rigidbody>();

        phWait = ph;
        this.wm = wm;
        this.ID = ID;
        gameScene = false;

        render.material = wm.PlayerMaterials[ID];

        Debug.Log(gm);
        Debug.Log(gameObject.name + " has join the party");
    }

    public void InitializeGame(GameplayPhotonManager ph, GameManager gm, int ID)
    {
        rb = gameObject.GetComponent<Rigidbody>();

        phMan = ph;
        this.gm = gm;
        this.ID = ID;
        gameScene = true;

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

        if (Input.GetMouseButtonDown(0) && counterCD > attackCD)
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            jump = true;
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
        if (canJump && jump)
        {
            Jump();
        }

    }

    public void Move()
    {
        Vector3 movement = new Vector3(moveDirection, 0f, 0f) * speed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, rb.linearVelocity.z);
    }

    public void Jump()
    {
        rb.AddForce(Vector3.up * jumpForces, ForceMode.Impulse);
        jump = false;
    }

    public void Attack()
    {
        counterCD = 0;
        if (gameScene)
        {
            LaserMovement attack = phMan.ReturnSpawnedObject(laser.name, laserSpawn.transform.position, laserSpawn.transform.rotation).GetComponent<LaserMovement>();
            attack.owner = this;
        }
        else
        {
            LaserMovement attack = phWait.ReturnSpawnedObject(laser.name, laserSpawn.transform.position, laserSpawn.transform.rotation).GetComponent<LaserMovement>();
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
