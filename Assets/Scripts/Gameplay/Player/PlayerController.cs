using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviourPun
{
    [Header("Movement configuration")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpForces;
    private float moveDirection;
    private bool isFacingRight = true;
    private bool canJump = false;
    private Rigidbody rb;
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private Transform checkFloor;
    [SerializeField] private float checkFloorDisntance;

    [Header("Render")]
    [SerializeField] private Renderer render;
    private Material material;

    public GameManager gm;
    private float points;

    [Header("Ray")]
    [SerializeField] private GameObject ray;
    [SerializeField] private GameObject raySpawn;

    [Header("Ray cooldown")]
    [SerializeField] private float attackCD;
    private float counterCD;

    [Header("Crown")]
    [SerializeField] private GameObject crownRender;
    [SerializeField] private GameObject crownObj;
    private bool haveCrown;


    public void Initialize(GameManager gm, Material playerMaterial)
    {
        rb = gameObject.GetComponent<Rigidbody>();

        this.gm = gm;
        material = playerMaterial;
        render.material = playerMaterial;

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

        Jump();
        Attack();

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

        canJump = Physics.CheckSphere(checkFloor.position, checkFloorDisntance, floorLayer);
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        Move();
        
    }

    public void Move()
    {
        Vector3 movement = new Vector3(moveDirection, 0f, 0f) * speed;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, rb.linearVelocity.z);
    }

    public void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            rb.AddForce(Vector3.up * jumpForces, ForceMode.Impulse);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (checkFloor == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(checkFloor.position, checkFloorDisntance);
    }

    public void Attack()
    {
        if (Input.GetMouseButtonDown(0) && counterCD > attackCD)
        {
            counterCD = 0;
            //raySpawn.transform.rotation
            RayMovement attack = gm.pm.ReturnSpawnedObject(ray.name, raySpawn.transform.position, raySpawn.transform.rotation).GetComponent<RayMovement>();
            attack.owner = this;
        }
    }

    private void AddPoint()
    {
        points += Time.deltaTime;
        int result = Convert.ToInt32(points);

        gm.AddPoint(this, result);
    }

    public void CallAddCrown()
    {
        photonView.RPC(nameof(AddCrown), RpcTarget.All);
    }

    public void CallQuitCrown()
    {
        photonView.RPC(nameof(QuitCrown), RpcTarget.All);
    }

    [PunRPC]
    private void AddCrown()
    {
        haveCrown = true;
        crownRender.SetActive(true);
    }

    [PunRPC]
    private void QuitCrown()
    {
        haveCrown = false;
        Debug.Log(gm);
        gm.pm.SpawnObject(crownObj.name, transform.position, Quaternion.identity);
    }

    void OnApplicationQuit()
    {
        photonView.RPC(nameof(gm.PlayerQuitParty), RpcTarget.All, this);
    }
}
