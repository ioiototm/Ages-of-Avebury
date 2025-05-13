using LoGaCulture.LUTE;
using Mapbox.Utils;
using System;
using UnityEngine;
using UnityEngine.Rendering;

public class ScannerManager : MonoBehaviour
{

    [SerializeField]
    GameObject objectToSpawn;

    [SerializeField]
    LUTELocationInfo targetLocation;

    [SerializeField]
    Animator scannerAnimator;

    public void scan()
    {
        // GPS coordinates of the target location
        string position = targetLocation.Position;

        // It's in the format "latitude,longitude"
        Vector2d targetLocationVector = new Vector2d(
            double.Parse(position.Split(',')[0]),
            double.Parse(position.Split(',')[1])
        );

        // GPS coordinates of the current location
        var flowEngine = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>();
        var mapManager = flowEngine.GetMapManager();
        var currentLocation = mapManager.TrackerPos();

        // Get device heading
        float deviceHeading = Input.compass.trueHeading;

        // Determine direction
        string direction = DetermineDirection(currentLocation, targetLocationVector, deviceHeading);

        // Play the scanner animation based on the direction
        if (direction == "left")
        {
            Debug.Log("Scanning left");
            //scannerAnimator.Play("ScanLeftAnimation");
        }
        else if (direction == "right")
        {
            Debug.Log("Scanning right");
            //scannerAnimator.Play("ScanRightAnimation");
        }
        else
        {
            Debug.Log("Scanning ahead");
            //scannerAnimator.Play("ScanForwardAnimation");
        }
    }

    LocationVariable locationVariable;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        //if key pressed is r, execute node called Change to Neolithic
        if (Input.GetKeyDown(KeyCode.R))
        {
            var flowEngine = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>();
            //call 
            flowEngine.ExecuteNode("Change to Neolithic");

        }

    }
    public double CalculateBearing(Vector2d from, Vector2d to)
    {
        double lat1 = from.x * Mathf.Deg2Rad;
        double lon1 = from.y * Mathf.Deg2Rad;
        double lat2 = to.x * Mathf.Deg2Rad;
        double lon2 = to.y * Mathf.Deg2Rad;

        double dLon = lon2 - lon1;

        double y = Math.Sin(dLon) * Math.Cos(lat2);
        double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);

        double bearing = Math.Atan2(y, x);
        bearing = bearing * Mathf.Rad2Deg;
        bearing = (bearing + 360) % 360;

        return bearing;
    }

    public string DetermineDirection(Vector2d currentLocation, Vector2d targetLocation, float deviceHeading)
    {
        // Compute bearing from current location to target location
        double bearingToTarget = CalculateBearing(currentLocation, targetLocation);

        // Compute angle difference between device heading and bearing to target
        float angleDifference = Mathf.DeltaAngle(deviceHeading, (float)bearingToTarget);

        if (angleDifference > 0)
        {
            return "right";
        }
        else if (angleDifference < 0)
        {
            return "left";
        }
        else
        {
            return "ahead";
        }
    }
}
