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

    void Start()
    {
        decisionPanel.SetActive(false);
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
            botonAceptar.onClick.RemoveAllListeners();
            botonRechazar.onClick.RemoveAllListeners();

            botonAceptar.onClick.AddListener(() => {
                decisionPanel.SetActive(false);
                TransportarAMinijuego();
            });

            botonRechazar.onClick.AddListener(() => {
                decisionPanel.SetActive(false);
                EndDialogue();
            });
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
            npcActual.FinalizarConversacion();
            npcActual = null;
        }
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            DisplayNextLine();
        }
    }

    public void TransportarAMinijuego()
    {
        // Podrías usar SceneManager para cargar otra escena
        // o activar un panel de minijuego
        // Aquí un ejemplo simple:

        PlayerPrefs.SetString("NPCQueAsignoTrabajo", npcActual.name);
        UnityEngine.SceneManagement.SceneManager.LoadScene("NombreDelMinijuego");
    }
}
