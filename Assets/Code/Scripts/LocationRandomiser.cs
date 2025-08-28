using JetBrains.Annotations;
using LoGaCulture.LUTE;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

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
                LUTELocationInfo location = Array.Find(locationInfos, x => x.name.ToLower().Contains(quadrant) && x.name.StartsWith(number));
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
                    targetLocation.Value = location;
                }
            }

            var mapManager = basicFlowEngine.GetMapManager();

            //if the digit is not 1, then hide it and it's not 10 or any other number that has a digit in it
            if (!name.Contains("1") || name.Contains("10") || name.Contains("11") || name.Contains("12") || name.Contains("13") || name.Contains("14"))
            {
                //if it's not the LastSeenLocation and TargetLocation
                if (name.Contains("LastSeenLocation") || name.Contains("TargetLocation"))
                {
                    continue;
                }

                //hide the location marker
                mapManager.HideLocationMarker(variable);

            }
            else
            {
                //show the location marker
                mapManager.ShowLocationMarker(variable);
            }
        }
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
                    LUTELocationInfo location = Array.Find(locationInfos, x => x.name.ToLower().Contains(quadrant) && x.name.StartsWith(number));
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


    private int numberOfTries = 0;

    public List<InterfaceGameEvent> interfaceGameEvent;

    public void UnreachableTargetLocation()
    {


        Animator animator = GameObject.Find("ICantGetThere").GetComponent<Animator>();

        animator.Play("ICGTClose");


        if (numberOfTries == 0)
        {

            //get the "CurrentLocation" variable from the basic flow engine
            LocationVariable targetLocation = basicFlowEngine.GetVariable<LocationVariable>("TargetLocation");

            //if the target location is null, then pick a random one by getting the last seen location and adding one
            if (targetLocation == null)
            {
                lastSeenLocation = basicFlowEngine.GetVariable<LocationVariable>("LastSeenLocation");

                targetLocation.Value = RandomiseNextLocation(lastSeenLocation.Value);


            }


            var randomNext = RandomiseCurrentLocation(targetLocation.Value);


            targetLocation.Value = randomNext;
            numberOfTries++;
            //print the name of the location
            Debug.Log("Current Location: " + targetLocation.Value.name);
        }
        else
        {
            numberOfTries = 0;

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

            currentLocationId++;



            targetLocation.Value = randomNext;

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
