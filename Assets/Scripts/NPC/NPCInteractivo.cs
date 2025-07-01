using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NPCInteractivo : MonoBehaviour
{
    public GameObject iconoE; // Prefab del ícono E (World Space UI)
    private GameObject instanciaIcono;
    private Transform jugador;
    public float distanciaParaInteractuar = 3f;

    private bool jugadorCerca = false;

    [Header("Altura del ícono E")]
    public float alturaIcono = 2.2f;

    [Header("Diálogo")]
    public DialogueSequence secuenciaDeDialogo;

    private DialogueManager dialogueManager;
    private PlayerMovement playerMovement;
    private CamaraFollow camaraFollow;

    [Header("Segundo diálogo después del minijuego")]
    public DialogueSequence segundoDialogo;

    [HideInInspector]
    public bool trabajoYaAsignado = false;

    [Header("Datos del contrato")]
    public JobData contratoAsignado;

    [Header("Opciones especiales")]
    public bool mostrarDecisionInicial = true;

    [HideInInspector]
    public bool ocultarAlFinalizarDialogo = true;

    void Start()
    {
    // 1. Buscar jugador si no está asignado
    if (jugador == null)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) jugador = playerObj.transform;
    }

    // 2. Obtener componentes de forma segura
    if (jugador != null)
    {
        playerMovement = jugador.GetComponent<PlayerMovement>();
    }
    else
    {
        Debug.LogError("No se encontró al jugador en la escena");
    }

    // 3. Buscar DialogueManager si no está asignado
    if (dialogueManager == null)
    {
        dialogueManager = Object.FindFirstObjectByType<DialogueManager>();
    }

    // 4. Buscar cámara si no está asignada
    if (camaraFollow == null && Camera.main != null)
    {
        camaraFollow = Camera.main.GetComponent<CamaraFollow>();
    }

    // 5. Crear ícono "E" solo si existe el prefab
    if (iconoE != null)
    {
        Vector3 posicionIcono = transform.position + Vector3.up * alturaIcono;
        instanciaIcono = Instantiate(iconoE, posicionIcono, Quaternion.identity, transform);
        instanciaIcono.SetActive(false);
    }
    else
    {
        Debug.LogWarning("Prefab del ícono E no asignado en NPC: " + gameObject.name);
    }
}

    void Update()
    {
        if (jugador == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                jugador = playerObj.transform;
                playerMovement = jugador.GetComponent<PlayerMovement>();
            }
            else
            {
                return;
            }
        }

        float distancia = Vector3.Distance(jugador.position, transform.position);
        jugadorCerca = distancia <= distanciaParaInteractuar;

        if (instanciaIcono != null)
        {
            instanciaIcono.SetActive(jugadorCerca);
            if (jugadorCerca)
            {
                instanciaIcono.transform.position = transform.position + Vector3.up * alturaIcono;
                instanciaIcono.transform.LookAt(Camera.main.transform);
                instanciaIcono.transform.Rotate(0, 180, 0);
            }
        }

        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            IniciarConversacion();
        }
    }

    public void IniciarConversacion()
    {
        if (playerMovement != null) playerMovement.enabled = false;
        if (camaraFollow != null) camaraFollow.SetFocus(transform);

        if (dialogueManager != null)
        {
            if (trabajoYaAsignado && segundoDialogo != null)
            {
                dialogueManager.StartDialogue(segundoDialogo, this);
            }
            else if (secuenciaDeDialogo != null)
            {
                dialogueManager.StartDialogue(secuenciaDeDialogo, this);
            }
        }
    }

    public void FinalizarConversacion(bool ocultarNPC = true)
    {
        if (playerMovement != null) playerMovement.enabled = true;
        if (camaraFollow != null) camaraFollow.ClearFocus();

        if (ocultarAlFinalizarDialogo)
            gameObject.SetActive(false);
    }

    internal NPCInteractivo FirstOrDefault(System.Func<object, bool> value)
    {
        throw new System.NotImplementedException();
    }

    public void AceptarTrabajoDesdeDialogo()
    {
        GameState estado = SaveSystem.CargarEstado() ?? new GameState();
        estado.npcActivoNombre = this.name; // Asegúrate de que cada NPC tenga un nombre único
        estado.mostrarDialogoSueldo = true;
        SaveSystem.GuardarEstado(estado);

        SceneManager.LoadScene("JUEGO"); // Cambiar por el nombre real de tu escena
    }
}
