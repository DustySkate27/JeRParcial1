using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    private GameplayPhotonManager phMan;
    private WaitingPhotonManager phWait;
    private GameManager gm;
    private WaitManager wm;
    private bool waitScene = false;

    private float rtPoints;
    public int points = 0;
    public int ID;

    [SerializeField] private LayerMask deathZoneLayer;
    [SerializeField] private LayerMask winZoneLayer;

    [Header("Movement configuration")]
    [SerializeField] public float speed;
    [SerializeField] private float jumpForces;
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private Transform checkFloor;
    [SerializeField] private float checkFloorDistance;
    public bool canMove = true;
    private float hMove;
    private float vMove;
    private bool canJump = false;
    private bool jump = false;
    private Rigidbody rb;

    [Header("Render")]
    [SerializeField] private Renderer render;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private List<GameObject> playerMaterials;

    [Header("Laser")]
    [SerializeField] private GameObject laser;
    [SerializeField] private GameObject laserSpawn;
    [SerializeField] private float attackCD;
    private float counterCD;

    public void InitializeWait(WaitingPhotonManager ph, WaitManager wm, int ID)
    {
        if (!photonView.IsMine) return;

        rb = gameObject.GetComponent<Rigidbody>();

        phWait = ph;
        this.wm = wm;
        this.ID = ID;
        waitScene = true;
        photonView.RPC(nameof(ColorForPlayer), RpcTarget.AllBuffered, ID);
    }

    public void InitializeGame(GameplayPhotonManager ph, GameManager gm, int ID)
    {
        if (!photonView.IsMine) return;

        rb = gameObject.GetComponent<Rigidbody>();

        phMan = ph;
        this.gm = gm;
        this.ID = ID;
        waitScene = false;
        photonView.RPC(nameof(ColorForPlayer), RpcTarget.AllBuffered, ID);
    }

    private void Start()
    {
        if (!photonView.IsMine) return;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        if (!canMove) return;

        if(waitScene == true && Input.GetKeyDown(KeyCode.F))
        {
            wm.TransitionIsReady();
        }

        hMove = Input.GetAxisRaw("Horizontal");
        vMove = Input.GetAxisRaw("Vertical");

        counterCD += Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && counterCD > attackCD && gm.modeToPlay == "targetLaserMode")
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump = true;
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
        Vector3 moveDir = (transform.right * hMove + transform.forward * vMove);

        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        Vector3 velocity = moveDir * speed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;
    }
    public void Jump()
    {
        rb.AddForce(Vector3.up * jumpForces, ForceMode.Impulse);
        jump = false;
    }

    public void Attack()
    {
        counterCD = 0;
        if (!waitScene)
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
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(points);
        }
        else
        {
            points = (int)stream.ReceiveNext();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (checkFloor == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(checkFloor.position, checkFloorDistance);
    }

    #region RPCMethods
    [PunRPC]
    private void ColorForPlayer(int id)
    {
        playerMaterials[id].SetActive(true);
    }

    [PunRPC]
    private void PlayerDeath(int id)
    {
        if (!photonView.IsMine) return;
        Destroy(gameObject);
        gm.SpawnPlayer(id);
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if ((winZoneLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            gm.photonView.RPC(nameof(gm.PlayerWinsParkour), RpcTarget.All);
        }

        if ((deathZoneLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            photonView.RPC(nameof(PlayerDeath), RpcTarget.All, ID);
        }
    }


}
