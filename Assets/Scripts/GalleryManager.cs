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

public class GalleryManager : MonoBehaviour
{   
    private string getGalleryURL = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/get-gallery";
    private string getTagsURL = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/get-tags";
    private string addTagURL = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/add-tag";

    JSONNode originalData;
    JSONNode templateData;

    private string[] tags = 
    {   
        "Love It",
        "Amazing Piece",
        "Fantastic Color",
        "Nice Composition",
        "Perfect Quality",
    };
    
    Color32 slectedColor = new Color32(107, 135, 233, 255);

    public GameObject tagButton;
    public GameObject painting;
    public GameObject originialContent;
    public GameObject templateContent;
    public GameObject paintingDetailWindow;
    public GameObject tagContent;
    public GameObject noSearchResultAlert;

    void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        paintingDetailWindow.SetActive(false);
        noSearchResultAlert.SetActive(false);
        StartCoroutine(GetGalleryData(getGalleryURL));
    }

    // Update is called once per frame
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

    IEnumerator GetGalleryData(string getGalleryURL)
    {
        UnityWebRequest galleryDataRequest = UnityWebRequest.Get(getGalleryURL);
        yield return galleryDataRequest.SendWebRequest();
        if(galleryDataRequest.isNetworkError || galleryDataRequest.isHttpError)
        {
            Debug.Log(galleryDataRequest.error);
        }

        JSONNode galleryData = JSON.Parse(galleryDataRequest.downloadHandler.text);
        originalData = galleryData["canvases"];
        templateData = galleryData["templates"];

        GameObject newObj;
        int i = 0;
        foreach (JSONNode artwork in originalData.Children)
        {
            newObj = (GameObject)Instantiate(painting, transform);
            newObj.transform.SetParent(originialContent.transform);
            
            Text title = newObj.transform.GetChild(1).GetComponent<Text>();
            title.text = artwork["title"];

            int j = i;
            Button button = newObj.transform.GetChild(0).GetComponent<Button>();
            button.onClick.AddListener(()=>HandleImageSelect(true,j));

            RawImage image = newObj.transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
            StartCoroutine(FetchImages(artwork["imageUrl"], image)); 

            i++;
        }

        foreach (JSONNode artwork in templateData.Children)
        {
            newObj = (GameObject)Instantiate(painting, transform);
            newObj.transform.parent = originialContent.transform;
            
            Text title = newObj.transform.GetChild(1).GetComponent<Text>();
            title.text = artwork["title"];

            int j = i;
            Button button = newObj.transform.GetChild(0).GetComponent<Button>();
            button.onClick.AddListener(()=>HandleImageSelect(true,j));

            RawImage image = newObj.transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
            StartCoroutine(FetchImages(artwork["imageUrl"], image)); 

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

    public void HandleImageSelect(bool isOriginal, int idx)
    {
        // generate paintingDetailWindow's contents
        Text title = paintingDetailWindow.transform.GetChild(0).GetChild(1).GetComponent<Text>();
        title.text = isOriginal ? originalData[idx]["title"] : templateData[idx]["title"];

        RawImage image = paintingDetailWindow.transform.GetChild(0).GetChild(2).GetComponent<RawImage>();
        RawImage otherImgae = isOriginal ? 
            originialContent.transform.GetChild(idx).GetChild(0).GetChild(0).GetComponent<RawImage>() :
            templateContent.transform.GetChild(idx).GetChild(0).GetChild(0).GetComponent<RawImage>();
        image.texture = otherImgae.texture;
        
        // get tags
        string drawingId = isOriginal ? originalData[idx]["drawingId"] : templateData[idx]["drawingId"];
        // Debug.Log(drawingId);
        StartCoroutine(GetTagsData(isOriginal, idx, drawingId));

        paintingDetailWindow.SetActive(true);
    }

    IEnumerator GetTagsData(bool isOriginal, int idx, string drawingId) 
    {
        UnityWebRequest request = UnityWebRequest.Get(getTagsURL);
        request.SetRequestHeader("deviceid", SystemInfo.deviceUniqueIdentifier.Replace("-",""));
        request.SetRequestHeader("drawingid", drawingId);
        yield return request.SendWebRequest();
        if(request.isNetworkError || request.isHttpError) 
        {
            Debug.Log(request.error);
            Debug.Log(request.downloadHandler.text);
        }

        // response item form: "{i}" : { "count" : int ,  "user_given": bool }
        JSONNode tagData = JSON.Parse(request.downloadHandler.text);

        GameObject newObj;
        int i = 0;
        foreach(JSONNode tag in tagData) 
        {
            newObj = (GameObject)Instantiate(tagButton, transform);
            newObj.transform.SetParent(tagContent.transform);

            int j = i;

            Button button = newObj.transform.GetComponent<Button>();
            button.onClick.AddListener(()=>AddTag(drawingId, j));
            
            Text text = button.transform.GetChild(0).GetComponent<Text>();
            text.text = tags[j];

            if(tag["user_given"]) 
            {
                button.GetComponent<Image>().color = slectedColor;
                button.interactable = false;
            }
            else
            {
                button.GetComponent<Image>().color = Color.white;
                button.interactable = true;
            }

            Text tagCount = button.transform.GetChild(1).GetChild(0).GetComponent<Text>();
            tagCount.text = tag["count"].ToString();

            i++;
        }
    }

    public void AddTag(string drawingId, int tagIdx)
    {
        Button button = tagContent.transform.GetChild(tagIdx).GetComponent<Button>();
        button.GetComponent<Image>().color = slectedColor;
        button.interactable = false;

        Text tagCount = button.transform.GetChild(1).GetChild(0).GetComponent<Text>();
        tagCount.text = (int.Parse(tagCount.text) + 1).ToString();
        
        StartCoroutine(PostTagData(drawingId, tagIdx));
    }

    IEnumerator PostTagData(string drawingId, int tagIdx)
    {
        using(UnityWebRequest request = new UnityWebRequest(addTagURL, "POST"))
        {
            request.SetRequestHeader("deviceid", SystemInfo.deviceUniqueIdentifier.Replace("-",""));
            request.SetRequestHeader("drawingid", drawingId);
            request.SetRequestHeader("tag", tagIdx.ToString());

            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
        }
    }

    public void ClosePaintingDetailWindow()
    {
        // destroy tags
        foreach (Transform child in tagContent.transform) 
        {
            GameObject.Destroy(child.gameObject);
        }
        paintingDetailWindow.SetActive(false);
    }

    public void SearchGalley(Text text)
    {
        string title = text.text;
        if(title == "") return;

        int i = 0;
        foreach(JSONNode artwork in originalData)
        {
            if(artwork["title"] == title)
            {
                HandleImageSelect(true, i);
                return;
            }
        }

        i = 0;
        foreach(JSONNode artwork in templateData)
        {
            if(artwork["title"] == title)
            {
                HandleImageSelect(false, i);
                return;
            }
        }

        // no result
        noSearchResultAlert.SetActive(true);
    }
}
