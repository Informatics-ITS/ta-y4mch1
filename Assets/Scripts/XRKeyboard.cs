using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cognitive3D;

public class XRKeyboard : MonoBehaviour 
{
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private string nextSceneName;
    
    public void AppendVal(int val) 
    {
        textField.text += val.ToString();
    }
    
    public void BackspaceEvent() 
    {
        if (!string.IsNullOrEmpty(textField.text)) 
        {
            textField.text = textField.text.Substring(0, textField.text.Length - 1);
        }
    }
    
    public void SubmitNumber() 
    {
        // Simpan ke PlayerPrefs seperti sebelumnya
        PlayerPrefs.SetString("PlayerID", textField.text);
        
        // Set Player ID ke Cognitive3D
        Cognitive3D_Manager.SetParticipantId(textField.text);
        
        // Kirim custom event ke Cognitive3D untuk mencatat Player ID
        new CustomEvent("PlayerID")
            .SetProperty("PlayerID", textField.text)
            .SetProperty("SceneIndex", SceneManager.GetActiveScene().buildIndex)
            .Send();
        
        Cognitive3D_Manager.SetParticipantProperty("PlayerID", textField.text);
   
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}