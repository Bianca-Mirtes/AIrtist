using Newtonsoft.Json;
using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RecordingController : MonoBehaviour
{
    private string micDevice;
    private AudioClip recordedClip;
    private AudioClip trimmedClip = null;
    public AudioClip audioTest;
    private bool isRecording = false;
    private bool lastPressed = false;
    private List<InputDevice> devices = new List<InputDevice>();

    [SerializeField] private GameObject spinner;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Button sendAudioBtn = null;
    [SerializeField] private Button newAudioBtn = null;
    [SerializeField] private Button returnBtn = null;
    private string baseUrl;
    private OpenAIApi openai = new OpenAIApi("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");
    private static RecordingController _instance;
    private bool wasSend = false;
    private bool wasVisualize = false;
    private bool isWaiting = false;

    public bool canRecording = false;

    // Singleton
    public static RecordingController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<RecordingController>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("RecordingController");
                    _instance = obj.AddComponent<RecordingController>();
                }
            }
            return _instance;
        }
    }

    void Start()
    {
        baseUrl = "https://07bab556234b.ngrok-free.app";
        // Se ainda não tem a permissão, pede
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            Debug.Log("Pedindo permissão de microfone...");
            Application.RequestUserAuthorization(UserAuthorization.Microphone);
        }

        if (Microphone.devices.Length > 0)
            micDevice = Microphone.devices[0];
        else
            Debug.LogError("Nenhum microfone encontrado!");

        returnBtn.onClick.AddListener(ReturnStep);
        sendAudioBtn.onClick.AddListener(SendAudio);
        newAudioBtn.onClick.AddListener(NewAudio);
    }

    private void Update()
    {
#if UNITY_EDITOR
        /*if (canRecording)
        {
            if (Input.GetKeyDown(KeyCode.L) && !isRecording)
            {
                StartRecording();
            }
            if (Input.GetKeyUp(KeyCode.L) && isRecording)
            {
                Stop();
            }
        }*/
        if (canRecording)
        {
            InputDeviceCharacteristics leftHandCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(leftHandCharacteristics, devices);
            devices[0].TryGetFeatureValue(CommonUsages.secondaryButton, out bool isPressed);

            if (isPressed && !lastPressed && !isRecording)
            {
                StartRecording();
            }
            if (!isPressed && lastPressed && isRecording)
            {
                Stop();
            }

            lastPressed = isPressed;
        }
#else
        if(canRecording){
            InputDeviceCharacteristics leftHandCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(leftHandCharacteristics, devices);
            devices[0].TryGetFeatureValue(CommonUsages.secondaryButton, out bool isPressed);

            if(isPressed && !lastPressed && !isRecording)
            {
                StartRecording();
            }
            if(!isPressed && lastPressed && isRecording)
            {
                Stop();
            }

            lastPressed = isPressed;
        }
#endif
    }
    void StartRecording()
    {
        isRecording = true;
        recordedClip = Microphone.Start(micDevice, false, 40, 44100);
        description.text = "Recording...";
        spinner.SetActive(isRecording);
        Debug.Log("Gravação iniciada...");
    }

    void Stop()
    {
        isRecording = false;
        description.text = "Recording ended!";
        spinner.SetActive(isRecording);

        // Pega quantos samples realmente foram gravados
        int position = Microphone.GetPosition(micDevice);
        Microphone.End(micDevice);

        float[] samples = new float[recordedClip.samples * recordedClip.channels];
        recordedClip.GetData(samples, 0);

        // Cria um novo clip só com a parte usada
        float[] trimmedSamples = new float[position * recordedClip.channels];
        Array.Copy(samples, trimmedSamples, trimmedSamples.Length);

        trimmedClip = AudioClip.Create("TrimmedClip", position, recordedClip.channels, 44100, false);
        trimmedClip.SetData(trimmedSamples, 0);
    }

    private void ReturnStep()
    {
        wasVisualize = false;
        canRecording = false;
        ResetSend();
        description.text = "Press Y to start recording...";
        transform.GetChild(2).gameObject.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(true);
    }

    private async void SendAudio()
    {
        if (trimmedClip == null)
            return;

        if (!wasSend) 
        {
            // converte para bytes WAV
            byte[] wavData = ConvertToWav(trimmedClip);

            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() { Data = wavData, Name = "audio.wav" },
                // File = Application.persistentDataPath + "/" + fileName,
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);

            // converte para Base64
            // string base64Audio = Convert.ToBase64String(wavData);

            Request payload = new Request { transcription = res.Text};

            // 4) Serializa para JSON
            string json = JsonUtility.ToJson(payload);

            string url = $"{baseUrl}/paint";
            // envia para API
            StartCoroutine(SendToAPI(json, url));

            wasSend = true;
        }
    }

    private void ResetSend()
    {
        wasSend = false;
        trimmedClip = null;
    }

    byte[] ConvertToWav(AudioClip clip)
    {
        MemoryStream stream = new MemoryStream();
        int samples = clip.samples * clip.channels;
        float[] data = new float[samples];
        clip.GetData(data, 0);

        short[] intData = new short[samples];
        byte[] bytesData = new byte[samples * 2];
        int rescaleFactor = 32767;
        for (int i = 0; i < data.Length; i++)
        {
            intData[i] = (short)(data[i] * rescaleFactor);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        int hz = clip.frequency;
        int channels = clip.channels;
        int byteRate = hz * channels * 2;

        stream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(bytesData.Length + 36), 0, 4);
        stream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);
        stream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4);
        stream.Write(BitConverter.GetBytes((short)1), 0, 2);
        stream.Write(BitConverter.GetBytes((short)channels), 0, 2);
        stream.Write(BitConverter.GetBytes(hz), 0, 4);
        stream.Write(BitConverter.GetBytes(byteRate), 0, 4);
        stream.Write(BitConverter.GetBytes((short)(channels * 2)), 0, 2);
        stream.Write(BitConverter.GetBytes((short)16), 0, 2);
        stream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(bytesData.Length), 0, 4);
        stream.Write(bytesData, 0, bytesData.Length);

        return stream.ToArray();
    }

    IEnumerator SendToAPI(string json, string apiUrl)
    {
        isWaiting = true;
        description.text = "Waiting API response...";
        spinner.SetActive(true);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 300;

            Debug.Log(">>> Sending request");
            yield return request.SendWebRequest();
            Debug.Log("<<< Request finished");

            Debug.Log($"result = {request.result}");
            Debug.Log($"code = {request.responseCode}");
            Debug.Log($"error = {request.error}");

            if (request.downloadHandler != null)
                Debug.Log($"body = {request.downloadHandler.text}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                Response response = JsonUtility.FromJson<Response>(request.downloadHandler.text);
                byte[] zipBytes = Convert.FromBase64String(response.zip);
                FindFirstObjectByType<FrameZipLoader>().LoadFromZipBytes(zipBytes, response.txtInfos);
                description.text = "New work generated! See in \"Choose a work\"";
                spinner.SetActive(false);
                isWaiting = false;
            }
            else
                Debug.LogError("Erro API: " + request.error);
        }
    }

    [Serializable]
    public class Request
    {
        public string transcription;
    }


    [Serializable]
    public class Response
    {
        public string zip;
        public string txtInfos;
    }

    private void NewAudio()
    {
        if (!isWaiting)
        {
            description.text = "Press Y to start recording...";
            wasSend = false;
            wasVisualize = false;
            canRecording = true;
        }
    }
}
