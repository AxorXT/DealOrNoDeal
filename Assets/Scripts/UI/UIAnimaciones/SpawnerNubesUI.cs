using System.Collections;
using UnityEngine;

public class SpawnerNubesUI : MonoBehaviour
{
    public RectTransform areaSpawn;          // El área de spawn (Canvas o NubesFondo)
    public GameObject nubePrefab;            // El prefab de la nube (desactivado)
    public float intervaloSpawn = 2f;        // Tiempo entre nubes
    public float velocidad = 20f;            // Velocidad de movimiento
    public float duracionNube = 8f;

    private float tiempo = 0f;
    private bool spawnActivo = true;

    void Update()
    {
        if (!spawnActivo) return;

        tiempo += Time.unscaledDeltaTime;

        if (tiempo >= intervaloSpawn)
        {
            CrearNube();
            tiempo = 0f;
        }
    }

    void CrearNube()
    {
        float y = Random.Range(-areaSpawn.rect.height / 2f, areaSpawn.rect.height / 2f);
        float x = Random.Range(-areaSpawn.rect.width / 2f, areaSpawn.rect.width / 2f);

        GameObject nueva = Instantiate(nubePrefab, areaSpawn); // Hijo del área del canvas
        nueva.SetActive(true);

        RectTransform rt = nueva.GetComponent<RectTransform>();
        if (rt == null)
        {
            Debug.LogError("Prefab nube no tiene RectTransform!");
            return;
        }
        rt.anchoredPosition = new Vector2(x, y);
        rt.localScale = Vector3.one * 0.25f;

        CanvasGroup canvasGroup = nueva.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = nueva.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        // Iniciar corrutina que mueve, agranda y destruye la nube
        StartCoroutine(AnimarYDestruirNube(rt, canvasGroup));
    }

    IEnumerator AnimarYDestruirNube(RectTransform rt, CanvasGroup canvasGroup)
    {
        Vector3 escalaInicial = rt.localScale;
        Vector3 escalaFinal = Vector3.one * Random.Range(0.5f, 0.75f);
        float tiempoZoom = 2f;
        float tiempoVida = duracionNube;
        float tiempo = 0f;

        // Zoom inicial
        while (tiempo < tiempoZoom)
        {
            if (rt == null) yield break;
            tiempo += Time.unscaledDeltaTime;
            float t = tiempo / tiempoZoom;
            rt.localScale = Vector3.Lerp(escalaInicial, escalaFinal, t);
            yield return null;
        }

        // Movimiento y espera antes del fade
        float tiempoMovimiento = tiempoVida - 2f; // 2 segundos para fade final
        tiempo = 0f;

        while (tiempo < tiempoMovimiento)
        {
            if (rt == null) yield break;
            tiempo += Time.unscaledDeltaTime;
            rt.anchoredPosition += Vector2.right * velocidad * Time.unscaledDeltaTime;
            yield return null;
        }

        // Fade out
        float duracionFade = 2f;
        tiempo = 0f;

        while (tiempo < duracionFade)
        {
            if (canvasGroup == null) yield break;
            tiempo += Time.unscaledDeltaTime;
            float t = tiempo / duracionFade;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            rt.anchoredPosition += Vector2.right * velocidad * Time.unscaledDeltaTime;
            yield return null;
        }

        Destroy(rt.gameObject);
    }

    public void DesvanecerTodasLasNubes()
    {
        for (int i = areaSpawn.childCount - 1; i >= 0; i--)
        {
            Transform nube = areaSpawn.GetChild(i);
            if (nube == nubePrefab.transform) continue;

            CanvasGroup cg = nube.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                StartCoroutine(FadeOutYDestruir(cg));
            }
            else
            {
                Destroy(nube.gameObject);
            }
        }
    }

    IEnumerator FadeOutYDestruir(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) yield break;

        float duracion = 1f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            if (canvasGroup == null) yield break;
            if (canvasGroup.gameObject == null) yield break;

            tiempo += Time.unscaledDeltaTime;
            float t = tiempo / duracion;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        if (canvasGroup != null && canvasGroup.gameObject != null)
            Destroy(canvasGroup.gameObject);
    }

    public void DetenerSpawn()
    {
        spawnActivo = false;
    }

    public void ActivarSpawn()
    {
        spawnActivo = true;
        tiempo = 0f;
    }
}