using UnityEngine;

public class MirarCamareBoton : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Calcula dirección hacia la cámara
        Vector3 dir = transform.position - cam.transform.position;

        // Mantiene solo rotación horizontal (evita que el botón se incline hacia arriba o abajo)
        dir.y = 0;

        // Apunta hacia la cámara
        transform.rotation = Quaternion.LookRotation(dir);
    }
}