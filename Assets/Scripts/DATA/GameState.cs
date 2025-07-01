using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class GameState
{
    // Jugador
    public Vector3 posicionJugador;
    public float rotacionJugadorY;

    // Cámara
    public Vector3 posicionCamara;
    public Vector3 rotacionCamara;

    // Contratos activos y asignados
    public List<JobDataSave> contratosAsignados;

    // Estado de NPCs (quién ya fue descartado o no)
    public List<NPCState> estadosNPCs;

    // Sueldos revelados (para UIListaSueldos)
    public List<int> sueldosRevelados;

    // Cualquier otro estado global que quieras guardar
    public bool juegoEnDialogoActivo;

    // ID del NPC activo al que se le asignó trabajo
    public string npcActivoNombre;

    // Bandera para saber si al regresar se debe mostrar el segundo diálogo
    public bool mostrarDialogoSueldo = false;

    public GameState()
    {
        contratosAsignados = new List<JobDataSave>();
        estadosNPCs = new List<NPCState>();
        sueldosRevelados = new List<int>();
    }
}

[Serializable]
public class JobDataSave
{
    public string nombre;
    public int sueldo;
}

[Serializable]
public class NPCState
{
    public string npcName;
    public bool trabajoAsignado;
    public bool npcVisible; // Si está activo o descartado en el mapa

    public Vector3 posicionNPC;
    public Vector3 rotacionNPC;
}


