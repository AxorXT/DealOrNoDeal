using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSmoothTime = 0.1f;
    public Transform cameraTransform;

    private Vector3 direction;
    private float currentVelocity;

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Direcci�n seg�n el input
        direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Si hay movimiento
        if (direction.magnitude >= 0.1f)
        {
            // �ngulo hacia el que rotar basado en la c�mara
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, rotationSmoothTime);

            // Rotar jugador suavemente
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Direcci�n en el mundo
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Movimiento con Translate
            transform.Translate(moveDir.normalized * speed * Time.deltaTime, Space.World);
        }
    }
}
