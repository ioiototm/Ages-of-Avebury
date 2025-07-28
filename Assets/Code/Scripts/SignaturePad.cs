using LoGaCulture.LUTE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;   // (namespace for UILineRenderer)

public class SignaturePad : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Visuals")]
    public UILineRenderer linePrefab;          // drag InkStroke prefab here
    public float minStrokeLength = 50f;        // pixels
    public float signatureFinalizeTime = 1f;   // seconds

    [Header("Stone Shape")]
    public UILineRenderer stoneShapeObject;

    [Header("Events")]
    [SerializeField]
    public System.Action OnSaveChosen;
    public System.Action OnBreakChosen;

    RectTransform rt;
    UILineRenderer currentLine;
    List<Vector2> pts = new List<Vector2>();
    Vector2 firstScreenPos;
    float totalStrokeLength;

    public bool alreadyChosen = false; // to prevent multiple calls

    private List<UILineRenderer> allLines = new List<UILineRenderer>();
    private Coroutine finalizeCoroutine;
    private bool isSigning = false;
    private Camera pressCamera;

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

        if (finalizeCoroutine != null)
        {
            StopCoroutine(finalizeCoroutine);
            finalizeCoroutine = null;
        }

        // new stroke
        currentLine = Instantiate(linePrefab, transform);   // 🔹 child of DrawingSurface
        allLines.Add(currentLine);
        currentLine.Points = new Vector2[0];
        pts.Clear();

        if (!isSigning)
        {
            firstScreenPos = ev.position;
            pressCamera = ev.pressEventCamera;
            isSigning = true;
        }

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
        foreach (var line in allLines)
        {
            if (line != null)
            {
                Destroy(line.gameObject);
            }
        }
        allLines.Clear();

        if (currentLine != null)
        {
            Destroy(currentLine.gameObject);
            currentLine = null;
        }

        pts.Clear();
        totalStrokeLength = 0f;
        isSigning = false;
        if (finalizeCoroutine != null)
        {
            StopCoroutine(finalizeCoroutine);
            finalizeCoroutine = null;
        }
    }


    public void OnPointerUp(PointerEventData ev)
    {
        if (alreadyChosen) return;

        finalizeCoroutine = StartCoroutine(FinalizeSignature());
    }

    private IEnumerator FinalizeSignature()
    {
        yield return new WaitForSeconds(signatureFinalizeTime);

        if (totalStrokeLength < minStrokeLength)
        {
            deleteSignature();
            yield break;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt, firstScreenPos, pressCamera, out var localStart);



        // decide side (left / right of centre)
        if (localStart.x < 0)
        {
            OnSaveChosen?.Invoke();


        }
        else
            OnBreakChosen?.Invoke();

        alreadyChosen = true; // prevent multiple calls
        isSigning = false;
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
        currentLine.Points = pts.ToArray();    // copies list
        currentLine.SetAllDirty();             // force redraw

        if (pts.Count > 1)
            totalStrokeLength += Vector2.Distance(pts[^2], pts[^1]);
    }
}