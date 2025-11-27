using JetBrains.Annotations;
using LoGaCulture.LUTE;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using static LocationRandomiser;

public class LocationRandomiser : MonoBehaviour
{
    [SerializeField]
    public BasicFlowEngine basicFlowEngine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //a list of location variables
    LocationVariable[] locationVariables;

    LUTELocationInfo[] locationInfos;

    [SerializeField]
    public bool debugMode = true;


    //public LUTELocationInfo currentLocation;
    public LocationVariable targetLocation;
    public LocationVariable lastSeenLocation;

    private int[] forcedFirstIds;


    public bool southQuadrant = false;

    static LocationRandomiser instance;

    static int currentLocationId = 0;
    public static LocationRandomiser Instance
    {
        get
        {
            if (instance == null)
            {
                throw new Exception("LocationRandomiser instance is null. Make sure it is initialized.");
            }
            return instance;
        }
    }


    public enum SkipResultType { AlternateSameId, AdvancedNextId, Completed }


    public void changeQuadrant()
    {
        LocationRandomiser.Instance.southQuadrant = !LocationRandomiser.Instance.southQuadrant;

        //filter locations based on quadrant, assume south quadrant
        string quadrant = southQuadrant ? "south" : "north";
        if (debugMode)
        {
            quadrant = "campus";
        }
        //get all location variables
        List<LocationVariable> locationVariables = basicFlowEngine.GetVariables<LocationVariable>();
        //set the value of each variable to the first location in the list that contains the quadrant

        Compass compassToUpdate = GameObject.FindAnyObjectByType<Compass>();

      

        foreach (var variable in locationVariables)
        {
            //get the name of the variable
            string name = variable.Key;
            //check if it contains a number
            Match match = Regex.Match(name, @"\d+$");
            if (match.Success)
            {
                //connect the last digit of the name, to the first digit of the locations
                //so, if the name is StartingLocation1, then set the value to the first location in the list that is south AND has 1 at the start of the name
                //get the last digit of the name
                string number = match.Value;
                //get the first location in the list that contains the quadrant and has the same id
                //LUTELocationInfo location = Array.Find(locationInfos, x => x.name.ToLower().Contains(quadrant) && x.name.StartsWith(number + "."));

                var location = GetOppositeLocation(variable.Value);


                //set the value of the variable to that location
                if (location != null)
                {
                    variable.Value = location;
                    if (compassToUpdate != null)
                    {

                        if(number == "8" )
                        {
                            compassToUpdate.middleAgeTargets[0].location = location;
                            Debug.Log("SET THE NPC NUMBER " + number + " TO " + location.name);

                        }
                        if(number == "9")
                        {
                            compassToUpdate.middleAgeTargets[1].location = location;
                            Debug.Log("SET THE NPC NUMBER " + number + " TO " + location.name);
                        }
                        if( number =="10")
                        {
                            compassToUpdate.middleAgeTargets[2].location = location;
                            Debug.Log("SET THE NPC NUMBER " + number + " TO " + location.name);
                        }


                    }

                }
            }

            //the first one should be set to the location that has barn in the name
            if (name.Contains("StartingLocation1"))
            {
                //get the first location in the list that contains barn in the name
                LUTELocationInfo location = Array.Find(locationInfos, x => x.name.ToLower().Contains("barn"));
                //set the value of the variable to that location
                if (location != null)
                {
                    variable.Value = location;
                    //set the last seen location to that as well
                    lastSeenLocation.Value = location;
                    var flippedTarget = GetOppositeLocation(targetLocation.Value);
                    targetLocation.Value = flippedTarget;
                }
            }

            var mapManager = basicFlowEngine.GetMapManager();

            //if the digit is not 1, then hide it and it's not 10 or any other number that has a digit in it
            if (!name.Contains("1") || name.Contains("10") || name.Contains("11") || name.Contains("12") || name.Contains("13") || name.Contains("14"))
            {
                //if it's not the LastSeenLocation and TargetLocation
                if (name.Contains("LastSeenLocation"))
                {

                    

                    continue;
                }
                if(name.Contains("TargetLocation"))
                {
                    //mapManager.ShowLocationMarker(variable);
                    continue;
                }
                

                //hide the location marker
                mapManager.HideLocationMarker(variable);

            }
            else
            {
                //show the location marker
                //var flippedTarget = GetOppositeLocation(variable.Value);
                //variable.Value = flippedTarget;

                mapManager.ShowLocationMarker(variable);
            }


        }


    }


    private int skipStage = 0;

    public LocationVariable GetLocationVariableFromLocationInfo(LUTELocationInfo location)
    {
        var locationVariables = basicFlowEngine.GetVariables<LocationVariable>();
        foreach (var locVar in locationVariables)
        {
            if (locVar.Value == location)
            {
                return locVar;
            }
        }
        return null;
    }

    public LocationVariable GetFlowEngineLocationVariableFromLocationInfo(LUTELocationInfo location)
    {
        //location has a name like "1.1-NameOfPlace"
        //extract the first number before the dot
        string id_full = location.name.Split('-')[0];
        string id = id_full.Split('.')[0];
        return GetLocationVariableWithID(int.Parse(id));
    }

