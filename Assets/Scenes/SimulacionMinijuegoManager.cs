using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulacionMinijuegoManager : MonoBehaviour
{
    public void VolverAlMapa()
    {
        GameManager.Instance.VolverAlMapaPrincipal();
        SceneManager.LoadScene("JUEGO");
    }
}
