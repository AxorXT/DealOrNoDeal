using UnityEngine;

public class CamaraFollow : MonoBehaviour
{
    public Transform target; // Player
    public Vector2 sensitivity = new Vector2(3f, 1.5f);
    public Vector2 pitchClamp = new Vector2(-35f, 60f);
    public float distance = 5f;

    private float yaw = 0f;
    private float pitch = 15f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        // Movimiento del mouse
        yaw += Input.GetAxis("Mouse X") * sensitivity.x;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity.y;
        pitch = Mathf.Clamp(pitch, pitchClamp.x, pitchClamp.y);

        // Rotación de la cámara
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.position = target.position + offset + Vector3.up * 1.5f; // ajustar altura
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}