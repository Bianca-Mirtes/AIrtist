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

    Queue<AudioClip> audioClipQueue = new Queue<AudioClip>();
    Queue<string> textsQueue = new Queue<string>();
    [SerializeField] private AudioSource audioSource;

    private bool isExplanationStage = false;
    private int count = 0;

    private void Start()
    {
        image.sprite = vanGogh;
        audioSource.Stop();
    }

    public void SetExplanationStage(Question[] awnsers, Sprite workImg)
    {   
        currentWork = workImg;  
        foreach (Question question in awnsers)
        {
            for (int ii = 0; ii < question.awnsers.Length; ii++)
            {
                audioClipQueue.Enqueue(question.awnsers[ii]);
                textsQueue.Enqueue(question.description[ii]);
            }
        }
        isExplanationStage = true;
    }

    private void Update()
    {
        if (isExplanationStage)
        {
            if (!audioSource.isPlaying && (audioClipQueue.Count == 0 && textsQueue.Count == 0))
            {
                FindAnyObjectByType<ChooseArtController>().StartPractise();
                isExplanationStage=false;
                count = 0;
                image.sprite = vanGogh;
            }

            if (!audioSource.isPlaying && (audioClipQueue.Count != 0 && textsQueue.Count != 0))
            {
                if(count == 3)
                {
                    image.sprite = currentWork;
                }
                audioSource.clip = audioClipQueue.Dequeue();
                text.text = textsQueue.Dequeue();
                audioSource.Play();
                count++;
            }
        }
    }
}
