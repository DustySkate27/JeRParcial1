using Photon.Pun;
using UnityEngine;

public class CameraManager : MonoBehaviourPun
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private Transform playerBody; // el objeto raíz que rota en horizontal (yaw)

    [Header("Configuración")]
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    private float pitch;

    private void Start()
    {
        bool isLocal = photonView.IsMine;

        // Solo el jugador local usa su cámara y listener
        playerCamera.enabled = isLocal;
        audioListener.enabled = isLocal;

        if (!isLocal)
        {
            enabled = false; // los remotos no ejecutan Update
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

        // Horizontal (yaw): rota el cuerpo entero
        playerBody.Rotate(Vector3.up * mouseX);

        // Vertical (pitch): rota solo la cámara, con límite para no dar la vuelta
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}