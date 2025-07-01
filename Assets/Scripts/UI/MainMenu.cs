using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject panelMenu;       // Arrastra el Panel_Menu aqu� en el Inspector
    public GameObject panelOpciones;
    public GameObject BotonMapaBack;
    public GameObject player;         // Arrastra el GameObject del jugador
    public GameObject mapContractsUI; // Panel con los botones de contratos
    public CameraManager cameraManager; // Referencia al script CameraManager

    void Start()
    {
        Debug.Log("Ruta de guardado: " + Application.persistentDataPath);

        GameState estadoGuardado = SaveSystem.CargarEstado();

        if (estadoGuardado != null)
        {
            // Restaurar el estado desde el JSON
            SaveSystem.RestaurarDesdeGameState(estadoGuardado);

            // Aplicar visibilidad y posici�n de NPCs seg�n el estado guardado
            AplicarEstadoNPCs(estadoGuardado);

            // Ocultar men�s
            panelMenu.SetActive(false);
            panelOpciones.SetActive(false);
            BotonMapaBack.SetActive(true);
            mapContractsUI.SetActive(true);

            player.SetActive(true);

            // Transici�n de c�mara desde estado guardado
            if (cameraManager != null)
            {
                cameraManager.RestaurarPosicionDesdeGuardado();
                cameraManager.TransitionToPlayer();
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;

            return;
        }

        // Si no hay partida guardada: mostrar men� principal
        Time.timeScale = 0f;
        panelMenu.SetActive(true);
        panelOpciones.SetActive(false);
        BotonMapaBack.SetActive(false);
        mapContractsUI.SetActive(false);
        player.SetActive(true); // O desact�valo si quieres mostrarlo despu�s

        if (cameraManager != null)
        {
            cameraManager.ConfigurarVistaInicial(); // Vista a�rea
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public void OnPlayPressed()
    {
        panelMenu.SetActive(false);
        mapContractsUI.SetActive(true);
        BotonMapaBack.SetActive(true);

        // Asegurarse que la c�mara est� en vista a�rea
        if (cameraManager != null)
        {
            cameraManager.ConfigurarVistaInicial();
        }
    }

    public void OnContractSelected()
    {
        mapContractsUI.SetActive(false);
        BotonMapaBack.SetActive(false);
        player.SetActive(true);

        if (cameraManager != null)
        {
            cameraManager.TransitionToPlayer();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }

        Time.timeScale = 1f; // Reanuda el juego cuando empieza a jugar
    }

    public void OnOptionsPressed()
    {
        panelOpciones.SetActive(true);
        panelMenu.SetActive(false);
        Debug.Log("Abrir opciones...");
        // Implementa l�gica de opciones aqu�
    }

    public void OnBackButton()
    {
        panelOpciones.SetActive(false);
        panelMenu.SetActive(true);
        mapContractsUI.SetActive(false);
        BotonMapaBack.SetActive(false);
    }

    public void OnQuitPressed()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    NPCInteractivo BuscarNPCPorNombre(string nombre)
    {
        return FindObjectsByType<NPCInteractivo>(FindObjectsSortMode.None).FirstOrDefault(n => n.name == nombre);
    }

    IEnumerator MostrarDialogoSueldo(NPCInteractivo npc)
    {
        // Evitar que desaparezca el NPC cuando se cierre este di�logo
        npc.ocultarAlFinalizarDialogo = false;

        player.GetComponent<PlayerMovement>().enabled = false;

        // Opcional: detener CamaraFollow si est� activa
        CamaraFollow follow = Camera.main.GetComponent<CamaraFollow>();
        if (follow != null) follow.enabled = false;

        // Posicionar c�mara detr�s del jugador mirando al NPC
        Camera.main.transform.position = npc.transform.position + new Vector3(0, 2.5f, -3);
        Camera.main.transform.LookAt(npc.transform.position + Vector3.up * 1.5f);

        yield return new WaitForSeconds(0.4f);

        DialogueManager dialogueManager = FindAnyObjectByType<DialogueManager>();
        dialogueManager.StartDialogue(CrearDialogoSueldo(npc), npc);

        // Espera a que el jugador cierre el di�logo
        yield return new WaitUntil(() => !dialogueManager.IsDialogueActive());

        // Activar movimiento nuevamente
        player.GetComponent<PlayerMovement>().enabled = true;
        if (follow != null) follow.enabled = true;

        // Limpiar estado
        GameState estado = SaveSystem.CargarEstado();
        estado.mostrarDialogoSueldo = false;
        estado.npcActivoNombre = null;
        SaveSystem.GuardarEstado(estado);
    }

    void AplicarEstadoNPCs(GameState estadoGuardado)
    {
        if (estadoGuardado == null || estadoGuardado.estadosNPCs == null) return;

        NPCInteractivo[] npcsEnEscena = FindObjectsByType<NPCInteractivo>(FindObjectsSortMode.None);

        foreach (var npcEstado in estadoGuardado.estadosNPCs)
        {
            var npc = System.Array.Find(npcsEnEscena, n => n.name == npcEstado.npcName);
            if (npc != null)
            {
                npc.gameObject.SetActive(npcEstado.npcVisible);

                // Opcional: actualizar posici�n y rotaci�n
                npc.transform.position = npcEstado.posicionNPC;
                npc.transform.rotation = Quaternion.Euler(npcEstado.rotacionNPC);
            }
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
            dialogueText = "Tu sueldo ser� de $" + npc.contratoAsignado.sueldo.ToString("N0")
        }
        };
        return seq;
    }
}
