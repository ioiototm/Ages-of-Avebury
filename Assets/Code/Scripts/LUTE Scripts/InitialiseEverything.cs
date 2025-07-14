using LoGaCulture.LUTE;
using LoGaCulture.LUTE.Logs;
using Mapbox.Examples;
using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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

    [SerializeField]
    private GameObject inboxMiddleCanvas;


    public static GameObject _inboxCanvas;
    public static GameObject _menuCanvas;
    public static GameObject _mapCanvas;
    public static GameObject _scannerCanvas;

    public static GameObject modernScreen;
    public static GameObject neolithicScreen;
    public static GameObject middleAgesScreen;


    public static GameObject neolithicMakeStoneButton;

    public static GameObject _inboxMiddleCanvas;


    



    private ImmediatePositionWithLocationProvider centering;


     

    public override void OnEnter()
    {


        Screen.sleepTimeout = SleepTimeout.NeverSleep; // Prevent the screen from sleeping


        // Use provided GameObjects if available, otherwise find them in the scene
        _inboxCanvas = inboxCanvas ? inboxCanvas : GameObject.Find("ModernInboxCanvas");
        _menuCanvas = menuCanvas ? menuCanvas : GameObject.Find("ModernMenuCanvas");
        _mapCanvas = mapCanvas ? mapCanvas : GameObject.Find("ModernMapCanvas");
        _scannerCanvas = scannerCanvas ? scannerCanvas : GameObject.Find("ScannerCanvas");

        _inboxMiddleCanvas = inboxMiddleCanvas ? inboxMiddleCanvas : GameObject.Find("MiddleInbox");

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
                if (!name.Contains("1") || name.Contains("10") || name.Contains("11") || name.Contains("12") || name.Contains("13"))
                {
                    //if it's not the LastSeenLocation and TargetLocation
                    if (name.Contains("LastSeenLocation") || name.Contains("TargetLocation"))
                    {
                        continue;
                    }
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

        GetEngine().GetAbstractMap().SetZoom(16.5f);

        // Start centering the map to the screen every second
        StartCoroutine(centerToScreenEverySecond());


        // Continue to the next order
        Continue();
    }

    public static List<Vector3> ParsePoints(string data)
    {
        var pts = new List<Vector3>();
        if (string.IsNullOrWhiteSpace(data)) return pts;

        // 1) split on ';'
        var entries = data.Split(';');

        foreach (var e in entries)
        {
            var t = e.Trim();
            // 2) skip empty or non-numeric junk
            if (string.IsNullOrEmpty(t) || t == "-") continue;

            // 3) split into three coords
            var xyz = t.Split(',');
            if (xyz.Length < 3) continue;

            // 4) try parse each float
            if (float.TryParse(xyz[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                float.TryParse(xyz[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) &&
                float.TryParse(xyz[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
            {
                pts.Add(new Vector3(x, y,z));
            }
            // else skip malformed entry
        }

        return pts;
    }

    public static bool activelyCentering = true;
    private static bool isMapMoved = false;

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
            GetEngine().GetAbstractMap().SetZoom(16.5f);
            centering.UpdateMapToPlayer();

            StartCoroutine(waitAbitAndRestartMap());


            //StartCoroutine(centerToScreenEverySecond());
            Debug.Log("Centering map to player location.");

        }
        else
        {
            Debug.LogError("ImmediatePositionWithLocationProvider component not found on PlayerTarget.");
        }
    }

    IEnumerator waitAbitAndRestartMap()
    {
        // Wait for 2 seconds before restarting the map
        yield return new WaitForSeconds(0.5f);
        // Restart the map
        activelyCentering = true;
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
