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

        var decisions = MapCompletion.decisions;
        var toSave = false;


        switch (stoneType)
        {
            case DecisionMedieval.StoneType.Stone1:

                foreach (var decision in decisions)
                {
                    if (decision.Type == DecisionMedieval.StoneType.Stone1)
                    {
                        toSave = decision.Save;
                        if(decision.Save)
                        {
                            itemToSpawn = mapCompletion.createdStone1;
                        }
                        else
                        {
                            itemToSpawn = mapCompletion.bakery;
                        }
                        break;
                    }
                }


                //itemToSpawn = mapCompletion.createdStone1;
                break;
            case DecisionMedieval.StoneType.Stone2:
                foreach (var decision in decisions)
                {
                    if (decision.Type == DecisionMedieval.StoneType.Stone2)
                    {
                        toSave = decision.Save;
                        if (decision.Save)
                        {
                            itemToSpawn = mapCompletion.createdStone2;
                        }
                        else
                        {
                            itemToSpawn = mapCompletion.cottage;
                        }
                        break;
                    }
                }
                break;
            case DecisionMedieval.StoneType.OtherStone:
                foreach (var decision in decisions)
                {
                    if (decision.Type == DecisionMedieval.StoneType.OtherStone)
                    {
                        toSave = decision.Save;
                        if (decision.Save)
                        {
                            itemToSpawn = mapCompletion.foundStone;
                        }
                        else
                        {
                            itemToSpawn = mapCompletion.church;
                        }
                        break;
                    }
                }
                break;
            default:
                Debug.LogError("Unknown stone type: " + stoneType);
                return;
        }


        scanner.SetItemToSpawn(itemToSpawn);

        if(!toSave)
        {
            //set the scale by ten
            itemToSpawn.transform.localScale = itemToSpawn.transform.localScale * 10f;
        }


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