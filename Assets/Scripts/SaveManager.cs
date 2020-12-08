using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;
using UnityEngine.UI;

// https://www.youtube.com/watch?v=sWWZZByVvlU
// https://answers.unity.com/questions/977881/save-a-list-of-a-class.html

public class SaveManager : MonoBehaviour
{
    // reference to the snapshot camera
    public SnapshotCamera camera;

    // reference to the line prefab
    public GameObject linePrefab;

    public static List<SaveLine> listSavedLines = new List<SaveLine>();
    public static List<SaveLine> undoneLines = new List<SaveLine>();

    public void PrintContentsExternal()
    {
        PrintContents(listSavedLines, true, true);
    }

    // when a new drawing is created, autosave it and name it "untitled"
    public void FirstAutoSave()
    { 
        // take snapshot and send
        camera.CallTakeSnapshot();
        SaveArtTitle(true);
    }

    // if artwork title is updated and saved, send post request w/ that field updated
    // also have to resend the listSavedLines to do it
    public void SaveArtTitle (bool isAutoSave)
    {
        string filepath = Path.Combine(Application.persistentDataPath, "saveTitle.dat");
        // get stroke data file 
        FileStream fs = new FileStream(filepath, FileMode.Create);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, listSavedLines);
        fs.Close();

        // get drawing ID
        string drawingId = PlayerPrefs.GetString("DRAWING_ID", "testUserAud"); // lol if it fails then i guess thats the default...

        string titleString = "Untitled";        
        if(!isAutoSave)
        {
            GameObject tempObj = GameObject.Find("InputField");
            InputField inputField = tempObj.GetComponent<InputField>();
            titleString = inputField.text;
        }
        Debug.Log("Title is: " + titleString);

