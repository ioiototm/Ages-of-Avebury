using LoGaCulture.LUTE;
using UnityEngine;

[OrderInfo("AgesOfAvebury",
              "Shows the two quadrants for the user to pick",
              "")]
[AddComponentMenu("")]
public class ShowQuadrantChoice : Order
{
    public override void OnEnter()
    {

        if (TinySave.loadGame)
        {
            //if we are loading a game, we don't want to change the quadrant
            Debug.Log("Loading game, not changing quadrant");
            Continue();
            return;
        }


        var northQuadrant = GetEngine().GetVariable<LocationVariable>("StartingPointNorth");
        var southQuadrant = GetEngine().GetVariable<LocationVariable>("StartingPointSouth");

        if (northQuadrant == null || southQuadrant == null)
        {
            Debug.LogError("Could not find StartingPointNorth or StartingPointSouth variables");
            Continue();
            return;
        }

        GetEngine().GetMapManager().ShowLocationMarker(northQuadrant);
        GetEngine().GetMapManager().ShowLocationMarker(southQuadrant);


        //bool southQuadrant;

        ////50/50 south or north

        //if (Random.Range(0, 2) == 0)
        //{
        //    southQuadrant = true;
        //}
        //else
        //{
        //    southQuadrant = false;
        //}


        //if (southQuadrant)
        //{
        //    //set up the south quadrant
        //    if (!LocationRandomiser.Instance.southQuadrant)
        //    {
        //        LocationRandomiser.Instance.changeQuadrant();
        //        Debug.Log("Changed to south quadrant");
        //    }
        //    else
        //    {
        //        Debug.Log("Already in south quadrant");
        //    }
        //}
        //else
        //{
        //    //set up the north quadrant
        //    if (LocationRandomiser.Instance.southQuadrant)
        //    {
        //        LocationRandomiser.Instance.changeQuadrant();
        //        Debug.Log("Changed to north quadrant");
        //    }
        //    else
        //    {
        //        Debug.Log("Already in north quadrant");
        //    }

        //}
        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order sets up the quadrant choice";
    }
}
