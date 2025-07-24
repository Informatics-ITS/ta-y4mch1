using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartReset : MonoBehaviour
{
    public void ResetHeart(){
        PlayerPrefs.SetInt("Score", 5);
    }
}
