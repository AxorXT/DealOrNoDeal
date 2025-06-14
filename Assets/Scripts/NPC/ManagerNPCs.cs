using System.Collections.Generic;
using UnityEngine;

public class ManagerNPCs : MonoBehaviour
{
    public int cantidadEmpleosActivos = 10;

    [Header("Prefabs de NPC")]
    public List<GameObject> npcConContratoPrefabs;
    public List<GameObject> npcDecorativoPrefab;

    [Header("Puntos con rotación tipo A (definida en escena)")]
    public List<Transform> puntosRotacionA;

    [Header("Puntos con rotación tipo B (definida en escena)")]
    public List<Transform> puntosRotacionB;

    private List<JobData> contratosAsignados = new List<JobData>();

    void Start()
    {
        GenerarContratos();
        ColocarNPCsAleatorios();
    }

    void GenerarContratos()
    {
        List<int> sueldos = new List<int> {
            100, 200, 300, 500, 800, 1000, 1500,
            2000, 3000, 5000, 8000, 10000, 15000, 20000, 50000, 100000
        };

        sueldos = Shuffle(sueldos);

        for (int i = 0; i < cantidadEmpleosActivos; i++)
        {
            JobData j = new JobData();
            j.nombre = "Empleo #" + (i + 1);
            j.sueldo = sueldos[i];
            contratosAsignados.Add(j);
        }
    }

    void ColocarNPCsAleatorios()
    {
        List<Transform> todosLosPuntos = new List<Transform>();
        todosLosPuntos.AddRange(puntosRotacionA);
        todosLosPuntos.AddRange(puntosRotacionB);

        List<Transform> puntosAleatorios = Shuffle(new List<Transform>(todosLosPuntos));

        for (int i = 0; i < contratosAsignados.Count; i++)
        {
            Transform punto = puntosAleatorios[i];
            GameObject prefabElegido = npcConContratoPrefabs[Random.Range(0, npcConContratoPrefabs.Count)];

            // Usa la rotación del punto
            GameObject npc = Instantiate(prefabElegido, punto.position, punto.rotation);

            //Asignar contrato al NPCInteractivo
            var npcScript = npc.GetComponent<NPCInteractivo>();
            if (npcScript != null)
            {
                npcScript.contratoAsignado = contratosAsignados[i];
            }

            var npcEmpleo = npc.GetComponent<NPCEmpleo>();
            if (npcEmpleo != null)
            {
                npcEmpleo.AsignarContrato(contratosAsignados[i]);
            }
        }

        for (int i = contratosAsignados.Count; i < puntosAleatorios.Count; i++)
        {
            Transform punto = puntosAleatorios[i];
            GameObject prefabElegido = npcDecorativoPrefab[Random.Range(0, npcDecorativoPrefab.Count)];

            Instantiate(prefabElegido, punto.position, punto.rotation);
        }
    }

    List<T> Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
        return list;
    }
}
