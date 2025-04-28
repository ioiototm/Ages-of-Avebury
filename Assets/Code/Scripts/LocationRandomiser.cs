using LoGaCulture.LUTE;
using System;
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


    //public LUTELocationInfo currentLocation;
    public LocationVariable targetLocation;


    public  bool southQuadrant = false;

    static LocationRandomiser instance;
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

    void Start()
    {

        instance = this;

        LUTEMapManager mapManager = basicFlowEngine.GetMapManager();

        locationInfos = Resources.LoadAll<LUTELocationInfo>("Locations");

        targetLocation = basicFlowEngine.GetVariable<LocationVariable>("TargetLocation");

        //currentLocation = locationInfos[0];

        //print the names
        foreach (var locationInfo in locationInfos)
        {
            Debug.Log(locationInfo.name);
        }

        //var listOfVariables = basicFlowEngine.GetVariables<LocationVariable>();

        ////print the names
        //foreach (var variable in listOfVariables)
        //{
        //    Debug.Log(variable.Key + " " + variable.Value.LocationName);
        //    //disable the location
        //    mapManager.HideLocationMarker(variable);

        //}

    }


    public void UnreachableTargetLocation()
    {


        //get the "CurrentLocation" variable from the basic flow engine
        LocationVariable targetLocation = basicFlowEngine.GetVariable<LocationVariable>("TargetLocation");

        //if the target location is null, then pick a random one by getting the last seen location and adding one
        if (targetLocation == null)
        {
            var lastSeenLoc = basicFlowEngine.GetVariable<LocationVariable>("LastSeenLocation");

            targetLocation.Value = RandomiseNextLocation(lastSeenLoc.Value);


        }


        var randomNext = RandomiseCurrentLocation(targetLocation.Value);


        targetLocation.Value = randomNext;
        //print the name of the location
        Debug.Log("Current Location: " + targetLocation.name);


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
        LUTELocationInfo[] filteredLocations = Array.FindAll(locationInfos, x => x.name.ToLower().Contains(quadrant));
        //get all locations with the same id within the filtered locations
        LUTELocationInfo[] locationsWithSameId = Array.FindAll(filteredLocations, x => x.name.StartsWith(newId.ToString()));
        //pick a random one that is not the current one
        int index = UnityEngine.Random.Range(0, locationsWithSameId.Length);
        while (locationsWithSameId[index] == currentLocation)
        {
            index = UnityEngine.Random.Range(0, locationsWithSameId.Length);
        }
       return locationsWithSameId[index];

    }

    public LUTELocationInfo RandomiseCurrentLocation(LUTELocationInfo currentLocation)
    {
        //get the id of the current location  
        string id_full = currentLocation.name.Split('-')[0];
        //split by the dot  
        string id = id_full.Split('.')[0];
        //filter locations based on quadrant  
        string quadrant = southQuadrant ? "south" : "north";
        LUTELocationInfo[] filteredLocations = Array.FindAll(locationInfos, x => x.name.ToLower().Contains(quadrant));
        //get all locations with the same id within the filtered locations  
        LUTELocationInfo[] locationsWithSameId = Array.FindAll(filteredLocations, x => x.name.StartsWith(id));
        //pick a random one that is not the current one  
        int index = UnityEngine.Random.Range(0, locationsWithSameId.Length);
        while (locationsWithSameId[index] == currentLocation)
        {
            index = UnityEngine.Random.Range(0, locationsWithSameId.Length);
        }
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
