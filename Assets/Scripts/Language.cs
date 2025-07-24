using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Language : MonoBehaviour {
    [SerializeField] public LanguageEnum languageEnum {get; set;}

    public void Start(){
        languageEnum = LanguageEnum.en;
    }
    public string GetEnumString(){
        return languageEnum.ToString();
    }

    public string GetEnumFullString(){
        switch(languageEnum){
            case LanguageEnum.en:
                return "English";
                break;
            case LanguageEnum.id:
                return "Indonesia";
                break;
            case LanguageEnum.jp:
                return "日本";
                break;
        }
        return "";
    }

    public void SetEnum(LanguageEnum languageEnum){
        languageEnum = languageEnum;
    }

}
public enum LanguageEnum {en, id, jp};
