using UnityEngine;

public class NPCEmpleo : MonoBehaviour
{
    public string idUnico;
    public JobData contrato;
    public bool tieneContrato;

    public void AsignarContrato(JobData job)
    {
        contrato = job;
        tieneContrato = true;

        GetComponentInChildren<Renderer>().material.color = Color.green;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (tieneContrato && !contrato.revelado && other.CompareTag("Player"))
        {
            contrato.revelado = true;
            Debug.Log("El NPC te ofreció un empleo: " + contrato.nombre + " con sueldo $" + contrato.sueldo);
            GetComponentInChildren<Renderer>().material.color = Color.gray;

            var listaUI = FindAnyObjectByType<UIListaSueldos>();
            if (listaUI != null)
            {
                listaUI.MarcarSueldoComoRevelado(contrato.sueldo);
            }
        }
    }
}
