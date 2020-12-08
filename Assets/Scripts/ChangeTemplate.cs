using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

// reference for downloading texture to sprite: https://dev.to/cemuka/download-images-from-a-url-endpoint-in-runtime-with-unity-4f2a

public class ChangeTemplate : MonoBehaviour
{
    private SpriteRenderer rend;
    private Sprite sprite;

    // Start is called before the first frame update
    void OnEnable()
    {
        Debug.Log("displaying template...");
        int isUsingTemplate = PlayerPrefs.GetInt("IS_USING_TEMPLATE", 0);
        string url = PlayerPrefs.GetString("CURRENT_TEMPLATE_URL", "");
        string templateName = PlayerPrefs.GetString("CURRENT_TEMPLATE_NAME");

        if(isUsingTemplate == 1)
        {
            rend = GetComponent<SpriteRenderer>();

            string path = string.Format("{0}/template${1}.pdb", Application.persistentDataPath, templateName);
            var imageBytes = System.IO.File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(imageBytes);
            var rect = new Rect(0, 0, tex.width, tex.height);
            rend.sprite = Sprite.Create(tex,rect,new Vector2(0.5f,0.5f));
            // Texture2D tex = new Texture2D(2, 2);
            // tex.LoadImage(imageBytes);
            // image.texture = tex;
            // StartCoroutine(FetchSprite(url, (response) => {
            //     sprite = response;
            //     rend.sprite = sprite;
            // }));
        }
        PlayerPrefs.SetInt("IS_USING_TEMPLATE", 0);
    }

    IEnumerator FetchSprite(string url, System.Action<Sprite> callback)
    {
        var www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            if (www.isDone)
            {
                var texture = DownloadHandlerTexture.GetContent(www);
                var rect = new Rect(0, 0, texture.width, texture.height);
                var sprite = Sprite.Create(texture,rect,new Vector2(0.5f,0.5f));
                callback(sprite);
            }
        }
    }

}
