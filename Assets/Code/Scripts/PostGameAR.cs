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


    IEnumerator CheckAndEnableOnceSpawned()
    {
        //this function checks if there is an object called "Spawnable(Clone)" every second, and once it finds it, it sets its all children to visible
        while (true)
        {
            GameObject spawnable = GameObject.Find("Spawnable(Clone)");
            if (spawnable != null)
            {
               
                // Enable all children of the spawnable object
                foreach (Transform child in spawnable.transform)
                {
                    child.gameObject.SetActive(true);
                }
                yield break; // Exit the coroutine once done
            }
            yield return new WaitForSeconds(1f); // Wait for 1 second before checking again
        }

    }
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
        GameObject stoneOrBuilding1Prefab = null;
        GameObject stoneOrBuilding2Prefab = null;
        GameObject stoneOrBuilding3Prefab = null;

        // Create a new parent GameObject that will serve as the prefab container.
        // It is created in the scene but immediately deactivated.
        GameObject stone = new GameObject("StonesAndBuildings");
        stone.SetActive(false); // Deactivate it so it and its children are not visible

        foreach (var decision in decisions)
        {
            if (decision.Type == DecisionMedieval.StoneType.Stone1)
            {
                stoneOrBuilding1Prefab = decision.Save ? mapCompletion.createdStone1 : mapCompletion.bakery;
                GameObject instance = Instantiate(stoneOrBuilding1Prefab, stone.transform);
                if (decision.Save)
                {
                    instance.transform.localScale = Vector3.one * 0.2f;
                }
                else
                {
                    instance.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = GetRandomBakeryName();
                    instance.transform.localScale = Vector3.one * 7f;
                }
            }
            else if (decision.Type == DecisionMedieval.StoneType.Stone2)
            {
                stoneOrBuilding2Prefab = decision.Save ? mapCompletion.createdStone2 : mapCompletion.cottage;
                GameObject instance = Instantiate(stoneOrBuilding2Prefab, stone.transform);
                if (decision.Save)
                {
                    instance.transform.localScale = Vector3.one * 0.2f;
                }
                else
                {
                    instance.transform.localScale = Vector3.one * 8f;
                }
            }
            else if (decision.Type == DecisionMedieval.StoneType.OtherStone)
            {
                stoneOrBuilding3Prefab = decision.Save ? mapCompletion.foundStone : mapCompletion.church;
                GameObject instance = Instantiate(stoneOrBuilding3Prefab, stone.transform);
                if (decision.Save)
                {
                    instance.transform.localScale = Vector3.one * 0.2f;
                }
                else
                {
                    instance.transform.localScale *= 8f;
                }
            }
        }

        // Now position the instantiated children within the deactivated parent
        float minRadius = 10f;
        float maxRadius = 20f;
        int maxPlacementAttempts = 5;

        // Child at index 1 should be stoneOrBuilding2, which goes in the center
        if (stone.transform.childCount > 1)
        {
            stone.transform.GetChild(1).localPosition = Vector3.zero;
            stone.transform.GetChild(1).gameObject.SetActive(true);
        }

        // Position the other objects, checking for overlaps
        if (stone.transform.childCount > 0)
        {
            PlaceObjectWithoutOverlap(stone.transform.GetChild(0), minRadius, maxRadius, maxPlacementAttempts);
            stone.transform.GetChild(0).gameObject.SetActive(true);
        }

        if (stone.transform.childCount > 2)
        {
            PlaceObjectWithoutOverlap(stone.transform.GetChild(2), minRadius, maxRadius, maxPlacementAttempts);
            stone.transform.GetChild(2).gameObject.SetActive(true);
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

        // Start the coroutine to check and enable the child once spawned
        StartCoroutine(CheckAndEnableOnceSpawned());


        Continue();

    }

    private void PlaceObjectWithoutOverlap(Transform objectToPlace, float minRadius, float maxRadius, int maxAttempts)
    {
        Collider objectCollider = objectToPlace.GetComponent<Collider>();
        if (objectCollider == null)
        {
            Debug.LogWarning($"Object {objectToPlace.name} is missing a Collider component. Overlap checks will be skipped.");
            objectToPlace.localPosition = GetRandomPositionOnCircle(minRadius, maxRadius);
            return;
        }

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 testPosition = GetRandomPositionOnCircle(minRadius, maxRadius);
            objectToPlace.localPosition = testPosition;

            // Temporarily activate the parent to allow physics checks
            Transform parent = objectToPlace.parent;
            bool wasParentActive = parent.gameObject.activeSelf;
            parent.gameObject.SetActive(true);

            Vector3 boxCenter = objectToPlace.TransformPoint(objectCollider.bounds.center);
            Vector3 halfExtents = Vector3.Scale(objectCollider.bounds.extents, objectToPlace.lossyScale);
            Quaternion orientation = objectToPlace.rotation;

            // Check for overlap with other colliders, ignoring the object itself
            Collider[] overlaps = Physics.OverlapBox(boxCenter, halfExtents, orientation);

            // Restore parent's original active state
            parent.gameObject.SetActive(wasParentActive);

            bool isOverlapping = false;
            foreach (var overlap in overlaps)
            {
                if (overlap.transform != objectToPlace && overlap.transform.IsChildOf(parent))
                {
                    isOverlapping = true;
                    break;
                }
            }

            if (!isOverlapping)
            {
                return; // Found a valid position
            }
        }

        Debug.LogWarning($"Could not find a non-overlapping position for {objectToPlace.name} after {maxAttempts} attempts.");
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