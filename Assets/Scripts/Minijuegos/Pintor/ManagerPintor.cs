using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ManagerPintor : MonoBehaviour
{
    [Header("Referencias")]
    public Camera camaraJugador;
    public Transform pincelVisual;
    public GameObject prefabMarcaPintura;
    public Transform zonaSpawnPared;

    [Header("Pintura")]
    public Color colorCubosPintados = Color.red;

    [Header("Paredes")]
    public GameObject[] paredes; // 10 prefabs
    private int indiceParedActual = 0;
    private GameObject paredActual;
    private List<Transform> cubosPintables = new();
    private HashSet<Transform> cubosYaPintados = new();

    [Header("UI")]
    public TMP_Text textoTiempo;
    public GameObject panelResultado;
    public TMP_Text textoResultado;
    public Button botonSalir;

    [Header("Configuración")]
    public float tiempoLimite = 60f;
    public float radioPintura = 0.3f;
    public LayerMask capaPared;

    private float tiempoActual;
    private bool juegoActivo = true;
    private HashSet<Vector3> zonasPintadas = new();

    void Start()
    {
        tiempoActual = tiempoLimite;
        panelResultado.SetActive(false);
        botonSalir.onClick.AddListener(() => SceneManager.LoadScene("Ciudad")); // Cambia por tu escena principal
        CargarPared();
    }

    void Update()
    {
        if (!juegoActivo) return;

        // Temporizador
        tiempoActual -= Time.deltaTime;
        if (tiempoActual < 0) tiempoActual = 0;
        textoTiempo.text = "00:" + Mathf.CeilToInt(tiempoActual).ToString("D2");

        // Entrada del mouse
        Vector2 pos = Input.mousePosition;
        ProcesarEntrada(pos, Input.GetMouseButton(0));

        if (cubosYaPintados.Count >= cubosPintables.Count)
        {
            indiceParedActual++;
            if (indiceParedActual >= paredes.Length)
            {
                FinDelJuego(true);
            }
            else
            {
                CargarPared();
            }
        }

        // Validación de condiciones
        if (tiempoActual <= 0)
        {
            FinDelJuego(false);
        }
    }

    void ProcesarEntrada(Vector2 pantallaPos, bool pintar)
    {
        Ray ray = camaraJugador.ScreenPointToRay(pantallaPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Vector3 punto = hit.point;
            pincelVisual.position = punto + hit.normal * 0.01f;
            pincelVisual.rotation = Quaternion.LookRotation(hit.normal);

            if (pintar)
            {
                // Pintar libre en Layer "Pared"
                if (((1 << hit.collider.gameObject.layer) & capaPared.value) != 0)
                {
                    Vector3 puntoClave = RedondearPosicion(punto, 0.3f);
                    if (!zonasPintadas.Contains(puntoClave))
                    {
                        zonasPintadas.Add(puntoClave);
                        GameObject marca = Instantiate(prefabMarcaPintura, puntoClave + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
                        marca.transform.localScale = Vector3.one * radioPintura;
                    }
                }

                // Pintar cubo con tag "Pintable" (para progreso)
                if (hit.collider.CompareTag("Pintable"))
                {
                    Transform cubo = hit.collider.transform;
                    if (!cubosYaPintados.Contains(cubo))
                    {
                        cubosYaPintados.Add(cubo);

                        // OPCIÓN 1: cambiar color del material
                        Renderer rend = cubo.GetComponent<Renderer>();
                        if (rend != null)
                        {
                            rend.material.color = colorCubosPintados;
                        }

                        // OPCIONAL: dejar también una marca visual si quieres
                        GameObject marca = Instantiate(prefabMarcaPintura, cubo.position + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
                        marca.transform.localScale = Vector3.one * radioPintura;
                    }
                }
            }
        }
    }

    void CargarPared()
    {
        if (paredActual != null)
            Destroy(paredActual);

        cubosPintables.Clear();
        cubosYaPintados.Clear();

        paredActual = Instantiate(paredes[indiceParedActual], zonaSpawnPared.position, Quaternion.identity);

        foreach (Transform child in paredActual.GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("Pintable"))
                cubosPintables.Add(child);
        }

        Debug.Log("Cargada nueva pared con " + cubosPintables.Count + " cubos pintables");
    }

    void FinDelJuego(bool victoria)
    {
        juegoActivo = false;
        panelResultado.SetActive(true);
        textoResultado.text = victoria ? "¡Completaste todas las paredes!" : "¡Se acabó el tiempo!";
    }

    Vector3 RedondearPosicion(Vector3 v, float escala)
    {
        return new Vector3(
            Mathf.Round(v.x / escala) * escala,
            Mathf.Round(v.y / escala) * escala,
            Mathf.Round(v.z / escala) * escala
        );
    }
}