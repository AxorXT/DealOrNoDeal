using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulacionMinijuegoManager : MonoBehaviour
{
    public void VolverAlMapa()
    {
        SceneManager.LoadScene("JUEGO");
    }
}
