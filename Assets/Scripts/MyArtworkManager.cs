using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;

// REST API Get Request in Unity: https://www.youtube.com/watch?v=GIxu8kA9EBU

public class MyArtworkManager : MonoBehaviour
{   
    // static string USERID = "testUser-1607123961156147949";
    private string getUserArtURL = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/get-user-art";
    private string postToGalleryURL = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/publish-to-gallery";
    private string sendToEmailURL = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/send-drawing";

    JSONNode originalData;
    JSONNode templateData;

    int selectedIdx;
    bool selectedIsOriginal;
    
    public GameObject painting;
    public GameObject originialContent;
    public GameObject templateContent;
    public GameObject postToGalleryConfirm;
    public GameObject sendToEmailConfirm;

    void Awake()
    {
        
    }

    void Start()
    {
        postToGalleryConfirm.SetActive(false);
        sendToEmailConfirm.SetActive(false);
        StartCoroutine(GetUserArtworkData());
    }

    void Update()
    {
        
    }

    public void GoToHome ()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void GoToSpirometer() 
    {
        SceneManager.LoadScene("SpirometerChallenge");
    }

    IEnumerator GetUserArtworkData()
    {
        UnityWebRequest request = UnityWebRequest.Get(getUserArtURL);
        request.SetRequestHeader("deviceid", SystemInfo.deviceUniqueIdentifier.Replace("-",""));
        yield return request.SendWebRequest();
        if(request.isNetworkError || request.isHttpError) 
        {
            Debug.Log(request.error);
            Debug.Log(request.downloadHandler.text);
        }

        JSONNode data = JSON.Parse(request.downloadHandler.text);
        // "png" "dat" "drawingId" "time" "title"
        originalData = data["canvas"];
        templateData = data["template"];

        GameObject newObj;
        int i = 0;
        foreach (JSONNode artwork in originalData.Children)
        {
            newObj = (GameObject)Instantiate(painting, transform);
            newObj.transform.SetParent(originialContent.transform);

            int j = i;
            Button button = newObj.transform.GetChild(0).GetComponent<Button>();
            button.onClick.AddListener(()=>OpenCanvas(true, j));

            RawImage image = newObj.transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
            StartCoroutine(FetchImages(artwork["png"], image));

            Text title = newObj.transform.GetChild(1).GetComponent<Text>();
            title.text = artwork["title"];

            Text time = newObj.transform.GetChild(2).GetComponent<Text>();
            time.text = ConvertTime(artwork["time"]);

            Button buttonPost = newObj.transform.GetChild(3).GetComponent<Button>();
            buttonPost.onClick.AddListener(()=>HandlePostToGallery(true, j));

            Button buttonSend = newObj.transform.GetChild(4).GetComponent<Button>();
            buttonSend.onClick.AddListener(()=>HandleSendToEmail(true, j));

            i++;
        }

        i = 0;
        foreach (JSONNode artwork in templateData.Children)
        {
            newObj = (GameObject)Instantiate(painting, transform);
            newObj.transform.parent = templateContent.transform;

            int j = i;
            Button button = newObj.transform.GetChild(0).GetComponent<Button>();
            button.onClick.AddListener(()=>OpenCanvas(false, j));
            RawImage image = newObj.transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
            StartCoroutine(FetchImages(artwork["png"], image));

            Text title = newObj.transform.GetChild(1).GetComponent<Text>();
            title.text = artwork["title"];

            Text time = newObj.transform.GetChild(2).GetComponent<Text>();
            time.text = ConvertTime(artwork["time"]);

            Button buttonPost = newObj.transform.GetChild(3).GetComponent<Button>();
            buttonPost.onClick.AddListener(()=>HandlePostToGallery(false, j));

            Button buttonSend = newObj.transform.GetChild(4).GetComponent<Button>();
            buttonSend.onClick.AddListener(()=>HandleSendToEmail(false, j));

            i++;
        }
    }

    IEnumerator FetchImages(string url, RawImage img)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if(request.isNetworkError || request.isHttpError) 
        {
            Debug.Log(request.error); 
        }
        
