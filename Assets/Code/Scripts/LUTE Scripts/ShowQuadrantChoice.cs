using LoGaCulture.LUTE;
using System.Linq;
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


        Debug.Log("Showing quadrant choice");

        var northQuadrantM = GetEngine().GetVariable<LocationVariable>("StartingPointNorthMain");
        var southQuadrantM = GetEngine().GetVariable<LocationVariable>("StartingPointSouthMain");
        var northQuadrantS = GetEngine().GetVariable<LocationVariable>("StartingPointNorthSecondary");
        var southQuadrantS = GetEngine().GetVariable<LocationVariable>("StartingPointSouthSecondary");

        if (northQuadrantM == null || southQuadrantM == null || northQuadrantS == null)
        {
            Debug.LogError("Could not find StartingPointNorthMain, StartingPointSouthMain, StartingPointNorthSecondary or StartingPointSouthSecondary variable");
            //Continue();
            //return;
        }

        //enable them all
        northQuadrantM.Value.LocationDisabled = false;
        southQuadrantM.Value.LocationDisabled = false;
        northQuadrantS.Value.LocationDisabled = false;
        southQuadrantS.Value.LocationDisabled = false;

        var node = GetEngine().FindNode(ParentNode._NodeName);
        var lastOrder = node.OrderList.Last<Order>();


        if (LocationRandomiser.Instance.debugMode)
        {

            Debug.Log("Debug mode - showing all quadrants");

            //GetEngine().GetMapManager().ShowLocationMarker(northQuadrantM);




            
        }
        else
        {

            Debug.Log("Normal mode - showing all quadrants");

            if (lastOrder is NextNode nextOrder)
            {

                lastOrder.enabled = false;

            }

            GetEngine().GetMapManager().ShowLocationMarker(northQuadrantM);
            GetEngine().GetMapManager().ShowLocationMarker(southQuadrantM);
            GetEngine().GetMapManager().ShowLocationMarker(northQuadrantS);
            GetEngine().GetMapManager().ShowLocationMarker(southQuadrantS);

        }


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
