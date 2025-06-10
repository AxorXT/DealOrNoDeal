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

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;

        // Instanciar pero desactivado al inicio
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
            instanciaIcono.SetActive(jugadorCerca);

        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            // Aquí abres tu UI de diálogo o conversación
            Debug.Log("Iniciar conversación con " + gameObject.name);
            // TODO: Llamar a tu sistema de diálogo
        }
    }
}
