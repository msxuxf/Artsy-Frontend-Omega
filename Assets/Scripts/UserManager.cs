using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System.Linq;
using System.IO;

public class UserManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
      loadSpirometerHistory();
    }


    /*
    ############################################################
    ######################## HISTORY ###########################
    ############################################################
    */

    public void loadSpirometerHistory () {
      int totalBreaths = PlayerPrefs.GetInt("TOTAL_BREATHS");
      int volume = PlayerPrefs.GetInt("MAX_VOL");
      int flow = PlayerPrefs.GetInt("MAX_FLOW");
      GameObject total = GameObject.Find("total");
      total.GetComponent<Text>().text = totalBreaths.ToString();
      GameObject vol = GameObject.Find("volume");
      vol.GetComponent<Text>().text = volume.ToString();
      GameObject air = GameObject.Find("flow");
      air.GetComponent<Text>().text = flow.ToString();


    }
    // IEnumerator getData() {
    //   string url = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/export-user";
    //
    //   UnityWebRequest request = UnityWebRequest.Get(url);
    //
    //   yield return request.SendWebRequest();
    //
    //   //can't actually use - get all user's spirometer history
    //
    // }

    /*
    ############################################################
    ########################## RESET ###########################
    ############################################################
    */

    IEnumerator createUser ()
    {
      // not doing what i want it to
      // var coins = PlayerPrefs.GetInt("COIN_BALANCE");
      string url = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/reset-user";
      //string deviceId = SystemInfo.deviceUniqueIdentifier.Replace("-","");
      //Debug.Log(deviceId);
      List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

      UnityWebRequest request = UnityWebRequest.Post(url, formData);
      request.SetRequestHeader("deviceid", SystemInfo.deviceUniqueIdentifier.Replace("-",""));

      yield return request.SendWebRequest();

      var userId = request.downloadHandler.text;
      PlayerPrefs.SetString("USER_ID", userId);
      // testing
      string userIdTest = PlayerPrefs.GetString("USER_ID");
      Debug.Log(userIdTest);

      if (request.isNetworkError || request.isHttpError)
      {
          Debug.Log(request.error);
      }

      //reset player prefs
      resetUserInfo();
      closeVerifyPopUp();

    }

    public void resetUser()
    {
      StartCoroutine(createUser());
    }

    public void resetUserInfo() {
      PlayerPrefs.SetInt("COIN_BALANCE", 375);
      PlayerPrefs.SetInt("BREATH_COUNT", 0);
      PlayerPrefs.SetInt("UNLIMITED_STATUS", 0);
      //userId reset in /reset-user call
      PlayerPrefs.SetInt("BASELINE", 0);
      PlayerPrefs.SetString("PALETTES", "0");
      PlayerPrefs.SetString("BRUSHES", "Basic");
      PlayerPrefs.SetString("TEMPLATES", "");
      PlayerPrefs.SetString("OWNED_TEMPLATES", "");
      PlayerPrefs.SetInt("TOTAL_BREATHS", 0);
      PlayerPrefs.SetInt("MAX_VOL", 0);
      PlayerPrefs.SetInt("MAX_FLOW", 0);
      loadSpirometerHistory();
    }

    public void closeVerifyPopUp()
    {
      GameObject popUp = GameObject.Find("VerifyPopUp");
      popUp.SetActive(false);
      GameObject blur = GameObject.Find("BlurPanel");
      blur.SetActive(false);
    }
    /*
    ############################################################
    ####################### NAVIGATION #########################
    ############################################################
    */

    public void GoToHome ()
    {
        SceneManager.LoadScene(0);
    }

    public void GoToTutorial ()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void GoToSpirometerTutorial ()
    {
        SceneManager.LoadScene("SpirometerHelp");
    }
}
