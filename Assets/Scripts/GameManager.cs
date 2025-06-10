using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<JobData> contratosDisponibles = new List<JobData>();
    public int totalContratos = 16;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); // Asegura Singleton

        DontDestroyOnLoad(gameObject); // Opcional: conserva entre escenas
    }

    void Start()
    {
        GenerarContratos();
    }

    void GenerarContratos()
    {
        List<int> sueldos = new List<int> {
            100, 200, 300, 500, 800, 1000, 1500,
            2000, 3000, 5000, 8000, 10000, 15000, 20000, 50000, 100000
        };

        sueldos = Shuffle(sueldos);

        for (int i = 0; i < totalContratos; i++)
        {
            JobData j = new JobData();
            j.nombre = "Empleo #" + (i + 1);
            j.sueldo = sueldos[i];
            j.revelado = false;
            contratosDisponibles.Add(j);
        }
    }

    List<int> Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
        return list;
    }
}
