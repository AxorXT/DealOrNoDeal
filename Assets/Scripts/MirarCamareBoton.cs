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

        // Calcula direcci�n hacia la c�mara
        Vector3 dir = transform.position - cam.transform.position;

        // Mantiene solo rotaci�n horizontal (evita que el bot�n se incline hacia arriba o abajo)
        dir.y = 0;

        // Apunta hacia la c�mara
        transform.rotation = Quaternion.LookRotation(dir);
    }
}