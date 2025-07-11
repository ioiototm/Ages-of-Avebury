using LoGaCulture.LUTE;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Change the app to the middle time",
              "Disables stone and enables middle interface")]
[AddComponentMenu("")]
public class ChangeToMiddleAges : Order
{

    [SerializeField]
    GameObject neolithicInterface;
    [SerializeField]
    GameObject middleAgesInterface;

    [SerializeField]
    InterfaceGameEvent middleAgesInterfaceEvent;


    public override void OnEnter()
    {

        // Disable the modern interface
        if (neolithicInterface != null)
        {
            neolithicInterface.SetActive(false);
        }
        // Enable the neolithic interface
        if (middleAgesInterface != null)
        {
            middleAgesInterface.SetActive(true);
        }

        // Trigger the neolithic interface event
        if (middleAgesInterfaceEvent != null)
        {
            middleAgesInterfaceEvent.Raise();
        }

        XRManager.Instance.SetXRActive(false);

        Compass compass = GameObject.Find("Compass Test").GetComponent<Compass>();
        compass.timePeriod = Compass.TimePeriod.MiddleAges;

        //get the TargetLocation locationvariable form the engine

        //var engine = GetEngine();
        //var targetLocation = engine.GetVariable<LocationVariable>("TargetLocation");

        //set this to location 8.1, north or south depending on the quadrant

        LocationRandomiser.Instance.SetLastSeenAndTargetToFirstNPC();

        //if(LocationRandomiser.Instance.southQuadrant)

        //GameObject.Find("Copper Pipe(Clone)").SetActive(false);
        //GameObject.Find("Pit(Clone)").SetActive(false);
        //GameObject.Find("Stone 7_LP(Clone)").SetActive(false);
        //GameObject.Find("Stone III_LP(Clone)").SetActive(false);


        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order sets up the Scanner";
    }
}