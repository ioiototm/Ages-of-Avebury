using LoGaCulture.LUTE;
using LoGaCulture.LUTE.Logs;
using UnityEngine;

[OrderInfo("AgesOfAvebury",
              "Set the quadrant",
              "Sets the quadrant and sets it up")]
[AddComponentMenu("")]
public class ChoseQuadrant : Order
{

    private enum Quadrant
    {
        North,
        South
    }

    [SerializeField]
    private Quadrant quadrant;
    public override void OnEnter()
    {

        //if (TinySave.loadGame)
        //{
        //    //if we are loading a game, we don't want to change the quadrant
        //    Debug.Log("Loading game, not changing quadrant");
        //    Continue();
        //    return;
        //}



        var northQuadrantM = GetEngine().GetVariable<LocationVariable>("StartingPointNorthMain");
        var southQuadrantM = GetEngine().GetVariable<LocationVariable>("StartingPointSouthMain");
        var northQuadrantS = GetEngine().GetVariable<LocationVariable>("StartingPointNorthSecondary");
        var southQuadrantS = GetEngine().GetVariable<LocationVariable>("StartingPointSouthSecondary");


        //disable them all 

        northQuadrantM.Value.LocationDisabled = true;
        southQuadrantM.Value.LocationDisabled = true;
        northQuadrantS.Value.LocationDisabled = true;
        southQuadrantS.Value.LocationDisabled = true;



        if (quadrant == Quadrant.South)
        {
            //set up the south quadrant
            if (!LocationRandomiser.Instance.southQuadrant)
            {
                LocationRandomiser.Instance.changeQuadrant();
                Debug.Log("Changed to south quadrant");
            }
            else
            {
                Debug.Log("Already in south quadrant");
            }
        }
        else
        {
            //set up the north quadrant
            if (LocationRandomiser.Instance.southQuadrant)
            {
                LocationRandomiser.Instance.changeQuadrant();
                Debug.Log("Changed to north quadrant");
            }
            else
            {
                Debug.Log("Already in north quadrant");
            }

        }

        LogaManager.Instance.LogManager.Log(LogLevel.Info, "Chosen quadrant: " + quadrant.ToString());


        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order sets up the quadrant";
    }
}