    public LUTELocationInfo VisitLocation(LUTELocationInfo location)
    {
        LUTELocationInfo nextLocation;



        nextLocation = GetNextRandomLocation(location);

        location.LocationStatus = LocationStatus.Visited;


        lastSeenLocation.Value = location;
        targetLocation.Value = nextLocation;

        LocationVariable currentLocationVar = GetFlowEngineLocationVariableFromLocationInfo(nextLocation);

        if (currentLocationVar != null)
        {
            currentLocationVar.Value = nextLocation;

            basicFlowEngine.GetMapManager().HideLocationMarker(currentLocationVar);
        }
            //nextLocation.LocationDisabled = true;
        //to do, better way to disable the location so it doesn't appear on the map again

        skipStage = 0;

        //todo CLOSE the ICan'tGetThere panel after a few seconds
        StartCoroutine(updateText("I can't get there!"));
        




        //get the ICantGetThere/Mask/Panel object and the script LocationClickHandler on it
        //LocationClickHandler locationClickHandler = GameObject.Find("ICantGetThere/Mask/Panel").GetComponent<LocationClickHandler>();
        //locationClickHandler.resetClickCount();



        return nextLocation;


        //switch(skipStage)
        // {
        //     case 0:
        //         //normal visit, no skipping
        //         //get a random next location
        //         nextLocation = GetNextRandomLocation();
        //         lastSeenLocation.Value = location;
        //         targetLocation.Value = nextLocation;

        //         return nextLocation;

        //     case 1:
        //         //they clicked skip once, but visited the second location
        //         skipStage = 0;
        //         nextLocation = GetNextRandomLocation();
        //         lastSeenLocation.Value = location;
        //         targetLocation.Value = nextLocation;
        //         return nextLocation;

        //     case 2:
        //     default:
        //         //they clicked skip twice, so advance to the next id
        //         skipStage = 0;
        //         nextLocation = GetNextRandomLocation();
        //         lastSeenLocation.Value = location;
        //         targetLocation.Value = nextLocation;
        //         return nextLocation;

        // }



    }


    public SkipResult SkipCurrentLocation()
    {
        var current = targetLocation.Value;

        var alternate = RandomiseCurrentLocation(current);

        if (alternate == null || alternate == current)
        {
            return new SkipResult { Type = SkipResultType.AlternateSameId, NewTarget = current, LastSeen = lastSeenLocation.Value };
        }

        targetLocation.Value = alternate;
        skipStage = 1;
        return new SkipResult { Type = SkipResultType.AlternateSameId, NewTarget = alternate, LastSeen = lastSeenLocation.Value };
    }


    public LUTELocationInfo GetOppositeLocation(LUTELocationInfo current)
    {
        if (current == null)
        {
            Debug.LogWarning("GetOppositeLocation: current is null.");
            return null;
        }

        // Ensure locations are loaded
        if (locationInfos == null || locationInfos.Length == 0)
        {
            locationInfos = Resources.LoadAll<LUTELocationInfo>("Locations");
            if (locationInfos == null || locationInfos.Length == 0)
            {
                Debug.LogWarning("GetOppositeLocation: No locationInfos loaded.");
                return null;
            }
        }

        string currentName = current.name;
        string currentLower = currentName.ToLowerInvariant();

        // Determine current and target quadrant
        bool isSouth = currentLower.Contains("south");
        bool isNorth = currentLower.Contains("north");
        if (!isSouth && !isNorth)
        {
            Debug.LogWarning($"GetOppositeLocation: '{currentName}' doesn't contain 'north' or 'south'.");
            return null;
        }
        string targetQuadrant = isSouth ? "north" : "south";

        // Extract id prefix (before '-'), e.g., "2.2"
        string idPrefix;
        int hyphenIdx = currentName.IndexOf('-');
        if (hyphenIdx >= 0)
            idPrefix = currentName.Substring(0, hyphenIdx);
        else
        {
            // Fallback: read leading number or number.number
            var m = Regex.Match(currentName, @"^\d+(\.\d+)?");
            idPrefix = m.Success ? m.Value : string.Empty;
        }

        // Find candidates with same id and opposite quadrant
        LUTELocationInfo[] candidates = Array.FindAll(locationInfos, x =>
            x != null &&
            (string.IsNullOrEmpty(idPrefix) || x.name.StartsWith(idPrefix)) &&
            x.name.ToLowerInvariant().Contains(targetQuadrant));

        if (candidates.Length == 0)
        {
            Debug.Log($"GetOppositeLocation: No opposite candidates for id '{idPrefix}' and quadrant '{targetQuadrant}'.");
            return null;
        }

        // Prefer exact same tail (after '-') ignoring quadrant token
        string currentTail = hyphenIdx >= 0 ? currentName.Substring(hyphenIdx + 1) : currentName;
        string currentBase = Regex.Replace(currentTail, "(?i)(north|south)", "").Trim();

        foreach (var cand in candidates)
        {
            int h = cand.name.IndexOf('-');
            string candTail = h >= 0 ? cand.name.Substring(h + 1) : cand.name;
            string candBase = Regex.Replace(candTail, "(?i)(north|south)", "").Trim();

            if (string.Equals(candBase, currentBase, StringComparison.OrdinalIgnoreCase))
                return cand;
        }

        // Fallback: first candidate
        return candidates[0];
    }


    //function to set the last seen and target to the first NPC in the specified quadrant
    public void SetLastSeenAndTargetToFirstNPC()
    {


        lastSeenLocation.Value = GetLocationWithID(7);
        targetLocation.Value = GetLocationWithID(8);


    }

