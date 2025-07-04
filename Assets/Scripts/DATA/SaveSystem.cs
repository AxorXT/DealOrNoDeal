using UnityEngine;
using System.IO;
using System.Linq;
using static MainMenu;

public class SaveSystem : MonoBehaviour
{
    private static string filePath => Application.persistentDataPath + "/gamestate.json";

    public static void GuardarEstado(GameState estado)
    {
        if (estado == null)
        {
            Debug.LogWarning("Estado es null al intentar guardar.");
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
        GameState estado = new GameState(); // Siempre crea uno limpio

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

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Camera camaraJugador = Camera.main;
        ManagerNPCs manager = GameObject.FindAnyObjectByType<ManagerNPCs>();
        UIListaSueldos uiLista = GameObject.FindAnyObjectByType<UIListaSueldos>();
        DialogueManager dialogueManager = GameObject.FindAnyObjectByType<DialogueManager>();

        if (player != null)
        {
            player.transform.position = estado.posicionJugador;
            player.transform.rotation = Quaternion.Euler(estado.rotacionJugador);
        }

        if (camaraJugador != null)
        {
            camaraJugador.transform.position = estado.posicionCamara;
            camaraJugador.transform.rotation = Quaternion.Euler(estado.rotacionCamara);
        }

        if (manager != null)
        {
            manager.RestaurarContratosDesdeSave(estado.contratosAsignados);
            manager.RestaurarNPCsDesdeSave(estado.estadosNPCs);
        }

        if (uiLista != null)
        {
            uiLista.RestaurarSueldosRevelados(estado.sueldosRevelados);
        }

        if (dialogueManager != null)
        {
            dialogueManager.SetDialogueActive(estado.juegoEnDialogoActivo);
        }

        if (estado.mostrarDialogoSueldo && !string.IsNullOrEmpty(estado.npcActivoNombre))
        {
            Debug.Log("Reanudando diálogo pendiente con: " + estado.npcActivoNombre);

            var npcs = Resources.FindObjectsOfTypeAll<NPCInteractivo>()
                .Where(n => n.gameObject.scene.isLoaded);
            NPCInteractivo npc = npcs.FirstOrDefault(n => n.idUnico == estado.npcActivoNombre);

            if (npc != null && dialogueManager != null)
            {
                var secuencia = npc.trabajoYaAsignado && npc.segundoDialogo != null
                    ? npc.segundoDialogo
                    : CrearDialogoSueldo(npc);

                dialogueManager.StartDialogue(secuencia, npc);
            }
            else
            {
                Debug.LogWarning("No se encontró el NPC o DialogueManager: " + estado.npcActivoNombre);
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