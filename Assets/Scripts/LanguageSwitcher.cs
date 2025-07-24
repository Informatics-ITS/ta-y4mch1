using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LanguageSwitcher : MonoBehaviour
{
    [SerializeField] private string id;
    [SerializeField] private string jp;

    [SerializeField] private TextMeshProUGUI textField;

    public void SwitchID(){
        textField.text = id;
    }

    public void SwitchJP(){
        textField.text = jp;
    }

}