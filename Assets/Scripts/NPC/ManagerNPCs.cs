using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ManagerNPCs : MonoBehaviour
{
    public UIListaSueldos uiListaSueldos;
    public int cantidadEmpleosActivos = 10;

    [Header("Prefabs de NPC")]
    public List<GameObject> npcConContratoPrefabs;
    public List<GameObject> npcDecorativoPrefab;

    [Header("Puntos con rotación tipo A (definida en escena)")]
    public List<Transform> puntosRotacionA;

    [Header("Puntos con rotación tipo B (definida en escena)")]
    public List<Transform> puntosRotacionB;

    public GameObject contratoButtonPrefab; // Prefab del botón (crea uno en UI/Button)
    public Transform canvasContratos; // Arrastra el Canvas World Space aquí

    private List<JobData> contratosAsignados = new List<JobData>();
    private Dictionary<Transform, NPCInteractivo> npcPorPunto = new Dictionary<Transform, NPCInteractivo>();

    private List<NPCInteractivo> npcs;

    void Start()
    {
        GenerarContratos();
        ColocarNPCsAleatorios();
        CrearBotonesContratos();
    }

    void Awake()
    {
        npcs = new List<NPCInteractivo>(FindObjectsByType<NPCInteractivo>(FindObjectsSortMode.None));
    }

    void CrearBotonesContratos()
    {
        foreach (var kvp in npcPorPunto)
        {
            Transform punto = kvp.Key;
            NPCInteractivo npc = kvp.Value;

            GameObject button = Instantiate(contratoButtonPrefab, canvasContratos.transform);
            button.transform.position = punto.position + Vector3.up * 13; // Puedes ajustar el offset Y si hace falta

            ContratoMapa contratoMapa = button.GetComponent<ContratoMapa>();
            contratoMapa.contratoAsignado = npc.contratoAsignado;
            contratoMapa.AsignarNPC(npc);

            // Agregar listener para manejar la selección
            button.GetComponent<Button>().onClick.AddListener(() => OnContratoSeleccionado(punto));
        }
    }

    void GenerarContratos()
    {
        List<int> sueldos = new List<int> {
            100, 200, 300, 500, 800, 1000, 1500,
            2000, 3000, 5000, 8000, 10000, 15000, 20000, 50000, 100000
        };

        sueldos = Shuffle(sueldos);

        for (int i = 0; i < cantidadEmpleosActivos; i++)
        {
            JobData j = new JobData();
            j.nombre = "Empleo #" + (i + 1);
            j.sueldo = sueldos[i];
            contratosAsignados.Add(j);
        }
    }

    void ColocarNPCsAleatorios()
    {
        List<Transform> todosLosPuntos = new List<Transform>();
        todosLosPuntos.AddRange(puntosRotacionA);
        todosLosPuntos.AddRange(puntosRotacionB);

        List<Transform> puntosAleatorios = Shuffle(new List<Transform>(todosLosPuntos));

        for (int i = 0; i < contratosAsignados.Count; i++)
        {
            Transform punto = puntosAleatorios[i];
            GameObject prefabElegido = npcConContratoPrefabs[Random.Range(0, npcConContratoPrefabs.Count)];

            // Usa la rotación del punto
            GameObject npc = Instantiate(prefabElegido, punto.position, punto.rotation);

            //Asignar contrato al NPCInteractivo
            var npcScript = npc.GetComponent<NPCInteractivo>();
            if (npcScript != null)
            {
                npcScript.contratoAsignado = contratosAsignados[i];
                npcPorPunto[punto] = npcScript;
            }

            var npcEmpleo = npc.GetComponent<NPCEmpleo>();
            if (npcEmpleo != null)
            {
                npcEmpleo.AsignarContrato(contratosAsignados[i]);
            }
        }

        for (int i = contratosAsignados.Count; i < puntosAleatorios.Count; i++)
        {
            Transform punto = puntosAleatorios[i];
            GameObject prefabElegido = npcDecorativoPrefab[Random.Range(0, npcDecorativoPrefab.Count)];

            Instantiate(prefabElegido, punto.position, punto.rotation);
        }
    }

    List<T> Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
        return list;
    }

    void OnContratoSeleccionado(Transform puntoSpawn)
    {
        // 1. Encuentra el NPC asociado a este punto
        NPCInteractivo npc = FindNPCAtSpawnPoint(puntoSpawn);
        if (npc != null)
        {
            // 2. Guarda el contrato seleccionado
            GameManager.Instance.ContratoSeleccionado = npc.contratoAsignado;

            // 3. Desactiva todos los botones
            foreach (Button btn in canvasContratos.GetComponentsInChildren<Button>())
            {
                btn.gameObject.SetActive(false);
            }

            // 4. Transición al jugador
            Object.FindFirstObjectByType<CameraManager>().TransitionToPlayer();
        }
    }

    public NPCInteractivo FindNPCAtSpawnPoint(Transform punto)
    {
        NPCInteractivo[] todosLosNPCs = FindObjectsByType<NPCInteractivo>(FindObjectsSortMode.None);

        foreach (var npc in todosLosNPCs)
        {
            if (Vector3.Distance(npc.transform.position, punto.position) < 0.1f)
            {
                return npc;
            }
        }

        return null;
    }

    void AsignarContratosANPCs()
    {
        // Busca todos los botones de contrato y NPCs en la escena
        ContratoMapa[] botonesContratos = FindObjectsByType<ContratoMapa>(FindObjectsSortMode.None);
        NPCInteractivo[] npcs = FindObjectsByType<NPCInteractivo>(FindObjectsSortMode.None);

        // Asigna cada botón a un NPC
        for (int i = 0; i < botonesContratos.Length; i++)
        {
            if (i < npcs.Length) // Asegura que haya suficientes NPCs
            {
                botonesContratos[i].AsignarNPC(npcs[i]); // ¡Vincula el botón al NPC!
            }
        }
    }

    public List<int> ObtenerSueldosGenerados()
    {
        return contratosAsignados.Select(c => c.sueldo).ToList();
    }

    public List<JobData> GetContratosAsignados()
    {
        return contratosAsignados;
    }

    public List<NPCInteractivo> GetNPCs()
    {
        return npcPorPunto.Values.ToList();
    }

    public void RestaurarContratosDesdeSave(List<JobDataSave> contratos)
    {
        contratosAsignados.Clear();
        foreach (var c in contratos)
        {
            contratosAsignados.Add(new JobData
            {
                nombre = c.nombre,
                sueldo = c.sueldo
            });
        }
    }

    public void RestaurarNPCsDesdeSave(List<NPCState> estadosNPCs)
    {
        // Limpia la lista para que no tenga referencias nulas
        npcs = npcs.Where(n => n != null).ToList();

        foreach (var estado in estadosNPCs)
        {
            var npc = npcs.Find(n => n.name == estado.npcName);
            if (npc != null)
            {
                npc.gameObject.SetActive(estado.npcVisible);
                npc.trabajoYaAsignado = estado.trabajoAsignado;
                npc.transform.position = estado.posicionNPC;
                npc.transform.rotation = Quaternion.Euler(estado.rotacionNPC);
            }
        }
    }
}
