using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;   // 🔹 (namespace for UILineRenderer)

public class SignaturePad : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Visuals")]
    public UILineRenderer linePrefab;          // 🔹 drag InkStroke prefab here
    public float minStrokeLength = 50f;        // pixels

    [Header("Stone Shape")]
    public UILineRenderer stoneShapeObject;

    [Header("Events")]
    [SerializeField]
    public System.Action OnSaveChosen;
    public System.Action OnBreakChosen;

    RectTransform rt;
    UILineRenderer currentLine;            // 🔹
    List<Vector2> pts = new List<Vector2>();
    Vector2 firstScreenPos;
    float strokeLen;

    public bool alreadyChosen = false; // to prevent multiple calls

    void Awake() => rt = GetComponent<RectTransform>();



    public void addToTheVisited()
    {
        //get the visitedNPCs variable from the flow engine
        var visitedNPCs = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>().GetVariable<IntegerVariable>("visitedNPCs");

        //50% chance to increment by one

        if (Random.Range(0f, 1f) < 0.5f)
        {
            visitedNPCs.Value++;
            Debug.Log("Visited NPCs incremented to: " + visitedNPCs.Value);
        }
        else
        {
            Debug.Log("Visited NPCs not incremented.");
        }

    }

    public void OnPointerDown(PointerEventData ev)
    {
        if (alreadyChosen) return; // prevent multiple calls
        // new stroke
        currentLine = Instantiate(linePrefab, transform);   // 🔹 child of DrawingSurface
        currentLine.Points = new Vector2[0];
        pts.Clear();

        firstScreenPos = ev.position;
        strokeLen = 0f;
        AddPoint(ev);
    }

    public void OnDrag(PointerEventData ev)
    {
        if (alreadyChosen) return; // prevent multiple calls
        // add point to current stroke
        AddPoint(ev);

    }

    public void deleteSignature()
    {
        if (currentLine != null)
        {
            Destroy(currentLine.gameObject);
            currentLine = null;
            pts.Clear();
            strokeLen = 0f;
        }
    }

   
    public void OnPointerUp(PointerEventData ev)
    {

        if (alreadyChosen) return;
        if (strokeLen < minStrokeLength)
        {
            Destroy(currentLine.gameObject);
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt, firstScreenPos, ev.pressEventCamera, out var localStart);

        // decide side (left / right of centre)
        if (localStart.x < 0)
        {
            OnSaveChosen?.Invoke();
           
        }
        else
            OnBreakChosen?.Invoke();

        alreadyChosen = true; // prevent multiple calls


    }

    // ───────────────────────────────────────────────
    void AddPoint(PointerEventData ev)
    {
        if (currentLine == null) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rt, ev.position, ev.pressEventCamera, out var local))
            return;

        pts.Add(local);

        // update renderer points
        currentLine.Points = pts.ToArray();    // 🔹 copies list
        currentLine.SetAllDirty();             // 🔹 force redraw

        if (pts.Count > 1)
            strokeLen += Vector2.Distance(pts[^2], pts[^1]);
    }
}
