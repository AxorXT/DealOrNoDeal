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

    void Start()
    {
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueSequence sequence, NPCInteractivo npc)
    {
        npcActual = npc;

        isDialogueActive = true;
        dialoguePanel.SetActive(true);

        dialogueLines.Clear();
        foreach (DialogueLine line in sequence.lines)
        {
            dialogueLines.Enqueue(line);
        }

        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (dialogueLines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = dialogueLines.Dequeue();
        dialogueText.text = line.dialogueText;
        speakerNameText.text = line.speakerName;
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        isDialogueActive = false;

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
}
