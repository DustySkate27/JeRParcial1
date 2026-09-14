using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;
using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    private GameplayPhotonManager phMan;
    private WaitingPhotonManager phWait;
    private GameManager gm;
    private WaitManager wm;
    private bool gameScene = false;

    private float rtPoints;
    public int points = 0;
    public int ID;

    public bool haveShield = false;
    public bool haveSpeedBost = false;
    private float speedBostDuration;
    [SerializeField] private float speedMult = 2f;

    [SerializeField] private LayerMask bulletDetectionLayer;

    [Header("Movement configuration")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpForces;
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private Transform checkFloor;
    [SerializeField] private float checkFloorDistance;
    public bool canMove = true;
    private float moveDirection;
    private bool isFacingRight = true;
    private bool canJump = false;
    private bool jump = false;
    private Rigidbody rb;

    [Header("Render")]
    [SerializeField] private Renderer render;
    [SerializeField] private GameObject playerModel;

    [Header("Laser")]
    [SerializeField] private GameObject laser;
    [SerializeField] private GameObject laserSpawn;
    [SerializeField] private float attackCD;
    private float counterCD;

    [Header("Crown")]
    public Transform crownPosition;
    private bool haveCrown;

    [Header("Camera Winning Position")]
    [SerializeField] public Transform cameraWinTransform;

    [Header("Canvas")]
    [SerializeField] public TextMeshProUGUI pointsUI;

    public void InitializeWait(WaitingPhotonManager ph, WaitManager wm, int ID)
    {
        if (!photonView.IsMine) return;

        rb = gameObject.GetComponent<Rigidbody>();

        phWait = ph;
        this.wm = wm;
        this.ID = ID;
        gameScene = false;

        pointsUI.text = points.ToString();
    }

    public void InitializeGame(GameplayPhotonManager ph, GameManager gm, int ID)
    {
        if (!photonView.IsMine) return;

        rb = gameObject.GetComponent<Rigidbody>();

        phMan = ph;
        this.gm = gm;
        this.ID = ID;
        gameScene = true;
    }

    private void Start()
    {
        photonView.RPC(nameof(ColorForPlayer), RpcTarget.All);
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        if (!canMove) return;

        if (haveSpeedBost)
        {
            speedBostDuration -= Time.deltaTime;

            if (speedBostDuration < 0)
            {
                haveSpeedBost = false;
            }
        }

        if(gameScene == false && Input.GetKeyDown(KeyCode.S))
        {
            wm.TransitionIsReady();
        }

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
            playerModel.transform.rotation = Quaternion.Euler(0f, 0f, 0f); // mirando a la derecha
            isFacingRight = true;
        }
        else if (moveDirection < 0f && isFacingRight)
        {
            playerModel.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // mirando a la izquierda
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
        Vector3 movement;

        if (haveSpeedBost)
        {
            movement = new Vector3(moveDirection, 0f, 0f) * speed * speedMult;
        }
        else
        {
            movement = new Vector3(moveDirection, 0f, 0f) * speed;
        }

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
        pointsUI.text = points.ToString();
    }

    public void CallAddCrown()
    {
        photonView.RPC(nameof(AddCrown), RpcTarget.All);
    }

    public void CallQuitCrown()
    {
        photonView.RPC(nameof(QuitCrown), RpcTarget.All);
    }

    public void AddShield()
    {
        photonView.RPC(nameof(ActiveShield), RpcTarget.All);
    }

    public void QuitShield()
    {
        photonView.RPC(nameof(DeactiveShield), RpcTarget.All);
    }

    public void AddSpeedBost(float speedDuration)
    {
        speedBostDuration = speedDuration;
        haveSpeedBost = true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(points);
            stream.SendNext(isFacingRight);
        }
        else
        {
            points = (int)stream.ReceiveNext();
            pointsUI.text = points.ToString();

            bool facingRight = (bool)stream.ReceiveNext();
            if (facingRight != isFacingRight)
            {
                isFacingRight = facingRight;
                if(isFacingRight)
                {
                    playerModel.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }
                else
                {
                    playerModel.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
            }
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
    private void ColorForPlayer()
    {
        if(gameScene)
            render.material = gm.PlayerMaterials[ID];
        else
            render.material = wm.PlayerMaterials[ID];
    }

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

    [PunRPC]
    private void ActiveShield()
    {
        haveShield = true;
        Debug.Log(this + "Have Shield" +  haveShield);
    }

    [PunRPC]
    private void DeactiveShield()
    {
        haveShield = false;
        Debug.Log(this + "Have Shield" + haveShield);
    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if ((bulletDetectionLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            if (haveShield)
            {
                Debug.Log("Shield quitado por" + other.name);
                QuitShield();
            }
        }
    }

    
}
