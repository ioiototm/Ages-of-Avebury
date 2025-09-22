using LoGaCulture.LUTE;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using static DecisionMedieval;


[OrderInfo("AgesOfAvebury",
              "Post Game AR Setup",
              "Sets up the preceding AR object placement")]
[AddComponentMenu("")]
public class PostGameAR : Order
{




    public override void OnEnter()
    {

        GameObject mapComplete = GameObject.Find("MapComplete");
        MapCompletion mapCompletion = mapComplete.GetComponent<MapCompletion>();

        var decisions = MapCompletion.decisions;
        var toSave = false;


        //switch (stoneType)
        //{
        //    case DecisionMedieval.StoneType.Stone1:

        //        foreach (var decision in decisions)
        //        {
        //            if (decision.Type == DecisionMedieval.StoneType.Stone1)
        //            {
        //                toSave = decision.Save;
        //                if (decision.Save)
        //                {
        //                    itemToSpawn = mapCompletion.createdStone1;
        //                }
        //                else
        //                {
        //                    itemToSpawn = mapCompletion.bakery;
        //                }
        //                break;
        //            }
        //        }


        //        //itemToSpawn = mapCompletion.createdStone1;
        //        break;
        //    case DecisionMedieval.StoneType.Stone2:
        //        foreach (var decision in decisions)
        //        {
        //            if (decision.Type == DecisionMedieval.StoneType.Stone2)
        //            {
        //                toSave = decision.Save;
        //                if (decision.Save)
        //                {
        //                    itemToSpawn = mapCompletion.createdStone2;
        //                }
        //                else
        //                {
        //                    itemToSpawn = mapCompletion.cottage;
        //                }
        //                break;
        //            }
        //        }
        //        break;
        //    case DecisionMedieval.StoneType.OtherStone:
        //        foreach (var decision in decisions)
        //        {
        //            if (decision.Type == DecisionMedieval.StoneType.OtherStone)
        //            {
        //                toSave = decision.Save;
        //                if (decision.Save)
        //                {
        //                    itemToSpawn = mapCompletion.foundStone;
        //                }
        //                else
        //                {
        //                    itemToSpawn = mapCompletion.church;
        //                }
        //                break;
        //            }
        //        }
        //        break;
        //    default:
        //        Debug.LogError("Unknown stone type: " + stoneType);
        //        return;
        //}


        //get all the stones or buildings to place (all 3), and create a new object, where the second stone/building is in the middle,
        ////and then the other two stones/buildings are in a radius around them, not too close, not to far, randomly placed

        //check the decisions list for each stone type, and get the corresponding prefab from mapCompletion
        GameObject stoneOrBuilding1 = null;
        GameObject stoneOrBuilding2 = null;
        GameObject stoneOrBuilding3 = null;

        foreach (var decision in decisions)
        {
            if (decision.Type == DecisionMedieval.StoneType.Stone1)
            {
                if (decision.Save)
                {
                    stoneOrBuilding1 = mapCompletion.createdStone1;
                }
                else
                {
                    stoneOrBuilding1 = mapCompletion.bakery;
                }
            }
            else if (decision.Type == DecisionMedieval.StoneType.Stone2)
            {
                if (decision.Save)
                {
                    stoneOrBuilding2 = mapCompletion.createdStone2;
                }
                else
                {
                    stoneOrBuilding2 = mapCompletion.cottage;
                }
            }
            else if (decision.Type == DecisionMedieval.StoneType.OtherStone)
            {
                if (decision.Save)
                {
                    stoneOrBuilding3 = mapCompletion.foundStone;
                }
                else
                {
                    stoneOrBuilding3 = mapCompletion.church;
                }
            }
        }


        //create a new game object to hold the three stones/buildings
        GameObject stone = new GameObject("StonesAndBuildings");


        // Set stoneOrBuilding2 at the center
        if (stoneOrBuilding2 != null)
        {
            GameObject prefabCopy = Instantiate(stoneOrBuilding2);
            prefabCopy.transform.SetParent(stone.transform);
            prefabCopy.transform.localPosition = Vector3.zero;
        }

        // Define radius for placing the other objects
        float minRadius = 2f;
        float maxRadius = 5f;

        // Place stoneOrBuilding1 randomly around the center
        if (stoneOrBuilding1 != null)
        {
            GameObject prefabCopy = Instantiate(stoneOrBuilding1);
            prefabCopy.transform.SetParent(stone.transform);
            prefabCopy.transform.localPosition = GetRandomPositionOnCircle(minRadius, maxRadius);
        }

        // Place stoneOrBuilding3 randomly around the center
        if (stoneOrBuilding3 != null)
        {
            GameObject prefabCopy = Instantiate(stoneOrBuilding3);
            prefabCopy.transform.SetParent(stone.transform);
            prefabCopy.transform.localPosition = GetRandomPositionOnCircle(minRadius, maxRadius);
        }


        var node = GetEngine().FindNode(ParentNode._NodeName);
        var lastOrder = node.OrderList.Last<Order>();

        //get the next order to this one

        foreach (Order order in node.OrderList)
        {
            if (order.OrderIndex == this.OrderIndex + 2)
            {

                //if it's of type PlaceObjectOnPlane
                if (order is PlaceObjectXR)
                {
                    PlaceObjectXR placeOrder = (PlaceObjectXR)order;
                    placeOrder.m_PrefabToPlace = stone;
                }


            }
        }
        Continue();

    }

    private Vector3 GetRandomPositionOnCircle(float minRadius, float maxRadius)
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = Random.Range(minRadius, maxRadius);

        float x = radius * Mathf.Cos(angle);
        float z = radius * Mathf.Sin(angle);

        return new Vector3(x, 0, z);
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "Sets up everything for the AR post game";
    }
}