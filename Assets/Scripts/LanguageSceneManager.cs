using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LanguageSceneManager : MonoBehaviour
{
    [SerializeField] private List<LanguageSet> textList;
    [SerializeField] private TMP_Dropdown languageDropdown;
    
    private void Start() {
        
        foreach (LanguageSet item in textList){
            item.textObj.text = item.english;
        }
    }

    public void ChangeLanguage(){
        switch (languageDropdown.options[languageDropdown.value].text) {
            case "English":
                foreach (LanguageSet item in textList){
                    item.textObj.text = item.english;
                }
                break;

            case "Indonesia":
                foreach (LanguageSet item in textList){
                    item.textObj.text = item.indonesian;
                }
                break;

            case "日本":
                foreach (LanguageSet item in textList){
                    item.textObj.text = item.japanese;
                }
                break;
        }
    }
}

[System.Serializable]
class LanguageSet{
    [SerializeField] public TMP_Text textObj;
    [SerializeField] public string english;
    [SerializeField] public string indonesian;
    [SerializeField] public string japanese;
}
