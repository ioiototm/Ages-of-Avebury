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
    private GameObject postGameScannerCanvas;

    [SerializeField]
    private GameObject inboxMiddleCanvas;


    public static GameObject _inboxCanvas;
    public static GameObject _menuCanvas;
    public static GameObject _mapCanvas;
    public static GameObject _scannerCanvas;
    public static GameObject _postGameScannerCanvas;

    public static GameObject modernScreen;
    public static GameObject neolithicScreen;
    public static GameObject middleAgesScreen;

    public static StoneCreator stone1, stone2, stone3;


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
        _scannerCanvas = scannerCanvas ? scannerCanvas : GameObject.Find("ModernScanner");
        _postGameScannerCanvas = postGameScannerCanvas ? postGameScannerCanvas : GameObject.Find("PostGameModernScanner");

        _inboxMiddleCanvas = inboxMiddleCanvas ? inboxMiddleCanvas : GameObject.Find("MiddleInbox");

        // Disable the inbox and enable the menu
        _inboxCanvas.SetActive(false);
        _menuCanvas.SetActive(false);
        _mapCanvas.SetActive(true);
        _scannerCanvas.SetActive(false);
        _postGameScannerCanvas.SetActive(false);


        modernScreen = GameObject.Find("ModernInterface");
        neolithicScreen = GameObject.Find("NeolithicInterface");

        stone1 = GameObject.Find("Stone1").GetComponent<StoneCreator>();
        stone2 = GameObject.Find("Stone2").GetComponent<StoneCreator>();
        stone3 = GameObject.Find("Stone3").GetComponent<StoneCreator>();

        TinySave.stone1 = stone1;
        TinySave.stone2 = stone2;
        TinySave.stone3 = stone3;

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

        if (TinySave.loadGame == false)
        {
            //go through each location variable name that has a number in it
            foreach (var locationVariable in locationVariables)
            {

                var name = locationVariable.Key;

                //the names are random but have a number somewhere, so check if the name contains any numbers, in any place
                if (name.Contains("barnCentre") || name.Contains("StartingPoint") || name.Contains("PostGame"))
                {
                    mapManager.HideLocationMarker(locationVariable);
                    //set to unvisited
                    locationVariable.Value.LocationStatus = LoGaCulture.LUTE.LocationStatus.Unvisited;
                }
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
        }
        centering = GameObject.Find("PlayerTarget").GetComponent<ImmediatePositionWithLocationProvider>();

        GetEngine().GetAbstractMap().SetZoom(16.5f);

        // Start centering the map to the screen every second
        StartCoroutine(centerToScreenEverySecond());


        //get the variable dateInApp
        var dateInApp = GetEngine().GetVariable<StringVariable>("dateInApp");
        if (dateInApp != null)
        {
            //get todays date and set it to the variable
            string today = System.DateTime.Now.ToString("dd-MM-yyyy");
            dateInApp.Value = today;
        }

        LogaManager.Instance.LogManager.Log(LogLevel.Info,"App Version: "+Application.version);

        //Continue();
        //return;

        if (TinySave.loadGame)
        {
            //GetEngine().StopNode("Node");

            //get the last order from the node this order is in
            var node = GetEngine().FindNode(ParentNode._NodeName);
            var lastOrder = node.OrderList.Last<Order>();
            var lastNodeSeen = GetEngine().FindNode(TinySave.LastNodeSeen);

          
            if (lastOrder is NextNode nextOrder)
            {

               

                if (TinySave.LastNodeSeen.Contains("Neolithic"))
                {
                  
                    var changeToNeolithicNode = GetEngine().FindNode("Change to Neolithic");
                    nextOrder.targetNode = changeToNeolithicNode;

                    //get the ChangeToNeolithic order and execute it

                    ChangeToNeolithic.skipToLoadedNode = true;


                }
                else if(TinySave.LastNodeSeen.Contains("Mid"))
                {
                  

                    var changeToMiddleAgesNode = GetEngine().FindNode("Change to Middle");


                    nextOrder.targetNode = changeToMiddleAgesNode;

                    //Continue();
                    //return;
                    //get the ChangeToMiddleAges order and execute it
                    ChangeToMiddleAges.skipToLoadedNode = true;
                }
                else
                {
     
                    nextOrder.targetNode = lastNodeSeen;
                }

                
            }
            if (lastNodeSeen.TargetKeyNode != null)
            {
                lastNodeSeen.TargetKeyNode.TargetUnlockNode = null;
                lastNodeSeen.TargetKeyNode = null;
            }

            //TinySave.Instance.LoadMessages();
            TinySave.Instance.LoadEngineVariables();
            //TinySave.Instance.LoadAllStoneData();

        }


        if (!TinySave.HasPlayedBefore)
        {
            //get the basicflowengine
            BasicFlowEngine basicFlowEngine = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>();
            basicFlowEngine.ExecuteNode("FirstPlay");

            //save the game state
            //Save();
        }

        

        Debug.Log("Initialised everything");

        // Continue to the next order
        Continue();
        //return;
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

    //on quit game, reenable all location markers
    private void OnApplicationQuit()
    {
        Debug.Log("Application quitting, re-enabling all location markers.");

        var locationVariables = GetEngine().GetVariables<LocationVariable>();
        var mapManager = GetEngine().GetMapManager();
        foreach (var locationVariable in locationVariables)
        {
            locationVariable.Value.LocationDisabled = false;
        }
    }

    public override string GetSummary()
    {
        // Return a summary of the order which is displayed in the inspector of the order
        return "This order will initialise everything needed";
    }
}
