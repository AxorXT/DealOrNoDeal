using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIListaSueldos : MonoBehaviour
{
    public GameObject panelSueldos;
    public Transform contenedorSueldos;
    public GameObject sueldoItemPrefab;
    public Color colorTachado = Color.gray;

    public float duracionAnimacion = 0.3f;
    private RectTransform panelRect;
    private bool animando = false;
    private Vector2 posicionInicial;
    private Vector2 posicionVisible;

    private Dictionary<int, GameObject> itemsSueldos = new();
    private bool jugadorActivo = true;
    private List<int> sueldosReveladosGuardados = new List<int>();

    IEnumerator Start()
    {
        panelRect = panelSueldos.GetComponent<RectTransform>();
        posicionVisible = panelRect.anchoredPosition; // Posición normal visible
        posicionInicial = posicionVisible + new Vector2(0, -Screen.height); // Fuera de pantalla abajo
        panelRect.anchoredPosition = posicionInicial;
        panelSueldos.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        
        ManagerNPCs manager = FindFirstObjectByType<ManagerNPCs>();
        if (manager != null)
        {
            List<int> sueldos = manager.ObtenerSueldosGenerados();
            CrearListaConSueldos(sueldos);
        }
        else
        {
            Debug.LogWarning("No se encontró ManagerNPCs en la escena.");
        }
    }
    void Update()
    {
        if (!jugadorActivo || animando) return;
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (panelSueldos.activeSelf)
            {
                StartCoroutine(AnimarPanel(false));
            }
            else
            {
                StartCoroutine(AnimarPanel(true));
            }
        }
    }

    public void ActivarInputJugador()
    {
        jugadorActivo = true;
    }

    void CrearListaConSueldos(List<int> sueldos)
    {
        // Limpiar hijos antes de crear
        foreach (Transform child in contenedorSueldos)
        {
            Destroy(child.gameObject);
        }

        itemsSueldos.Clear();

        foreach (int sueldo in sueldos.OrderByDescending(s => s))
        {
            GameObject item = Instantiate(sueldoItemPrefab, contenedorSueldos);
            item.GetComponentInChildren<TMP_Text>().text = "$" + sueldo;
            itemsSueldos[sueldo] = item;
        }

        RestaurarSueldosRevelados(sueldosReveladosGuardados);
    }



    public void MarcarSueldoComoRevelado(int sueldo)
    {
        if (itemsSueldos.TryGetValue(sueldo, out GameObject item))
        {
            var texto = item.GetComponentInChildren<TMP_Text>();
            texto.color = colorTachado;
            texto.fontStyle = FontStyles.Strikethrough;
        }
    }

    IEnumerator AnimarPanel(bool mostrar)
    {
        animando = true;
        
        if (mostrar)
        {
            panelSueldos.SetActive(true);
        }
        Vector2 inicio = mostrar ? posicionInicial : posicionVisible;
        Vector2 destino = mostrar ? posicionVisible : posicionInicial;
        
        float t = 0;
        
        while (t < 1)
        {
            t += Time.deltaTime / duracionAnimacion;
            panelRect.anchoredPosition = Vector2.Lerp(inicio, destino, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        if (!mostrar)
        {
            panelSueldos.SetActive(false);
        }

        animando = false;
    }
    
    public List<int> ObtenerSueldosTachados()
    {
        List<int> tachados = new();
        foreach (var kvp in itemsSueldos)
        {
            TMP_Text texto = kvp.Value.GetComponentInChildren<TMP_Text>();
            if (texto != null && texto.fontStyle.HasFlag(FontStyles.Strikethrough))
            {
                tachados.Add(kvp.Key);
            }
        }
        return tachados;
    }

    public List<int> GetSueldosRevelados()
    {
        return itemsSueldos.Where(kv => kv.Value.GetComponentInChildren<TMP_Text>().fontStyle.HasFlag(FontStyles.Strikethrough)).Select(kv => kv.Key).ToList();
    }

    public void RestaurarSueldosRevelados(List<int> sueldosRevelados)
    {
        sueldosReveladosGuardados = new List<int>(sueldosRevelados);

        foreach (var sueldo in sueldosRevelados)
        {
            if (itemsSueldos.TryGetValue(sueldo, out GameObject item))
            {
                var texto = item.GetComponentInChildren<TMP_Text>();
                texto.color = colorTachado;
                texto.fontStyle = FontStyles.Strikethrough;
            }
        }
    }
}