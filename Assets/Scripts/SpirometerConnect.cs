using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using CodeMonkey.Utils;


public class SpirometerConnect : MonoBehaviour
{
    public int[] _volList;
    public int[] _flowList;

    // for API call need arrays to be in string format
    public string flowString = "";
    public string volumeString = "";

    public List<int> volumeList = new List<int>();
    public List<int> flowList = new List<int>();

    // game objects to be updated
    public Text volumeValue;
    public Text flowValue;
    public Text balanceHere;
    public Text NewBalance;
    public Text feedback;
    public GameObject goodPanel;
    public GameObject badPanel;

    // graphs
    [SerializeField] private Sprite circleSprite;
    public RectTransform graphContainer;
    public RectTransform graphContainer2;

    void Start ()
    {
      SetCoinBalance();
      SetBar();
    }

    /*
    ############################################################
    ######################## GRAPHS ###########################
    ############################################################
    */
    /*
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

    Used functions from CodeMonkey Window_Graph.cs

               unitycodemonkey.com
    --------------------------------------------------
    */
    private void DisplayGraph() {
      // destroy previous graph
      foreach(Transform child in graphContainer) {
        Destroy(child.gameObject);
      }

      foreach(Transform child in graphContainer2) {
        Destroy(child.gameObject);
      }

      ShowGraph(flowList);
      ShowGraph2(volumeList);

      flowList.Clear();
      volumeList.Clear();
    }

