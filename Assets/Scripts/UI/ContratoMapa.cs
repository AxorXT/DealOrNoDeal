using UnityEngine;
using UnityEngine.UI;

public class ContratoMapa : MonoBehaviour
{
    public JobData contratoAsignado;  // Asigna el ScriptableObject del contrato en el Inspector
    private Button botonContrato;
    private NPCInteractivo npcAsignado;

    [System.Obsolete]
    void Start()
    {
        botonContrato = GetComponent<Button>();
        if (botonContrato != null)
        {
            botonContrato.onClick.AddListener(OnContratoSeleccionado);
        }
    }

    public void AsignarNPC(NPCInteractivo npc)
    {
        npcAsignado = npc;
        if (npcAsignado != null)
        {
            contratoAsignado = npcAsignado.contratoAsignado;
        }
    }

    
    public void OnContratoSeleccionado()
    {
        if (npcAsignado != null)
        {
            // 1. Asignar este contrato al NPC vinculado
            npcAsignado.contratoAsignado = contratoAsignado;

            // 2. Notificar al GameManager
            GameManager.Instance.ContratoSeleccionado = contratoAsignado;

            // 3. Ocultar todos los botones de contrato
            foreach (var boton in Object.FindObjectsByType<ContratoMapa>(FindObjectsSortMode.None))
            {
                boton.gameObject.SetActive(false);
            }

            // 4. Transición a la vista del jugador
            FindAnyObjectByType<CameraManager>().TransitionToPlayer();
        }
    }
}
