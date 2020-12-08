using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;

public class fetchRecentTemplates : MonoBehaviour
{
    List<ArtworkItem> fiveDrawings = new List<ArtworkItem>();

    public GameObject prefab; // our prefab object

    public void setTemplateDrawingsRow(List<ArtworkItem> fiveMostRecentTemplates)
    {
        Debug.Log("setting drawing row");
        fiveDrawings = fiveMostRecentTemplates;

        GameObject tempObj = GameObject.Find("HomeScreen");
        HomeScreen homeScreen = tempObj.GetComponent<HomeScreen>();

        GameObject newObj; // create gameobject instance
        for(int tileIdx = 0; tileIdx < fiveMostRecentTemplates.Count; tileIdx++)
        {
            ArtworkItem artworkItem = fiveMostRecentTemplates[tileIdx];
            // create new instances of prefab
            newObj = (GameObject)Instantiate(prefab, transform); 
            string url = artworkItem.png;
            
            // if empty object then dont add anything 
            if(!String.IsNullOrEmpty(url))
            {
                int j = tileIdx;

                // Add onClick to button
                Button button = newObj.transform.GetChild(0).GetComponent<Button>();
                button.onClick.AddListener(()=>loadDrawingWithTemplate(j)); 

                // Set the image of the template
                RawImage image = newObj.transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
                if(image!=null)
                {
                    StartCoroutine(FetchImages(url, image));
                }
                
                // Set the time
                Text timeText = newObj.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>();
                timeText.text = convertTime(artworkItem.time);
            }
        }
    }

    string convertTime(string date)
    {
        double timesec = Convert.ToDouble(date)/1000000;
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(timesec);
        DateTime startdate = new DateTime(1970, 1, 1) + timeSpan;
        string dateText = startdate.ToString("MMMM dd, yyyy");
        return dateText;
    }

    // loads in the drawing and goes to drawing scene 
    // also has to load in the template!!
    public void loadDrawingWithTemplate(int idx)
    {
        Debug.Log("testing local load drawing");
        string dataURL = fiveDrawings[idx].dat;
        string drawingId = fiveDrawings[idx].drawingId;
        string templateUrl = fiveDrawings[idx].templateUrl;
        string titleString = fiveDrawings[idx].title;

        PlayerPrefs.SetString("CURRENT_LOADED_DRAWING_DATA_URL", dataURL);
        PlayerPrefs.SetString("DRAWING_ID", drawingId);
        PlayerPrefs.SetInt("IS_USING_LOADED_DRAWING", 1);
        PlayerPrefs.SetString("CURRENT_TEMPLATE_NAME", templateUrl);
        PlayerPrefs.SetInt("IS_USING_TEMPLATE", 1);
        PlayerPrefs.SetString("CURRENT_ARTWORK_TITLE", titleString);

        SceneManager.LoadScene(2);
    }

    IEnumerator FetchImages(string url, RawImage img)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        }
        else {
            img.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }
}