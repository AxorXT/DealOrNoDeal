using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSmoothTime = 0.1f;
    public Transform cameraTransform;

    private Vector3 direction;
    private float currentVelocity;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Dirección del input
        direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Si hay movimiento, rotamos al jugador
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    void FixedUpdate()
    {
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Movimiento usando física (respetando colisiones)
            Vector3 newPosition = rb.position + moveDir.normalized * speed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
    }
}
