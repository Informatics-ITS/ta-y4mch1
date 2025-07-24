using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreferenceHandler : MonoBehaviour
{
    public void SetLanguage(string language){
        PlayerPrefs.SetString("language", language);
    }
}
