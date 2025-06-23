using LoGaCulture.LUTE;
using System.Collections;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Sets up the panel for decision",
              "Updates all the messages to be read")]
[AddComponentMenu("")]
public class DecisionMedieval : Order
{
    [SerializeField]
    GameObject DecisionPanel;

    IEnumerator wait()
    {
        // Wait for 1 second
        yield return new WaitForSeconds(5f);
        // Continue with the next step

        //hide the DecisionPanel
        DecisionPanel.GetComponentInChildren<SignaturePad>().deleteSignature();

        DecisionPanel.SetActive(false);

        Continue();
    }

    void saveTheStone()
    {

        Debug.Log("Stone saved successfully!");

        StartCoroutine(wait());
    }

    void breakTheStone()
    {
        Debug.Log("Stone broken successfully!");
        StartCoroutine(wait());
    }


    public override void OnEnter()
    {
       var sign= DecisionPanel.GetComponentInChildren<SignaturePad>();

        if (sign != null)
        {
            sign.OnSaveChosen += saveTheStone;
            sign.OnBreakChosen += breakTheStone;
        }
        else
        {
            Debug.LogError("SignaturePad component not found on DecisionPanel.");
        }
        // Set the DecisionPanel active to show it
        DecisionPanel.SetActive(true);
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "Reads all messages";
    }
}