    void Start()
    {

        instance = this;

        forcedFirstIds = new int[] {5};


        LUTEMapManager mapManager = basicFlowEngine.GetMapManager();

        locationInfos = Resources.LoadAll<LUTELocationInfo>("Locations");

        targetLocation = basicFlowEngine.GetVariable<LocationVariable>("TargetLocation");
        lastSeenLocation = basicFlowEngine.GetVariable<LocationVariable>("LastSeenLocation");

        //currentLocation = locationInfos[0];

        //print the names
        //foreach (var locationInfo in locationInfos)
        //{
        //    Debug.Log(locationInfo.name);
        //}

        //var listOfVariables = basicFlowEngine.GetVariables<LocationVariable>();

        ////print the names
        //foreach (var variable in listOfVariables)
        //{
        //    Debug.Log(variable.Key + " " + variable.Value.LocationName);
        //    //disable the location
        //    mapManager.HideLocationMarker(variable);

        //}


        //if it's the debug mode, then get all location variables, and get all locations
        //set each variable to the south quadrant, and set the location to the first one in the list
        //variables are StartingLocation1, EmptyPit2, Remnant3, BuriedStone4, Portal5,FirstStoneCreation6, SecondStoneCreation7, SocialStoneCreation8
        //and the locations are in the resources folder, in the format x.y-NameOfPlace, where x is the id of the location, and y is the sub id in case there are multiple places that have the same function
        //so, if debug on pick the ones that contain campus, otherwise, check if south or north, and filter the locations based on that
        if (debugMode)
        {
            //get all location variables
            List<LocationVariable> locationVariables = basicFlowEngine.GetVariables<LocationVariable>();
            //set the value of each variable to the first location in the list that contains campus
            foreach (var variable in locationVariables)
            {
                //get the name of the variable
                string name = variable.Key;
                //check if it contains a number
                Match match = Regex.Match(name, @"\d+$");
                if (match.Success)
                {
                    //connect the last digit of the name, to the first digit of the locations
                    //so, if the name is StartingLocation1, then set the value to the first location in the list that is campus AND has 1 at the start of the name
                    //get the last digit of the name
                    string number = match.Value;
                    //get the first location in the list that contains campus and has the same id
                    LUTELocationInfo location = Array.Find(locationInfos, x => x.name.ToLower().Contains("campus") && x.name.StartsWith(number));
                    //set the value of the variable to that location
                    if (location != null)
                    {
                        variable.Value = location;

                    }
                }
            }
        }
        else
        {
            //filter locations based on quadrant, assume south quadrant
            string quadrant = southQuadrant ? "south" : "north";
            //get all location variables
            List<LocationVariable> locationVariables = basicFlowEngine.GetVariables<LocationVariable>();
            //set the value of each variable to the first location in the list that contains the quadrant
            foreach (var variable in locationVariables)
            {
                //get the name of the variable
                string name = variable.Key;
                //check if it contains a number
                Match match = Regex.Match(name, @"\d+$");
                if (match.Success)
                {
                    //connect the last digit of the name, to the first digit of the locations
                    //so, if the name is StartingLocation1, then set the value to the first location in the list that is south AND has 1 at the start of the name
                    //get the last digit of the name
                    string number = match.Value;
                    //get the first location in the list that contains the quadrant and has the same id
                    LUTELocationInfo location = Array.Find(locationInfos, x => x.name.ToLower().Contains(quadrant) && x.name.StartsWith(number + "."));
                    //set the value of the variable to that location
                    if (location != null)
                    {
                        variable.Value = location;


                    }
                }

                //the first one should be set to the location that has barn in the name
                if (name.Contains("StartingLocation1"))
                {
                    //get the first location in the list that contains barn in the name
                    LUTELocationInfo location = Array.Find(locationInfos, x => x.name.ToLower().Contains("barn"));
                    //set the value of the variable to that location
                    if (location != null)
                    {
                        variable.Value = location;
                        //set the last seen location to that as well
                        lastSeenLocation.Value = location;
                        targetLocation.Value = location;
                    }
                }
            }
        }

    }


    public LocationVariable GetLocationVariableWithID(int id)
    {
        if (basicFlowEngine == null)
        {
            Debug.LogWarning("BasicFlowEngine is null in GetLocationVariableWithID.");
            return null;
        }

        var vars = basicFlowEngine.GetVariables<LocationVariable>();
        if (vars == null || vars.Count == 0)
        {
            return null;
        }

        string idStr = id.ToString();
        LocationVariable firstMatch = null;
        int matchCount = 0;

        foreach (var v in vars)
        {
            if (v == null || string.IsNullOrEmpty(v.Key))
                continue;

            // Match trailing digits and compare to the requested id
            Match m = Regex.Match(v.Key, @"\d+$");
            if (m.Success && m.Value == idStr)
            {
                matchCount++;
                if (firstMatch == null)
                    firstMatch = v;
            }
        }

        if (matchCount > 1)
        {
            Debug.LogWarning($"Multiple LocationVariables end with id '{id}'. Returning the first match: {firstMatch.Key}");
        }

        return firstMatch;
    }