        img.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    }

    string ConvertTime(string date)
    {
        double timesec = Convert.ToDouble(date)/1000000;
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(timesec);
        DateTime startdate = new DateTime(1970, 1, 1) + timeSpan;
        string dateText = startdate.ToString("MMMM dd, yyyy");
        return dateText;
    }
    
    public void OpenCanvas(bool isOriginal, int idx)
    {
        if(isOriginal)
        {
            loadDrawing(originalData[idx]["dat"], 
                        originalData[idx]["drawingId"],
                        originalData[idx]["title"]);
        }
        else
        {
            loadDrawingWithTemplate(templateData[idx]["dat"], 
                                    templateData[idx]["drawingId"],
                                    templateData[idx]["title"],
                                    templateData[idx]["templateUrl"]);
        }
    }

    // loads in the drawing and goes to drawing scene 
    public void loadDrawing(string dataURL, string drawingId, string titleString)
    {
        Debug.Log("testing local load drawing");
        PlayerPrefs.SetString("CURRENT_LOADED_DRAWING_DATA_URL", dataURL);
        PlayerPrefs.SetString("DRAWING_ID", drawingId);
        PlayerPrefs.SetString("CURRENT_ARTWORK_TITLE", titleString);

        PlayerPrefs.SetInt("IS_USING_LOADED_DRAWING", 1);
        SceneManager.LoadScene("DrawingScene");
    }

    // loads in the drawing WITH TEMPLATE and goes to drawing scene 
    public void loadDrawingWithTemplate(string dataURL, string drawingId, string titleString, string templateUrl)
    {
        Debug.Log("testing local load drawing");
        PlayerPrefs.SetString("CURRENT_LOADED_DRAWING_DATA_URL", dataURL);
        PlayerPrefs.SetString("DRAWING_ID", drawingId);
        PlayerPrefs.SetString("CURRENT_ARTWORK_TITLE", titleString);
        PlayerPrefs.SetString("CURRENT_TEMPLATE_NAME", templateUrl);
        PlayerPrefs.SetInt("IS_USING_TEMPLATE", 1);

        PlayerPrefs.SetInt("IS_USING_LOADED_DRAWING", 1);
        SceneManager.LoadScene("DrawingScene");
    }


    public void HandlePostToGallery(bool isOriginal, int idx)
    {
        RawImage img = postToGalleryConfirm.transform.GetChild(0).GetChild(1).GetComponent<RawImage>();
        RawImage imgInCnt = isOriginal ? 
            originialContent.transform.GetChild(idx).GetChild(0).GetChild(0).GetComponent<RawImage>() :
            templateContent.transform.GetChild(idx).GetChild(0).GetChild(0).GetComponent<RawImage>();
        img.texture = imgInCnt.texture;

        selectedIdx = idx;
        selectedIsOriginal = isOriginal;

        postToGalleryConfirm.SetActive(true);
    }

    public void PostToGalleryConfirm(Text title)
    {
        StartCoroutine(PostToGallery(title.text));
    }

    IEnumerator PostToGallery(string title)
    {   
        string drawingId = selectedIsOriginal ? originalData[selectedIdx]["drawingId"] : templateData[selectedIdx]["drawingId"];

        using(UnityWebRequest request = new UnityWebRequest(postToGalleryURL, "POST"))
        {
            request.SetRequestHeader("drawingid", drawingId);
            request.SetRequestHeader("title", title);

            postToGalleryConfirm.SetActive(false);

            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
        }
    }

    public void HandleSendToEmail(bool isOriginal, int idx)
    {
        RawImage img = sendToEmailConfirm.transform.GetChild(0).GetChild(1).GetComponent<RawImage>();
        RawImage imgInCnt = isOriginal ? 
            originialContent.transform.GetChild(idx).GetChild(0).GetChild(0).GetComponent<RawImage>() :
            templateContent.transform.GetChild(idx).GetChild(0).GetChild(0).GetComponent<RawImage>();
        img.texture = imgInCnt.texture;

        selectedIdx = idx;
        selectedIsOriginal = isOriginal;

        sendToEmailConfirm.SetActive(true);
    }

    public void SendToEmailConfirm(Text title)
    {
        StartCoroutine(SendToEmail(title.text));
    }

    IEnumerator SendToEmail(string address)
    {
        string drawingId = selectedIsOriginal ? originalData[selectedIdx]["drawingId"] : templateData[selectedIdx]["drawingId"];

        using(UnityWebRequest request = new UnityWebRequest(sendToEmailURL, "POST"))
        {
            request.SetRequestHeader("drawingid", drawingId);
            request.SetRequestHeader("address", address);

            sendToEmailConfirm.SetActive(false);
            
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
        }
    }

    public void ClosePostToGalleryConfirm()
    {
        postToGalleryConfirm.SetActive(false);
    }

    public void CloseSendToEmailConfirm()
    {
        sendToEmailConfirm.SetActive(false);
    }
}
