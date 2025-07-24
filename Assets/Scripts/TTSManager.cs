using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using OpenAI;
using Newtonsoft.Json;
using System.IO;

public class TTSManager : OpenAIParent{
    public static TTSManager Instance { get; private set; }
    private string directoryPath;
    private string audioInPath;
    private void Awake(){
        if (Instance != null && Instance != this) { 
            Destroy(this); 
        } 
        else { 
            Instance = this; 
        } 
        
        directoryPath = Path.Combine(Application.persistentDataPath, "Resources");
        audioInPath = Path.Combine(directoryPath, "audioIn.wav");
    }

    public async Task<bool> SendReplyTTS(string input){

        bool ttsResponse = await openai.CreateTTS(new CreateTTSRequest()
        {
            Model = "tts-1",
            Input = input,
            Voice = "alloy"
        }, directoryPath, audioInPath);

        Debug.Log("IS FINISHED: " + ttsResponse);

        return ttsResponse;
    }
}