using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// grid layout and scroll window: https://www.youtube.com/watch?v=VyIo5tlNNeA

public class fetchAllPurchasedTemplates : MonoBehaviour
{
    public RawImage template1;
    public GameObject prefab; // our prefab object

    public Dictionary<string, string> allTemplates = new Dictionary<string, string>();

    List<string> templates = new List<string>();
    List<string> templateTitles = new List<string>();

    // private void setTemplates ()
    // {

    //     HashSet<string> ownedTemplates = new HashSet<string>();

    //     // // Add any templates owned to the owned templates array
    //     if (PlayerPrefs.HasKey("TEMPLATES"))
    //     {
    //         string templateString = PlayerPrefs.GetString("TEMPLATES");
    //         Debug.Log("SET TEMPLATES: " + templateString);
    //         // templates are stored as a string seperated by a : in PlayerPrefs
    //         if (templateString != "") // for some reason, split returns size of 1 if empty
    //         {
    //             string[] templatesOwned = templateString.Split(':');
    //             foreach (string template in templatesOwned)
    //             {
    //                 ownedTemplates.Add(template);
    //             }
    //         }
    //     }

    //     //creating UI grid elements
    //     GameObject newObj; // create gameobject instance
    //     GameObject contentObj = GameObject.Find("TemplateContainer");
    //     foreach (string path in System.IO.Directory.GetFiles(Application.persistentDataPath))
    //     {
    //         string fileName = Path.GetFileName(path);
    //         if (fileName.Split('$').Length == 2 && fileName.Split('$')[0] == "template")
    //         {
    //             string name = fileName.Split('$')[1].Split('.')[0];
    //             newObj = (GameObject)Instantiate(prefab, transform); // create new instances of prefab
    //             newObj.transform.SetParent(contentObj.transform);
    //             newObj.transform.GetChild(0).name = name;

    //             // Add onClick to button
    //             Button button = newObj.transform.GetChild(0).GetComponent<Button>();
    //             button.onClick.AddListener(delegate {
    //                 string templateTitle = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
    //                 // Temporary Value used for purchasing
    //                 PlayerPrefs.SetString("PURCHASED_BRUSH_TITLE", templateTitle);

    //                 // Set Blur Panel
    //                 // Need to find the root obj of BlurPanel first as it is currenlty inactive (Find won't find it)
    //                 GameObject ShopScreenObj = GameObject.Find("ShopScreen");
    //                 GameObject BlurPanelObj = ShopScreenObj.transform.Find("BlurPanel").gameObject;
    //                 BlurPanelObj.SetActive(true);

    //                 // Need to find the root obj of PopUpPanel first as it is currenlty inactive (Find won't find it)
    //                 GameObject CanvasObj = GameObject.Find("Canvas");
    //                 GameObject PopUpPanelTemplatesObj = CanvasObj.transform.Find("PopUpPanelTemplates").gameObject;
    //                 PopUpPanelTemplatesObj.SetActive(true);

    //                 // Set the Title
    //                 GameObject PopUpTitleObj = GameObject.Find("TemplateButtonItem/Title");
    //                 PopUpTitleObj.GetComponent<Text>().text = name;
                    
    //                 // Set the Image
    //                 GameObject PopUpImageObj = GameObject.Find("TemplateButtonItem/TemplatePreview");
    //                 RawImage popUpImage = PopUpImageObj.GetComponent<RawImage>();
    //                 if(popUpImage != null)
    //                 {
    //                     var imageBytes = System.IO.File.ReadAllBytes(path);
    //                     Texture2D tex = new Texture2D(2, 2);
    //                     tex.LoadImage(imageBytes);
    //                     popUpImage.texture = tex;
    //                 }

    //                 // Handle the Purchase Button
    //                 GameObject purchaseObject = GameObject.Find("PopUpPanelTemplates/Purchase");
    //                 GameObject purchaseTextObject = GameObject.Find("PopUpPanelTemplates/Purchase/Text");
                    
