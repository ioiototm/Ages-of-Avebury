using LoGaCulture.LUTE;
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
        targetLocation.Value = LocationRandomiser.Instance.GetLocationWithID(6);
        lastSeenLocation.Value = LocationRandomiser.Instance.GetLocationWithID(5);


        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order sets up the Scanner";
    }
}