using LoGaCulture.LUTE;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;


[OrderInfo("AgesOfAvebury",
              "Sets up the panel for decision",
              "Updates all the messages to be read")]
[AddComponentMenu("")]
public class DecisionMedieval : Order
{
    [SerializeField]
    GameObject DecisionPanel;



    [SerializeField]
    GameObject stone;

    [SerializeField]
    GameObject stoneShapeUI;

    [SerializeField]
    Material stoneSketchMaterial;

    [SerializeField]
    RenderTexture rt;

    private GameObject instantiatedStone;

    private void sendLogToServer()
    {

        //if visit number is one, make it "first", 2 is "second", 3 is "third"
        string visitNumberText = visitNumber switch
        {
            1 => "first",
            2 => "second",
            3 => "third",
            _ => $"{visitNumber}th"
        };

        visitNumber++;

        string decisionText = decision.Save ? "save" : "break";

        LogaManager.Instance.LogManager.Log(LoGaCulture.LUTE.Logs.LogLevel.Info, "On their " + visitNumberText + " visit, the player signed to " + decisionText + " stone " + decision.Type.ToString());
    }


    //enum to be either stone1, stone2, otherStone, bakery, cottage, church
    public enum StoneType
    {
        Stone1,
        Stone2,
        OtherStone
    }


    //a class for a double, stonetype and bool save or not
    public class StoneDecision
    {
        public StoneType Type { get; set; }
        public bool Save { get; set; }
        public StoneDecision(StoneType type, bool save)
        {
            Type = type;
            Save = save;
        }
    }

    [SerializeField] StoneType stoneType = StoneType.Stone1;

    [SerializeField]
    StoneDecision decision;


    private static int visitNumber = 1;



    IEnumerator wait()
    {
        // Wait for 1 second
        yield return new WaitForSeconds(5f);
        // Continue with the next step

        //remove the saveTheStone and breakTheStone listeners
        var sign = DecisionPanel.GetComponentInChildren<SignaturePad>();
        if (sign != null)
        {
            sign.OnSaveChosen -= saveTheStone;
            sign.OnBreakChosen -= breakTheStone;
        }
        else
        {
            Debug.LogError("SignaturePad component not found on DecisionPanel.");
        }

        //hide the DecisionPanel
        sign.deleteSignature();

        sign.alreadyChosen = false;

        // Destroy the instantiated stone
        if (instantiatedStone != null)
        {
            Destroy(instantiatedStone);
        }
        else
        {
            Debug.LogWarning("No instantiated stone to destroy.");
        }

        //destroy the camera
        GameObject camGO = GameObject.Find("StonePreviewCam");
        if (camGO != null)
        {
            Destroy(camGO);
        }
        else
        {
            Debug.LogWarning("No camera to destroy.");
        }

        DecisionPanel.SetActive(false);

        Continue();
    }

    void saveTheStone()
    {


        decision.Save = true;
        sendLogToServer();
        Debug.Log("Stone saved successfully!");

        MapCompletion.decisions.Add(decision);


        var savedStones = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>().GetVariable<IntegerVariable>("SavedStones");
        savedStones.Value++;



        StartCoroutine(wait());
    }

    void breakTheStone()
    {
        decision.Save = false;
        sendLogToServer();
        Debug.Log("Stone broken successfully!");

        MapCompletion.decisions.Add(decision);

        StartCoroutine(wait());
    }


    public override void OnEnter()
    {
        var sign = DecisionPanel.GetComponentInChildren<SignaturePad>();

        if (sign != null)
        {
            sign.OnSaveChosen += saveTheStone;
            sign.OnBreakChosen += breakTheStone;
        }
        else
        {
            Debug.LogError("SignaturePad component not found on DecisionPanel.");

        }



        //instantiate a copy of the stone prefab
        if (stone == null)
        {
            Debug.LogError("Stone prefab is not assigned in the inspector.");
            //return;
        }
        instantiatedStone = Instantiate(stone, Vector3.zero, Quaternion.identity);

        instantiatedStone.layer = LayerMask.NameToLayer("StonePreview");

        instantiatedStone.SetActive(true);

        //set the material of the instantiated stone
        if (instantiatedStone.TryGetComponent<Renderer>(out Renderer renderer))
        {
            renderer.material = stoneSketchMaterial;
        }
        else
        {
            Debug.LogError("Renderer component not found on the instantiated stone.");
        }

        GameObject camGO = new GameObject("StonePreviewCam");
        Camera cam = camGO.AddComponent<Camera>();

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear;          // transparent
        cam.orthographic = true;                 // or perspective, your call
        cam.cullingMask = 1 << LayerMask.NameToLayer("StonePreview");
        cam.enabled = false;                // only on when pop-up visible

        cam.targetTexture = rt; // Assign the RenderTexture



        Bounds b = instantiatedStone.GetComponent<Renderer>().bounds;
        camGO.transform.position = b.center + new Vector3(0, 0, -b.extents.magnitude * 2);
        camGO.transform.LookAt(b.center);

        cam.enabled = true;          // start rendering


        //var outlinePoints = stone.GetComponent<StoneCreator>().outlinePoints;

        ////scale them by 5
        //for (int i = 0; i < outlinePoints.Count; i++)
        //{
        //    outlinePoints[i] *= 20f;
        //}

        //stoneShapeUI.GetComponent<UILineRenderer>().Points = outlinePoints.ToArray();

        decision = new StoneDecision(stoneType, false);

        // Set the DecisionPanel active to show it
        DecisionPanel.SetActive(true);
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "Sets up everything for the decision panel";
    }
}