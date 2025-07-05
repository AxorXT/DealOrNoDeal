using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject playerGameObject;
    public GameObject cameraGameObject;
    public List<GameObject> npcGameObjects = new List<GameObject>();

    public JobData ContratoSeleccionado { get; set; }

    private bool estadoRestaurado = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Este GameObject (GameManager) es raíz? Si no, tomar root para DontDestroyOnLoad
            if (this.transform.parent == null)
                DontDestroyOnLoad(this.gameObject);
            else
                DontDestroyOnLoad(this.transform.root.gameObject);

            // No asignar player ni cámara aquí porque no están cargados todavía

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene previousScene, Scene newScene)
    {
        if (previousScene.name == "JUEGO") // Solo guardar si sales del mapa
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                SaveSystem.Instance.GuardarEstadoActual();
                Debug.Log("Estado guardado correctamente al salir del mapa.");
            }
            else
            {
                Debug.LogWarning("Jugador no estaba presente al salir del mapa.");
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "JUEGO")
        {
            // --- PLAYER ---
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var p in players)
            {
                if (playerGameObject == null)
                {
                    playerGameObject = p;
                    DontDestroyOnLoadSafe(playerGameObject);
                }
                else if (p != playerGameObject)
                {
                    Destroy(p);
                    Debug.Log("Player duplicado destruido.");
                }
            }

            // --- CAMERA ---
            GameObject[] cameras = GameObject.FindGameObjectsWithTag("MainCamera");
            foreach (var c in cameras)
            {
                if (cameraGameObject == null)
                {
                    cameraGameObject = c;
                    DontDestroyOnLoadSafe(cameraGameObject);
                }
                else if (c != cameraGameObject)
                {
                    Destroy(c);
                    Debug.Log("Cámara duplicada destruida.");
                }
            }

            // --- NPCs ---
            var nuevosNPCs = FindObjectsByType<NPCInteractivo>(FindObjectsSortMode.None);
            foreach (var npc in nuevosNPCs)
            {
                // Verifica si ya está en la lista persistente
                if (!npcGameObjects.Contains(npc.gameObject))
                {
                    npcGameObjects.Add(npc.gameObject);
                    DontDestroyOnLoadSafe(npc.gameObject);
                    Debug.Log($"NPC agregado y marcado como persistente: {npc.name}");
                }
                else if (npc.gameObject.scene.name == "JUEGO") // NPC extra en escena, destruirlo
                {
                    Destroy(npc.gameObject);
                    Debug.Log($"NPC duplicado destruido: {npc.name}");
                }
            }

            // Restaurar estado si es necesario
            if (!estadoRestaurado)
            {
                GameState estado = SaveSystem.CargarEstado();
                if (estado != null)
                {
                    SaveSystem.RestaurarDesdeGameState(estado);
                    estadoRestaurado = true;
                }
                else
                {
                    ManagerNPCs.Instance.InicializarEscena(); // Solo debe ejecutarse 1 vez
                }
            }

            // Activar objetos persistentes al volver al mapa
            if (playerGameObject != null) playerGameObject.SetActive(true);
            if (cameraGameObject != null) cameraGameObject.SetActive(true);
            foreach (var npcGO in npcGameObjects)
            {
                if (npcGO != null) npcGO.SetActive(true);
            }
        }
        else
        {
            // En otras escenas, ocultar los objetos persistentes
            if (playerGameObject != null) playerGameObject.SetActive(false);
            if (cameraGameObject != null) cameraGameObject.SetActive(false);
            foreach (var npcGO in npcGameObjects)
            {
                if (npcGO != null) npcGO.SetActive(false);
            }
        }
    }

    // Marca para que no se destruyan al cambiar escena
    public void DontDestroyOnLoadSafe(GameObject go)
    {
        if (go == null) return;
        DontDestroyOnLoad(go.transform.root.gameObject);
    }


    private IEnumerator EsperarYRestaura(GameState estado)
    {
        yield return new WaitForSeconds(0.1f); // Espera que termine de cargar escena

        SaveSystem.RestaurarDesdeGameState(estado);

        if (estado.mostrarDialogoSueldo && !string.IsNullOrEmpty(estado.npcActivoNombre))
        {
            Debug.Log("Reanudando diálogo pendiente con: " + estado.npcActivoNombre);
            NPCInteractivo[] npcs = FindObjectsByType<NPCInteractivo>(FindObjectsSortMode.None);
            NPCInteractivo npc = System.Array.Find(npcs, n => n != null && (n.idUnico == estado.npcActivoNombre || n.name == estado.npcActivoNombre));
            if (npc != null)
            {
                DialogueManager dialogueManager = GameObject.FindAnyObjectByType<DialogueManager>();
                if (dialogueManager != null)
                {
                    DialogueSequence dialogo = npc.trabajoYaAsignado && npc.segundoDialogo != null
                    ? npc.segundoDialogo
                    : CrearDialogoSueldo(npc);

                    dialogueManager.StartDialogue(dialogo, npc);
                }
            }
        }

        estadoRestaurado = true;
    }

    public void GuardarEstadoAutomatico()
    {
        GameState estadoActual = SaveSystem.CrearGameStateActual();
        SaveSystem.GuardarEstado(estadoActual);
        Debug.Log("Estado guardado automáticamente.");
    }

    public void GuardarManual()
    {
        GuardarEstadoAutomatico();
    }

    public void RestaurarManual()
    {
        GameState estado = SaveSystem.CargarEstado();
        if (estado != null)
        {
            SaveSystem.RestaurarDesdeGameState(estado);
            Debug.Log("Restauración manual completada.");
        }
    }

    private DialogueSequence CrearDialogoSueldo(NPCInteractivo npc)
    {
        DialogueSequence seq = ScriptableObject.CreateInstance<DialogueSequence>();
        seq.lines = new DialogueLine[]
        {
            new DialogueLine
            {
                speakerName = npc.name,
                dialogueText = "Tu sueldo será de $" + npc.contratoAsignado.sueldo.ToString("N0")
            }
        };
        return seq;
    }

    public void VolverAlMapaPrincipal()
    {
        estadoRestaurado = false;

        if (SceneManager.GetActiveScene().name == "JUEGO")
            GuardarEstadoAutomatico();

        SceneManager.LoadScene("JUEGO");
    }
}