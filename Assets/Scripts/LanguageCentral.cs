using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageCentral : MonoBehaviour
{
    public static LanguageCentral Instance { get; private set;}
    [SerializeField] private OpenAIParent openAIParent;
    public string language;
    private void Awake() {
        if (Instance != null && Instance != this) { 
            Destroy(this); 
        } 
        else { 
            Instance = this; 
        }

        language = PlayerPrefs.GetString("language", "id");
        Debug.Log("language central: "+ language);
        
        LanguageSwitcher[] uiObjects = FindObjectsOfType<LanguageSwitcher>();

        List<LanguageSwitcher> targetClassList = new List<LanguageSwitcher>(uiObjects);

        switch (language) {
            case "id":
                foreach (LanguageSwitcher item in targetClassList){
                    item.SwitchID();
                }
                break;
            case "ja":
                foreach (LanguageSwitcher item in targetClassList){
                    item.SwitchJP();
                }
                break;
        }
    }

    public string GetFullLanguageName() {
        switch(this.language){
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
