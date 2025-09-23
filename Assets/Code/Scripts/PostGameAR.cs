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



    string GetRandomBakeryName()
    {
        string[] peopleNames = new string[]
        {
            "Alice", "Bob", "Charlie", "Diana", "Ethan", "Fiona", "George", "Hannah",
            "Ian", "Julia", "Kevin", "Laura", "Michael", "Nina", "Oliver", "Paula",
            "Quinn", "Rachel", "Sam", "Tina", "Umar", "Violet", "Wendy", "Xander",
            "Yara", "Zane"
        };

        // Select a random name from the array and do "Name's Bakery"
        string randomName = peopleNames[Random.Range(0, peopleNames.Length)];
        return randomName + "'s Bakery";


    }


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
                    stoneOrBuilding1 = Instantiate(mapCompletion.createdStone1);

                    //set scale to 0.2
                    stoneOrBuilding1.transform.localScale = Vector3.one * 0.2f;
                }
                else
                {
                    stoneOrBuilding1 = Instantiate(mapCompletion.bakery);


                
                    //get the Text (TMP) component and change the text to test
                    stoneOrBuilding1.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = GetRandomBakeryName();

                    //set scale to 4

                    stoneOrBuilding1.transform.localScale = Vector3.one * 4f;
                }


                stoneOrBuilding1.SetActive(false);


            }
            else if (decision.Type == DecisionMedieval.StoneType.Stone2)
            {
                if (decision.Save)
                {
                    stoneOrBuilding2 = Instantiate(mapCompletion.createdStone2);
                    //set scale to 0.2
                    stoneOrBuilding2.transform.localScale = Vector3.one * 0.2f;
                }
                else
                {
                    stoneOrBuilding2 = Instantiate(mapCompletion.cottage);

                    //set scale to 5
                    stoneOrBuilding2.transform.localScale  = Vector3.one * 5f;
                }

                stoneOrBuilding2.SetActive(false);
            }
            else if (decision.Type == DecisionMedieval.StoneType.OtherStone)
            {
                if (decision.Save)
                {
                    stoneOrBuilding3 = Instantiate(mapCompletion.foundStone);
                    //set scale to 0.2
                    stoneOrBuilding3.transform.localScale = Vector3.one * 0.2f;
                }
                else
                {
                    stoneOrBuilding3 = Instantiate(mapCompletion.church);
                    //add 5 to the scale of the object
                    stoneOrBuilding3.transform.localScale *= 5f;
                }
                stoneOrBuilding3.SetActive(false);
            }
        }


        //create a new game object to hold the three stones/buildings
        GameObject stone = new GameObject("StonesAndBuildings");


        // Set stoneOrBuilding2 at the center
        if (stoneOrBuilding2 != null)
        {

            stoneOrBuilding2.transform.SetParent(stone.transform);
            stoneOrBuilding2.transform.localPosition = Vector3.zero;
        }

        // Define radius for placing the other objects
        float minRadius = 2f;
        float maxRadius = 5f;

        // Place stoneOrBuilding1 randomly around the center
        if (stoneOrBuilding1 != null)
        {

            stoneOrBuilding1.transform.SetParent(stone.transform);
            stoneOrBuilding1.transform.localPosition = GetRandomPositionOnCircle(minRadius, maxRadius);
        }

        // Place stoneOrBuilding3 randomly around the center
        if (stoneOrBuilding3 != null)
        {

            stoneOrBuilding3.transform.SetParent(stone.transform);
            stoneOrBuilding3.transform.localPosition = GetRandomPositionOnCircle(minRadius, maxRadius);
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