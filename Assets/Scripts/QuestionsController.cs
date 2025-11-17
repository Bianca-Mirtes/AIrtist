using UnityEngine;
using UnityEngine.UI;

public class QuestionsController : MonoBehaviour
{
    [SerializeField] private Button[] questions;
    [SerializeField] private AudioClip[] awnsers;

    [SerializeField] private AudioSource audioSource;

    private bool isPlaying = false;
    private int count = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var question in questions)
        {
            question.onClick.AddListener(() => PlayAwnser(awnsers[count]));
            count++;
        }
        count = 0;
    }

    private void Update()
    {
        if (!audioSource.isPlaying)
            isPlaying = false;
    }

    private void PlayAwnser(AudioClip clip)
    {
        if (!isPlaying)
        {
            audioSource.clip = clip;
            audioSource.Play();
            isPlaying = true;
        }
    }
}
