using LoGaCulture.LUTE;
using System.Linq;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Change the app to the neolithic time",
              "Disables modern interface anbd enables stone")]
[AddComponentMenu("")]
public class ChangeToNeolithic : Order
{

    [SerializeField]
    GameObject modernInterface;
    [SerializeField]
    GameObject neolithicInterface;

    [SerializeField]
    InterfaceGameEvent neolithicInterfaceEvent;

    public static bool skipToLoadedNode = false;


    public override void OnEnter()
    {

        // Disable the modern interface
        if (modernInterface != null)
        {
            modernInterface.SetActive(false);
        }
        // Enable the neolithic interface
        if (neolithicInterface != null)
        {
            neolithicInterface.SetActive(true);
        }

        // Trigger the neolithic interface event
        if (neolithicInterfaceEvent != null)
        {
            neolithicInterfaceEvent.Raise();
        }

        XRManager.Instance.SetXRActive(false);

        try
        {
            GameObject.Find("Copper Pipe(Clone)").SetActive(false);
            GameObject.Find("Pit(Clone)").SetActive(false);
            GameObject.Find("Stone 7_LP(Clone)").SetActive(false);
            GameObject.Find("Stone III_LP(Clone)").SetActive(false);

        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not find objects to disable: " + e.Message);
        }


        LUTEMapManager mapManager = GetEngine().GetMapManager();

        LUTELocationInfo[] locationInfos = Resources.LoadAll<LUTELocationInfo>("Locations");

        LocationVariable targetLocation = GetEngine().GetVariable<LocationVariable>("TargetLocation");
        LocationVariable lastSeenLocation = GetEngine().GetVariable<LocationVariable>("LastSeenLocation");

        //set the target location to the location that has 6 as the ID
        targetLocation.Value = LocationRandomiser.Instance.GetLocationWithID(6,true);
        GetEngine().GetVariable<LocationVariable>("FirstStoneCreation6").Value = targetLocation.Value;

        var portalLoc = GetEngine().GetVariable<LocationVariable>("Portal5");

        lastSeenLocation.Value = portalLoc.Value;

        portalLoc.Value.LocationStatus = LocationStatus.Completed;



        Compass compass = GameObject.Find("Compass Test").GetComponent<Compass>();
        compass.timePeriod = Compass.TimePeriod.Neolithic;


        if(skipToLoadedNode)
        {
            //get the last order from the node this order is in
            var node = GetEngine().FindNode(ParentNode._NodeName);
            var lastOrder = node.OrderList.Last<Order>();
            var lastNodeSeen = GetEngine().FindNode(TinySave.LastNodeSeen);

            if (lastOrder is NextNode nextOrder)
            {
                nextOrder.targetNode = lastNodeSeen;
            }

        }
        else
        {
            TinySave.LastNodeSeen = ParentNode._NodeName;
        }


            Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order sets up the Scanner";
    }
}