using LoGaCulture.LUTE;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Scanner",
              "Sets up the Scanner")]
[AddComponentMenu("")]
public class Scanner : Order
{

    [SerializeField]
    HiddenItemScanner scanner;

    [SerializeField]
    GameObject itemToSpawn;

    [SerializeField]
    LocationVariable locationInfo;


    public void OnItemDiscovered(GameObject gameObject)
    {
        // Handle the discovered item here
        Debug.Log("Item Discovered: " + gameObject.name);
        scanner.SetIsActive(false);
        scanner.OnItemDiscovered -= OnItemDiscovered;

       GetEngine().GetVariable<BooleanVariable>("toClearObject").Value = true;



        Continue();
    }

    public override void OnEnter()
    {
        scanner.OnItemDiscovered += OnItemDiscovered;
        scanner.SetItemToSpawn(itemToSpawn);
        scanner.SetIsActive(true);

        //get the last seen location 
        BasicFlowEngine engine = GetEngine();
        var lastSeenLocation = engine.GetVariable<LocationVariable>("LastSeenLocation");
        scanner.SetLocation(lastSeenLocation.Value);
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order sets up the Scanner";
    }
}