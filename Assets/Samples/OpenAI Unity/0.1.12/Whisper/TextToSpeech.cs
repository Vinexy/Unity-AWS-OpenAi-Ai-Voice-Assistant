using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon.Polly;
using Amazon.Runtime;
using Amazon;
using Amazon.Polly.Model;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TextToSpeech : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    private AmazonPollyClient client;

    // Start is called before the first frame update
    private void Start()
    {
        var credentials = new BasicAWSCredentials("********", "*********");
        client = new AmazonPollyClient(credentials, RegionEndpoint.EUCentral1);
    }

    public void WriteIntoFile(Stream stream)
    {
        using (var fileStream = new FileStream($"{Application.persistentDataPath}/aud.mp3", FileMode.Create))
        {
            byte[] buffer = new byte[8 * 1024];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }
        }
    }

    public async void myTextChanged(string texty)
    {
        var request = new SynthesizeSpeechRequest()
        {
            LanguageCode = LanguageCode.TrTR,
            Text = texty,
            Engine = Engine.Standard,
            VoiceId = VoiceId.Filiz,
            OutputFormat = OutputFormat.Mp3,
            SampleRate = "24000"
        };

        var response = await client.SynthesizeSpeechAsync(request);

        WriteIntoFile(response.AudioStream);

        string audioPath;

#if UNITY_ANDROID && !UNITY_EDITOR
            audioPath = $"jar:file://{Application.persistentDataPath}/aud.mp3";
#else
        audioPath = $"{Application.persistentDataPath}/aud.mp3";
#endif

        using (var www = UnityWebRequestMultimedia.GetAudioClip(audioPath, AudioType.MPEG))
        {
            var op = www.SendWebRequest();

            while (!op.isDone) await Task.Yield();

            var clip = DownloadHandlerAudioClip.GetContent(www);

            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
