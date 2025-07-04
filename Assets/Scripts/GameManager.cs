using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public JobData ContratoSeleccionado { get; set; }

    private bool estadoRestaurado = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene current)
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            GuardarEstadoAutomatico();
        }
        else
        {
            Debug.LogWarning("Jugador no estaba presente al salir de la escena. No se guardó.");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (estadoRestaurado) return;

        GameState estado = SaveSystem.CargarEstado();
        if (estado != null)
        {
            SaveSystem.RestaurarDesdeGameState(estado);

        }
        else
        {
            // No hay guardado, inicializa normalmente
            FindAnyObjectByType<ManagerNPCs>()?.InicializarEscena();
        }
    }

    private IEnumerator EsperarYRestaura(GameState estado)
    {
        yield return new WaitForSeconds(0.1f); // Asegura que la escena haya terminado de cargar

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

    // También útil si quieres forzar el guardado desde otro script
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
}