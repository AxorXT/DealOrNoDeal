using UnityEngine;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    private static string filePath => Application.persistentDataPath + "/gamestate.json";

    public static void GuardarEstado(GameState estado)
    {
        string json = JsonUtility.ToJson(estado, true);
        File.WriteAllText(filePath, json);
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

        // Guarda jugador y cámara
        estado.posicionJugador = player.transform.position;
        estado.rotacionJugadorY = player.transform.eulerAngles.y;

        estado.posicionCamara = camaraJugador.transform.position;
        estado.rotacionCamara = camaraJugador.transform.eulerAngles;

        // Guarda contratos asignados
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

            // Guarda estado NPCs
            foreach (var npc in manager.GetNPCs())
            {
                estado.estadosNPCs.Add(new NPCState
                {
                    npcName = npc.name,
                    trabajoAsignado = npc.trabajoYaAsignado,
                    npcVisible = npc.gameObject.activeSelf,
                    posicionNPC = npc.transform.position,
                    rotacionNPC = npc.transform.eulerAngles
                });
            }
        }

        // Guarda sueldos revelados (desde UIListaSueldos o donde tengas ese estado)
        UIListaSueldos uiLista = GameObject.FindAnyObjectByType<UIListaSueldos>();
        if (uiLista != null)
        {
            estado.sueldosRevelados = uiLista.GetSueldosRevelados();
        }

        // Estado general (por ejemplo, si hay diálogo activo)
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

        // Restaurar jugador y cámara
        if (player != null)
        {
            player.transform.position = estado.posicionJugador;
            player.transform.rotation = Quaternion.Euler(0, estado.rotacionJugadorY, 0);
        }

        if (camaraJugador != null)
        {
            camaraJugador.transform.position = estado.posicionCamara;
            camaraJugador.transform.rotation = Quaternion.Euler(estado.rotacionCamara);
        }

        // Restaurar contratos y NPCs
        if (manager != null)
        {
            manager.RestaurarContratosDesdeSave(estado.contratosAsignados);
            manager.RestaurarNPCsDesdeSave(estado.estadosNPCs);
        }

        // Restaurar sueldos revelados en UI
        if (uiLista != null)
        {
            uiLista.RestaurarSueldosRevelados(estado.sueldosRevelados);
        }

        // Restaurar estado de diálogo
        if (dialogueManager != null)
        {
            dialogueManager.SetDialogueActive(estado.juegoEnDialogoActivo);
        }
    }
}
