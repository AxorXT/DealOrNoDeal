using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class GameState
{
    public Vector3 posicionJugador;
    public Vector3 rotacionJugador;

    public Vector3 posicionCamara;
    public Vector3 rotacionCamara;

    public List<JobDataSave> contratosAsignados = new List<JobDataSave>();
    public List<NPCState> estadosNPCs = new List<NPCState>();
    public List<int> sueldosRevelados = new List<int>();

    public bool juegoEnDialogoActivo;
    public string npcActivoNombre;
    public bool mostrarDialogoSueldo = false;
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
    public string idUnico;
    public bool trabajoAsignado;
    public bool npcVisible;
    public Vector3 posicionNPC;
    public Vector3 rotacionNPC;
    public int indexPrefab;
}


