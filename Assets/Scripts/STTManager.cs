using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using OpenAI;
using Newtonsoft.Json;
using System.IO;

public class STTManager : OpenAIParent{
    public static STTManager Instance { get; private set; }
    private void Awake(){
        if (Instance != null && Instance != this) { 
            Destroy(this); 
        } 
        else { 
            Instance = this; 
        } 
    }
    public async Task<string> SendReplySTT(string audioOutPath){
        var sttResponse = await openai.CreateSTT(new CreateSTTRequest()
        {
            Model = "whisper-1",
            Language = (language == null || language == "" || language ==" ") ? "id" : language,
            FilePath = audioOutPath
        });

        Debug.Log("GOT REPLY: " + sttResponse);

        return sttResponse;
    }
}
