using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class HomeScreen : MonoBehaviour
{
    // TODO: Make this device specific

    public GameObject TemplatePanel;
    public GameObject BlurPanel;
    public GameObject templatePrefab;
    public Dictionary<string, string> allTemplates = new Dictionary<string, string>();


    void Start ()
    {
        // Debug.Log("Device id: " + SystemInfo.deviceUniqueIdentifier.Replace("-",""));
        DestroyPanels();
        TemplatePanel.SetActive(false);
        BlurPanel.SetActive(false);        
        
        // We only want to call the API once when the application starts
        // Not every time the scene is started
        // PlayerPrefs will be deleted every time the application is quit
        if (!PlayerPrefs.HasKey("fetchedUserInfo"))
        {
            Debug.Log("NO PLAYER PREFS");
            this.StartCoroutine(fetchUserInfo());
            this.StartCoroutine(fetchAllTemplatesAndSave());
            PlayerPrefs.SetInt("fetchedUserInfo", 1);
            // TODO: FETCH TEMPLATES IS TOO SLOW
            PlayerPrefs.SetInt("TOTAL_NUM_TEMPLATES", 19);
        } 
        else
        {
            if (PlayerPrefs.GetInt("BREATH_COUNT") >= 10)
            {
                setUnlimitedText();
            }
            else
            {
                setBreathsLeft();
            }

        }
        this.StartCoroutine(fetchRecentArtwork());
        // this.StartCoroutine(fetchAllTemplates());
    }

    public void GoToShop ()
    {
        SceneManager.LoadScene("ShopScene");
    }

    public void GoToGallery ()
    {
        SceneManager.LoadScene("GalleryScene");
    }

    public void GoToUserArtwork ()
    {
        SceneManager.LoadScene("UserArtworkScene");
    }

    public void GoToUserSettings()
    {
        SceneManager.LoadScene("UserInfo");
    }

    public void GoToDraw ()
    {
        PlayerPrefs.SetInt("IS_USING_TEMPLATE", 0);
        SceneManager.LoadScene(2);
    }

    public void GoToSpirometer ()
    {
        SceneManager.LoadScene("SpirometerChallenge");
    }

    public void OpenTemplatesPanel ()
    {
        if (TemplatePanel != null)
        {
            TemplatePanel.SetActive(true);
            BlurPanel.SetActive(true);

            // Check if user has any templates, if they do, disable the NoTemplates panel
            string templatesOwned = PlayerPrefs.GetString("TEMPLATES");
            if (templatesOwned != "")
            {
                // deactive no templates message
                TemplatePanel.transform.GetChild(2).gameObject.SetActive(false);

                GameObject newObj; // create gameobject instance
                GameObject contentObj = GameObject.Find("TemplateContainer");
                string[] templates = templatesOwned.Split(':');
                foreach (string name in templates)
                {
                    newObj = (GameObject)Instantiate(templatePrefab, transform); // create new instances of prefab
                    newObj.transform.SetParent(contentObj.transform);
                    newObj.transform.GetChild(0).name = name;

                    // Add onclick to button
                    // Add onClick to button
                    
                    Button button = newObj.transform.GetChild(0).GetComponent<Button>();
                    button.onClick.AddListener(delegate {
                        LoadCanvasWithTemplate(name);
                    });

                    // Set the title of the template
                    Text title = newObj.transform.GetChild(1).GetComponent<Text>();
                    title.text = name;

                    // Set the image of the template
                    string path = string.Format("{0}/template${1}.pdb", Application.persistentDataPath, name);

                    RawImage image = newObj.transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
                    if(image != null)
                    {
                        var imageBytes = System.IO.File.ReadAllBytes(path);
                        Texture2D tex = new Texture2D(2, 2);
                        tex.LoadImage(imageBytes);
                        image.texture = tex;
                    }
                }
            }
        }
    }

    public void LoadCanvasWithTemplate (string templateName)
    {
        PlayerPrefs.SetString("CURRENT_TEMPLATE_NAME", templateName);
        Debug.Log("SETTING TEMPLATES GOT TO HERE");
        // string templateURL = templates[idx];
        PlayerPrefs.SetInt("IS_USING_TEMPLATE", 1);

        // make the api call here to POST a new drawing w/ template number
        // Debug.Log(templateURL);
        // Debug.Log(templateName);
        this.StartCoroutine(postNewTemplateDrawing(templateName));
        // SceneManager.LoadScene(2);

    }

    // create new artwork
    IEnumerator postNewTemplateDrawing (string templateTitle) {
        Debug.Log("ASDFASDF");
        string url = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/create-drawing";
        
        // Using the static constructor
        // var request = UnityWebRequest.Post(url, formData);
        var request = new UnityWebRequest(url, "POST");

        // Add data to body
        request.SetRequestHeader("deviceid", SystemInfo.deviceUniqueIdentifier.Replace("-",""));

        request.SetRequestHeader("template", templateTitle);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        // Wait for the response and then get our data
        yield return request.SendWebRequest();
        
        var data = request.downloadHandler.text;
        string drawingId = data.Replace('"', ' ').Trim(); 
        PlayerPrefs.SetString("DRAWING_ID", drawingId);
        Debug.Log("Just created drawing: " + drawingId + " with template: " + templateTitle);
        SceneManager.LoadScene(2);
    }


    public void CloseTemplatesPanel ()
    {
        DestroyPanels();
        if (TemplatePanel != null)
        {
            TemplatePanel.SetActive(false);
            BlurPanel.SetActive(false);
        }
    }

    public void DestroyPanels ()
    {
        // destroy the child components
        GameObject[] templatePanels = GameObject.FindGameObjectsWithTag("TemplatePanel");
        foreach(GameObject panel in templatePanels)
        {
            GameObject.Destroy(panel);
        }
    }

    public IEnumerator fetchUserInfo ()
    {
        string url = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/get-user-info";

        // Using the static constructor
        var request = UnityWebRequest.Get(url);
        request.SetRequestHeader("deviceid", SystemInfo.deviceUniqueIdentifier.Replace("-",""));

        // Wait for the response and then get our data
        yield return request.SendWebRequest();
        var data = request.downloadHandler.text;

        UserInfo user = UserInfo.CreateFromJSON(data);
        // Create a string to store the list of palettes seperated by :
        string paletteString = "";
        foreach (string palette in user.paints)
        {
            if (paletteString != "")
            {
                paletteString += ":";
            }
            paletteString += palette;
        }
        PlayerPrefs.SetString("PALETTES", paletteString);

        PlayerPrefs.SetInt("COIN_BALANCE", user.coins);

        string brushString = "";
        foreach (string brush in user.brushes)
        {
            if (brushString != "")
            {
                brushString += ":";
            }
            brushString += brush;
        }
        PlayerPrefs.SetString("BRUSHES", brushString);

        string templateString = "";
        foreach (string template in user.backgrounds)
        {
            if (templateString != "")
            {
                templateString += ":";
            }
            templateString += template;
        }
        PlayerPrefs.SetString("TEMPLATES", templateString);

        PlayerPrefs.SetString("USER_ID", user.userId);

        PlayerPrefs.SetInt("UNLIMITED_STATUS", user.unlimitedExpiration);

        PlayerPrefs.SetInt("BREATH_COUNT", user.breathCount);
        if (user.breathCount < 10)
        {
            setBreathsLeft();
        }
        else
        {
            setUnlimitedText();
        }
        

        string ownedTemplatesString = "";
        foreach (string background in user.backgrounds)
        {
            if (ownedTemplatesString != "")
            {
                ownedTemplatesString += ":";
            }
            ownedTemplatesString += background;
        }
        PlayerPrefs.SetString("OWNED_TEMPLATES", ownedTemplatesString);
    }

    // gets all templates
    // IEnumerator fetchAllTemplates ()
    // {
    //     string url = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/get-templates";

    //     // Using the static constructor
    //     var request = UnityWebRequest.Get(url);

    //     // Wait for the response and then get our data
    //     yield return request.SendWebRequest();
    //     var data = request.downloadHandler.text;

    //     TemplateCollection collection = TemplateCollection.CreateFromJSON(data);
    //     // Debug.Log("hello");
    //     // Debug.Log("size: " + collection.templates.Length);

    //     foreach(TemplateItem templateItem in collection.templates)
    //     {
    //         allTemplates[templateItem.title] = templateItem.url;
    //     }
    //     Debug.Log("Done fetching templates :)");
    // }

    IEnumerator fetchAllTemplatesAndSave ()
    {
        string url = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/get-templates";

        // Using the static constructor
        var request = UnityWebRequest.Get(url);

        // Wait for the response and then get our data
        yield return request.SendWebRequest();
        var data = request.downloadHandler.text;

        TemplateCollection collection = TemplateCollection.CreateFromJSON(data);

        PlayerPrefs.SetInt("TOTAL_NUM_TEMPLATES", collection.numTemplates);

        foreach(TemplateItem templateItem in collection.templates)
        {
            string savePath = string.Format("{0}/template${1}.pdb", Application.persistentDataPath, templateItem.title);
            if (!System.IO.File.Exists(savePath))
            {
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(templateItem.url);
                yield return www.SendWebRequest();

                if(www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    System.IO.File.WriteAllBytes(savePath, www.downloadHandler.data);
                }
            }
        }
    }

    // gets five most recent drawings and template artworks
    IEnumerator fetchRecentArtwork ()
    {
        string url = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/get-user-art";

        // Using the static constructor
        var request = UnityWebRequest.Get(url);
        request.SetRequestHeader("deviceid", SystemInfo.deviceUniqueIdentifier.Replace("-",""));

        // Wait for the response and then get our data
        yield return request.SendWebRequest();
        var data = request.downloadHandler.text;

        ArtworkCollection collection = ArtworkCollection.CreateFromJSON(data);
        // original artworks
        // first sort
        Array.Sort(collection.canvas,
            delegate(ArtworkItem x, ArtworkItem y) { return y.time.CompareTo(x.time); } );
        // only want first five
        // but if less than five, just add empty objects
        List<ArtworkItem> fiveMostRecentDrawings = new List<ArtworkItem>();
        if(collection.canvas.Length < 5)
        {
            for(int idx = 0; idx < collection.canvas.Length; idx++)
            {
                ArtworkItem artworkItem = collection.canvas[idx];
                fiveMostRecentDrawings.Add(artworkItem);
            }
            for(int idx = collection.canvas.Length; idx < 5; idx++)
            {
                ArtworkItem emptyArtworkItem = new ArtworkItem();
                fiveMostRecentDrawings.Add(emptyArtworkItem);
            }
        }
        else {
            for(int idx = 0; idx < 5; idx++)
            {
                ArtworkItem artworkItem = collection.canvas[idx];
                fiveMostRecentDrawings.Add(artworkItem);
            }
        }

        GameObject tempObj = GameObject.Find("DrawingContainer");
        fetchRecentDrawings fetchRecentDrawings = tempObj.GetComponent<fetchRecentDrawings>();
        fetchRecentDrawings.setOriginalDrawingsRow(fiveMostRecentDrawings);


        // templates
        //
        //
        // first sort
        Array.Sort(collection.template,
            delegate(ArtworkItem x, ArtworkItem y) { return y.time.CompareTo(x.time); } );
        // only want first five
        // but if less than five, just add empty objects
        List<ArtworkItem> fiveMostRecentTemplates = new List<ArtworkItem>();
        if(collection.template.Length < 5)
        {
            for(int idx = 0; idx < collection.template.Length; idx++)
            {
                ArtworkItem artworkItem = collection.template[idx];
                fiveMostRecentTemplates.Add(artworkItem);
            }
            for(int idx = collection.template.Length; idx < 5; idx++)
            {
                ArtworkItem emptyArtworkItem = new ArtworkItem();
                fiveMostRecentTemplates.Add(emptyArtworkItem);
            }
        }
        else {
            for(int idx = 0; idx < 5; idx++)
            {
                ArtworkItem artworkItem = collection.template[idx];
                fiveMostRecentTemplates.Add(artworkItem);
            }
        }

        GameObject tempObj2 = GameObject.Find("TemplateDrawingContainer");
        fetchRecentTemplates fetchRecentTemplates = tempObj2.GetComponent<fetchRecentTemplates>();
        fetchRecentTemplates.setTemplateDrawingsRow(fiveMostRecentTemplates);

        Debug.Log("done getting most recent artworks in main menu :)");
        // Show objects
        // GameObject loadingObj1 = GameObject.Find("Loading (1)");
        // GameObject loadingObj2 = GameObject.Find("Loading (2)");
        // GameObject loadingObj3 = GameObject.Find("Loading (3)");
        // GameObject loadingObj4 = GameObject.Find("Loading (4)");
        // GameObject loadingObj5 = GameObject.Find("Loading (5)");
        // GameObject loadingObj6 = GameObject.Find("Loading (6)");
        // GameObject loadingObj7 = GameObject.Find("Loading (7)");
        // GameObject loadingObj8 = GameObject.Find("Loading (8)");
        // GameObject loadingObj9 = GameObject.Find("Loading (9)");
        // GameObject loadingObj = GameObject.Find("LoadingFade");

        // yield return new WaitForSeconds((float) 0.25);
        // loadingObj1.SetActive(false);
        // yield return new WaitForSeconds((float) 0.06);
        // loadingObj2.SetActive(false);
        // yield return new WaitForSeconds((float) 0.05);
        // loadingObj3.SetActive(false);
        // yield return new WaitForSeconds((float) 0.05);
        // loadingObj4.SetActive(false);
        // yield return new WaitForSeconds((float) 0.04);
        // loadingObj5.SetActive(false);
        // yield return new WaitForSeconds((float) 0.04);
        // loadingObj6.SetActive(false);
        // yield return new WaitForSeconds((float) 0.03);
        // loadingObj7.SetActive(false);
        // yield return new WaitForSeconds((float) 0.03);
        // loadingObj8.SetActive(false);
        // yield return new WaitForSeconds((float) 0.02);
        // loadingObj9.SetActive(false);
        // // The Overall object being active until the load is complete makes sure the user does not click anywhere
        // loadingObj.SetActive(false);

    }

    public void CreateOriginalDrawing ()
    {
        this.StartCoroutine(postNewDrawing());
        Debug.Log("New drawing created");
    }
    
    // create new artwork
    IEnumerator postNewDrawing () {
        string drawingId = "";
        string url = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/create-drawing";
        // List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        // Using the static constructor
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            // Add data to body
            request.SetRequestHeader("deviceid", SystemInfo.deviceUniqueIdentifier.Replace("-",""));

            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            // Wait for the response and then get our data
            yield return request.SendWebRequest();
            
            var data = request.downloadHandler.text;
            
            drawingId = data.Replace('"', ' ').Trim(); // jesus christ. i cant believe it took me THREE days to figure this out
            Debug.Log("Just created drawing: " + drawingId);
            PlayerPrefs.SetString("DRAWING_ID", drawingId);
            GoToDraw();
        }
    }

    public void setBreathsLeft()
    {
        int breaths = PlayerPrefs.GetInt("BREATH_COUNT");
        GameObject unlimitedTextObject = GameObject.Find("UnlimitedText");
        unlimitedTextObject.GetComponent<Text>().text = "Breaths left to unlock unlimited Brushes and Colors:  ";
        GameObject breathsLeftObject = GameObject.Find("BreathsLeftText");
        breathsLeftObject.GetComponent<Text>().text = (10-breaths).ToString();

    }

    public void setUnlimitedText ()
    {
        GameObject breathsLeftObject = GameObject.Find("UnlimitedText");
        breathsLeftObject.GetComponent<Text>().text = "You can use all Colors and Brushes for the Next Hour!";

        GameObject unlimitedTextObject = GameObject.Find("BreathsLeftText");
        unlimitedTextObject.GetComponent<Text>().text = "";
    }


    void OnApplicationQuit()
    {
        Debug.Log("QUITTING APPLICATION");
        PlayerPrefs.DeleteAll();
    }

}


[System.Serializable]
public class ArtworkItem
{
    public string png;
    public string dat;
    public string drawingId;
    public string title;
    public string time;
    public string templateUrl;
}

[System.Serializable]
public class ArtworkCollection
{
    public ArtworkItem[] canvas;
    public ArtworkItem[] template;

    public static ArtworkCollection CreateFromJSON(string jsonString = "helloWorld")
    {
        return JsonUtility.FromJson<ArtworkCollection>(jsonString);
    }

}


[System.Serializable]
public class TemplateItem
{
    public string title;
    public string url;
}

[System.Serializable]
public class TemplateCollection
{
    public TemplateItem[] templates;
    public int numTemplates;

    public static TemplateCollection CreateFromJSON(string jsonString = "helloWorld")
    {
        return JsonUtility.FromJson<TemplateCollection>(jsonString);
    }

}


[System.Serializable]
public class UserInfo
{
    public string[] drawings;
    public int baseline;
    public string[] brushes;
    public string userId;
    public string[] backgrounds;
    public int coins;
    public string[] paints;
    public int unlimitedExpiration;
    public int breathCount;

    public static UserInfo CreateFromJSON(string jsonString = "helloWorld")
    {
        return JsonUtility.FromJson<UserInfo>(jsonString);
    }
}
