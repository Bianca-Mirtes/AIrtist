using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private Image character;
    [SerializeField] private TextMeshProUGUI dialogueLine;
    [SerializeField] private AudioSource audioSource;

    private Queue<(string, AudioClip)> lines = new Queue<(string, AudioClip)>();
    public bool dialogueIsActive = false;
    private float typingSpeed = 0.001f;
    public static DialogueManager _instance;
    public int currentTrigger;
    public Sprite currentIcon;
    public static DialogueManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DialogueManager>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject("DialogueManager");
                    _instance = singleton.AddComponent<DialogueManager>();
                    DontDestroyOnLoad(singleton);
                }
            }

            return _instance;
        }
    }

    public void StartDialogue(Question dialogue, int trigger, Sprite icon)
    {
        dialogueIsActive = true;
        lines.Clear();

        currentTrigger = trigger;
        currentIcon = icon;

        int count = 0;
        foreach (var line in dialogue.description) { 
            lines.Enqueue((line, dialogue.awnsers[count]));
            count++;
        }
        DisplayNextLine();
    }

    public void DisplayNextLine()
    {  
        audioSource.Stop();
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }
        (string, AudioClip) currentLine = lines.Dequeue();

        character.sprite = currentIcon;

        StopAllCoroutines();

        StartCoroutine(StartTextLine(currentLine));
    }

    private void CurrentLineAudio((string, AudioClip) currentLine)
    {
        if (currentLine.Item2 != null)
        {
            audioSource.clip = currentLine.Item2;
            audioSource.Play();
        }
    }

    IEnumerator StartTextLine((string, AudioClip) currentLine)
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(TypeSentence(currentLine));
    }

    IEnumerator TypeSentence((string, AudioClip) currentLine)
    {
        dialogueLine.text = "";

        CurrentLineAudio(currentLine);

        foreach (char letter in currentLine.Item1.ToCharArray())
        {
            dialogueLine.text += letter;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(currentLine.Item2.length/3);

        DisplayNextLine();
    }


    private void EndDialogue()
    {
        Debug.Log("End Dialog");
        if (currentTrigger == 1)
        {
            FindAnyObjectByType<ChooseArtController>().StartPractise();
        }
        else {
            FindAnyObjectByType<ExplanationController>().NextQuestion();
        }
    }
}
