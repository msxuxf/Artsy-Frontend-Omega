using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SpirometerHelp : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Test() {
      Debug.Log("clicked button");
    }

    public void GoToChallenge()
    {
      Debug.Log("clicked");
      SceneManager.LoadScene("SpirometerChallenge");
    }

    public void OnApplicationQuit()
    {
        Debug.Log("QUITTING APPLICATION");
        PlayerPrefs.DeleteAll();
    }
}
