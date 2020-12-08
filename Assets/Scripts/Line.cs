using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class LayerOrder 
{
    static int order = 0;
    public static int Order
    {
        get
        {
            return order++;
        }
    }
}

[System.Serializable]
public class Line : MonoBehaviour
{
    // reference to linerenderer
    public LineRenderer lineRenderer;

    // store points in line 
    List<Vector2> points;

    // instantiate new SaveLine Class as well 
    SaveLine saveLine = new SaveLine();

    // either start new line or continue drawing line
    public void UpdateLine (Vector2 mousePos) {
        if (points == null){
            points = new List<Vector2>();
            lineRenderer.sortingOrder = LayerOrder.Order;
            SetPoint(mousePos);
            return;
        }

        // Check if mouse has moved enough for us to insert new point
        // if it has, insert point at mouse/touch position?
        // Update: .1f -> .001f to make lines smoother
        if (Vector2.Distance(points.Last(), mousePos) > .001f){
            SetPoint(mousePos);
        }
    }

    public void SetPoint(Vector2 point){
        points.Add(point);
        SerializableVector2 serVec2Point = new SerializableVector2(point);
        saveLine.points.Add(serVec2Point);

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPosition(points.Count - 1, point);

    }

    // for instantiating points before loading
    public void CreateLineBeforeLoadPoint()
    {        
        if(points == null)
        {
            points = new List<Vector2>();
        }
    }

    public void SetColor(Color color) {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        saveLine.serColor = new SerializableColor(color);
    }

    public void SetWidth(float width) {
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        saveLine.width = width;
    }

    public void SetTexture(string texture) 
    {
        string url = "Materials/" + texture;
        var mat = Resources.Load<Material>(url);
        lineRenderer.material =  mat;
        saveLine.texture = texture;
    }

    public void SaveLineObject()
    {
        SaveManager.listSavedLines.Add(saveLine);
    }
}


[System.Serializable]
public class SaveLine 
{
    public List<SerializableVector2> points = new List<SerializableVector2>();
    public SerializableColor serColor;
    public float width;
    public string texture;

}

// from https://answers.unity.com/questions/1645980/cant-serialize-color-or-vector2.html 
[System.Serializable]
public class SerializableColor
{
    public float _r;
    public float _g;
    public float _b;
    public float _a;

    public Color Color
    {
        get
        {
            return new Color(_r, _g, _b, _a);
        }
        set
        {
            _r = value.r;
            _g = value.g;
            _b = value.b;
            _a = value.a;
        }
    }

    public SerializableColor(Color color)
    {
        _r = color.r;
        _g = color.g;
        _b = color.b;
        _a = color.a;
    }
}

[System.Serializable]
public class SerializableVector2
{
    public float x;
    public float y;

    public SerializableVector2(Vector2 vec)
    {
        x = vec.x;
        y = vec.y;
    }
}
