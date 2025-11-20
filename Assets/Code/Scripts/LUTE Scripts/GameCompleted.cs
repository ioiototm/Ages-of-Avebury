using UnityEngine;

[OrderInfo("AgesOfAvebury",
              "Complete the Game",
              "Sets the flags for game completion")]
[AddComponentMenu("")]
public class GameCompleted : Order
{


    public static bool GameCompletedFlag = false;
    public override void OnEnter()
    {
        //get the MapCompletion script from any GameObject in the scene
        MapCompletion mapCompletion = FindFirstObjectByType<MapCompletion>();
        mapCompletion.gameCompleted = true;
        GameCompletedFlag = true;

        TinySave.Instance.CompleteGame();


        //get the FirstStoneCreation6 and SecondStoneCreation7 location variables from the engine
        var firstStoneCreation = GetEngine().GetVariable<LocationVariable>("FirstStoneCreation6");
        var secondStoneCreation = GetEngine().GetVariable<LocationVariable>("SecondStoneCreation7");

        //get the PostGameStoneFirst and PostGameStoneSecond location variables from the engine
        var postGameStoneFirst = GetEngine().GetVariable<LocationVariable>("PostGameStoneFirst");
        var postGameStoneSecond = GetEngine().GetVariable<LocationVariable>("PostGameStoneSecond");

        postGameStoneFirst.Value.SetNewPosition(firstStoneCreation.Value.Position);
        postGameStoneSecond.Value.SetNewPosition(secondStoneCreation.Value.Position);

        postGameStoneFirst.Value.LocationStatus = LoGaCulture.LUTE.LocationStatus.Unvisited;
        postGameStoneSecond.Value.LocationStatus = LoGaCulture.LUTE.LocationStatus.Unvisited;




        Continue();

    }
    public override string GetSummary()
    {
        // Return a summary of the order which is displayed in the inspector of the order
        return "This order sets the game completion flags.";
    }


}