    /// <summary>
    /// Returns a random location from the next id group (e.g. from 2.x after being at 1.x).
    /// Unlike GetNextNormalLocation() this considers all sub-id variants (e.g. 3.1, 3.2, 3.3)
    /// instead of forcing ".1". Falls back to the last seen location if none are found.
    /// </summary>
    public LUTELocationInfo GetNextRandomLocation(LUTELocationInfo currentLocation)
    {
        // Refresh last seen
        lastSeenLocation = basicFlowEngine.GetVariable<LocationVariable>("LastSeenLocation");
        if (lastSeenLocation?.Value == null)
        {
            Debug.LogWarning("LastSeenLocation variable or its value is null.");
            return null;
        }

        // Extract current numeric id (before any dot)
        string idFull = currentLocation.name.Split('-')[0];     // e.g. "2.1"
        string idNumericPart = idFull.Split('.')[0];                   // e.g. "2"

        // Compute next id
        if (!int.TryParse(idNumericPart, out int currentId))
        {
            Debug.LogWarning("Failed to parse current location id: " + idNumericPart);
            return currentLocation;
        }
        int nextId = currentId + 1;
        string nextIdPrefix = nextId.ToString(); // We intentionally do NOT append ".1" so we can fetch all 3.x variants.

        // Determine quadrant filter
        string quadrant = southQuadrant ? "south" : "north";
        if (debugMode) quadrant = "campus";

        // Filter all locations in the quadrant
        LUTELocationInfo[] filtered = Array.FindAll(locationInfos, x =>
            x != null && x.name.ToLower().Contains(quadrant));

        // Get all locations whose name starts with the next id prefix (allows 3.1, 3.2, 3.5, etc.)
        LUTELocationInfo[] candidates = Array.FindAll(filtered, x =>
            x != null && x.name.StartsWith(nextIdPrefix));

        if (candidates.Length == 0)
        {
            Debug.Log("No locations found for next id " + nextIdPrefix);
            return currentLocation;
        }

        // If this id is in forceFirstIds, prefer the ".1" variant (no tracking — always prefer .1)
        bool isForced = forcedFirstIds != null && Array.IndexOf(forcedFirstIds, nextId) >= 0;
        if (isForced)
        {
            LUTELocationInfo primary = Array.Find(candidates, c => c.name.StartsWith(nextIdPrefix + ".1"));
            if (primary != null)
            {
                Debug.Log("Forcing selection of primary .1 location: " + primary.name);

                return primary;
            }
            // if no .1 exists, fall through to random selection
        }


        // Pick a random candidate
        int randomIndex = UnityEngine.Random.Range(0, candidates.Length);
        LUTELocationInfo chosen = candidates[randomIndex];

        //// Optionally update a matching LocationVariable (mirrors original pattern)
        //List<LocationVariable> locationVariables = basicFlowEngine.GetVariables<LocationVariable>();
        //foreach (var locVar in locationVariables)
        //{
        //    // Match the variable key with the numeric id we are moving to
        //    if (locVar.Key.Contains(nextIdPrefix))
        //    {
        //        locVar.Value = chosen;
        //        break;
        //    }
        //}

        return chosen;
    }

    public LUTELocationInfo GetNextNormalLocation()
    {
        //get the last seen location
        lastSeenLocation = basicFlowEngine.GetVariable<LocationVariable>("LastSeenLocation");
        //the next normal location is, if the current location is in the format of "1.1-NameOfPlace", the next one will be "2.1-NameOfPlace", or 1.2 goes to 2.2
        //so, get the id of the current location
        string id_full = lastSeenLocation.Value.name.Split('-')[0];
        //split by the dot
        string id = id_full.Split('.')[0];
        //increment the id by 1
        int newId = int.Parse(id) + 1;

        //get the whole new id string with the sub id (sometimes the sub id is not there, so be sure to not do it if it is not there
        string newId_full = newId.ToString();
        //check if the current id has a sub id
        if (id_full.Contains("."))
        {
            //get the sub id
            string subId = id_full.Split('.')[1];
            //add it to the new id
            newId_full += ".1";
        }
        //get the new id



        //filter locations based on quadrant, assume south quadrant
        string quadrant = southQuadrant ? "south" : "north";

        if (debugMode)
        {
            quadrant = "campus";
        }

        //
        LUTELocationInfo[] filteredLocations = Array.FindAll(locationInfos, x => x.name.ToLower().Contains(quadrant));
        //get all locations with the same id within the filtered locations
        LUTELocationInfo[] locationsWithSameId = Array.FindAll(filteredLocations, x => x.name.StartsWith(newId_full.ToString()));

        //go to all location variables, 
        List<LocationVariable> locationVariables = basicFlowEngine.GetVariables<LocationVariable>();
        // get the locationvariable name that has the same id
        //basically, the locaitno variable is in the format of "NamerOfPlaceX" where x is the order of each location variable, so we want to get the new location variable name

        //we want to get that location variable that matches the newId, and replace the value it's pointing at with the locationsWithSameId[index]

        LocationVariable locationVariable = null;
        foreach (var locationVar in locationVariables)
        {
            //get the name of the location variable
            string name = locationVar.Key;
            //check if it contains the new id
            if (name.Contains(id.ToString()))
            {
                //if it does, set the location variable to that
                locationVariable = locationVar;
                break;
            }
        }

        //locationVariable.Value = locationsWithSameId[0];


        //print the location Value name of both the locationVariable and the lastseen location
        //Debug.Log("Location Variable: " + locationVariable.Value.name);
        //Debug.Log("Last Seen Location: " + lastSeenLocation.Value.name);

        //pick the first one that in the array
        if(locationsWithSameId.Length == 0)
        {
            Debug.Log("No locations with the same id found");
            return lastSeenLocation.Value;
        }
        else
        {
            return locationsWithSameId[0];
        }


    }

