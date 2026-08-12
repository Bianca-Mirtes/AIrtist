using Meta.XR.BuildingBlocks.AIBlocks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using WorkData;

public class AskController : MonoBehaviour
{
    private string baseUrl;
    private string micDevice;
    private bool wasSend = false;
    private bool isWaiting = false;
    private AudioClip recordedClip;
    private AudioClip trimmedClip = null;
    private string openAIKey = "";

    public TextToSpeechAgent ttsAgent;

    private bool isRecording = false;
    private bool lastPressed = false;
    private List<InputDevice> devices = new List<InputDevice>();

    [SerializeField] private GameObject spinner;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Button sendAudioBtn = null;
    [SerializeField] private Button newAudioBtn = null;

    void Start()
    {
        baseUrl = "https://api.akcit.fun";

        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            Debug.Log("Pedindo permissão de microfone...");
            Application.RequestUserAuthorization(UserAuthorization.Microphone);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }

        if (Microphone.devices.Length > 0)
            micDevice = Microphone.devices[0];
        else
            Debug.LogError("Nenhum microfone encontrado!");

        sendAudioBtn.onClick.AddListener(SendAudio);
        newAudioBtn.onClick.AddListener(NewAudio);

        description.text = "Press B to start recording...";
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
        if (ChooseArtController.Instance.isRunningWorkWithAPI)
        {
            InputDeviceCharacteristics rightHandCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(rightHandCharacteristics, devices);

            if (devices.Count == 0)
                return;

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
        if (ChooseArtController.Instance.isRunningWorkWithAPI)
        {
            InputDeviceCharacteristics rightHandCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(rightHandCharacteristics, devices);

            if (devices.Count == 0)
                return;

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
#endif
    }

    public void StartRecording()
    {
        isRecording = true;
        recordedClip = Microphone.Start(micDevice, false, 40, 44100);
        description.text = "Recording...";
        spinner.SetActive(isRecording);
        Debug.Log("Gravação iniciada...");
    }

    public void Stop()
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

        float[] data = new float[trimmedClip.samples * trimmedClip.channels];
        trimmedClip.GetData(data, 0);
    }

    IEnumerator GetAPIKey(string openAIUrl)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(openAIUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                yield break;
            }

            openAIKey = request.downloadHandler.text.Trim();
        }

        Debug.Log("OpenAI Key obtained successfully!: " + openAIKey);

        SendAudio();
    }

    private void NewAudio()
    {
        if (!isWaiting)
        {
            description.text = "Press B to start recording...";
            wasSend = false;
        }
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

    private void SendAudio()
    {
        if (trimmedClip == null)
            return;

        if (trimmedClip.length < 1)
        {
            description.text = "Audio too short! Please record again.";
            return;
        }

        if (!wasSend)
        {
            isWaiting = true;
            // converte para bytes WAV
            byte[] wavData = ConvertToWav(trimmedClip);

            // Salvar WAV para teste
            string path = Path.Combine(Application.persistentDataPath, "teste.wav");
            File.WriteAllBytes(path, wavData);

            Debug.Log("WAV salvo em: " + path);

            StartCoroutine(SendAudioToOpenAI(wavData));
        }
    }

    IEnumerator SendAudioToOpenAI(byte[] wavData)
    {
        string urlOpenAI = "https://api.openai.com/v1/audio/transcriptions";

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", wavData, "audio.wav", "audio/wav");
        form.AddField("model", "whisper-1");
        form.AddField("language", "en");

        using (UnityWebRequest request = UnityWebRequest.Post(urlOpenAI, form))
        {
            request.SetRequestHeader(
                "Authorization",
                "Bearer " + openAIKey
            );

            // IMPORTANTE: não setar Content-Type manualmente
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("OpenAI error: " + request.error);
                Debug.LogError(request.downloadHandler.text);
                yield break;
            }

            string json = request.downloadHandler.text;
            Debug.Log("OpenAI response: " + json);

            // parse simples
            WhisperResponse response = JsonUtility.FromJson<WhisperResponse>(json);

            Debug.Log("Texto transcrito: " + response.text);

            if (ChooseArtController.Instance.isRunningWork)
            {
                AskRequest payload = new AskRequest
                {
                    question = response.text == null || response.text == "you" ? "Who is the author?" : response.text,
                    artwork_context = RecordingController.Instance.currentArtworkContext
                };

                Debug.Log("Texto armazenado: " + payload.question);
                Debug.Log("ArtworkContext: " + payload.artwork_context);

                string url = $"{baseUrl}/ask";
                string payloadJson = JsonUtility.ToJson(payload);
                StartCoroutine(SendToAPI(payloadJson, url));
            }
        }
    }

    IEnumerator SendToAPI(string json, string apiUrl)
    {
        wasSend = true;

        description.text = "Waiting Agent response...";
        spinner.SetActive(true);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log(">>> Sending request");
            yield return request.SendWebRequest();
            Debug.Log("<<< Request finished");

            Debug.Log($"result = {request.result}");
            Debug.Log($"code = {request.responseCode}");
            Debug.Log($"error = {request.error}");
            Debug.Log($"Body: {request.downloadHandler.text}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                description.text = "Agent Awnser Generated...";
                spinner.SetActive(false);

                AskResponse askResponse = JsonUtility.FromJson<AskResponse>(request.downloadHandler.text);
                ttsAgent.SpeakText(askResponse.answer);
                isWaiting = false;
            }
            else
                Debug.LogError("Erro API: " + request.error);
        }
    }

    [Serializable]
    public class AskRequest
    {
        public string question;
        public ArtWorkContext artwork_context;
    }

    [Serializable]
    public class AskResponse
    {
        public string answer;
        public string session_id;
    }
}
