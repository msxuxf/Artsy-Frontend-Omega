using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

// https://answers.unity.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html

[RequireComponent(typeof(Camera))]
public class SnapshotCamera : MonoBehaviour 
{
	public int resWidth = 1024; 
	public int resHeight = 1366;

	public string saveFilename = "";
	public byte[] bytes = {};

	private bool takeHiResShot = false;

	public static string ScreenShotName(int width, int height) 
	{
		return string.Format("{0}/Snapshots/screen_{1}x{2}_{3}.png", 
							Application.dataPath, 
							width, height, 
							System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
	}

	public void CallTakeSnapshot() 
	{
		takeHiResShot = true;
	}

	// takes the screenshot if takeHiResShot is set true
	void LateUpdate() 
	{
		if (takeHiResShot) 
		{
			RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
			GetComponent<Camera>().targetTexture = rt;
			Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
			GetComponent<Camera>().Render();
			RenderTexture.active = rt;
			screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
			GetComponent<Camera>().targetTexture = null;
			RenderTexture.active = null;
			Destroy(rt);
			bytes = screenShot.EncodeToPNG(); // this is the thing to send over network

			// save file locally - only keep in when testing 
			// string filename = ScreenShotName(resWidth, resHeight);
			// System.IO.File.WriteAllBytes(filename, bytes);
			// Debug.Log(string.Format("Took screenshot to: {0}", filename));

			takeHiResShot = false;

			// send to api
        	this.StartCoroutine(postSaveDrawingPNG());
		}
	}

    // send post request to send screenshot png 
    IEnumerator postSaveDrawingPNG ()
    {
        string url = "https://uql53bqfta.execute-api.us-east-1.amazonaws.com/Artsy/save-drawing";
        // UnityWebRequest
        using(UnityWebRequest request = new UnityWebRequest(url,  "POST"))
		{
			string drawingId = PlayerPrefs.GetString("DRAWING_ID", "testUserAud"); // lol if it fails then i guess thats the default...
			Debug.Log("Saving snapshot, drawingID is: " + drawingId);
			
			request.SetRequestHeader("drawingid", drawingId);
			// add image data to body

			request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bytes);
            request.uploadHandler.contentType = "image/png";
			request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "image/png");

			// Wait for the response and then get our data
			yield return request.SendWebRequest();
			var data = request.downloadHandler.text;
			
			if (request.isNetworkError || request.isHttpError)
			{
				Debug.Log(request.error);
			}
			if(request.isDone)
			{
				Debug.Log("Post save png resp code: "+ request.responseCode);
				Debug.Log("Done posting save png");
			}
		}
    }
}

[System.Serializable]
public class Save 
{
	public string png_file;
	public string svg_file;

  	public static Save CreateFromJSON(string jsonString = "helloWorld")
	{
		return JsonUtility.FromJson<Save>(jsonString);
	}
}
