using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ExplanationController : MonoBehaviour
{
    public Sprite vanGogh;
    public Image image;
    public Sprite currentWork;
    public TextMeshProUGUI text;

    Queue<Question> questionsQueue = new Queue<Question>();

    private void Start()
    {
        image.sprite = vanGogh;
    }

    public void SetExplanationStage(Question[] awnsers, Sprite workImg)
    {   
        currentWork = workImg;
        foreach (Question question in awnsers) {
            questionsQueue.Enqueue(question);
        }
        NextQuestion();
    }

    public void NextQuestion()
    {
        if (questionsQueue.Count == 0)
        {
            RecordingController.Instance.canRecording = true;
            return;
        }
        if (questionsQueue.Count == 1)
        {
            DialogueManager.Instance.StartDialogue(questionsQueue.Dequeue(), 1, currentWork);
        }
        else
        {
            if(questionsQueue.Count == 3)
                DialogueManager.Instance.StartDialogue(questionsQueue.Dequeue(), 0, vanGogh);
            else
                DialogueManager.Instance.StartDialogue(questionsQueue.Dequeue(), 0, currentWork);
        }
    }
}
