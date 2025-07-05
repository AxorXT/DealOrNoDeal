using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject panelMenu;� � � �// Arrastra el Panel_Menu aqu� en el Inspector
� � public GameObject panelOpciones;
    public GameObject BotonMapaBack;
    public GameObject player;� � � � �// Arrastra el GameObject del jugador
� � public GameObject mapContractsUI; // Panel con los botones de contratos
� � public CameraManager cameraManager; // Referencia al script CameraManager

    void Awake()
    {
        SaveFlags.estadoRestauradoDesdeArchivo = false;
    }

    void Start()
    {
        StartCoroutine(FlujoInicial());
    }

    IEnumerator FlujoInicial()
    {
        SaveFlags.estadoRestauradoDesdeArchivo = false;

        Debug.Log("Ruta de guardado: " + Application.persistentDataPath);

        GameState estadoGuardado = SaveSystem.CargarEstado();
        ManagerNPCs manager = FindFirstObjectByType<ManagerNPCs>();

        if (estadoGuardado != null)
        {
            if (!string.IsNullOrEmpty(estadoGuardado.npcActivoNombre))
            {
                yield return new WaitUntil(() =>
                {
                    bool npcExiste = FindObjectsByType<NPCInteractivo>(FindObjectsSortMode.None)
                        .Any(n => n.idUnico == estadoGuardado.npcActivoNombre);
                    bool dialogueManagerExiste = FindAnyObjectByType<DialogueManager>() != null;

                    return npcExiste && dialogueManagerExiste;
                });
            }
            SaveSystem.RestaurarDesdeGameState(estadoGuardado);

            if (manager != null)
            {
                manager.RestaurarNPCsDesdeSave(estadoGuardado.estadosNPCs);
            }

            panelMenu.SetActive(false);
            panelOpciones.SetActive(false);
            BotonMapaBack.SetActive(false);
            mapContractsUI.SetActive(false);
            if (player != null)
            {
                player.SetActive(true);
            }
            else
            {
                Debug.LogWarning("player es null o fue destruido");
            }

            if (cameraManager != null)
            {
                if (!SaveFlags.estadoRestauradoDesdeArchivo)
                {
                    cameraManager.RestaurarPosicionDesdeGuardado(); // O ConfigurarVistaInicial()
                }

                cameraManager.TransitionToPlayer(); // Esto s� lo puedes dejar
            }

            if (estadoGuardado.mostrarDialogoSueldo && !string.IsNullOrEmpty(estadoGuardado.npcActivoNombre))
            {
                cameraManager.permitirControlCamara = false; //Desactiva transici�n autom�tica

                NPCInteractivo npc = BuscarNPCPorNombre(estadoGuardado.npcActivoNombre);
                if (npc != null)
                    yield return StartCoroutine(MostrarDialogoSueldo(npc));
            }
            GameObject nubesMenu = GameObject.FindGameObjectWithTag("FondoNubes"); // o por tag
            if (nubesMenu != null)
            {
                nubesMenu.SetActive(false);
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

        ManagerNPCs manager = FindAnyObjectByType<ManagerNPCs>();

        if (!SaveFlags.estadoRestauradoDesdeArchivo)
        {
            GameState estadoGuardado = SaveSystem.CargarEstado();

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
        }
        else
        {
            Debug.Log("El estado ya fue restaurado previamente, se evita la sobreescritura.");
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
� � }

    public void OnOptionsPressed()
    {
        panelOpciones.SetActive(true);
        panelMenu.SetActive(false);
        Debug.Log("Abrir opciones...");
� � � � // Implementa l�gica de opciones aqu�
� � }

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
� � � � UnityEditor.EditorApplication.isPlaying = false;
#endif
� � }

    NPCInteractivo BuscarNPCPorNombre(string nombre)
    {
        return FindObjectsByType<NPCInteractivo>(FindObjectsSortMode.None)
        .FirstOrDefault(n => n.idUnico == nombre || n.name == nombre);
    }

    IEnumerator MostrarDialogoSueldo(NPCInteractivo npc)
    {
        npc.ocultarAlFinalizarDialogo = true;
        player.GetComponent<PlayerMovement>().enabled = true;

        CamaraFollow follow = Camera.main.GetComponent<CamaraFollow>();
        if (follow != null)
        {
            follow.SetFocus(npc.transform); // Usar el sistema de foco
            follow.enabled = true;
        }

        yield return new WaitForSeconds(0.4f);

        DialogueManager dialogueManager = FindAnyObjectByType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogWarning("DialogueManager no encontrado o destruido. No se puede mostrar di�logo.");
            yield break; // Salir de la coroutine para evitar errores
        }

        // Elegir el segundo di�logo si el trabajo ya fue aceptado
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

        // Limpiar estado para no repetir el di�logo
        GameState estado = SaveSystem.CargarEstado();
        estado.mostrarDialogoSueldo = false;
        estado.npcActivoNombre = null;
        SaveSystem.GuardarEstado(estado);
        GameManager.Instance.GuardarManual();
    }

    private DialogueSequence CrearDialogoSueldo(NPCInteractivo npc)
    {
        DialogueSequence seq = ScriptableObject.CreateInstance<DialogueSequence>();
        seq.lines = new DialogueLine[]
        {
            new DialogueLine
            {
                speakerName = npc.name, dialogueText = "Tu sueldo ser� de $" + npc.contratoAsignado.sueldo.ToString("N0")
            }
        };
        return seq;
    }

    public static class SaveFlags
    {
        public static bool estadoRestauradoDesdeArchivo = false;
    }
}
