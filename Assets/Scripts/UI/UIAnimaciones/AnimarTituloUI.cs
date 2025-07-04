using UnityEngine;
using System.Collections;

public class AnimarTituloUI : MonoBehaviour
{
    public float zoomAmplitude = 0.05f;
    public float zoomSpeed = 2f;

    public float rotationAmplitude = 5f;
    public float rotationSpeed = 1.5f;

    public float moveAmplitude = 10f;
    public float moveSpeed = 1f;

    public float entradaDuracion = 1f;
    public float salidaDuracion = 0.7f;

    private RectTransform rt;
    private CanvasGroup canvasGroup;
    private Vector3 escalaInicial;
    private Vector2 posicionAncladaInicial;
    private float rotacionInicialZ;

    private bool animacionActiva = false;

    void Start()
    {
        rt = GetComponent<RectTransform>();

        // Si no tiene CanvasGroup, lo agregamos aquí mismo
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        escalaInicial = rt.localScale;
        posicionAncladaInicial = rt.anchoredPosition;
        rotacionInicialZ = rt.localEulerAngles.z;

        // Estado inicial: fuera de pantalla arriba
        rt.anchoredPosition = posicionAncladaInicial + new Vector2(0f, 300f);
        rt.localRotation = Quaternion.Euler(0f, 0f, rotacionInicialZ);
        rt.localScale = escalaInicial;
        canvasGroup.alpha = 1f;

        animacionActiva = false;

        // Iniciar entrada
        StartCoroutine(AnimacionEntrada());
    }

    IEnumerator AnimacionEntrada()
    {
        Vector2 inicio = rt.anchoredPosition;
        Vector2 destino = posicionAncladaInicial;
        float tiempo = 0f;

        while (tiempo < entradaDuracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, tiempo / entradaDuracion);
            rt.anchoredPosition = Vector2.Lerp(inicio, destino, t);
            canvasGroup.alpha = t;
            yield return null;
        }

        rt.anchoredPosition = destino;
        canvasGroup.alpha = 1f;
        animacionActiva = true;
    }

    void Update()
    {
        if (!animacionActiva) return;

        float t = Time.unscaledTime;

        float escala = 1f + Mathf.Sin(t * zoomSpeed) * zoomAmplitude;
        rt.localScale = escalaInicial * escala;

        float offsetY = Mathf.Sin(t * moveSpeed) * moveAmplitude;
        rt.anchoredPosition = posicionAncladaInicial + new Vector2(0f, offsetY);

        float anguloZ = rotacionInicialZ + Mathf.Sin(t * rotationSpeed) * rotationAmplitude;
        rt.localRotation = Quaternion.Euler(0f, 0f, anguloZ);
    }

    public void SalirConTransicion()
    {
        animacionActiva = true;
        Debug.Log("Se llamó a SalirConTransicion()");
        if (gameObject.activeInHierarchy)
            StartCoroutine(AnimacionSalida());
    }

    IEnumerator AnimacionSalida()
    {
        animacionActiva = false;

        if (canvasGroup == null || rt == null) yield break;

        Vector2 inicio = rt.anchoredPosition;
        Vector2 destino = posicionAncladaInicial + new Vector2(0f, 500f); // movimiento hacia arriba
        float tiempo = 0f;

        while (tiempo < salidaDuracion)
        {
            if (canvasGroup == null || canvasGroup.gameObject == null) yield break;

            tiempo += Time.unscaledDeltaTime;
            float t = tiempo / salidaDuracion;

            rt.anchoredPosition = Vector2.Lerp(inicio, destino, t);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        // Asegura estado final
        rt.anchoredPosition = destino;
        canvasGroup.alpha = 0f;

        // Opcional: destruir el objeto al final
        if (canvasGroup != null && canvasGroup.gameObject != null)
            Destroy(canvasGroup.gameObject);
    }

    public void ReiniciarAnimacion()
    {
        rt.anchoredPosition = posicionAncladaInicial + new Vector2(0f, 300f);
        rt.localRotation = Quaternion.Euler(0f, 0f, rotacionInicialZ);
        rt.localScale = escalaInicial;
        canvasGroup.alpha = 1f;
        animacionActiva = false;

        StartCoroutine(AnimacionEntrada());
    }
}
