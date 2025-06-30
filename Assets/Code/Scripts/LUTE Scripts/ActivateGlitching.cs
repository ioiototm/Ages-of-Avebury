using LoGaCulture.LUTE;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Glitch",
              "Activates the glitch")]
[AddComponentMenu("")]
public class ActivateGlitching : Order
{


    public override void OnEnter()
    {
        Glitching glitch = GameObject.Find("Glitch Overlay").GetComponent<Glitching>();
        if (glitch != null)
        {
            glitch.running = true; // Set the running variable to true to activate the glitch

            //get the TragetLocation variable
            var targetLocation = GetEngine().GetVariable<LocationVariable>("Portal5");

            if (targetLocation != null)
            {
                // Set the target location to the current location
                glitch.portalLocation = targetLocation;
            }
            else
            {
                Debug.LogWarning("TargetLocation variable not found.");
            }


            Debug.Log("Glitching activated.");
        }
        else
        {
            Debug.LogError("Glitching component not found on Clitch Overlay.");
        }

        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order activates the glitch so it starts";
    }
}
