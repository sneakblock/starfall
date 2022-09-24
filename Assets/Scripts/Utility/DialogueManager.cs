using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DialogueManager : MonoBehaviour
{
    [SerializeField] private List<DialogueEvent> dialogueEvents; // manually put the dialogueevent scriptable objects in
    private Dictionary<string, DialogueEvent> dialogueDictionary = new Dictionary<string, DialogueEvent>();
    private GameObject dialogueBox; 
    private TextMeshProUGUI dialogueText;
    private int lettersPerSecond = 30;
    private SpriteAnimator spriteAnimator = null;

    void Start()
    {
        dialogueBox = this.gameObject;
        dialogueBox.SetActive(false);
        dialogueText = (this.gameObject.transform.GetChild(0)).GetComponent<TextMeshProUGUI>();
        dialogueText.text = "THIS IS A MESSAGE";
        for (int i = 0; i < dialogueEvents.Count; i++)
        {
            dialogueEvents[i].Animator = new SpriteAnimator(dialogueEvents[i].Sprites, (this.gameObject.transform.GetChild(1)).GetComponent<Image>());
            // key is scriptable object name
            dialogueDictionary.Add(dialogueEvents[i].name, dialogueEvents[i]);
        }
    }

    void Update()
    {
        if (spriteAnimator != null) {
            spriteAnimator.HandleUpdate();
        }
    }

    public IEnumerator TriggerDialogue(string dialogueKey)
    { 
        // make it so you can't play multiple dialogue events at the same time or override the old one
        DialogueEvent currentEvent = dialogueDictionary[dialogueKey];
        dialogueBox.SetActive(true);
        spriteAnimator = currentEvent.Animator;
        spriteAnimator.Start();
        for (int i = 0; i < currentEvent.Dialogue.Count; i++)
        {
            yield return StartCoroutine(TypeDialogue(currentEvent.Dialogue[i]));
        }
        dialogueBox.SetActive(false);
        spriteAnimator.Stop();
        spriteAnimator = null;
    }

    public IEnumerator TypeDialogue(string line) // types out letters 1 by 1
    {
        dialogueText.text = "";
        foreach (var letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        yield return new WaitForSeconds(1);
    }
}
