using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public GameObject menuInicialUI;
    public Camera camaraOverview;
    public GameObject camaraJugadorObj;
    public GameObject player;
    public GameObject simbolosContratos; // contenedor de íconos

    public CanvasGroup fadeGroup; // para fade in/out opcional

    private void Start()
    {
        Time.timeScale = 0f;

        camaraOverview.enabled = true;
        camaraJugadorObj.SetActive(false);
        player.GetComponent<PlayerMovement>().enabled = false;
        simbolosContratos.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Jugar()
    {
        menuInicialUI.SetActive(false);
        StartCoroutine(TransicionZoomMapa());
    }

    public void AbrirOpciones() => Debug.Log("Abrir opciones");
    public void SalirDelJuego() => Application.Quit();

    IEnumerator TransicionZoomMapa()
    {
        Time.timeScale = 1f;

        simbolosContratos.SetActive(true);

        Vector3 inicio = camaraOverview.transform.position;
        Vector3 fin = new Vector3(0, 20, 0); // Ajusta al centro del mapa

        float duracion = 2f;
        float tiempo = 0;

        while (tiempo < duracion)
        {
            camaraOverview.transform.position = Vector3.Lerp(inicio, fin, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        // Ahora el jugador debe elegir un contrato
        Debug.Log("Elige tu contrato inicial...");
    }

    // Se llama desde el botón del contrato elegido
    public void ConfirmarContratoElegido()
    {
        StartCoroutine(TransicionAJugador());
    }

    IEnumerator TransicionAJugador()
    {
        // Fade opcional
        if (fadeGroup != null)
        {
            for (float t = 0; t < 1; t += Time.deltaTime)
            {
                fadeGroup.alpha = t;
                yield return null;
            }
        }

        camaraOverview.enabled = false;
        camaraJugadorObj.SetActive(true);
        player.GetComponent<PlayerMovement>().enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Fade in
        if (fadeGroup != null)
        {
            for (float t = 1; t > 0; t -= Time.deltaTime)
            {
                fadeGroup.alpha = t;
                yield return null;
            }
        }
    }
}
