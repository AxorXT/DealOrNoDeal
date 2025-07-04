using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject panelMenu;       // Arrastra el Panel_Menu aquí en el Inspector
    public GameObject panelOpciones;
    public GameObject BotonMapaBack;
    public GameObject player;         // Arrastra el GameObject del jugador
    public GameObject mapContractsUI; // Panel con los botones de contratos
    public CameraManager cameraManager; // Referencia al script CameraManager

    void Start()
    {
        StartCoroutine(FlujoInicial());
    }

    IEnumerator FlujoInicial()
    {
        Debug.Log("Ruta de guardado: " + Application.persistentDataPath);

        GameState estadoGuardado = SaveSystem.CargarEstado();
        ManagerNPCs manager = FindFirstObjectByType<ManagerNPCs>();

        if (estadoGuardado != null)
        {

            // Esperar hasta que los NPCs existan en la escena
            yield return new WaitUntil(() => FindObjectsByType<NPCInteractivo>(FindObjectsSortMode.None).Length > 0);

            SaveSystem.RestaurarDesdeGameState(estadoGuardado);

            if (manager != null)
            {
                manager.RestaurarNPCsDesdeSave(estadoGuardado.estadosNPCs);
            }

            panelMenu.SetActive(false);
            panelOpciones.SetActive(false);
            BotonMapaBack.SetActive(false);
            mapContractsUI.SetActive(false);
            player.SetActive(true);

            if (cameraManager != null)
            {
                if (!SaveFlags.estadoRestauradoDesdeArchivo)
                {
                    cameraManager.RestaurarPosicionDesdeGuardado(); // O ConfigurarVistaInicial()
                }

                cameraManager.TransitionToPlayer(); // Esto sí lo puedes dejar
            }

            if (estadoGuardado.mostrarDialogoSueldo && !string.IsNullOrEmpty(estadoGuardado.npcActivoNombre))
            {
                cameraManager.permitirControlCamara = false; //Desactiva transición automática

                NPCInteractivo npc = BuscarNPCPorNombre(estadoGuardado.npcActivoNombre);
                if (npc != null)
                    yield return StartCoroutine(MostrarDialogoSueldo(npc));
            }
            else
            {
                if (cameraManager != null)
                {
                    cameraManager.RestaurarPosicionDesdeGuardado();
                    cameraManager.TransitionToPlayer();
                }
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            
        }
        else
        {
            Time.timeScale = 0f;
            panelMenu.SetActive(true);
            panelOpciones.SetActive(false);
            BotonMapaBack.SetActive(false);
            mapContractsUI.SetActive(false);
            player.SetActive(true);

            if (cameraManager != null)
            {
                cameraManager.ConfigurarVistaInicial();
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


    public void OnPlayPressed()
    {
        panelMenu.SetActive(false);
        mapContractsUI.SetActive(true);
        BotonMapaBack.SetActive(true);

        GameState estadoGuardado = SaveSystem.CargarEstado();
        ManagerNPCs manager = FindAnyObjectByType<ManagerNPCs>();

        if (estadoGuardado != null)
        {
            SaveSystem.RestaurarDesdeGameState(estadoGuardado);
        }
        else
        {
            if (manager != null)
            {
                manager.InicializarEscena(); // Solo si no hay guardado
            }
        }

        if (cameraManager != null)
        {
            if (!SaveFlags.estadoRestauradoDesdeArchivo)
            {
                cameraManager.ConfigurarVistaInicial();
            }
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
        // Implementa lógica de opciones aquí
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
        return FindObjectsByType<NPCInteractivo>(FindObjectsSortMode.None)
        .FirstOrDefault(n => n.idUnico == nombre || n.name == nombre);
    }

    IEnumerator MostrarDialogoSueldo(NPCInteractivo npc)
    {
        npc.ocultarAlFinalizarDialogo = false;
        player.GetComponent<PlayerMovement>().enabled = false;

        CamaraFollow follow = Camera.main.GetComponent<CamaraFollow>();
        if (follow != null)
        {
            follow.SetFocus(npc.transform); // Usar el sistema de foco
            follow.enabled = true;
        }

        yield return new WaitForSeconds(0.4f);

        DialogueManager dialogueManager = FindAnyObjectByType<DialogueManager>();

        //Elegir el segundo diálogo si el trabajo ya fue aceptado
        if (npc.trabajoYaAsignado && npc.segundoDialogo != null)
        {
            dialogueManager.StartDialogue(npc.segundoDialogo, npc);
        }
        else
        {
            dialogueManager.StartDialogue(CrearDialogoSueldo(npc), npc);
        }

        yield return new WaitUntil(() => !dialogueManager.IsDialogueActive());

        player.GetComponent<PlayerMovement>().enabled = true;
        if (follow != null)
        {
            follow.ClearFocus();

            if (cameraManager != null && cameraManager.permitirControlCamara)
                follow.enabled = true;
            else
                follow.enabled = false;
        }

        //Limpiar estado para no repetir el diálogo
        GameState estado = SaveSystem.CargarEstado();
        estado.mostrarDialogoSueldo = false;
        estado.npcActivoNombre = null;
        SaveSystem.GuardarEstado(estado);
    }

    private DialogueSequence CrearDialogoSueldo(NPCInteractivo npc)
    {
        DialogueSequence seq = ScriptableObject.CreateInstance<DialogueSequence>();
        seq.lines = new DialogueLine[]
        {
            new DialogueLine
            {
                speakerName = npc.name, dialogueText = "Tu sueldo será de $" + npc.contratoAsignado.sueldo.ToString("N0")
            }
        };
        return seq;
    }

    public static class SaveFlags
    {
        public static bool estadoRestauradoDesdeArchivo = false;
    }
}
