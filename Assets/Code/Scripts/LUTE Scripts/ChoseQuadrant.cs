using LoGaCulture.LUTE;
using UnityEngine;

[OrderInfo("AgesOfAvebury",
              "Set the quadrant",
              "Sets the quadrant and sets it up")]
[AddComponentMenu("")]
public class ChoseQuadrant : Order
{

    private enum Quadrant
    {
        North,
        South
    }

    [SerializeField]
    private Quadrant quadrant;
    public override void OnEnter()
    {

        //if (TinySave.loadGame)
        //{
        //    //if we are loading a game, we don't want to change the quadrant
        //    Debug.Log("Loading game, not changing quadrant");
        //    Continue();
        //    return;
        //}





        if (quadrant == Quadrant.South)
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