    public void SetAllLocationsToEnabled()
    {
        ////go through all of the locations and set them to not visited
        ////get all the locations from the resources
        //var locations = Resources.LoadAll<LUTELocationInfo>("Locations");
        //foreach (var location in locations)
        //{
        //    //set the status to unvisited
        //    location.LocationStatus = LocationStatus.Unvisited;

        //}


        var locationVariables = basicFlowEngine.GetVariables<LocationVariable>();
        var mapManager = basicFlowEngine.GetMapManager();
        foreach (var locationVariable in locationVariables)
        {
            locationVariable.Value.LocationDisabled = false;
        }

    }

    //on destroy, go through all of the locations, and set them to not visited
    private void OnDestroy()
    {
     
        SetAllLocationsToEnabled();

    }
    

    public void RestartTries()
    {
        numberOfTries = 0;
    }

    private int numberOfTries = 0;

    public List<InterfaceGameEvent> interfaceGameEvent;

    public bool unreachedLocation = false;

    /// <summary>
    /// Extracts the trailing numeric id from a LocationVariable key (e.g. "Remnant3", "NPC10").
    /// Returns -1 if the key doesn't end with digits.
    /// </summary>
    public int GetIdFromVariableKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return -1;

        Match m = Regex.Match(key, @"(\d+)$");
        if (m.Success && int.TryParse(m.Value, out int id))
            return id;

