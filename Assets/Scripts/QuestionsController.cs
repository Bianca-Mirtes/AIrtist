using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionsController : MonoBehaviour
{
    [SerializeField] private Button[] questions;

    [SerializeField] private AudioSource audioSource;

    private Queue<AudioClip> audioClipQueue = new Queue<AudioClip>();

    private bool isPlaying = false;
    private int count = 0;
    private void Start()
    {
        audioSource.Stop();
    }

    public void SetQuestions(string authorName, Question[] awnsers)
    {
        foreach (var question in questions)
        {
            if (count == 0)
                question.gameObject.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = "Who was " + authorName + "?";
              
            question.onClick.AddListener(() => PlayAwnser(awnsers[count]));
            count++;
        }
        count = 0;
    }

    private void Update()
    {
       if (!audioSource.isPlaying && audioClipQueue.Count == 0)
            isPlaying = false;

        if(!audioSource.isPlaying && audioClipQueue.Count != 0)
        {
            audioSource.clip = audioClipQueue.Dequeue();
            audioSource.Play();
            isPlaying = true;
        }
    }

    private void PlayAwnser(Question question)
    {
        if (question.awnsers.Length == 0)
            return;

        if(question.awnsers.Length > 1)
        {
            foreach(var awnser in question.awnsers)
            {
                audioClipQueue.Enqueue(awnser);
            }
        }
        else
        {
            audioClipQueue.Enqueue(question.awnsers[0]);
        }

        if (!isPlaying)
        {
            audioSource.clip = audioClipQueue.Dequeue();
            audioSource.Play();
            isPlaying = true;
        }
    }
}
