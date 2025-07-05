using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;


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

    private Dictionary<string, GameObject> prefabPorId = new Dictionary<string, GameObject>();
    private List<NPCInteractivo> npcs;

    private static int contador = 0;
    private bool yaInicializado = false;
    public static ManagerNPCs Instance;

    public void InicializarEscena()
    {
        if (yaInicializado) return;
        yaInicializado = true;

        contratosAsignados.Clear();
        npcPorPunto.Clear();

        npcs = FindObjectsByType<NPCInteractivo>(FindObjectsSortMode.None).ToList();

        GenerarContratos();
        ColocarNPCsAleatorios();
        CrearBotonesContratos();
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void CrearBotonesContratos()
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

            if (!npcPorPunto.ContainsKey(punto))
            {
                GameObject prefabElegido = npcConContratoPrefabs[UnityEngine.Random.Range(0, npcConContratoPrefabs.Count)];

                string idUnico = "Contrato_" + contador;

                NPCInteractivo npcScript = InstanciarNPCPersistente(prefabElegido, punto, idUnico);

                if (npcScript != null)
                {
                    npcScript.contratoAsignado = contratosAsignados[i];
                    prefabPorId[idUnico] = prefabElegido;
                    contador++;

                    npcPorPunto[punto] = npcScript;

                    NPCEmpleo npcEmpleo = npcScript.GetComponent<NPCEmpleo>();
                    if (npcEmpleo != null)
                    {
                        npcEmpleo.AsignarContrato(contratosAsignados[i]);
                    }
                }
            }
        }

        // NPCs decorativos (puedes también usar InstanciarNPCPersistente si quieres persistencia)
        for (int i = contratosAsignados.Count; i < puntosAleatorios.Count; i++)
        {
            Transform punto = puntosAleatorios[i];
            if (!npcPorPunto.ContainsKey(punto))
            {
                GameObject prefabElegido = npcDecorativoPrefab[UnityEngine.Random.Range(0, npcDecorativoPrefab.Count)];
                GameObject decorativoGO = Instantiate(prefabElegido, punto.position, punto.rotation);
                // Si quieres que los NPC decorativos también sean persistentes:
                GameManager.Instance.npcGameObjects.Add(decorativoGO);
                GameManager.Instance.DontDestroyOnLoadSafe(decorativoGO);
            }
        }
    }

    List<T> Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, list.Count);
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
            UnityEngine.Object.FindFirstObjectByType<CameraManager>().TransitionToPlayer();
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
        return FindObjectsByType<NPCInteractivo>(FindObjectsSortMode.None).ToList();
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
        if (estadosNPCs == null || estadosNPCs.Count == 0)
        {
            Debug.LogWarning("[ManagerNPCs] No hay estados NPCs para restaurar.");
            return;
        }

        // Actualizar contador basado en el mayor id numérico encontrado
        int maxId = estadosNPCs
            .Select(e => {
                if (int.TryParse(e.idUnico.Replace("Contrato_", ""), out int num))
                    return num;
                else
                    return -1;
            })
            .Max();

        contador = Mathf.Max(contador, maxId + 1);

        // Obtener NPCs actuales en escena
        List<NPCInteractivo> npcs = FindObjectsByType<NPCInteractivo>(FindObjectsSortMode.None).ToList();

        var idsGuardados = estadosNPCs.Select(e => e.idUnico).ToHashSet();

        // Desactivar NPCs no presentes en el guardado
        foreach (var npc in npcs)
        {
            if (!idsGuardados.Contains(npc.idUnico))
            {
                npc.gameObject.SetActive(false);
            }
        }

        if (npcConContratoPrefabs == null || npcConContratoPrefabs.Count == 0)
        {
            Debug.LogWarning("[ManagerNPCs] Lista de prefabs npcConContratoPrefabs vacía o nula.");
            return;
        }

        // Restaurar o crear NPCs según el estado guardado
        foreach (var estado in estadosNPCs)
        {
            NPCInteractivo npc = npcs.Find(n => n != null && n.idUnico == estado.idUnico);

            if (npc != null)
            {
                // Restaurar estado NPC existente
                npc.gameObject.SetActive(estado.npcVisible);
                npc.trabajoYaAsignado = estado.trabajoAsignado;
                npc.transform.position = estado.posicionNPC;
                npc.transform.rotation = Quaternion.Euler(estado.rotacionNPC);
                npc.indexPrefab = estado.indexPrefab;
            }
            else
            {
                // Crear nuevo NPC si índice es válido
                if (estado.indexPrefab >= 0 && estado.indexPrefab < npcConContratoPrefabs.Count)
                {
                    GameObject prefab = npcConContratoPrefabs[estado.indexPrefab];

                    if (prefab != null)
                    {
                        GameObject nuevoNPC = Instantiate(prefab, estado.posicionNPC, Quaternion.Euler(estado.rotacionNPC));
                        NPCInteractivo nuevoScript = nuevoNPC.GetComponent<NPCInteractivo>();

                        if (nuevoScript != null)
                        {
                            nuevoScript.idUnico = estado.idUnico;
                            nuevoScript.trabajoYaAsignado = estado.trabajoAsignado;
                            nuevoScript.indexPrefab = estado.indexPrefab;
                            nuevoNPC.SetActive(estado.npcVisible);
                            npcs.Add(nuevoScript);
                        }
                        else
                        {
                            Debug.LogWarning($"El prefab en índice {estado.indexPrefab} no tiene componente NPCInteractivo.");
                            Destroy(nuevoNPC); // Destruir para evitar objetos sin script
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Prefab en índice {estado.indexPrefab} es null.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Índice de prefab inválido ({estado.indexPrefab}) para NPC con idUnico: {estado.idUnico}");
                }
            }
        }
        foreach (var npc in FindObjectsByType<NPCInteractivo>(FindObjectsSortMode.None))
        {
            if (npc.trabajoYaAsignado)
            {
                npc.IniciarDialogoAutomatico();
                break; // Solo uno, o puedes decidir cuántos iniciar
            }
        }
    }

    private NPCInteractivo InstanciarNPCPersistente(GameObject prefab, Transform punto, string idUnico)
    {
        GameObject npcGO = Instantiate(prefab, punto.position, punto.rotation);
        npcGO.name = idUnico;

        NPCInteractivo npc = npcGO.GetComponent<NPCInteractivo>();
        if (npc != null)
        {
            npc.idUnico = idUnico;
            npc.indexPrefab = npcConContratoPrefabs.IndexOf(prefab); // o ajusta según lista

            // Registrar en GameManager para persistencia
            if (!GameManager.Instance.npcGameObjects.Contains(npcGO))
            {
                GameManager.Instance.npcGameObjects.Add(npcGO);
                GameManager.Instance.DontDestroyOnLoadSafe(npcGO);
            }
        }

        return npc;
    }
}
