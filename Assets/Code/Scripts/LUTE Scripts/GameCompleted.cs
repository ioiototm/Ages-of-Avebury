using UnityEngine;

[OrderInfo("AgesOfAvebury",
              "Complete the Game",
              "Sets the flags for game completion")]
[AddComponentMenu("")]
public class GameCompleted : Order
{
     public override void OnEnter()
    {
        //get the MapCompletion script from any GameObject in the scene
        MapCompletion mapCompletion = FindFirstObjectByType<MapCompletion>();
        mapCompletion.gameCompleted = true;

        Continue();

    }
    public override string GetSummary()
    {
        // Return a summary of the order which is displayed in the inspector of the order
        return "This order sets the game completion flags.";
    }


}
