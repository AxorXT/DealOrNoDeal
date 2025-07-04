using UnityEngine;
using UnityEngine.UI;

public class ContratoMapa : MonoBehaviour
{
    
    public JobData contratoAsignado;  // Asigna el ScriptableObject del contrato en el Inspector
    private Button botonContrato;
    private NPCInteractivo npcAsignado;
    
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
            npcAsignado.contratoAsignado = contratoAsignado;
            GameManager.Instance.ContratoSeleccionado = contratoAsignado;

            
            //Guardar el estado del diálogo pendiente
            GameState estado = SaveSystem.CargarEstado();
            if (estado == null)
            {
                estado = new GameState();
            }
            estado.mostrarDialogoSueldo = true;
            estado.npcActivoNombre = npcAsignado.name;
            SaveSystem.GuardarEstado(estado); //IMPORTANTE

            // Ocultar todos los botones de contrato
            foreach (var boton in Object.FindObjectsByType<ContratoMapa>(FindObjectsSortMode.None))
            {
                boton.gameObject.SetActive(false);
            }

            // Hacer la transición
            FindAnyObjectByType<CameraManager>().TransitionToPlayer();
        }
    }
}
