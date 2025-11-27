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

                // Position the first three children relative to the parent's local axes (left/right/forward)
                var xrObject = XRManager.Instance.GetXRObject();
                Camera xrCam = xrObject.GetComponentInChildren<Camera>();

                // Align the parent yaw to camera so local forward = camera forward
                Vector3 camForward = xrCam.transform.forward; camForward.y = 0f; if (camForward.sqrMagnitude < 0.0001f) camForward = Vector3.forward; camForward.Normalize();
                spawnable.transform.rotation = Quaternion.LookRotation(camForward, Vector3.up);

                // Use local positions to simply move them apart
                float offsetDistance = 10f; // increased spacing

                GameObject stoneAndBuildings = spawnable.transform.GetChild(1).gameObject; // Assuming the stones and buildings container is the second 
                //Fix in the future

                int childCount = stoneAndBuildings.transform.childCount;

                if (childCount > 0)
                {
                    var c0 = stoneAndBuildings.transform.GetChild(0);
                    c0.localPosition = Vector3.left * offsetDistance;
                }
                if (childCount > 1)
                {
                    var c1 = stoneAndBuildings.transform.GetChild(1);
                    c1.localPosition = Vector3.forward * offsetDistance;
                }
                if (childCount > 2)
                {
                    var c2 = stoneAndBuildings.transform.GetChild(2);
                    c2.localPosition = Vector3.right * offsetDistance;
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

        var xrObject = XRManager.Instance.GetXRObject();

        //get the camera rig in the XR object
        Camera xrCam = xrObject.GetComponentInChildren<Camera>();

        //set the far clip plane to 50
        xrCam.farClipPlane = 50f;

        var decisions = MapCompletion.decisions;
        var toSave = false;


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
                    instance.transform.localScale = Vector3.one * 0.4f;
                }
                else
                {
                    instance.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = GetRandomBakeryName();
                    instance.transform.localScale = Vector3.one * 8.5f;
                }
            }
            else if (decision.Type == DecisionMedieval.StoneType.Stone2)
            {
                stoneOrBuilding2Prefab = decision.Save ? mapCompletion.createdStone2 : mapCompletion.cottage;
                GameObject instance = Instantiate(stoneOrBuilding2Prefab, stone.transform);
                if (decision.Save)
                {
                    instance.transform.localScale = Vector3.one * 0.4f;
                }
                else
                {
                    instance.transform.localScale = Vector3.one * 9.5f;
                }
            }
            else if (decision.Type == DecisionMedieval.StoneType.OtherStone)
            {
                stoneOrBuilding3Prefab = decision.Save ? mapCompletion.foundStone : mapCompletion.church;
                GameObject instance = Instantiate(stoneOrBuilding3Prefab, stone.transform);
                if (decision.Save)
                {
                    instance.transform.localScale = Vector3.one * 0.4f;
                }
                else
                {
                    instance.transform.localScale = Vector3.one * 9.5f;
                }
            }
        }

        var node = GetEngine().FindNode(ParentNode._NodeName);
        var lastOrder = node.OrderList.Last<Order>();

        foreach (Order order in node.OrderList)
        {
            if (order.OrderIndex == this.OrderIndex + 2)
            {
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

            Transform parent = objectToPlace.parent;
            bool wasParentActive = parent.gameObject.activeSelf;
            parent.gameObject.SetActive(true);

            Vector3 boxCenter = objectToPlace.TransformPoint(objectCollider.bounds.center);
            Vector3 halfExtents = Vector3.Scale(objectCollider.bounds.extents, objectToPlace.lossyScale);
            Quaternion orientation = objectToPlace.rotation;

            Collider[] overlaps = Physics.OverlapBox(boxCenter, halfExtents, orientation);

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
        return "Sets up everything for the AR post game";
    }
}