    // FOR FLOW
    private GameObject CreateCircle(Vector2 anchoredPosition) {
        GameObject gameObject = new GameObject("dot", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObject;
    }

    private void ShowGraph(List<int> valueList) {
        float graphHeight = graphContainer.sizeDelta.y;
        float yMaximum = 4f;
        float xSize = 30f;

        GameObject lastCircleGameObject = null;
        for (int i = 0; i < valueList.Count; i++) {
            float xPosition = xSize + i * xSize;
            float yPosition = (valueList[i] / yMaximum) * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));
            if (lastCircleGameObject != null) {
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObject = circleGameObject;
        }
    }

    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB) {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = new Color(0,0,0, .5f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
    }

    // FOR VOLUME
    private GameObject CreateCircle2(Vector2 anchoredPosition) {
        GameObject gameObject = new GameObject("dot", typeof(Image));
        gameObject.transform.SetParent(graphContainer2, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObject;
    }

    private void ShowGraph2(List<int> valueList) {
        float graphHeight = graphContainer2.sizeDelta.y;
        float yMaximum = 11f;
        float xSize = 30f;

        GameObject lastCircleGameObject = null;
        for (int i = 0; i < valueList.Count; i++) {
            float xPosition = xSize + i * xSize;
            float yPosition = (valueList[i] / yMaximum) * graphHeight;
            GameObject circleGameObject = CreateCircle2(new Vector2(xPosition, yPosition));
            if (lastCircleGameObject != null) {
                CreateDotConnection2(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObject = circleGameObject;
        }
    }

    private void CreateDotConnection2(Vector2 dotPositionA, Vector2 dotPositionB) {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer2, false);
        gameObject.GetComponent<Image>().color = new Color(0,0,0, .5f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
    }

    /*
    ############################################################
    ######################## BREATH BAR ########################
    ############################################################
    */

    public void SetBar()
    {
      var breaths = PlayerPrefs.GetInt("BREATH_COUNT");
      for (int i = 1; i <= breaths; ++i) {
        //string bar = "Bar"+i;
        GameObject showBar = GameObject.Find("Display/BreathBar/Bar"+i);
        showBar.SetActive(true);
      }

    }

    public void UpdateBar()
    {
      var breaths = PlayerPrefs.GetInt("BREATH_COUNT");
      GameObject showBar = GameObject.Find("Display/BreathBar/Bar"+breaths);
      showBar.SetActive(true);
    }

    /*
    ############################################################
    ######################## CHALLENGE #########################
    ############################################################
    */

    IEnumerator GetText(string flowString, string volString) {
      int oldBalance = PlayerPrefs.GetInt("COIN_BALANCE");
      string flow = flowString.Remove(flowString.Length - 1, 1);
      string volume = volString.Remove(volString.Length - 1, 1);

      string url = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/add-breath";

      // TODO: Waiting on backend to change Headers to Form Data
      List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
      UnityWebRequest request = UnityWebRequest.Post(url, formData);

      request.SetRequestHeader("deviceid", SystemInfo.deviceUniqueIdentifier.Replace("-",""));
      request.SetRequestHeader("volume", volume);
      request.SetRequestHeader("flow", flow);

      yield return request.SendWebRequest();

      string data = request.downloadHandler.text;
      Spirometer spiro = Spirometer.CreateFromJSON(data);
      PlayerPrefs.SetString("BREATH_GRADE", spiro.grade);
      PlayerPrefs.SetString("BREATH_FEEDBACK", spiro.feedback);
      PlayerPrefs.SetInt("BREATH_SCORE", spiro.score);
      PlayerPrefs.SetInt("COIN_BALANCE", spiro.balance);
      PlayerPrefs.SetInt("BREATH_COUNT", spiro.breathCount);

      if (spiro.breathCount >= 10)
      {
          PlayerPrefs.SetInt("UNLIMITED_STATUS", 1);
          // Set up the time of one hour
          DateTime dt = DateTime.UtcNow + TimeSpan.FromSeconds(3600);
          GlobalTimer.SetEndTime(dt);
          // Debug.Log("Timer is set up to:" + dt.ToString("yyyy-MM-dd-HH-mm-ss"));
      }

      SetCoinBalance();
      UpdateBar();

      int score = PlayerPrefs.GetInt("BREATH_SCORE");
      BreathQuality(score, oldBalance);

    }

    public void takeBreath() 
    {
      var seconds = UnityEngine.Random.Range(2, 11);
      Debug.Log("seconds: " + seconds);
      StartVariableLengthTest(seconds);
    }

    public void StartVariableLengthTest(int seconds)
    {
        // This test will give a random breath. We recommend altering it to consistently provide
        // deterministic poor/good/better/best breaths by playing with the probability.
        var rand = new System.Random();
        _volList = new int[seconds * 2 + 3];
        _flowList = new int[seconds * 2 + 3];
        _volList[0] = 0;
        _flowList[0] = 0;
        var currentVol = 1f;
        var lastVal = 0;
        for (var i = 1; i <= seconds * 2; ++i)
        {
            // Rand int determines flow. Its smart in its generation to ensure flow
            // does not jump around erratically
            var rInt = 1;
            if (lastVal == 1)
            {
                rInt = rand.Next(1, 3);
            }
            else if (lastVal == 2)
            {
                rInt = rand.Next(1, 4);
            }
            else if (lastVal == 3)
            {
                rInt = rand.Next(2, 4);
            }
            lastVal = rInt;
            _flowList[i] = rInt;

            // Volume read increases based on flow reading
            if (rInt == 1)
            {
                currentVol += 0.4f;
            }
            else if (rInt == 2)
            {
                currentVol += 0.6f;
            }
            else
            {
                currentVol += 1f;
            }
            // Volume floored to an int as spirometer can only return ints
            _volList[i] = Math.Floor(currentVol) > 10 ? 10 : (int) Math.Floor(currentVol);
        }

        // Two zeros here in flow indicate end of breath. (1 second of no flow)
        _flowList[seconds * 2 + 1] = 0;
        _flowList[seconds * 2 + 2] = 0;
        _volList[seconds * 2 + 1] = _volList[seconds * 2];
        _volList[seconds * 2 + 2] = _volList[seconds * 2];

        for (var i = 0; i < (seconds * 2 + 3); ++i) 
        {
          flowString = flowString + _flowList[i].ToString() + " ";
          flowList.Add(_flowList[i]);
        }

        for (var i = 0; i < (seconds * 2 + 3); ++i) 
        {
          volumeString = volumeString + _volList[i].ToString() + " ";
          volumeList.Add(_volList[i]);
        }

        // for updating flow and volume values that are displayed
        StartCoroutine(RunChallenge2(seconds));

        DisplayGraph();
    }

    private IEnumerator RunChallenge2(int seconds)
    {
        // Sends an element of each array every second for a variable number of seconds
        for (var i = 0; i < seconds * 2 + 3; ++i)
        {
            HandleInput(i, _volList[i], _flowList[i]);
            yield return new WaitForSeconds(0.5f);
        }

        StartCoroutine(GetText(flowString, volumeString));
    }

    private void HandleInput(int dataPos, int vol, int flow)
    {
        //display volume and flow values
        flowValue.text = flow.ToString();
        volumeValue.text = vol.ToString();
    }

    private void SetCoinBalance ()
    {
      if (PlayerPrefs.HasKey("COIN_BALANCE")) 
      {
        int coins = PlayerPrefs.GetInt("COIN_BALANCE");
        GameObject balanceObject = GameObject.Find("CoinBalance");
        balanceObject.GetComponent<Text>().text = coins.ToString();
      }
    }

    private void BreathQuality(int score, int oldBalance)
    {
      int newBalance = PlayerPrefs.GetInt("COIN_BALANCE");
      // breath not good enough so nothing changed
      if (newBalance == oldBalance) 
      {
          // display redo popUp
          Debug.Log("BAD BREATH");
          badPanel.SetActive(true);
      }
      else 
      {
        if (PlayerPrefs.HasKey("BREATH_SCORE")) 
        {
            NewBalance.text = score.ToString();
        }
        if (PlayerPrefs.HasKey("BREATH_GRADE") && PlayerPrefs.HasKey("BREATH_FEEDBACK")) 
        {
            string grade = PlayerPrefs.GetString("BREATH_GRADE");
            string info = PlayerPrefs.GetString("BREATH_FEEDBACK");
            feedback.text = grade + " breath! " + info + " You earned:";
        }
        //update successful number of breaths
        int current = PlayerPrefs.GetInt("TOTAL_BREATHS");
        current += 1;
        PlayerPrefs.SetInt("TOTAL_BREATHS", current);
        PlayerPrefs.SetInt("MAX_VOL", 10);
        PlayerPrefs.SetInt("MAX_FLOW", 3);
        // display popUp
        goodPanel.SetActive(true);
      }
    }

    /*
    ############################################################
    ####################### NAVIGATION #########################
    ############################################################
    */

    public void GoToHome()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void GoToTutorial()
    {
        SceneManager.LoadScene("SpirometerHelp");
    }

    public void OnApplicationQuit()
    {
        Debug.Log("QUITTING APPLICATION");
        PlayerPrefs.DeleteAll();
    }
}

[System.Serializable]
public class Spirometer
{
    public string grade;
    public string feedback;
    public int score;
    public int balance;
    public int breathCount;


    public static Spirometer CreateFromJSON(string jsonString = "helloWorld")
    {
        return JsonUtility.FromJson<Spirometer>(jsonString);
    }

}
