using LoGaCulture.LUTE;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "PostGameScanner",
              "Sets up the Post Game Scanner")]
[AddComponentMenu("")]
public class PostGameScanner : Order
{

    [SerializeField]
    HiddenItemScanner scanner;

    [SerializeField]
    GameObject itemToSpawn;

    [SerializeField]
    LocationVariable locationInfo;

    


    [SerializeField]
    DecisionMedieval.StoneType stoneType;


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

        GameObject mapComplete = GameObject.Find("MapComplete");
        MapCompletion mapCompletion = mapComplete.GetComponent<MapCompletion>();

        switch (stoneType)
        {
            case DecisionMedieval.StoneType.Stone1:
                itemToSpawn = mapCompletion.createdStone1;
                break;
            case DecisionMedieval.StoneType.Stone2:
                itemToSpawn = mapCompletion.createdStone2;
                break;
            case DecisionMedieval.StoneType.OtherStone:
                itemToSpawn = mapCompletion.foundStone;
                break;
            default:
                Debug.LogError("Unknown stone type: " + stoneType);
                return;
        }


        scanner.SetItemToSpawn(itemToSpawn);
        scanner.maxPingAmmount = 0;
        scanner.SetIsActive(true);

        ////get the last seen location 
        //BasicFlowEngine engine = GetEngine();
        //var lastSeenLocation = engine.GetVariable<LocationVariable>("LastSeenLocation");
        //scanner.SetLocation(lastSeenLocation.Value);
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order sets up the Scanner";
    }
}