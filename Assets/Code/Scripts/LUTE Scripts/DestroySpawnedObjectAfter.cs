using LoGaCulture.LUTE;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Destroy spawned object",
              "Destroys the spawned object")]
[AddComponentMenu("")]
public class DestroySpawnedObjectAfter : Order
{

  
    [SerializeField]
    HiddenItemScanner scanner;



    public override void OnEnter()
    {
        scanner.destroyDiscoveredObject();

        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order destroys the spawned object";
    }
}