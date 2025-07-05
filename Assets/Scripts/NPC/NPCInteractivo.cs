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

    public string idUnico;
    public int indexPrefab;

    void Awake()
    {
        // Busca el root
        GameObject root = gameObject.transform.root.gameObject;
        DontDestroyOnLoad(root);
    }

    void Start()
    {
        // Obtener referencias necesarias
        jugador = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (jugador != null)
            playerMovement = jugador.GetComponent<PlayerMovement>();

        dialogueManager = FindAnyObjectByType<DialogueManager>();
        camaraFollow = Camera.main?.GetComponent<CamaraFollow>();

        if (iconoE != null)
        {
            instanciaIcono = Instantiate(iconoE, transform.position + Vector3.up * alturaIcono, Quaternion.identity, transform);
            instanciaIcono.SetActive(false);
        }
    }

    void Update()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(jugador.position, transform.position);
        bool jugadorCerca = distancia <= distanciaParaInteractuar;

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
                dialogueManager.StartDialogue(segundoDialogo, this);
            else
                dialogueManager.StartDialogue(secuenciaDeDialogo, this);
        }
    }

    public void FinalizarConversacion(bool ocultarNPC = true)
    {
        if (playerMovement != null) playerMovement.enabled = true;
        if (camaraFollow != null) camaraFollow.ClearFocus();

        if (ocultarAlFinalizarDialogo)
            gameObject.SetActive(true);
    }

    public void AceptarTrabajoDesdeDialogo()
    {
        // 1. Marcar que este NPC ya fue asignado
        trabajoYaAsignado = true;

        // 2. Actualizar datos relevantes en GameState
        GameState estado = SaveSystem.CrearGameStateActual(); // Snapshot completo

        estado.npcActivoNombre = idUnico; // Usa el ID único, no el nombre del GameObject
        estado.mostrarDialogoSueldo = true;

        // 3. Verificar si este NPC ya tiene estado previo
        NPCState npcState = estado.estadosNPCs.Find(n => n.idUnico == idUnico);
        if (npcState != null)
        {
            npcState.trabajoAsignado = true;
            npcState.npcVisible = true;
            npcState.posicionNPC = transform.position;
            npcState.rotacionNPC = transform.eulerAngles;
        }
        else
        {
            estado.estadosNPCs.Add(new NPCState
            {
                idUnico = idUnico,
                trabajoAsignado = true,
                npcVisible = true,
                posicionNPC = transform.position,
                rotacionNPC = transform.eulerAngles,
                indexPrefab = indexPrefab
            });
        }

        // 4. Guardar el estado completo
        SaveSystem.GuardarEstado(estado);

        // 5. Cambiar de escena
        SceneManager.LoadScene("SimulacionMinijuego");
    }

    public void IniciarDialogoAutomatico()
    {
        if (playerMovement == null)
            playerMovement = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerMovement>();

        if (camaraFollow == null)
            camaraFollow = Camera.main?.GetComponent<CamaraFollow>();

        if (playerMovement != null) playerMovement.enabled = false;
        if (camaraFollow != null) camaraFollow.SetFocus(transform);

        if (dialogueManager == null)
            dialogueManager = FindAnyObjectByType<DialogueManager>();

        if (dialogueManager != null)
        {
            if (trabajoYaAsignado && segundoDialogo != null)
                dialogueManager.StartDialogue(segundoDialogo, this);
            else
                dialogueManager.StartDialogue(secuenciaDeDialogo, this);
        }
    }
}
