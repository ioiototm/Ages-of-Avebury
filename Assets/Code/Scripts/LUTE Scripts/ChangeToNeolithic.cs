using LoGaCulture.LUTE;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Change the app to the neolithic time",
              "Disables modern interface anbd enables stone")]
[AddComponentMenu("")]
public class ChangeToNeolithic : Order
{

    [SerializeField]
    GameObject modernInterface;
    [SerializeField]
    GameObject neolithicInterface;

    [SerializeField]
    InterfaceGameEvent neolithicInterfaceEvent;


    public override void OnEnter()
    {

        // Disable the modern interface
        if (modernInterface != null)
        {
            modernInterface.SetActive(false);
        }
        // Enable the neolithic interface
        if (neolithicInterface != null)
        {
            neolithicInterface.SetActive(true);
        }

        // Trigger the neolithic interface event
        if (neolithicInterfaceEvent != null)
        {
            neolithicInterfaceEvent.Raise();
        }

        XRManager.Instance.SetXRActive(false);


        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order sets up the Scanner";
    }
}