        return -1;
    }


    public TextMeshProUGUI iCantGetThereText;
    public GameObject cantGetThereObject;
    public Animator animator;
    private IEnumerator updateText(string text)
    {
        //wait 1 second
        yield return new WaitForSeconds(1f);
        iCantGetThereText.text = text;

    }

    private IEnumerator PlayAnimationAndUpdateText()
    {
        yield return new WaitForSeconds(3f); // Wait before starting animation

        //animator.Play("ICGTClose");

        // Wait for animation to finish
        float duration = GetAnimationLength("ICGTClose");
        yield return new WaitForSeconds(duration + 3);


        iCantGetThereText.text = "I can't get there!";

        if (cantGetThereObject != null)
        {
            cantGetThereObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameObject 'ICantGetThere' not assigned.");
        }
    }


    static int middlePeriodSkipCount = 0;
    public void MiddlePeriodSkip()
    {



        //Mid NPC01, Mid NPC02, Mid NPC03 are the node names
        //when called, check the middlePeriodSkipCount
        //on first call, execute node 01, then increment the count
        //on second call, execute node 02, then increment the count, etc

        if(middlePeriodSkipCount == 0)
        {
            basicFlowEngine.ExecuteNode("Mid NPC01");
        }
        else if(middlePeriodSkipCount == 1)
        {
            basicFlowEngine.ExecuteNode("Mid NPC02");
        }
        else if(middlePeriodSkipCount == 2)
        {
            basicFlowEngine.ExecuteNode("Mid NPC03");
        }

        middlePeriodSkipCount++;


    }

    private float GetAnimationLength(string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 1f;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == stateName)
                return clip.length;
        }

        Debug.LogWarning($"Animation '{stateName}' not found. Defaulting to 1 second.");
        return 1f;
    }

    public void UnreachableTargetLocation()
    {


        Animator animator = GameObject.Find("ICantGetThere").GetComponent<Animator>();
        LocationVariable currentLocationVar;
        animator.Play("ICGTClose");

        Debug.Log("Skip stage is " + skipStage);


        switch (skipStage)
        {
            case 0:
                //this is the first time they click skip
                //just pick an alternate location with the same id
                skipStage = 1;

                var randomCurrent = RandomiseCurrentLocation(targetLocation.Value);
                targetLocation.Value = randomCurrent;

                basicFlowEngine.GetMapManager().ShowLocationMarker(targetLocation);

                currentLocationVar = GetFlowEngineLocationVariableFromLocationInfo(randomCurrent);
                currentLocationVar.Value = randomCurrent;

                StartCoroutine(updateText("I can't get there either!"));

                break;
            case 1:
            default:
              
                //they clicked skip twice , so just pick a random next location
                skipStage = 0;

                var randomNext = GetNextRandomLocation(targetLocation.Value);

                lastSeenLocation.Value = targetLocation.Value;


                //extract the id from the target locaiton.value (the first character is the digit)
                //get the id of target location
                string id_full_target = targetLocation.Value.name.Split('-')[0];
                //split by the dot
                string id_target = id_full_target.Split('.')[0];

                //to int
                int intIdTarget = int.Parse(id_target);
                currentLocationId = intIdTarget;

                LocationVariable currentLocationVariable = GetLocationVariableWithID(currentLocationId);


                currentLocationVariable.Value = lastSeenLocation.Value;
                lastSeenLocation.Value.LocationStatus = LocationStatus.Visited;
                //currentLocationVariable.Value.LocationStatus = LocationStatus.Completed;
                //to do for portal5

                Debug.Log("Changed current from " + lastSeenLocation.Value.name + " to " + currentLocationVariable.Value.name);



                targetLocation.Value = randomNext;
                basicFlowEngine.GetMapManager().HideLocationMarker(targetLocation);


                currentLocationVar = GetFlowEngineLocationVariableFromLocationInfo(randomNext);
                currentLocationVar.Value = randomNext;

                basicFlowEngine.GetMapManager().HideLocationMarker(currentLocationVar);


                //iCantGetThereText.text = "I can't get there!";

                if (animator != null)
                {
                    StartCoroutine(PlayAnimationAndUpdateText());
                }
                else
                {
                    Debug.LogWarning("Animator not assigned.");
                }


                string id_full = lastSeenLocation.Value.name.Split('-')[0];
                //split by the dot
                string id = id_full.Split('.')[0];

                //to int
                int newId = int.Parse(id) - 1;

                interfaceGameEvent[newId].Raise();

                break;

              
        }



        //if (numberOfTries == 0)
        //{

        //    //get the "CurrentLocation" variable from the basic flow engine
        //    LocationVariable targetLocation = basicFlowEngine.GetVariable<LocationVariable>("TargetLocation");

        //    //if the target location is null, then pick a random one by getting the last seen location and adding one
        //    if (targetLocation == null)
        //    {
        //        lastSeenLocation = basicFlowEngine.GetVariable<LocationVariable>("LastSeenLocation");

        //        targetLocation.Value = RandomiseNextLocation(lastSeenLocation.Value);


        //    }


        //    var randomNext = RandomiseCurrentLocation(targetLocation.Value);


        //    unreachedLocation = true;

        //    targetLocation.Value = randomNext;

        //    basicFlowEngine.GetMapManager().ShowLocationMarker(targetLocation);

        //    numberOfTries++;
        //    //print the name of the location
        //    Debug.Log("Current Location: " + targetLocation.Value.name);
        //}
        //else
        //{
        //    numberOfTries = 0;

        //    //get the "CurrentLocation" variable from the basic flow engine
        //    LocationVariable targetLocation = basicFlowEngine.GetVariable<LocationVariable>("TargetLocation");

        //    //if the target location is null, then pick a random one by getting the last seen location and adding one
        //    if (targetLocation == null)
        //    {
        //        lastSeenLocation = basicFlowEngine.GetVariable<LocationVariable>("LastSeenLocation");
        //        targetLocation.Value = RandomiseNextLocation(lastSeenLocation.Value);
        //    }

        //    lastSeenLocation.Value = targetLocation.Value;

        //    //var randomNext = GetNextNormalLocation();
        //    var randomNext = GetNextRandomLocation();

        //    Debug.Log("The random next location is: " + randomNext.name);

        //    unreachedLocation = true;

        //    //extract the id from the target locaiton.value (the first character is the digit)
        //    //get the id of target location
        //    string id_full_target = targetLocation.Value.name.Split('-')[0];
        //    //split by the dot
        //    string id_target = id_full_target.Split('.')[0];

        //    //to int
        //    int intIdTarget = int.Parse(id_target);

        //    currentLocationId = intIdTarget;


        //    //currentLocationId++;

        //    LocationVariable currentLocationVariable = GetLocationVariableWithID(currentLocationId);

        //    targetLocation.Value = randomNext;

        //    int nextLocationId = GetIdFromVariableKey(currentLocationVariable.Key);

        //    nextLocationId++;


        //    currentLocationVariable.Value = lastSeenLocation.Value;

        //    Debug.Log("Changed current from " + lastSeenLocation.Value.name + " to " + currentLocationVariable.Value.name);


        //    //TODO fix later
        //    if (currentLocationVariable.Key == "Portal5")
        //    {

        //        currentLocationVariable.Value.LocationStatus = LocationStatus.Completed;
        //    }

        //    LocationVariable fullLocationVariable = GetLocationVariableWithID(nextLocationId);
        //    Debug.Log("Key is " + fullLocationVariable.Key);

        //    fullLocationVariable.Value = randomNext;


        //    basicFlowEngine.GetMapManager().HideLocationMarker(fullLocationVariable);


        //    //get the id of target location
        //    string id_full = lastSeenLocation.Value.name.Split('-')[0];
        //    //split by the dot
        //    string id = id_full.Split('.')[0];

        //    //to int
        //    int newId = int.Parse(id) - 1;

        //    //raise the event
        //    interfaceGameEvent[newId].Raise();


        //    //print the name of the location
        //    Debug.Log("Next Location is Location: " + targetLocation.Value.name);

        //}
    }

    public void setCorrectLocations()
    {
        //get the "CurrentLocation" variable from the basic flow engine
        LocationVariable targetLocation = basicFlowEngine.GetVariable<LocationVariable>("TargetLocation");

        //if the target location is null, then pick a random one by getting the last seen location and adding one
        if (targetLocation == null)
        {
            lastSeenLocation = basicFlowEngine.GetVariable<LocationVariable>("LastSeenLocation");
            targetLocation.Value = RandomiseNextLocation(lastSeenLocation.Value);
        }

        lastSeenLocation.Value = targetLocation.Value;

        //var randomNext = GetNextNormalLocation();
        var randomNext = GetNextRandomLocation(targetLocation.Value);

        Debug.Log("The random next location is: " + randomNext.name);

        unreachedLocation = true;

        //randomNext.
        //extract the id from the target location
        int currentLocationId = GetIdFromVariableKey(targetLocation.Key);

        currentLocationId++;

        LocationVariable currentLocationVariable = GetLocationVariableWithID(currentLocationId);

        targetLocation.Value = randomNext;

        int nextLocationId = GetIdFromVariableKey(currentLocationVariable.Key);

        nextLocationId++;


        currentLocationVariable.Value = lastSeenLocation.Value;

        Debug.Log("Changed current from " + lastSeenLocation.Value.name + " to " + currentLocationVariable.Value.name);


        //TODO fix later
        if (currentLocationVariable.Key == "Portal5")
        {

            currentLocationVariable.Value.LocationStatus = LocationStatus.Completed;
        }

        LocationVariable fullLocationVariable = GetLocationVariableWithID(nextLocationId);
        Debug.Log("Key is " + fullLocationVariable.Key);

        fullLocationVariable.Value = randomNext;


        basicFlowEngine.GetMapManager().HideLocationMarker(fullLocationVariable);


        //get the id of target location
        string id_full = lastSeenLocation.Value.name.Split('-')[0];
        //split by the dot
        string id = id_full.Split('.')[0];

        //to int
        int newId = int.Parse(id) - 1;

        //raise the event
        interfaceGameEvent[newId].Raise();


        //print the name of the location
        Debug.Log("Next Location is Location: " + targetLocation.Value.name);

    }

    public void skipDirectlyToNextNode()
    {
        //get the "CurrentLocation" variable from the basic flow engine
        LocationVariable targetLocation = basicFlowEngine.GetVariable<LocationVariable>("TargetLocation");
        //if the target location is null, then pick a random one by getting the last seen location and adding one
        if (targetLocation == null)
        {
            lastSeenLocation = basicFlowEngine.GetVariable<LocationVariable>("LastSeenLocation");
            targetLocation.Value = RandomiseNextLocation(lastSeenLocation.Value);
        }
        lastSeenLocation.Value = targetLocation.Value;

        var randomNext = GetNextNormalLocation();



        targetLocation.Value = randomNext;

        //get the id of target location
        string id_full = lastSeenLocation.Value.name.Split('-')[0];
        //split by the dot
        string id = id_full.Split('.')[0];

        //to int
        int newId = int.Parse(id) - 1;

        //raise the event
        interfaceGameEvent[newId].Raise();

        Debug.Log("Called interface event: " + interfaceGameEvent[newId].name);

    }

    public LUTELocationInfo RandomiseNextLocation(LUTELocationInfo currentLocation)
    {
        //get the id of the current location  
        string id_full = currentLocation.name.Split('-')[0];
        //split by the dot  
        string id = id_full.Split('.')[0];

        //increment the id by 1
        int newId = int.Parse(id) + 1;
        //filter locations based on quadrant
        string quadrant = southQuadrant ? "south" : "north";

        if (debugMode)
        {
            quadrant = "campus";
        }


        LUTELocationInfo[] filteredLocations = Array.FindAll(locationInfos, x => x.name.ToLower().Contains(quadrant));
        //get all locations with the same id within the filtered locations
        LUTELocationInfo[] locationsWithSameId = Array.FindAll(filteredLocations, x => x.name.StartsWith(newId.ToString()));
        //pick a random one that is not the current one
        int index = UnityEngine.Random.Range(0, locationsWithSameId.Length);

        if (locationsWithSameId.Length == 1)
        {
            Debug.Log("Only one location with the same id found");
            return locationsWithSameId[0];
        }

        while (locationsWithSameId[index] == currentLocation)
        {
            index = UnityEngine.Random.Range(0, locationsWithSameId.Length);
        }

        ////go to all location variables, 
        //List<LocationVariable> locationVariables = basicFlowEngine.GetVariables<LocationVariable>();
        //// get the locationvariable name that has the same id
        ////basically, the locaitno variable is in the format of "NamerOfPlaceX" where x is the order of each location variable, so we want to get the new location variable name

        ////we want to get that location variable that matches the newId, and replace the value it's pointing at with the locationsWithSameId[index]

        //LocationVariable locationVariable = null;
        //foreach (var locationVar in locationVariables)
        //{
        //    //get the name of the location variable
        //    string name = locationVar.Key;
        //    //check if it contains the new id
        //    if (name.Contains(id.ToString()))
        //    {
        //        //if it does, set the location variable to that
        //        locationVariable = locationVar;
        //        break;
        //    }
        //}

        //locationVariable.Value = locationsWithSameId[index];



        return locationsWithSameId[index];

    }


    public LUTELocationInfo GetLocationWithID(int id, bool randomise = false)
    {
        //filter locations based on quadrant  
        string quadrant = southQuadrant ? "south" : "north";
        if (debugMode)
        {
            quadrant = "campus";
        }
        LUTELocationInfo[] filteredLocations = Array.FindAll(locationInfos, x => x.name.ToLower().Contains(quadrant));
        //get all locations with the same id within the filtered locations  
        LUTELocationInfo[] locationsWithSameId = Array.FindAll(filteredLocations, x => x.name.StartsWith(id.ToString()));
        if (locationsWithSameId.Length == 0)
        {
            Debug.Log("No locations with the same id found");
            return null;
        }

        if (locationsWithSameId.Length == 1)
        {
            Debug.Log("Only one location with the same id found");
            return locationsWithSameId[0];
        }

        if (randomise)
        {
            //pick a random location from the locations with the same id and return it

            int index = UnityEngine.Random.Range(0, locationsWithSameId.Length);

            return locationsWithSameId[index];

        }

        else
        {
            return locationsWithSameId[0];
        }
    }

    public LUTELocationInfo RandomiseCurrentLocation(LUTELocationInfo currentLocation)
    {
        //get the id of the current location  
        string id_full = currentLocation.name.Split('-')[0];
        //split by the dot  
        string id = id_full.Split('.')[0];
        //filter locations based on quadrant  
        string quadrant = southQuadrant ? "south" : "north";

        if (debugMode)
        {
            quadrant = "campus";
        }

        LUTELocationInfo[] filteredLocations = Array.FindAll(locationInfos, x => x.name.ToLower().Contains(quadrant));
        //get all locations with the same id within the filtered locations  
        LUTELocationInfo[] locationsWithSameId = Array.FindAll(filteredLocations, x => {
            string[] parts = x.name.Split('-', '.');
            return parts.Length > 0 && parts[0] == id;
        });
        //pick a random one that is not the current one  
        int index = UnityEngine.Random.Range(0, locationsWithSameId.Length);

        if (locationsWithSameId.Length == 1)
        {
            Debug.Log("Only one location with the same id found");
            return locationsWithSameId[0];
        }

        if (locationsWithSameId.Length != 0)
        {

            while (locationsWithSameId[index] == currentLocation)
            {
                index = UnityEngine.Random.Range(0, locationsWithSameId.Length);
            }
        }
        //go to all location variables, 
        List<LocationVariable> locationVariables = basicFlowEngine.GetVariables<LocationVariable>();
        // get the locationvariable name that has the same id
        //basically, the locaitno variable is in the format of "NamerOfPlaceX" where x is the order of each location variable, so we want to get the new location variable name

        //we want to get that location variable that matches the newId, and replace the value it's pointing at with the locationsWithSameId[index]

        LocationVariable locationVariable = null;
        foreach (var locationVar in locationVariables)
        {
            //get the name of the location variable
            string name = locationVar.Key;
            //check if it contains the new id
            if (name.Contains(id.ToString()))
            {
                //if it does, set the location variable to that
                locationVariable = locationVar;
                break;
            }
        }
        if (locationsWithSameId.Length == 0)
        {
            Debug.Log("No locations with the same id found");
            return currentLocation;
        }
        locationVariable.Value = locationsWithSameId[index];

        return locationsWithSameId[index];

    }
    // Update is called once per frame
    void Update()
    {

        //if press r, randomise the location  
        //to do that, get the current location name, which will be in the format of "a.b-NameOfPlace" where a is the id of the location, and b is the sub id in case there are multiple places that have the same function  
        //get the id, and then get all other locations that have the same id, and pick one of them (not the current one)  
        //if (currentLocation != null)
        //{
        //    if (Input.GetKeyDown(KeyCode.R))
        //    {
        //        //get the id of the current location  
        //        string id = currentLocation.name.Split('-')[0];

        //        //split by the dot  
        //        id = id.Split('.')[0];

        //        //filter locations based on quadrant  
        //        string quadrant = southQuadrant ? "south" : "north";
        //        LUTELocationInfo[] filteredLocations = Array.FindAll(locationInfos, x => x.name.ToLower().Contains(quadrant));

        //        //get all locations with the same id within the filtered locations  
        //        LUTELocationInfo[] locationsWithSameId = Array.FindAll(filteredLocations, x => x.name.StartsWith(id));

        //        //pick a random one that is not the current one  
        //        int index = UnityEngine.Random.Range(0, locationsWithSameId.Length);
        //        while (locationsWithSameId[index] == currentLocation)
        //        {
        //            index = UnityEngine.Random.Range(0, locationsWithSameId.Length);
        //        }
        //        currentLocation = locationsWithSameId[index];
        //        //print the name of the location  
        //        Debug.Log("Current Location: " + currentLocation.name);
        //    }
        //}


        //if key is k, unreachable target location
        if (Input.GetKeyDown(KeyCode.K))
        {
            skipDirectlyToNextNode();
        }
        //if press g, go to next location  
        //similar as above, but now it's the next id up, so if the first location had an id of 1.x-, the next one will be 2.x-  
        //so, increment the id by 1, and get all locations with that id  
        if (Input.GetKeyDown(KeyCode.G))
        {

            var currentLocation = basicFlowEngine.GetVariable<LocationVariable>("TargetLocation").Value;

            //get the id of the current location  
            string id = currentLocation.name.Split('-')[0];

            //split by the dot  
            id = id.Split('.')[0];

            //increment the id by 1  
            int newId = int.Parse(id) + 1;

            //filter locations based on quadrant  
            string quadrant = southQuadrant ? "south" : "north";

            if (debugMode)
            {
                quadrant = "campus";
            }

            LUTELocationInfo[] filteredLocations = Array.FindAll(locationInfos, x => x.name.ToLower().Contains(quadrant));

            //get all locations with the same id within the filtered locations  
            LUTELocationInfo[] locationsWithSameId = Array.FindAll(filteredLocations, x => x.name.StartsWith(newId.ToString()));

            //pick a random one that is not the current one  
            int index = UnityEngine.Random.Range(0, locationsWithSameId.Length);
            while (locationsWithSameId[index] == currentLocation)
            {
                index = UnityEngine.Random.Range(0, locationsWithSameId.Length);
            }
            targetLocation.Value = locationsWithSameId[index];
            //print the name of the location  
            Debug.Log("Current Location: " + targetLocation.Value.name);
        }

    }
}

public class SkipResult
{
    public SkipResultType Type;
    public LUTELocationInfo NewTarget;   // null if Completed
    public LUTELocationInfo LastSeen;
}

