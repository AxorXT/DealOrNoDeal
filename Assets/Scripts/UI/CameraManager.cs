using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform vistaMapa;    // Posición/rotación desde arriba
    public Transform jugador;      // Objeto a seguir en modo juego
    public float duracionTransicion = 2f;

    private CamaraFollow camaraFollow;
    private bool enTransicion = false;

    public bool permitirControlCamara = true;

    void Start()
    {
        camaraFollow = GetComponent<CamaraFollow>();
        ConfigurarVistaInicial();
    }

    public void ConfigurarVistaInicial()
    {
        // Desactiva el seguimiento al jugador inicialmente
        if (camaraFollow != null)
        {
            camaraFollow.enabled = false;
        }

        // Coloca la cámara en la vista del mapa
        transform.position = vistaMapa.position;
        transform.rotation = vistaMapa.rotation;
    }

    public void TransitionToPlayer()
    {
        if (!enTransicion && permitirControlCamara)
        {
            StartCoroutine(Transicion());
        }
    }

    IEnumerator Transicion()
    {
        enTransicion = true;

        float tiempo = 0;
        Vector3 posInicial = transform.position;
        Quaternion rotInicial = transform.rotation;

        Vector3 offset = new Vector3(0, 5f, -5f); // Ajusta según tu ángulo deseado
        Vector3 destino = jugador.position + offset;
        Quaternion rotDestino = Quaternion.LookRotation(jugador.position - destino); // Mirar al jugador

        while (tiempo < duracionTransicion)
        {
            float progreso = tiempo / duracionTransicion;
            transform.position = Vector3.Lerp(posInicial, destino, progreso);
            transform.rotation = Quaternion.Slerp(rotInicial, rotDestino, progreso);

            tiempo += Time.deltaTime;
            yield return null;
        }

        // Activa el seguimiento normal
        if (camaraFollow != null)
        {
            camaraFollow.enabled = true;
        }

        enTransicion = false;
    }

    // Método adicional para volver a vista de mapa si es necesario
    public void VolverAVistaMapa()
    {
        if (camaraFollow != null)
        {
            camaraFollow.enabled = false;
        }
        transform.position = vistaMapa.position;
        transform.rotation = vistaMapa.rotation;
    }

    public void RestaurarPosicionDesdeGuardado()
    {
        GameState estado = SaveSystem.CargarEstado();
        if (estado != null)
        {
            transform.position = estado.posicionCamara;
            transform.rotation = Quaternion.Euler(estado.rotacionCamara);
        }
        else
        {
            // Si no hay datos guardados, volvemos a la vista inicial del mapa
            ConfigurarVistaInicial();
        }
    }
}

