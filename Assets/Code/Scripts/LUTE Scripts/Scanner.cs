using LoGaCulture.LUTE;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Scanner",
              "Sets up the Scanner")]
[AddComponentMenu("")]
public class Scanner : Order
{

    [SerializeField]
    LocationVariable locationOfScanning;


    public override void OnEnter()
    {

        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order sets up the Scanner";
    }
}