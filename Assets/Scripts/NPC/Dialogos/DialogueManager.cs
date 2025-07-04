using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;

    private Queue<DialogueLine> dialogueLines = new Queue<DialogueLine>();
    private NPCInteractivo npcActual;
    private bool isDialogueActive = false;

    public GameObject decisionPanel; // Contenedor de botones (sí/no)
    public Button botonAceptar;
    public Button botonRechazar;
    public Button botonCerrar;

    private UIListaSueldos uiListaSueldos;

    void Start()
    {
        if (decisionPanel != null) decisionPanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (botonCerrar != null) botonCerrar.gameObject.SetActive(false);

        uiListaSueldos = FindAnyObjectByType<UIListaSueldos>();
    }

    public void StartDialogue(DialogueSequence sequence, NPCInteractivo npc)
    {
        npcActual = npc;

        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        decisionPanel.SetActive(false);

        dialogueLines.Clear();
        foreach (DialogueLine line in sequence.lines)
        {
            dialogueLines.Enqueue(line);
        }

        // Mostrar la primera línea (intro) y luego botones
        DisplayNextLine();

        if (npcActual.mostrarDecisionInicial)
        {
            decisionPanel.SetActive(true);
            botonCerrar.gameObject.SetActive(false);
            botonAceptar.onClick.RemoveAllListeners();
            botonRechazar.onClick.RemoveAllListeners();

            botonAceptar.onClick.AddListener(() =>
            {
                decisionPanel.SetActive(false);
                if (npcActual != null)
                {
                    npcActual.AceptarTrabajoDesdeDialogo();
                }
            });

            botonRechazar.onClick.AddListener(() =>
            {
                decisionPanel.SetActive(false);
                dialoguePanel.SetActive(false);
                isDialogueActive = false;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                npcActual.FinalizarConversacion(false);
                npcActual = null;
            });
        }
        else if (npcActual.trabajoYaAsignado)
        {
            // Estamos en segundo diálogo: ocultar aceptar/rechazar, mostrar solo cerrar
            decisionPanel.SetActive(true);
            botonAceptar.gameObject.SetActive(false);
            botonRechazar.gameObject.SetActive(false);
            botonCerrar.gameObject.SetActive(true);

            botonCerrar.onClick.RemoveAllListeners();
            botonCerrar.onClick.AddListener(() =>
            {
                // Aquí llamamos a la función para cerrar el diálogo y reactivar todo
                // Necesitas exponer esta función en DialogueManager o llamar a MainMenu (puede inyectarse o usarse singleton)
                CerrarDialogoManual();
            });
        }
        else
        {
            // Por defecto, ocultar todo
            decisionPanel.SetActive(false);
            botonCerrar.gameObject.SetActive(false);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void DisplayNextLine()
    {
        if (dialogueLines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = dialogueLines.Dequeue();
        string texto = line.dialogueText;

        if (npcActual != null && npcActual.contratoAsignado != null)
        {
            texto = texto.Replace("{SUELDO}", npcActual.contratoAsignado.sueldo.ToString("N0"));
        }
        dialogueText.text = texto;
        speakerNameText.text = line.speakerName;
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        isDialogueActive = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (npcActual != null)
        {
            if (npcActual.trabajoYaAsignado)
            {
                npcActual.gameObject.SetActive(false);
                uiListaSueldos?.MarcarSueldoComoRevelado(npcActual.contratoAsignado?.sueldo ?? 0);
            }

            npcActual.FinalizarConversacion();
            npcActual = null;
        }
    }

    public bool IsDialogueActive() => isDialogueActive;

    public void SetDialogueActive(bool estado)
    {
        isDialogueActive = estado;
        dialoguePanel.SetActive(estado);
    }

    public void CerrarDialogoManual()
    {
        if (npcActual != null)
        {
            if (npcActual != null)
            {
                PlayerMovement pm = npcActual.GetComponentInParent<PlayerMovement>();
                if (pm != null) pm.enabled = true;

                CamaraFollow camFollow = Camera.main?.GetComponent<CamaraFollow>();
                if (camFollow != null)
                {
                    camFollow.ClearFocus();
                    camFollow.enabled = true;
                }

                SetDialogueActive(false);

                uiListaSueldos?.MarcarSueldoComoRevelado(npcActual.contratoAsignado?.sueldo ?? 0);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
