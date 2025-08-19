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


        TinySave.LastNodeSeen = ParentNode._NodeName;
        TinySave.Instance.SaveMessages();
        TinySave.Instance.SaveMessagesMedieval();
        TinySave.Instance.SaveEngineVariables();
        

        TinySave.Instance.Save();

        Debug.Log("Last node seen set to: " + TinySave.LastNodeSeen);

        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "";
    }


    //on destroy, go through all of the locations, and set them to not visited
    private void OnDestroy()
    {
        //go through all of the locations and set them to not visited

        //get all the locations from the resources
        var locations = Resources.LoadAll<LUTELocationInfo>("Locations");

        foreach (var location in locations)
        {
            //set the status to unvisited
            location.LocationStatus = LocationStatus.Unvisited;
        }

    }
}