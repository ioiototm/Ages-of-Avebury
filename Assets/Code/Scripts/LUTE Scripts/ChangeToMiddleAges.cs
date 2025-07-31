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


    [SerializeField]
    Character drLangston;


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


        //get the variable dateInApp
        var dateInApp = GetEngine().GetVariable<StringVariable>("dateInApp");
        if (dateInApp != null)
        {
            //get todays date and set it to the variable
            string today = System.DateTime.Now.ToString("dd-MM")+"-1722";
            dateInApp.Value = today;
        }

        //drLangston.SetStandardText("Dr. Langston - " + dateInApp.Value);

        //langstonName.text += " " + dateInApp.Value;

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