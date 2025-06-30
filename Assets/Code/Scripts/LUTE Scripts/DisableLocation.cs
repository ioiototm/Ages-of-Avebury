using LoGaCulture.LUTE;
using System.Collections;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Disable Location",
              "Disables the location so it's not re-triggered")]
[AddComponentMenu("")]
public class DisableLocation : Order
{




    public override void OnEnter()
    {
        if (ParentNode._EventHandler != null)
        {
            //if it's of type locaitonclickeventhandler
            if (ParentNode._EventHandler is LocationClickEventHandler locationClickEventHandler)
            {

                //if lastseenlocation is null, get it from the engine

                locationClickEventHandler.Location.Value.LocationDisabled = true;

                //To continue or not depending on if this order takes care of everything or diffrent orders take care of the rest
                
            }
        }

        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order sets up the Stone Creation";
    }
}