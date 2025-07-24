using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cognitive3D;

public class SceneChanger : MonoBehaviour
{
    public void ChangeScene(string sceneName){
        new Cognitive3D.CustomEvent("Change scene")
            .SetProperty("target scene", sceneName)
            .Send();

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
