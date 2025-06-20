using LoGaCulture.LUTE;
using LoGaCulture.LUTE.Logs;
using Mapbox.Examples;
using System.Collections;
using System.Linq;
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
    [SerializeField]
    private GameObject scannerCanvas;


    public static GameObject _inboxCanvas;
    public static GameObject _menuCanvas;
    public static GameObject _mapCanvas;
    public static GameObject _scannerCanvas;

    public static GameObject modernScreen;
    public static GameObject neolithicScreen;
    public static GameObject middleAgesScreen;


    public static GameObject neolithicMakeStoneButton;



    private ImmediatePositionWithLocationProvider centering;

    public override void OnEnter()
    {


        Screen.sleepTimeout = SleepTimeout.NeverSleep; // Prevent the screen from sleeping


        // Use provided GameObjects if available, otherwise find them in the scene
        _inboxCanvas = inboxCanvas ? inboxCanvas : GameObject.Find("ModernInboxCanvas");
        _menuCanvas = menuCanvas ? menuCanvas : GameObject.Find("ModernMenuCanvas");
        _mapCanvas = mapCanvas ? mapCanvas : GameObject.Find("ModernMapCanvas");
        _scannerCanvas = scannerCanvas ? scannerCanvas : GameObject.Find("ScannerCanvas");

        // Disable the inbox and enable the menu
        _inboxCanvas.SetActive(false);
        _menuCanvas.SetActive(false);
        _mapCanvas.SetActive(true);
        _scannerCanvas.SetActive(false);


        modernScreen = GameObject.Find("ModernInterface");
        neolithicScreen = GameObject.Find("NeolithicInterface");
        middleAgesScreen = GameObject.Find("MiddlePeriodInterface");



        neolithicMakeStoneButton = GameObject.Find("MakeStone");
        neolithicMakeStoneButton.SetActive(false);

        modernScreen.SetActive(true);
        neolithicScreen.SetActive(false);
        middleAgesScreen.SetActive(false);


        //get the TargetLocation and LastSeenLocation location variables from the flow engine

        var targetLocation = GetEngine().GetVariable<LocationVariable>("TargetLocation");
        var lastSeenLocation = GetEngine().GetVariable<LocationVariable>("LastSeenLocation");

        targetLocation.Value = GetEngine().GetVariable<LocationVariable>("StartingLocation1").Value;
        lastSeenLocation.Value = targetLocation.Value;


        //get the map manager
        var mapManager = GetEngine().GetMapManager();

        //get all the location variables in the flow engine
        var locationVariables = GetEngine().GetVariables<LocationVariable>();

        //go through each location variable name that has a number in it
        foreach (var locationVariable in locationVariables)
        {

            var name = locationVariable.Key;

            //the names are random but have a number somewhere, so check if the name contains any numbers, in any place

            // Check if the name contains any number  
            if (name.Any(char.IsDigit))
            {

                locationVariable.Value.LocationStatus = LoGaCulture.LUTE.LocationStatus.Unvisited;


                //if the digit is not 1, then hide it
                if (!name.Contains("1"))
                {
                    //hide the location marker
                    mapManager.HideLocationMarker(locationVariable);
                    
                }
                else
                {
                    //show the location marker
                    mapManager.ShowLocationMarker(locationVariable);
                }



            }



        }



        centering = GameObject.Find("PlayerTarget").GetComponent<ImmediatePositionWithLocationProvider>();



        //ConnectionManager.Instance.FetchSharedVariables("stone1",
        //    (variables) =>
        //    {
        //        if (variables != null && variables.Length > 0)
        //        {

        //            //go through each variable and just print out the name and value
        //            foreach (var variable in variables)
        //            {
        //                Debug.Log($"Variable created at: {variable.createdAt}, Name: {variable.variableName}");
        //            }

        //        }
        //    },
        //    2);



        // Start centering the map to the screen every second
        StartCoroutine(centerToScreenEverySecond());


        // Continue to the next order
        Continue();
    }

    private static bool activelyCentering = true;

    public static void movedMap()
            {
        // If the map has been moved, stop actively centering
        activelyCentering = false;
        Debug.Log("Map has been moved, stopping active centering.");
    }


    public void centreToScreen()
    {
    
        if (centering != null)
        {
            // Center the map to the current location
            activelyCentering = true;
            centering.UpdateMapToPlayer();
            //StartCoroutine(centerToScreenEverySecond());
            Debug.Log("Centering map to player location.");

        }
        else
        {
            Debug.LogError("ImmediatePositionWithLocationProvider component not found on PlayerTarget.");
        }
    }

    IEnumerator centerToScreenEverySecond()
    {

       

        while (true)
        {
            yield return new WaitForSeconds(1f);
            // Center the map to the current location
            if (activelyCentering)
            {
                centering.UpdateMapToPlayer();
            }
        }
    }

    public override string GetSummary()
    {
        // Return a summary of the order which is displayed in the inspector of the order
        return "This order will initialise everything needed";
    }
}
