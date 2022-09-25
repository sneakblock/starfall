using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private string key;
    [SerializeField] private DialogueManager dm;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other) {
        Debug.Log("hi");
        if (other.tag == "Player") {
            StartCoroutine(dm.TriggerDialogue("LowHP"));
        }
    }
}