using LoGaCulture.LUTE;
using UnityEngine;

[OrderInfo("AgesOfAvebury",
              "Pick random quadrant at the start",
              "Picks a random quadrant and sets it up")]
[AddComponentMenu("")]
public class ChoseRandomQuadrant : Order
{
    public override void OnEnter()
    {

        bool southQuadrant;

        //50/50 south or north

        if (Random.Range(0, 2) == 0)
        {
            southQuadrant = true;
        }
        else
        {
            southQuadrant = false;
        }


        if(southQuadrant)
        {
            //set up the south quadrant
            if (!LocationRandomiser.Instance.southQuadrant)
            {
                LocationRandomiser.Instance.changeQuadrant();
                Debug.Log("Changed to south quadrant");
            }
            else
            {
                Debug.Log("Already in south quadrant");
            }
        }
        else
        {
            //set up the north quadrant
            if (LocationRandomiser.Instance.southQuadrant)
            {
                LocationRandomiser.Instance.changeQuadrant();
                Debug.Log("Changed to north quadrant");
            }
            else
            {
                Debug.Log("Already in north quadrant");
            }

        }
        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order sets up the quadrant";
    }
}
