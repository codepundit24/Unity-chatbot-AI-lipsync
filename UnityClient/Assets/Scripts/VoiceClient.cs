using System.Collections;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class VoiceClient : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void StartWebGLRecording();

    [DllImport("__Internal")]
    private static extern void StopWebGLRecording();
#endif

    [Header("Voice Settings")]
    public AudioSource audioSource;
    public string backendURL;
    public string micName;

    [Header("UI")]
    public TMP_Text userSubtitle;
    public TMP_Text subtitle;

    [Header("NPC Animation")]
    public AnimationManger animationManger;

    private AudioClip recordedClip;
    private bool isRecording;
    private int recordingSampleRate = 44100;

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

#if !UNITY_WEBGL || UNITY_EDITOR
        // Mic hardware detect aur capability setup
        if (Microphone.devices.Length > 0)
        {
            micName = Microphone.devices[0];
            Debug.Log($"Connected Microphone: {micName}");

            // Driver ka supported frequency range check karein
            int minFreq, maxFreq;
            Microphone.GetDeviceCaps(micName, out minFreq, out maxFreq);

            if (minFreq == 0 && maxFreq == 0)
            {
                // Driver allows any rate, 16000 is optimal for Whisper
                recordingSampleRate = 16000;
            }
            else if (maxFreq < 44100 && maxFreq > 0)
            {
                recordingSampleRate = maxFreq;
            }
            else
            {
                recordingSampleRate = 44100;
            }
        }
        else
        {
            Debug.LogError("No microphone devices found on this PC!");
        }
#endif
    }

    public void StartRecording()
    {
        if (isRecording)
            return;

#if UNITY_WEBGL && !UNITY_EDITOR
        isRecording = true;
        Debug.Log("Start Recording");
        StartWebGLRecording();
#else
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("Recording failed: No Microphone detected!");
            return;
        }

        isRecording = true;
        Debug.Log($"Start Recording with mic: {micName} at {recordingSampleRate}Hz");

        // Exact detected hardware device name pass karein
        recordedClip = Microphone.Start(
            micName,
            false,
            30,
            recordingSampleRate
        );
#endif
    }

    public void StopRecording()
    {
        if (!isRecording)
            return;

        isRecording = false;
        Debug.Log("Stop Recording");

#if UNITY_WEBGL && !UNITY_EDITOR
        StopWebGLRecording();
#else
        if (Microphone.devices.Length == 0)
            return;

        int lastPos = Microphone.GetPosition(micName);
        Microphone.End(micName);

        if (lastPos <= 0)
        {
            Debug.LogWarning("Recording position is 0. Audio was not captured.");
            return;
        }

        // Trim actual recorded audio
        float[] soundData = new float[lastPos * recordedClip.channels];
        recordedClip.GetData(soundData, 0);

        AudioClip trimmedClip = AudioClip.Create(
            "TrimmedVoice",
            lastPos,
            recordedClip.channels,
            recordedClip.frequency,
            false
        );
        trimmedClip.SetData(soundData, 0);

        byte[] wavData = WavUtility.FromAudioClip(trimmedClip);
        Destroy(trimmedClip);

        StartCoroutine(SendAudio(wavData, "voice.wav", "audio/wav"));
#endif
    }

    public void OnWebGLRecordingReady(string base64Audio)
    {
        byte[] audioData = System.Convert.FromBase64String(base64Audio);
        StartCoroutine(SendAudio(audioData, "voice.webm", "audio/webm"));
    }

    private IEnumerator SendAudio(byte[] audioData, string fileName, string contentType)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, fileName, contentType);

        using (UnityWebRequest request = UnityWebRequest.Post(backendURL, form))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Backend Request Failed: " + request.error);
                yield break;
            }

            VoiceResponse reply = JsonUtility.FromJson<VoiceResponse>(request.downloadHandler.text);
            HandleResponse(reply);
        }
    }

    private void HandleResponse(VoiceResponse reply)
    {
        if (userSubtitle != null)
        {
            userSubtitle.text = "<color=#58A6FF><b>You:</b></color> " + reply.user_text;
        }
        if (subtitle != null)
            subtitle.text = "<color=#3FB950><b>AI chatbot:</b></color> " + reply.message;

        if (animationManger != null)
            animationManger.PlayGesture(reply.gesture);

        byte[] audioBytes = System.Convert.FromBase64String(reply.audio_base64);
        AudioClip clip = WavBytesToAudioClip(audioBytes);

        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private AudioClip WavBytesToAudioClip(byte[] wav)
    {
        int channels = System.BitConverter.ToInt16(wav, 22);
        int sampleRate = System.BitConverter.ToInt32(wav, 24);
        int bitsPerSample = System.BitConverter.ToInt16(wav, 34);

        int dataIndex = 12;
        while (dataIndex < wav.Length - 8)
        {
            string chunkId = System.Text.Encoding.ASCII.GetString(wav, dataIndex, 4);
            int chunkSize = System.BitConverter.ToInt32(wav, dataIndex + 4);

            if (chunkId == "data")
            {
                dataIndex += 8;
                break;
            }
            dataIndex += 8 + chunkSize;
        }

        if (dataIndex >= wav.Length || bitsPerSample != 16)
        {
            Debug.LogError("Invalid or unsupported WAV format");
            return null;
        }

        int sampleCount = (wav.Length - dataIndex) / 2;
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            short sample = System.BitConverter.ToInt16(wav, dataIndex + i * 2);
            samples[i] = sample / 32768f;
        }

        AudioClip clip = AudioClip.Create("NPC_TTS", sampleCount / channels, channels, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void AppQuit()
    {
        Application.Quit();
    }
}

[System.Serializable]
public class VoiceResponse
{
    public string user_text;
    public string message;
    public string gesture;
    public string audio_base64;
}