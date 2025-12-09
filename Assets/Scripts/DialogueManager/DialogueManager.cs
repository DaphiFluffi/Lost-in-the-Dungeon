using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Animator animator;
    public float typingSpeed = 0.02f;  // Time delay between each character
    public Loader.Scene loadablescene;

    private Queue<DialogueLine> lines;
    private bool animating = true;

    // Define the event
    public UnityEvent<string> OnDialogueDisplayed;

    void Start()
    {
        lines = new Queue<DialogueLine>();
        GetComponent<DialogueTrigger>().TriggerDialogue();

        // Initialize the event
        if (OnDialogueDisplayed == null)
            OnDialogueDisplayed = new UnityEvent<string>();
    }

    private void Update()
    {
        // if you press G the dialogue will skip to the last line of dialogue
        if (Input.GetKeyDown(KeyCode.G))
        {
            while (lines.Count > 0)
            {
                DisplayNextLine();
            }
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        animator.SetBool("IsOpen", true);
        lines.Clear();

        foreach (DialogueLine line in dialogue.lines)
        {
            lines.Enqueue(line);
        }

        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = lines.Dequeue();
        nameText.text = line.speakerName;
        dialogueText.text = line.sentence;

        // Invoke the event when dialogue is displayed
        OnDialogueDisplayed.Invoke(line.sentence);

        StopAllCoroutines();
        StartCoroutine(TypeSentence(line.sentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";  // Clear the current dialogue text
        foreach (char letter in sentence.ToCharArray())  // Loop through each character in the sentence
        {
            dialogueText.text += letter;  // Add the character to the dialogue text
            yield return new WaitForSeconds(typingSpeed);  // Wait for the specified typing speed before continuing
        }
    }

    void EndDialogue()
    {
        animator.SetBool("IsOpen", false);
        Debug.Log("End of conversation.");

        if (Loader.GetActiveSceneName() == Loader.Scene.StoryScene.ToString())
        {
            AudioManager.instance.StopPlay("StoryMusic");
            AudioManager.instance.Play("GameMusic");
            LoadSceneDialogManager();
        }
    }


    public void LoadSceneDialogManager()
    {
        Loader.LoadNetwork(loadablescene);
    }

}
