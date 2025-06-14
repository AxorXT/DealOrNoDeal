using UnityEngine;
using UnityEngine.UI;

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

    [Header("Datos del contrato")]
    public JobData contratoAsignado;

    [Header("Opciones especiales")]
    public bool mostrarDecisionInicial = true;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        playerMovement = jugador.GetComponent<PlayerMovement>();
        dialogueManager = Object.FindFirstObjectByType<DialogueManager>();
        camaraFollow = Camera.main.GetComponent<CamaraFollow>();

        if (iconoE != null)
        {
            Vector3 posicionIcono = transform.position + Vector3.up * alturaIcono;
            instanciaIcono = Instantiate(iconoE, posicionIcono, Quaternion.identity, transform);
            instanciaIcono.SetActive(false);
        }
    }

    void Update()
    {
        if (jugador == null) return;

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

        if (dialogueManager != null && secuenciaDeDialogo != null)
        {
            dialogueManager.StartDialogue(secuenciaDeDialogo, this);
        }
    }

    public void FinalizarConversacion()
    {
        if (playerMovement != null) playerMovement.enabled = true;
        if (camaraFollow != null) camaraFollow.ClearFocus();
        gameObject.SetActive(false);
    }
}
