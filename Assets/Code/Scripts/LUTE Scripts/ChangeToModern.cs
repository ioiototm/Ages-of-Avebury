using LoGaCulture.LUTE;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Change the app to the modern time",
              "Disables stone and enables modern interface")]
[AddComponentMenu("")]
public class ChangeToModern : Order
{

    [SerializeField]
    GameObject middleAgesInterface;
    [SerializeField]
    GameObject modernInterface;

    [SerializeField]
    InterfaceGameEvent modernInterfaceEvent;


    public override void OnEnter()
    {

        // Disable the modern interface
        if (middleAgesInterface != null)
        {
            middleAgesInterface.SetActive(false);
        }
        // Enable the neolithic interface
        if (modernInterface != null)
        {
            modernInterface.SetActive(true);
        }

        // Trigger the neolithic interface event
        if (modernInterfaceEvent != null)
        {
            modernInterfaceEvent.Raise();
        }

        XRManager.Instance.SetXRActive(false);

        Compass compass = GameObject.Find("Compass Test").GetComponent<Compass>();
        compass.timePeriod = Compass.TimePeriod.Modern;

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