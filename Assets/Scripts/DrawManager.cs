using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DrawManager : MonoBehaviour
{
    // reference to the line prefab
    public GameObject linePrefab;

    public GameObject targetRectArea;
    public GameObject headerRectArea;

    public GameObject RenamePanel;
    public GameObject ColorPanel;
    public GameObject BrushPanel;

    public Text titleText;

    // reference to save manager
    public SaveManager sm;

    // keeping track of the line gameobjects for undo/redo
    Stack<GameObject> goStack = new Stack<GameObject>();

    Line activeLine;
    bool EraserUsed = false;

    // Canvas
    [SerializeField] private string m_PaintName;
    // Color
    [SerializeField] private Color m_CurrentColor;
    [SerializeField] private Image m_CurrentColorButtonImg;
    // Brush
    [SerializeField] private float m_BrushSize;
    [SerializeField] private float m_BrushOpacity;
    [SerializeField] private string m_BrushTexture;
    // Eraser
    [SerializeField] private Color m_SavedColorBeforeEraser;
    [SerializeField] private float m_SavedSizeBeforeEraser;
    [SerializeField] private float m_SavedBrushOpacityBeforeEraser;
    [SerializeField] private string m_SavedBrushTextureBeforeEraser;
    [SerializeField] private float m_EraserSize;

    void Start()
    {
        m_PaintName = "Untitled";
        m_CurrentColorButtonImg.color = Color.red;
        m_CurrentColor = Color.red;
        m_BrushSize = 0.008F;
        m_BrushTexture = "Basic";
        m_BrushOpacity = 1.00F;

        m_SavedColorBeforeEraser = m_CurrentColor;
        m_SavedSizeBeforeEraser = m_BrushSize;
        m_SavedBrushTextureBeforeEraser = m_BrushTexture;
        m_SavedBrushOpacityBeforeEraser = m_BrushOpacity;

        // default is some random string lol
        string drawingId = PlayerPrefs.GetString("DRAWING_ID", "testUser-1605385650936673515-1605512325144110671");
        Debug.Log("Now displaying drawing: " + drawingId);

        // clear out undolines list
        SaveManager.undoneLines.Clear();

        // load in drawing, if necessary
        int isUsingLoadedDrawing = PlayerPrefs.GetInt("IS_USING_LOADED_DRAWING", 0);
        string dataURL = PlayerPrefs.GetString("CURRENT_LOADED_DRAWING_DATA_URL", "");
        if(isUsingLoadedDrawing == 1)
        {
            PlayerPrefs.SetInt("IS_USING_LOADED_DRAWING", 0);
            Debug.Log("Is using loaded drawing, so loading starting");
            m_PaintName = PlayerPrefs.GetString("CURRENT_ARTWORK_TITLE", "ahhhhhh");
            titleText.text = m_PaintName;
            Debug.Log("PAINTING NAME: " + m_PaintName);
            sm.Load(dataURL);
        }
        else
        {
            Debug.Log("Is using new drawing");
            // reset list of lines
            SaveManager.listSavedLines.Clear();

            // immediately save
            sm.FirstAutoSave();
        }
    }

    void Update ()
    {
        bool hasPanelOpen = RenamePanel.activeSelf || ColorPanel.activeSelf || BrushPanel.activeSelf;
        if (Input.GetMouseButton(0) && 
            RectTransformUtility.RectangleContainsScreenPoint(
                headerRectArea.GetComponent<RectTransform>(), 
                Input.mousePosition, 
                Camera.main))
                {
                    return;
                }

        // // // // if (Input.GetMouseButtonDown(0) && !hasPanelOpen){
        // // // //     // see if within rectangle
        // // // //     Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // // // //     bool onRect = targetRectArea.GetComponent<RectTransform>().rect.Contains(mousePos);

        // // // //     bool onHeaderRect = headerRectArea.GetComponent<RectTransform>().rect.Contains(mousePos);
        // // // //     Debug.Log("current mouse position: " + mousePos);
        // // // //     Debug.Log("on header? " + onHeaderRect);


        // // // //     if(!onRect)
        // // // //     {
        // // // //         return;
        // // // //     }

        // // // //     // Lines don't draw when cliking on UIs
        // // // //     Debug.Log("Over gameobject? " + EventSystem.current.IsPointerOverGameObject());
        // // // //     if (EventSystem.current.IsPointerOverGameObject())
        // // // //     {
        // // // //         return;
        // // // //     }

        // // // //     GameObject lineGameObj = Instantiate(linePrefab);
        // // // //     activeLine = lineGameObj.GetComponent<Line>();
        // // // //     goStack.Push(lineGameObj);

        // // // //     activeLine.SetColor(m_CurrentColor);
        // // // //     activeLine.SetWidth(m_BrushSize);
        // // // //     activeLine.SetTexture(m_BrushTexture);
        // // // //     // Debug.Log(m_CurrentColor);
        // // // // }

        // // // // if (Input.GetMouseButtonUp(0)) {
        // // // //     // add line to save list of saved lines
        // // // //     if(activeLine != null){
        // // // //         activeLine.SaveLineObject();
        // // // //         activeLine = null;
        // // // //     }
        // // // // }

        // // // // if(activeLine != null) {
        // // // //     // see if within rectangle
        // // // //     Vector2 mousePos1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // // // //     bool onRect = targetRectArea.GetComponent<RectTransform>().rect.Contains(mousePos1);

        // // // //     if(!onRect)
        // // // //     {
        // // // //         return;
        // // // //     }

        // // // //     Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // // // //     activeLine.UpdateLine(mousePos);
        // // // // }

        // if (Input.touchCount == 1 && Input.GetMouseButtonDown(0) && !hasPanelOpen){
        if(Input.touchCount == 1)
        {
            if (Input.touches[0].phase == TouchPhase.Began && !hasPanelOpen){
                // see if within rectangle
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                bool onRect = targetRectArea.GetComponent<RectTransform>().rect.Contains(mousePos);

                if(!onRect)
                {
                    return;
                }

                // Lines don't draw when cliking on UIs
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                GameObject lineGameObj = Instantiate(linePrefab);
                activeLine = lineGameObj.GetComponent<Line>();
                goStack.Push(lineGameObj);

                activeLine.SetColor(m_CurrentColor);
                activeLine.SetWidth(m_BrushSize);
                activeLine.SetTexture(m_BrushTexture);
                // Debug.Log(m_CurrentColor);
            }

            // if (Input.GetMouseButtonUp(0)) {
            if (Input.touches[0].phase == TouchPhase.Ended) {
                // add line to save list of saved lines
                if(activeLine != null){
                    activeLine.SaveLineObject();
                    activeLine = null;
                }
            }

            if(activeLine != null) {
                // see if within rectangle
                Vector2 mousePos1 = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                bool onRect = targetRectArea.GetComponent<RectTransform>().rect.Contains(mousePos1);

                if(!onRect)
                {
                    return;
                }

                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                activeLine.UpdateLine(mousePos);
            }
        }
    }

    public void Redo()
    {
        Debug.Log("Calling Redo");
        // Check that there is at least one line in undoneLines to redo
        if(SaveManager.undoneLines.Count > 0)
        {
            // take off last item in undoneLines
            SaveLine line = SaveManager.undoneLines[SaveManager.undoneLines.Count - 1];
            SaveManager.undoneLines.RemoveAt(SaveManager.undoneLines.Count - 1);

            // add to listSavedLines
            SaveManager.listSavedLines.Add(line);

            // create line and load in the stroke 
            GameObject lineGameObj = Instantiate(linePrefab);
            Line currentLine = lineGameObj.GetComponent<Line>();
            // update line order
            currentLine.lineRenderer.sortingOrder = LayerOrder.Order;
            goStack.Push(lineGameObj);

            // get the saved line data
            Color currentSavedColor = new Color(line.serColor._r, 
                                                line.serColor._g, 
                                                line.serColor._b, 
                                                line.serColor._a);
            currentLine.SetColor(currentSavedColor);
            currentLine.SetWidth(line.width);
            currentLine.SetTexture(line.texture);
            // setting the vector2 data
            currentLine.CreateLineBeforeLoadPoint();
            foreach(SerializableVector2 point in line.points)
            {
                Vector2 tempPoint = new Vector2(point.x, point.y);
                currentLine.SetPoint(tempPoint);
            }

            // done?
        }
        else {
            Debug.Log("No lines to redo!!!");
        }
        sm.PrintContentsExternal();
    }

    public void Undo()
    {
        Debug.Log("Calling Undo");
        // check that there is at least one line in goStack to undo
        if(goStack.Count > 0)
        {
            Debug.Log("Stack Count: " + goStack.Count);
            Debug.Log("List Count: " + SaveManager.listSavedLines.Count);
            // take off last item in listSavedLines
            SaveLine line = SaveManager.listSavedLines[SaveManager.listSavedLines.Count - 1];
            SaveManager.listSavedLines.RemoveAt(SaveManager.listSavedLines.Count - 1);

            // add to undoneLines
            SaveManager.undoneLines.Add(line);
            
            // get the game object and destroy it 
            GameObject destroyTarget = goStack.Pop();
            GameObject.Destroy(destroyTarget);
        }
        else 
        {
            Debug.Log("No lines to undo!!!");
        }
        // sm.PrintContentsExternal();
    }

    #region Private Methods
    private void SetBrushColor(Color color)
    {
        m_CurrentColor = color;
        m_CurrentColor.a = m_BrushOpacity;
        m_CurrentColorButtonImg.color = m_CurrentColor;
    }

    private void SetBrushSize(float size)
    {
        //Remapping size values
        // Slider has a value of 0-1, LineRenderer uses percents as width units
        // size = (size - minValue) / (maxValue - minValue);
        // size = (m_MaxBrushSize - m_MinBrushSize) * size + m_MinBrushSize;
        m_BrushSize = size;
    }

    private void SetBrushOpacity(float opacity)
    {
        //Remapping opacity values
        // Slider: 0-1 range, LineRenderer color alpha: 0-1 range
        m_BrushOpacity = opacity;
        // Alpha component of the color (0 is transparent, 1 is opaque).
        m_CurrentColor.a = opacity;
    }

    private void SetBrushTexture(string tex) 
    {
        m_BrushTexture = tex;
    }
    #endregion

    #region Public Methods

    public void RenamePainting(Text name)
    {
        m_PaintName = name.text;
        titleText.text = m_PaintName;
    }

    public string GetPaintingName()
    {
        return m_PaintName;
    }

    public void SelectColor(Color color)
    {
        SetBrushColor(color);
    }

    public Color GetColor()
    {
        return m_CurrentColor;
    }

    public void ChangeBrushSize(Slider slider)
    {
        // slider.minValue, slider.maxValue = (0, 1)
        // which is consistent with Linerenderer's width range
        SetBrushSize(slider.value);
    }

    public float GetBrushSize()
    {
        return m_BrushSize;
    }

    public void SelectTexture(string BrushTitle) {
        SetBrushTexture(BrushTitle);
    }

    public string GetTexture()
    {
        return m_BrushTexture;
    }

    public void ChangeBrushOpacity(Slider slider)
    {
        SetBrushOpacity(slider.value);
    }

    public float GetBrushOpacity()
    {
        return m_BrushOpacity;
    }

    public void SelectEraser ()
    {
        // save previously used color,size,opacity,texture to go back to if user toggles set pencil after
        m_SavedColorBeforeEraser = m_CurrentColor;
        m_SavedSizeBeforeEraser = m_BrushSize;
        m_SavedBrushTextureBeforeEraser = m_BrushTexture;
        m_SavedBrushOpacityBeforeEraser = m_BrushOpacity;
        Debug.Log("size" + m_SavedSizeBeforeEraser);

        // set color to white (or same as canvas)
        m_CurrentColor = Color.white;
        SetBrushSize(1);
        SetBrushOpacity(1);
        SetBrushTexture("Eraser");
        EraserUsed = true;
    }

    public void ChangeEraserSize ()
    {

    }

    public void SelectPencilOrColorAfterEraser ()
    {
        if(EraserUsed) 
        {
            SetBrushColor(m_SavedColorBeforeEraser);
            SetBrushSize(m_SavedSizeBeforeEraser);
            SetBrushTexture(m_SavedBrushTextureBeforeEraser);
            SetBrushOpacity(m_SavedBrushOpacityBeforeEraser);
        }
        EraserUsed = false;
    }

    #endregion
}
