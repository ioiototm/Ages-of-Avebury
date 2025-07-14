using LoGaCulture.LUTE;
using System.Collections;
using UnityEngine;
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

        DecisionPanel.SetActive(false);

        Continue();
    }

    void saveTheStone()
    {

        Debug.Log("Stone saved successfully!");
        decision.Save = true;

        var savedStones = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>().GetVariable<IntegerVariable>("SavedStones");
        savedStones.Value++;

        

        StartCoroutine(wait());
    }

    void breakTheStone()
    {
        Debug.Log("Stone broken successfully!");
        decision.Save = false;

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



        var outlinePoints = stone.GetComponent<StoneCreator>().outlinePoints;

        //scale them by 5
        for (int i = 0; i < outlinePoints.Count; i++)
        {
            outlinePoints[i] *= 20f;
        }

        stoneShapeUI.GetComponent<UILineRenderer>().Points = outlinePoints.ToArray();

        decision = new StoneDecision(stoneType, false);

        // Set the DecisionPanel active to show it
        DecisionPanel.SetActive(true);
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "Reads all messages";
    }
}