using Photon.Pun;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
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
    private float networkYaw; // Rotación Y del modelo recibida por red (jugadores remotos)
    private const float remoteRotationSpeed = 720f; // grados/seg, solo para suavizar a los remotos

    [Header("Username")]
    [SerializeField] private TextMeshProUGUI userText;
    private Camera cam;

    [Header("Laser")]
    [SerializeField] private GameObject laser;
    [SerializeField] private GameObject laserSpawn;
    [SerializeField] private float attackCD;

    [Header("Animations")]
    [SerializeField] private Animator[] playerAnimators;

    private Animator activeAnimator;
    private float counterCD;

    public void InitializeWait(WaitingPhotonManager ph, WaitManager wm, int ID)
    {
        if (!photonView.IsMine) return;

        rb = gameObject.GetComponent<Rigidbody>();

        phWait = ph;
        this.wm = wm;
        this.ID = ID;
        waitScene = true;
    }

    public void InitializeGame(GameplayPhotonManager ph, GameManager gm, int ID)
    {
        if (!photonView.IsMine) return;

        rb = gameObject.GetComponent<Rigidbody>();

        phMan = ph;
        this.gm = gm;
        this.ID = ID;
        waitScene = false;
    }

    private void Start()
    {
        cam = Camera.main;

        if (playerModel != null)
            networkYaw = playerModel.transform.eulerAngles.y;

        // Se ejecuta para TODOS los jugadores (antes del return de IsMine)
        UpdateUserName();

        if (!photonView.IsMine) return;
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        if (!canMove) return;

        if (waitScene == true && Input.GetKeyDown(KeyCode.F))
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

    private void LateUpdate()
    {
        if (cam == null) cam = Camera.main;

        // Rotación del modelo
        if (playerModel != null)
        {
            if (photonView.IsMine)
            {
                RotateModelToCamera();
            }
            else
            {
                ApplyRemoteRotation();
            }
        }

        // El nombre siempre mira a la cámara
        if (userText != null && cam != null)
        {
            userText.transform.rotation = cam.transform.rotation;
        }
    }

    #region Rotation

    // Jugador local: el modelo mira al mismo lado que la cámara, al instante (solo eje Y)
    private void RotateModelToCamera()
    {
        if (cam == null) return;

        playerModel.transform.rotation = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f);
    }

    // Jugadores remotos: suavizan la rotación recibida por red
    private void ApplyRemoteRotation()
    {
        Quaternion target = Quaternion.Euler(0f, networkYaw, 0f);
        playerModel.transform.rotation = Quaternion.RotateTowards(
            playerModel.transform.rotation, target, remoteRotationSpeed * Time.deltaTime);
    }

    #endregion

    #region Username

    private void UpdateUserName()
    {
        if (userText == null || photonView.Owner == null) return;

        userText.text = photonView.Owner.NickName;
    }

    // Se llama cuando cambian las propiedades de un jugador (incluido el NickName)
    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == photonView.Owner)
        {
            UpdateUserName();
        }
    }

    #endregion

    public void Move()
    {
        // Como el objeto raíz ya no rota, el movimiento se calcula con la dirección de la cámara
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        if (cam != null)
        {
            forward = cam.transform.forward;
            right = cam.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
        }

        Vector3 moveDir = (right * hMove + forward * vMove);

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
        photonView.RPC(nameof(PlayAttackAnimation), RpcTarget.All);
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
            stream.SendNext(playerModel != null ? playerModel.transform.eulerAngles.y : 0f);
        }
        else
        {
            points = (int)stream.ReceiveNext();
            networkYaw = (float)stream.ReceiveNext();
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
    private void PlayerDeath(int id)
    {
        if (!photonView.IsMine) return;
        Destroy(gameObject);
        gm.SpawnPlayer(id);
    }

    [PunRPC]
    private void PlayAttackAnimation()
    {
        if (activeAnimator == null) return;

        activeAnimator.SetTrigger("Attack");
    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if ((winZoneLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            // Solo el cliente del ganador suma la victoria en su JSON
            PlayerDataManager.Instance.AddWin(MiniGame.Parkour);
            gm.photonView.RPC(nameof(gm.PlayerWinsParkour), RpcTarget.All);
        }

        if ((deathZoneLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            photonView.RPC(nameof(PlayerDeath), RpcTarget.All, ID);
        }
    }
}