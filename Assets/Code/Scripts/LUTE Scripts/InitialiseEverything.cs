using UnityEngine;

[OrderInfo("AgesOfAvebury",
              "InitialiseEverything",
              "Initialises all variables needed for the game")]
[AddComponentMenu("")]
public class InitialiseEverything : Order
{
    [SerializeField]
    private static GameObject messagePrefab;

    [SerializeField]
    private GameObject mapCanvas;
    [SerializeField]
    private GameObject inboxCanvas;
    [SerializeField]
    private GameObject menuCanvas;

    public static GameObject _inboxCanvas;
    public static GameObject _menuCanvas;
    public static GameObject _mapCanvas;

    public static GameObject modernScreen;

    public override void OnEnter()
    {
        // Use provided GameObjects if available, otherwise find them in the scene
        _inboxCanvas = inboxCanvas ? inboxCanvas : GameObject.Find("ModernInboxCanvas");
        _menuCanvas = menuCanvas ? menuCanvas : GameObject.Find("ModernMenuCanvas");
        _mapCanvas = mapCanvas ? mapCanvas : GameObject.Find("ModernMapCanvas");

        // Disable the inbox and enable the menu
        _inboxCanvas.SetActive(false);
        _menuCanvas.SetActive(true);
        _mapCanvas.SetActive(false);


        //get the TargetLocation and LastSeenLocation location variables from the flow engine

        var targetLocation = GetEngine().GetVariable<LocationVariable>("TargetLocation");
        var lastSeenLocation = GetEngine().GetVariable<LocationVariable>("LastSeenLocation");

        targetLocation.Value = GetEngine().GetVariable<LocationVariable>("StartingLocation1").Value;
        lastSeenLocation.Value = targetLocation.Value;

        // Continue to the next order
        Continue();
    }

    public override string GetSummary()
    {
        // Return a summary of the order which is displayed in the inspector of the order
        return "This order will initialise everything needed";
    }
}
