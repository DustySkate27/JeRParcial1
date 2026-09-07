using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviourPun
{
    [Header("Movement configuration")]
    [SerializeField] private float speed;
    private Vector3 direction;
    private bool isFacingRight = true;
    private Rigidbody rb;
    [SerializeField] private LayerMask floorLayer;

    [Header("Render")]
    [SerializeField] private Renderer render;
    private Material material;

    public GameManager gm;
    private float points;

    [Header("Ray")]
    [SerializeField] private GameObject ray;
    [SerializeField] private GameObject raySpawn;

    [Header("Crown")]
    [SerializeField] private GameObject crownRender;
    [SerializeField] private GameObject crownObj;
    private bool haveCrown;

    [Header("PlayerDetection")]
    [SerializeField] private LayerMask playerDetectionLayer;

    [SerializeField] private float attackCD;
    private float counterCD;

    public void Initialize(GameManager gm, Material playerMaterial)
    {
        this.gm = gm;
        material = playerMaterial;
        render.material = playerMaterial;

        Debug.Log(gameObject.name + " has join the party");
    }

    // Update is called once per frame
    void Update()
    {
        if (haveCrown)
        {
            AddPoint();
        }
    }

    public void OnMove()
    {

    }

    public void OnJump()
    {

    }

    public void OnAttack()
    {

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
        gm.pm.SpawnObject(crownObj.name, transform.position, Quaternion.identity);
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }
}
