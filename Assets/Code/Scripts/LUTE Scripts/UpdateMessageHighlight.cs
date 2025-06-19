using LoGaCulture.LUTE;
using System.Collections;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Updates the messages to read",
              "Updates all the messages to be read")]
[AddComponentMenu("")]
public class UpdateMessageHighlight : Order
{
    [SerializeField]
    GameObject messageContentPanel;



    public override void OnEnter()
    {
        //go through each child of the messageContentPanel and set the highlight to false
        //each child has an object called Highlight, disable it
        foreach (Transform child in messageContentPanel.transform)
        {
            //find the highlight object and disable it
            Transform highlight = child.Find("Highlight");
            if (highlight != null)
            {
                highlight.gameObject.SetActive(false);
            }
        }

        //To continue or not depending on if this order takes care of everything or diffrent orders take care of the rest
        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "Reads all messages";
    }
}