using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Pinch to zoom and panning:  https://www.youtube.com/watch?v=0G4vcH9N0gc

public class PanZoom : MonoBehaviour
{
    Vector3 touchStart;
    public float zoomOutMin = 1;
    public float zoomOutMax = 8;

    // Start is called before the first frame update
    void Start()
    {
        if(Input.touchSupported)
        {
            Debug.Log("touch supported");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount == 2) 
        {
            // PAN (move camera)
            // check start of touch
            if(Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Vector3 touchDeltaPos = Input.GetTouch(0).deltaPosition;
                Debug.Log("camera pos: " + Camera.main.transform.position);
                Debug.Log("transforming by: " + -touchDeltaPos.x + ", " +  -touchDeltaPos.y);
                transform.Translate(-touchDeltaPos.x, -touchDeltaPos.y, 0);
            }

            // // // // if(Input.GetMouseButtonDown(0)) {
            // // // if(Input.touches[0].phase == TouchPhase.Began) {
            // // //     Debug.Log("First touch registered");
            // // //     touchStart = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            // // // }
            // // // // check rest of touch, ie the movement
            // // // if(Input.touches[0].phase == TouchPhase.Ended) {
            // // //     Debug.Log("Getting rest of touch");
            // // //     Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            // // //     Camera.main.transform.position += direction; 
            // // // }

            // ZOOM
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector3 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector3 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // get magnitude differences btwn two fingers before and after 
            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            // calc difference
            float difference = currentMagnitude - prevMagnitude;
            // multiply value to slow down zoom
            zoom(difference*0.01f);
        }

        // IF NO TOUCHSCREEN 
        zoom(Input.GetAxis("Mouse ScrollWheel"));
        // right click to pan with mouse
        if(Input.GetMouseButtonDown(1)) 
        {
            touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        // check rest of touch, ie the movement
        if(Input.GetMouseButton(1)) 
        {
            Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.position += direction; 
        }
    }

    void zoom(float increment) 
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, zoomOutMin, zoomOutMax);
    }
}