    //                 if (PlayerPrefs.GetInt("COIN_BALANCE") < TEMPLATE_COST)
    //                 {
    //                     var cantPurchaseImage = Resources.Load<Sprite>("Images/CantPurchase");
    //                     purchaseObject.GetComponent<Image>().sprite = cantPurchaseImage;
    //                     purchaseObject.GetComponent<Button>().interactable = false;
    //                     purchaseTextObject.GetComponent<Text>().text = "Need Coins";
    //                 } 
    //                 else
    //                 {
    //                     var confirmPurchaseImage = Resources.Load<Sprite>("Images/Confirm");
    //                     purchaseObject.GetComponent<Image>().sprite = confirmPurchaseImage;
    //                     purchaseObject.GetComponent<Button>().interactable = true;
    //                     purchaseTextObject.GetComponent<Text>().text = "Purchase";
    //                 }
    //             });

    //             // Set the title of the template
    //             Text title = newObj.transform.GetChild(1).GetComponent<Text>();
    //             title.text = name;

    //             // Set the image of the template
    //             RawImage image = newObj.transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
    //             if(image != null)
    //             {
    //                 var imageBytes = System.IO.File.ReadAllBytes(path);
    //                 Texture2D tex = new Texture2D(2, 2);
    //                 tex.LoadImage(imageBytes);
    //                 image.texture = tex;
    //             }

    //             // Check if the template is already owned, if so, then make it unclickable and display owned
    //             if (ownedTemplates.Contains(name))
    //             {
    //                 // Update coin balance to show owned
    //                 Text cost = newObj.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>();
    //                 cost.text = "Owned";

    //                 // Get the check mark png
    //                 var checkMark = Resources.Load<Texture2D>("Images/CheckMark");
                    
    //                 // Update the coin icon to now show a check mark
    //                 RawImage coinIcon = newObj.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<RawImage>();
    //                 coinIcon.texture = checkMark;

    //                 // Disable clickable palette
    //                 newObj.transform.GetChild(0).GetComponent<Button>().interactable = false;
    //             }
    //         }
    //     }
    // }

    public void setTemplates()
    {
        GameObject tempObj = GameObject.Find("HomeScreen");
        HomeScreen homeScreen = tempObj.GetComponent<HomeScreen>();
        allTemplates = homeScreen.allTemplates;
        // Add any templates owned to the owned templates array
        string[] templatesOwned = {};
        if (PlayerPrefs.HasKey("OWNED_TEMPLATES"))
        {
            string ownedTemplatesString = PlayerPrefs.GetString("OWNED_TEMPLATES");
            // Templates are stored as a string seperated by a : in PlayerPrefs
            templatesOwned = ownedTemplatesString.Split(':');
        }

        foreach(string template in templatesOwned)
        {
            if(allTemplates.ContainsKey(template))
            {
                templateTitles.Add(template);
                templates.Add(allTemplates[template]);
            }
        }

        // creating UI grid elements
        GameObject newObj; // create gameobject instance
        for (int templateIdx = 0; templateIdx < templates.Count; templateIdx++)
        {
            newObj = (GameObject)Instantiate(prefab, transform); // create new instances of prefab
            
            int j = templateIdx;
            // Add onClick to button
            Button button = newObj.transform.GetChild(0).GetComponent<Button>();
            button.onClick.AddListener(()=>LoadCanvasSceneWithTemplate(j)); // much cleaner now!! ty maple :) 

            // Set the title of the template
            Text title = newObj.transform.GetChild(1).GetComponent<Text>();
            title.text = templateTitles[templateIdx];

            // Set the image of the template
            RawImage image = newObj.transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
            string url = templates[templateIdx];
            if(image!=null)
            {
                StartCoroutine(FetchImages(url, image));
            }

        }
    }

    public void LoadCanvasSceneWithTemplate(int idx)
    {
        string templateURL = templates[idx];
        PlayerPrefs.SetString("CURRENT_TEMPLATE_URL", templateURL);
        PlayerPrefs.SetInt("IS_USING_TEMPLATE", 1);

        // make the api call here to POST a new drawing w/ template number
        string templateName = templateTitles[idx];

        // Debug.Log(templateURL);
        // Debug.Log(templateName);
        this.StartCoroutine(postNewTemplateDrawing(templateName));

    }

    // create new artwork
    IEnumerator postNewTemplateDrawing (string templateTitle) {
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
