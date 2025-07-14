using System.Collections;
using UnityEngine;

[OrderInfo("AgesOfAvebury",
              "Update Map to location",
              "Makes it so the map actively centers around the player every second")]
[AddComponentMenu("")]
public class MapUpdater : Order
{

    public override void OnEnter()
    {
       
        StartCoroutine(waitAbit());
        Continue(); 


    }

    IEnumerator waitAbit()
    {
               // Wait for 1 second
        yield return new WaitForSeconds(1f);
        InitialiseEverything.activelyCentering = true;
    }

    public override string GetSummary()
    {
        // Return a summary of the order which is displayed in the inspector of the order
        return "Makes it so the map actively centers around the player every second";
    }

}