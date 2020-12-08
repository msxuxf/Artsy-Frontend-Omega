using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SpirometerAlert : MonoBehaviour
{
    public GameObject Alert;
    // Start is called before the first frame update
    void Start()
    {
       Alert.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GlobalTimer.TimeLeft == TimeSpan.Zero)
        {
            PlayerPrefs.SetInt("UNLIMITED_STATUS", 0);
            Alert.SetActive(true);
        }
    }

    public void GoToSpirometer()
    {
        
        SceneManager.LoadScene("SpirometerChallenge");
    }

    public void CloseChallenge()
    {
        GlobalTimer.SetEndTime(DateTime.MaxValue);
        Alert.SetActive(false);
    }

    public void OnApplicationQuit()
    {
        Debug.Log("QUITTING APPLICATION");
        PlayerPrefs.DeleteAll();
    }
}