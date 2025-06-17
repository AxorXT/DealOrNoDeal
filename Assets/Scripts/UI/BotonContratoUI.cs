using UnityEngine;
using UnityEngine.UI;

public class BotonContratoUI : MonoBehaviour
{
    private JobData contrato;
    private ManagerNPCs manager;

    public void AsignarContrato(JobData job, ManagerNPCs managerNPCs)
    {
        contrato = job;
        manager = managerNPCs;

        
    }

    void Update()
    {
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0);
    }
}