        // send to api
        this.StartCoroutine(postSaveTitle(drawingId, titleString, filepath));
    }

    // send post request to send title w/ stroke data 
    IEnumerator postSaveTitle (string drawingId, string titleString, string filepath)
    {
        string url = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/save-drawing";
        using(UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.SetRequestHeader("drawingid", drawingId);

            // Add data to body
            request.SetRequestHeader("title", titleString);

            request.uploadHandler = (UploadHandler) new UploadHandlerFile(filepath);
            request.uploadHandler.contentType = "image/svg+xml";
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "image/svg+xml");

            // Wait for the response and then get our data
            yield return request.SendWebRequest();
            var data = request.downloadHandler.text;

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            
            if(request.isDone)
            {
                Debug.Log("Post save data resp code: "+ request.responseCode);
                Debug.Log("Done posting save data");
            }
        }
    }

    public void Save()
    { 
        // take snapshot and send
        camera.CallTakeSnapshot();

        // get stroke data file 
        string filepath = Path.Combine(Application.persistentDataPath, "save.dat");
        FileStream fs = new FileStream(filepath, FileMode.Create);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs, listSavedLines);
        fs.Close();

        // get drawing ID
        string drawingId = PlayerPrefs.GetString("DRAWING_ID", "testUserAud"); // lol if it fails then i guess thats the default...
        Debug.Log("Saving, drawingID is: " + drawingId);

        // send to api
        this.StartCoroutine(postSaveStrokeData(drawingId, filepath));
    }

    // send post request to send stroke data 
    IEnumerator postSaveStrokeData (string drawingId, string filepath)
    {
        string url = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/save-drawing";
        using(UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.SetRequestHeader("drawingid", drawingId);

            // Add data to body
            request.uploadHandler = (UploadHandler) new UploadHandlerFile(filepath);
            request.uploadHandler.contentType = "image/svg+xml";
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "image/svg+xml");

            // Wait for the response and then get our data
            yield return request.SendWebRequest();
            var data = request.downloadHandler.text;

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            if(request.isDone)
            {
                Debug.Log("Post save data resp code: "+ request.responseCode);
                Debug.Log("Done posting save data");
            }
        }
    }

    public void Load(string dataURL)
    {
        // loadTest();
        StartCoroutine(fetchStrokeData(dataURL));
    }

    IEnumerator fetchStrokeData(string url)
    {
        var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
        string path = Path.Combine(Application.persistentDataPath, "saveFINAL.dat");
        uwr.downloadHandler = new DownloadHandlerFile(path);
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError || uwr.isHttpError)
            Debug.LogError(uwr.error);
        else
            Debug.Log("File successfully downloaded and saved to " + path);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(path, FileMode.Open);

            List<SaveLine> listLoadLines = bf.Deserialize(fs) as List<SaveLine>;
            fs.Close();

            // set the savedlines as the loaded in lines
            listSavedLines = listLoadLines;
            loadStrokeData();
    }

    public void loadStrokeData ()
    {
        // loading part, might need to move this bit somewhere later 
        // also i foresee this part taking a long ass time to do.
        for (int i = 0; i<listSavedLines.Count; i++)
        {
            // create a line
            GameObject lineGameObj = Instantiate(linePrefab);
            Line currentLine = lineGameObj.GetComponent<Line>();

            // update line order...
            currentLine.lineRenderer.sortingOrder = LayerOrder.Order;

            // get the saved line data
            SaveLine currentSavedLine = listSavedLines[i];
            Color currentSavedColor = new Color(currentSavedLine.serColor._r, 
                                                currentSavedLine.serColor._g, 
                                                currentSavedLine.serColor._b, 
                                                currentSavedLine.serColor._a);
            currentLine.SetColor(currentSavedColor);
            currentLine.SetWidth(currentSavedLine.width);
            currentLine.SetTexture(currentSavedLine.texture);

            // setting the vector2 data
            currentLine.CreateLineBeforeLoadPoint();
            foreach(SerializableVector2 point in currentSavedLine.points)
            {
                Vector2 tempPoint = new Vector2(point.x, point.y);
                currentLine.SetPoint(tempPoint);
            }

        }
    }

    // TESTING FUNCTIONS

    // test local dat files 
    void loadTest()
    {
        string path = "saveTEST.dat";
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream(path, FileMode.Open);

        List<SaveLine> listLoadLines = bf.Deserialize(fs) as List<SaveLine>;
        fs.Close();

        // set the savedlines as the loaded in lines
        listSavedLines = listLoadLines;

        // loading part, might need to move this bit somewhere later 
        // also i foresee this part taking a long ass time to do.
        for (int i = 0; i<listSavedLines.Count; i++)
        {
            // create a line
            GameObject lineGameObj = Instantiate(linePrefab);
            Line currentLine = lineGameObj.GetComponent<Line>();

            // get the saved line data
            SaveLine currentSavedLine = listLoadLines[i];
            Color currentSavedColor = new Color(currentSavedLine.serColor._r, 
                                                currentSavedLine.serColor._g, 
                                                currentSavedLine.serColor._b, 
                                                currentSavedLine.serColor._a);
            currentLine.SetColor(currentSavedColor);
            currentLine.SetWidth(currentSavedLine.width);
            currentLine.SetTexture(currentSavedLine.texture);

            // setting the vector2 data
            currentLine.CreateLineBeforeLoadPoint();
            foreach(SerializableVector2 point in currentSavedLine.points)
            {
                Vector2 tempPoint = new Vector2(point.x, point.y);
                currentLine.SetPoint(tempPoint);
            }

        }
    }

    // debugging function to inspect the contents of the List of savedlines to make sure its getting everything correctly.
    public static void PrintContents(List<SaveLine> collection, bool simplifiedCount, bool simplifiedPoints)
    {
        Debug.Log("Saved lines count: " + collection.Count);
        if(!simplifiedCount)
        {
            foreach(SaveLine line in collection)
            {
                Debug.Log("*****************");
                Debug.Log("Color: " + line.serColor._r + " " + line.serColor._g  + " " + line.serColor._b  + " " + line.serColor._a);
                Debug.Log("Width: " + line.width);
                Debug.Log("Texture: " + line.texture);
                if(simplifiedPoints)
                {
                    // print out how many points are there
                    Debug.Log("Points count: " + line.points.Count);
                }
                else {
                    Debug.Log("Points: ");
                    // print out each point 
                    foreach(SerializableVector2 point in line.points)
                    {
                        Debug.Log("Pt: " + point.x + ", " + point.y);
                    }
                }
            } 
        }
    }
}

