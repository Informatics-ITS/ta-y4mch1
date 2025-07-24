using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using OpenAI;

public class OpenAIParent : MonoBehaviour{
    protected OpenAIApi openai = new OpenAIApi("");
    protected string language = "id";

    public void SetLanguage(string language){
        this.language = language;
    }

    public string GetLanguage(){
        return this.language;
    }

    public string GetFullLanguageName() {
        switch(language){
            case "ja":
                return "japanese";
                break;
            case "id":
                return "indonesian";
                break;

            default:
                return "indonesian";
                break;
        }
    }
}
