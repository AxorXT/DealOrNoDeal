using UnityEngine;

public class CamaraFollow : MonoBehaviour
{
    public Transform target;
    public Vector2 sensitivity = new Vector2(3f, 1.5f);
    public Vector2 pitchClamp = new Vector2(-35f, 60f);
    public float distance = 5f;

    private float yaw = 0f;
    private float pitch = 15f;

    private bool focusMode = false;
    private Transform focusTarget;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!focusMode)
        {
            yaw += Input.GetAxis("Mouse X") * sensitivity.x;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity.y;
            pitch = Mathf.Clamp(pitch, pitchClamp.x, pitchClamp.y);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 offset = rotation * new Vector3(0, 0, -distance);
            transform.position = target.position + offset + Vector3.up * 1.5f;
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }
        else
        {
            Vector3 offset = new Vector3(1f, 1.5f, 0);
            transform.position = focusTarget.position + offset;
            transform.LookAt(focusTarget.position + Vector3.up * 1.5f);
        }
    }

    public void SetFocus(Transform npc)
    {
        focusTarget = npc;
        focusMode = true;
    }

    public void ClearFocus()
    {
        focusMode = false;
    }
}