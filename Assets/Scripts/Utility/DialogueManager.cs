using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DialogueManager : MonoBehaviour
{
    private TextMeshProUGUI dialogueText;
    private string currentDialogue;
    // Start is called before the first frame update
    void Start()
    {
        currentDialogue = "no dialogue yet";
        dialogueText = (this.gameObject.transform.GetChild(0)).GetComponent<TextMeshProUGUI>();
        dialogueText.text = currentDialogue;
    }

    // Update is called once per frame
    void Update()
    {
        dialogueText.text = currentDialogue;
    }

    public void TriggerDialogue(string dialogueKey)
    {
        if(dialogueKey == "test") {
            currentDialogue = "test!";
        }
        if(dialogueKey == "clear") {
            currentDialogue = "";
        }
    }
}
