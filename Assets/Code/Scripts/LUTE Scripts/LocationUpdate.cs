using LoGaCulture.LUTE;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Location Update",
              "Updates the locations needed")]
[AddComponentMenu("")]
public class LocationUpdate : Order
{

    bool alreadyExecuted = false;

    public override void OnEnter()
    {
        if (alreadyExecuted)
        {
            Continue();
            return;
        }

        alreadyExecuted = true;
        if (ParentNode._EventHandler != null)
        {
            //if it's of type locaitonclickeventhandler
            if (ParentNode._EventHandler is LocationClickEventHandler locationClickEventHandler)
            {

                //if lastseenlocation is null, get it from the engine
                if (LocationRandomiser.Instance.lastSeenLocation == null)
                {
                    LocationRandomiser.Instance.lastSeenLocation = GetEngine().GetVariable<LocationVariable>("lastSeenLocation");
                }

                LocationRandomiser.Instance.lastSeenLocation.Value = locationClickEventHandler.Location.Value;

                var nextLoc = LocationRandomiser.Instance.GetNextNormalLocation();

                //set the targetlocation 
                LocationRandomiser.Instance.targetLocation.Value = nextLoc;


                //print the last seen and target location
                Debug.Log("Last seen location: " + LocationRandomiser.Instance.lastSeenLocation.Value.name);
                Debug.Log("Target location: " + LocationRandomiser.Instance.targetLocation.Value.name);

                //get the location data
                //LocationData locationData = locationClickEventHandler.Location;
                //Debug.Log("Location from node is: " + locationData.Value.name);
            }
        }


        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "";
    }
}