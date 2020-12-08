// using System.Collections;
// using System.Collections.Generic;
// using System.Text.RegularExpressions;
// using System;
// using System.Linq;
// using UnityEngine;
// using UnityEngine.Networking;
// using UnityEngine.UI;
//
//
// public class SpirometerSensor : MonoBehaviour
// {
//     private enum State {Disabled, Zeroing, WaitToStart, Transition, Challenge, Results, Finished};
//     private bool stateChanged = true;
//
//     private State currentState = State.Zeroing;
//     private State nextState = State.Finished;
//
//     private float length = 10f;
//     private float flowTolerance = 0.1f;
//     private float volumeTolerance = 0.1f;
//     private bool timerRunning;
//     private float timer;
//     private bool oldData = true;
//
//     private string[] wwwArray = {};
//     private float initialVolume;
//     private float initialQual;
//     private float previousVolume;
//     private float previousQual;
//     private float newVolume;
//     private float newQual;
//     private SpirometerData spiroChallenge;
//     public bool Challenges = true;
//
//
//
//     // Start is called before the first frame update
//     void Start()
//     {
//       spiroChallenge = GameObject.FindObjectOfType<SpirometerData>();
//       if (Challenges) {
//         StartCoroutine(GetText(true));
//       }
//     }
//
//     // Update is called once per frame
//     void Update()
//     {
//       if (timerRunning) {
//         timer += Time.deltaTime;
//       }
//     }
//
//     public void NewTest() {
//       currentState = State.Zeroing;
//       StartCoroutine(GetText(false));
//     }
//
//     IEnumerator GetText(bool initial) {
//       spiroChallenge.Display();
//
//       List<float> flows = new List<float>();
//       List<float> volumes = new List<float>();
//
//       var lastSecond = length;
//       while (currentState != State.Finished) {
//         string url = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/add-breath";
//
//         // TODO: Waiting on backend to change Headers to Form Data
//         List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
//         UnityWebRequest www = UnityWebRequest.Post(url, formData);
//         UnityWebRequest request = UnityWebRequest.Post(url, formData);
//         request.SetRequestHeader("userId", "testUser-1605385650936673515");
//         request.SetRequestHeader("volume", wwwArray);
//         request.SetRequestHeader("flow", wwwArray);
//
//         float flow;
//         float volume;
//         if (currentState == State.Zeroing) {
//           if (stateChanged) {
//             // add pop ups later
//             // spiroChallenge.SetStatusText("Please Zero Spirometer");
//             stateChanged = false;
//           }
//
//           yield return www.SendWebRequest();
//
//           if (www.isNetworkError) {
//             Debug.Log(www.error);
//           }
//           else {
//             // old data
//             string data = www.downloadHandler.text;
//             Spirometer spiro = Spirometer.CreateFromJSON(data);
//             PlayerPrefs.SetInt("BALANCE", spiro.balance);
//             PlayerPrefs.SetInt("BREATHS_REMAINING", spiro.breathsRemaining);
//             if (oldData) {
//               wwwArray = Regex.Split(input, @"\D+");
//               wwwArray[0] = wwwArray[1];
//               wwwArray[1] = wwwArray[2];
//             }
//
//             volume = oldData ? 0.0f : float.Parse(wwwArray[0]);
//             flow = oldData ? 0.0f : float.Parse(wwwArray[1]);
//
//             spiroChallenge.SetAirflowText(flow.ToString());
//             spiroChallenge.SetVolumeText(volume.ToString());
//
//             if (Math.Abs(volume) < volumeTolerance && Math.Abs(flow) < flowTolerance) {
//               timerRunning = true;
//               if (timer > 2.0f) {
//                 // later
//                 // spiroChallenge.SetStatusText("Spirometer has been zeroed"));
//                 timerRunning = false;
//                 timer = 0.0f;
//                 currentState = State.Transition;
//                 nextState = State.WaitToStart;
//                 stateChanged = true;
//               }
//             }
//             else {
//               timerRunning = false;
//               timer = 0;
//             }
//           }
//           yield return null;
//         }
//         else if (currentState == State.Transition) {
//           if (stateChanged) {
//             timer = 0.0f;
//             timerRunning = true;
//             stateChanged = true;
//           }
//
//           if (timer > 1.0f) {
//             timerRunning = false;
//             timer = 0.0f;
//             currentState = nextState;
//             stateChanged = true;
//           }
//           yield return null;
//         }
//         else if (currentState == State.WaitToStart || currentState == State.Challenge) {
//           if (stateChanged) {
//             if (currentState == State.WaitToStart) {
//               // later
//               // spiroChallenge.SetStatusText("Start spirometer challenge");
//             }
//             else if (currentState == State.Challenge) {
//               // later
//               // spiroChallenge.SetStatusText(lastSecond + " seconds remaining");
//               timer = 0.0f;
//               timerRunning = true;
//               stateChanged = false;
//             }
//           }
//           yield return www.SendWebRequest();
//
//           if (www.isNetworkError) {
//             Debug.Log(www.error);
//           }
//           else {
//             string data = www.downloadHandler.text;
//             Spirometer spiro = Spirometer.CreateFromJSON(data);
//             PlayerPrefs.SetInt("BALANCE", spiro.balance);
//             PlayerPrefs.SetInt("BREATHS_REMAINING", spiro.breathsRemaining);
//             if (oldData) {
//               wwwArray = input.Split(',');
//             }
//             else {
//               //what is this
//               wwwArray = Regex.Split(input, @"\D+");
//               wwwArray[0] = wwwArray[1];
//               wwwArray[1] = wwwArray[2];
//             }
//
//             volume = float.Parse(wwwArray[0]);
//             flow = float.Parse(wwwArray[1]);
//             spiroChallenge.SetVolumeText(volume.ToString());
//             spiroChallenge.SetAirflowText(flow.ToString());
//             if (currentState == State.Challenge) {
//               volumes.Add(volume);
//               flows.Add(flow);
//
//               if (timer > length) {
//                 // later
//                 // spiroChallenge.SetStatusText("Spirometer Challenge completed");
//                 timerRunning = false;
//                 timer = 0.0f;
//                 currentState = State.Results;
//                 stateChanged = true;
//               }
//
//               if (length - Math.Floor(timer) < lastSecond) {
//                 --lastSecond;
//                 // later
//                 // spiroChallenge.SetStatusText(lastSecond + " seconds remaining");
//               }
//             }
//           }
//           yield return null;
//         }
//         else if (currentState == State.Results) {
//           var volumesSum = volumes.Sum();
//           float volumeMean = volumesSum / volumes.Count;
//           var flowsSum = flows.Sum();
//           float flowMean = flowsSum / volumes.Count;
//
//           if (initial) {
//             initialVolume = volumeMean;
//             initialQual = flowMean;
//             spiroChallenge.SetVolumeText(initialVolume.ToString());
//             spiroChallenge.SetAirflowText(initialQual.ToString());
//             // later
//             // spiroChallenge.SetStatusText("initial values recorded");
//             previousVolume = initialVolume;
//             previousQual = initialQual;
//           } else {
//             // not sure if needed?
//           }
//           nextState = State.Finished;
//           currentState = State.Transition;
//           stateChanged = true;
//           yield return null;
//         }
//       }
//       string temp = "0";
//       spiroChallenge.SetVolumeText(temp);
//       spiroChallenge.SetAirflowText(temp);
//       spiroChallenge.Display();
//     }
// }
//
// [System.Serializable]
// public class Spirometer
// {
//     public int balance;
//     public int breathsRemaining;
//
//     public static Spirometer CreateFromJSON(string jsonString = "helloWorld")
//     {
//         return JsonUtility.FromJson<Spirometer>(jsonString);
//     }
//
// }
