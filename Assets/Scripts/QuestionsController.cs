using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionsController : MonoBehaviour
{
    [SerializeField] private Button[] questions;

    [SerializeField] private AudioSource audioSource;

    private Queue<AudioClip> audioClipQueue = new Queue<AudioClip>();

    private Question[] currentQuestions;

    private void Start()
    {
        audioSource.Stop();
    }

    public void SetQuestions(string authorName, Question[] awnsers)
    {
        currentQuestions = awnsers;
        for(int ii=0; ii < questions.Length; ii++)
        {
            if (ii == 0)
                questions[ii].gameObject.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = "Who was " + authorName + "?";
        }
    }

    private void Update()
    {
        if(!audioSource.isPlaying && audioClipQueue.Count != 0)
        {
            audioSource.clip = audioClipQueue.Dequeue();
            audioSource.Play();
        }
    }

    public void PlayAwnser(int number)
    {
        if (audioClipQueue.Count != 0)
            return;

        if (currentQuestions[number].awnsers.Length == 0)
        {
            Debug.Log("sem audios");
            return;
        }

        if (currentQuestions[number].awnsers.Length > 1)
        {
            foreach (var awnser in currentQuestions[number].awnsers)
            {
                audioClipQueue.Enqueue(awnser);
            }
        }
        else
        {
            audioClipQueue.Enqueue(currentQuestions[number].awnsers[0]);
        }
    }
}
