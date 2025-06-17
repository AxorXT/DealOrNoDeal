using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject panelMenu;       // Arrastra el Panel_Menu aquí en el Inspector
    public GameObject player;         // Arrastra el GameObject del jugador
    public GameObject mapContractsUI; // Panel con los botones de contratos
    public CameraManager cameraManager; // Referencia al script CameraManager

    void Start()
    {
        panelMenu.SetActive(true);
        player.SetActive(true);
        mapContractsUI.SetActive(false);

        // Opcional: Asegurar que la cámara empiece en vista de mapa
        if (cameraManager != null)
        {
            cameraManager.ConfigurarVistaInicial();
        }
    }

    public void OnPlayPressed()
    {
        panelMenu.SetActive(false);
        mapContractsUI.SetActive(true);

        // Asegurarse que la cámara está en vista aérea
        if (cameraManager != null)
        {
            cameraManager.ConfigurarVistaInicial();
        }
    }

    public void OnContractSelected()
    {
        mapContractsUI.SetActive(false);
        player.SetActive(true);

        if (cameraManager != null)
        {
            cameraManager.TransitionToPlayer(); // Transición a la vista del jugador
        }
    }

    public void OnOptionsPressed()
    {
        Debug.Log("Abrir opciones...");
        // Implementa lógica de opciones aquí
    }

    public void OnQuitPressed()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
