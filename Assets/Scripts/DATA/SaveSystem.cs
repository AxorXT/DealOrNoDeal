using UnityEngine;
using System.IO;
using System.Linq;
using static MainMenu;

public class SaveSystem : MonoBehaviour
{
    private bool hayCambiosParaGuardar = false;
    public static SaveSystem Instance { get; private set; }
    private static string filePath => Application.persistentDataPath + "/gamestate.json";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void MarcarCambio()
    {
        hayCambiosParaGuardar = true;
    }

    public void GuardarEstadoActual()
    {
        if (!hayCambiosParaGuardar)
        {
            Debug.Log("No hay cambios para guardar, se omite guardado.");
            return;
        }

        GameState estado = CrearGameStateActual();
        GuardarEstado(estado);
        hayCambiosParaGuardar = false;
    }

    public static void GuardarEstado(GameState estado)
    {
        if (estado == null)
        {
            Debug.LogWarning("Intento de guardar un estado inválido o vacío.");
            return;
        }

        string json = JsonUtility.ToJson(estado, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Estado guardado exitosamente en: " + filePath);
    }

    public static GameState CargarEstado()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<GameState>(json);
        }
        return null;
    }

    public static void BorrarGuardado()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("Guardado borrado correctamente.");
        }
        else
        {
            Debug.Log("No existe archivo guardado para borrar.");
        }
    }

    public static GameState CrearGameStateActual()
    {
        GameState estado = new GameState();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Camera camaraJugador = Camera.main;

        if (player != null)
        {
            estado.posicionJugador = player.transform.position;
            estado.rotacionJugador = player.transform.eulerAngles;
        }

        if (camaraJugador != null)
        {
            estado.posicionCamara = camaraJugador.transform.position;
            estado.rotacionCamara = camaraJugador.transform.eulerAngles;
        }

        ManagerNPCs manager = GameObject.FindAnyObjectByType<ManagerNPCs>();
        if (manager != null)
        {
            foreach (var contrato in manager.GetContratosAsignados())
            {
                estado.contratosAsignados.Add(new JobDataSave
                {
                    nombre = contrato.nombre,
                    sueldo = contrato.sueldo
                });
            }

            foreach (var npc in manager.GetNPCs())
            {
                if (npc == null) continue;

                estado.estadosNPCs.Add(new NPCState
                {
                    idUnico = npc.idUnico,
                    trabajoAsignado = npc.trabajoYaAsignado,
                    npcVisible = npc.gameObject.activeSelf,
                    posicionNPC = npc.transform.position,
                    rotacionNPC = npc.transform.eulerAngles,
                    indexPrefab = npc.indexPrefab
                });
            }
        }

        UIListaSueldos uiLista = GameObject.FindAnyObjectByType<UIListaSueldos>();
        if (uiLista != null)
        {
            estado.sueldosRevelados = uiLista.GetSueldosRevelados();
        }

        DialogueManager dialogueManager = GameObject.FindAnyObjectByType<DialogueManager>();
        if (dialogueManager != null)
        {
            estado.juegoEnDialogoActivo = dialogueManager.IsDialogueActive();
        }

        return estado;
    }

    public static void RestaurarDesdeGameState(GameState estado)
    {
        if (estado == null) return;

        var player = GameObject.FindGameObjectWithTag("Player");
        var camaraJugador = Camera.main;
        var manager = GameObject.FindAnyObjectByType<ManagerNPCs>();
        var uiLista = GameObject.FindAnyObjectByType<UIListaSueldos>();
        var dialogueManager = GameObject.FindAnyObjectByType<DialogueManager>();

        // Restaurar posición del jugador
        if (player != null)
        {
            player.transform.position = estado.posicionJugador;
            player.transform.rotation = Quaternion.Euler(estado.rotacionJugador);
        }

        // Restaurar posición de cámara
        if (camaraJugador != null)
        {
            camaraJugador.transform.position = estado.posicionCamara;
            camaraJugador.transform.rotation = Quaternion.Euler(estado.rotacionCamara);
        }

        // Restaurar estado de NPCs y contratos
        if (manager != null)
        {
            manager.RestaurarContratosDesdeSave(estado.contratosAsignados);
            manager.RestaurarNPCsDesdeSave(estado.estadosNPCs);
        }

        // Restaurar sueldos mostrados en UI
        if (uiLista != null)
        {
            uiLista.RestaurarSueldosRevelados(estado.sueldosRevelados);
        }

        // Restaurar estado del diálogo
        if (dialogueManager != null && estado.mostrarDialogoSueldo && !string.IsNullOrEmpty(estado.npcActivoNombre))
        {
            NPCInteractivo npc = manager.GetNPCs().FirstOrDefault(n => n.idUnico == estado.npcActivoNombre);
            if (npc != null)
            {
                var dialogo = npc.trabajoYaAsignado && npc.segundoDialogo != null
                    ? npc.segundoDialogo
                    : CrearDialogoSueldo(npc);

                dialogueManager.StartDialogue(dialogo, npc);
            }
        }

        SaveFlags.estadoRestauradoDesdeArchivo = true;
    }

    private static DialogueSequence CrearDialogoSueldo(NPCInteractivo npc)
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