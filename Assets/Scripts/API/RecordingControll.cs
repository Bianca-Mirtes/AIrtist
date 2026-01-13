using Newtonsoft.Json;
using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
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
    private string openAIKey;
    private static RecordingController _instance;
    private bool wasSend = false;
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
        baseUrl = "https://app.akcitgaming.top";
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
        sendAudioBtn.onClick.AddListener(() => StartCoroutine(GetAPIKey($"{baseUrl}/apiKey")));
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

    private void ReturnStep()
    {
        canRecording = false;
        ResetSend();
        description.text = "Press Y to start recording...";
        transform.GetChild(2).gameObject.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(true);
    }

    IEnumerator GetAPIKey(string openAIUrl)
    {
        description.text = "Getting the OpenAI Key...";
        spinner.SetActive(true);

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

    private void SendAudio()
    {
        if (trimmedClip == null)
            return;

        if (!wasSend) 
        {
            // converte para bytes WAV
            byte[] wavData = ConvertToWav(trimmedClip);

            StartCoroutine(SendAudioToOpenAI(wavData));

            /*var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() { Data = wavData, Name = "audio.wav" },
                // File = Application.persistentDataPath + "/" + fileName,
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);

            // converte para Base64
            // string base64Audio = Convert.ToBase64String(wavData);

            PaintRequest payload = new PaintRequest {transcription = res.Text == null ? "Generate for me Monalisa of Da Vinci" : res.Text};

            // 4) Serializa para JSON
            string json = JsonUtility.ToJson(payload);

            string url = $"{baseUrl}/paint";
            // envia para API
            StartCoroutine(SendToAPI(json, url));
            wasSend = true;*/
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

            // segue seu fluxo normal
            PaintRequest payload = new PaintRequest
            {
                transcription = response.text == null ? "Generate for me Monalisa of Da Vinci" : response.text
            };

            string url = $"{baseUrl}/paint";
            string payloadJson = JsonUtility.ToJson(payload);
            StartCoroutine(
                SendToAPI(payloadJson, url)
            );
            wasSend = true;
        }
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

            Debug.Log(">>> Sending request");
            yield return request.SendWebRequest();
            Debug.Log("<<< Request finished");

            Debug.Log($"result = {request.result}");
            Debug.Log($"code = {request.responseCode}");
            Debug.Log($"error = {request.error}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                PaintingJobResponse response = JsonUtility.FromJson<PaintingJobResponse>(request.downloadHandler.text);

                string verifUrl = $"{baseUrl}/jobs/{response.job_id}";

                StartCoroutine(VerifPayload(verifUrl, response.job_id, response.txtInfos));
            }
            else
                Debug.LogError("Erro API: " + request.error);
        }
    }

    IEnumerator VerifPayload(string verifUrl, string jobID, string txtInfos)
    {
        description.text = "Checking generation finalization...";

        while (true)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(verifUrl))
            {
                request.SetRequestHeader("User-Agent", "UnityPlayer");
                request.downloadHandler = new DownloadHandlerBuffer();
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Erro API: " + request.error);
                    yield break;
                }

                JobStatus response =
                    JsonUtility.FromJson<JobStatus>(request.downloadHandler.text);

                if (response.status == "queued" || response.status == "running")
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                if (response.status == "error")
                {
                    Debug.LogError("Erro API: Job error");
                    yield break;
                }

                if (response.status == "done")
                {
                    string zipUrl = $"{baseUrl}/jobs/{jobID}/download";
                    StartCoroutine(DownloadZip(zipUrl, txtInfos, jobID));
                    yield break;
                }
            }
        }
    }

    IEnumerator DownloadZip(string zipUrl, string txtInfos, string jobID)
    {
        description.text = "Downloading new Work...";

        string zipPath = Path.Combine(
            Application.persistentDataPath,
            $"{jobID}.zip"
        );

        using (UnityWebRequest request = UnityWebRequest.Get(zipUrl))
        {
            request.SetRequestHeader("User-Agent", "UnityPlayer");
            request.downloadHandler = new DownloadHandlerFile(zipPath); 
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Download error: {request.error}");
                Debug.LogError($"HTTP Code: {request.responseCode}");
                yield break;
            }

            long size = new FileInfo(zipPath).Length;
            Debug.Log("ZIP DOWNLOADED SIZE: " + size);

            FindFirstObjectByType<FrameZipLoader>().LoadFromZipPath(zipPath, txtInfos);

            description.text = "New work generated! See in \"Choose a work\"";
            spinner.SetActive(false);
            isWaiting = false;
        }
    }

    [Serializable]
    public class PaintingJobResponse
    {
        public string job_id;
        public string txtInfos;
    }


    [Serializable]
    public class PaintRequest
    {
        public string transcription;
    }


    [Serializable]
    public class PaintingResponse
    {
        public string zip;
        public string txtInfos;
    }

    [Serializable]
    public class JobStatus
    {
        public string status;
    }


    [Serializable]
    public class WhisperResponse
    {
        public string text;
    }

    private void ResetSend()
    {
        wasSend = false;
        trimmedClip = null;
    }

    private void NewAudio()
    {
        if (!isWaiting)
        {
            description.text = "Press Y to start recording...";
            wasSend = false;
            canRecording = true;
        }
    }
}
