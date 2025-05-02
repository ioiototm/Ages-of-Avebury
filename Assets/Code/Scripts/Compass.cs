using UnityEngine;
using LoGaCulture.LUTE;
using System;
using UnityEngine.UI;
using Mapbox.Unity.Location;

public class Compass : MonoBehaviour
{
    public BasicFlowEngine main;

    private bool locationServiceStarted = false;

    enum TimePeriod
    {
        Modern,
        Neolithic,
        MiddleAges
    }

    [Header("Current Time Period")]
    [SerializeField] TimePeriod timePeriod = TimePeriod.Modern;

    [Header("UI Elements")]
    [SerializeField] RawImage compassImage;

    private float currentAngle = 0f; // We'll lerp toward this

    // Example target location (Tokyo)
    [Header("Target Location (Lat, Lon)")]
    [SerializeField] private Vector2 targetLatLon = new Vector2(35.6895f, 139.6917f);


    LocationVariable targetLocation;

    // TEMP: Hard-coded test location if you can’t test with real GPS
    [SerializeField] private Vector2 testPosition = new Vector2(50.936673298842f, -1.3958901038337264f);

    [Header("Smoothing")]
    [SerializeField] private float rotationSpeed = 2f;

    [Header("Neolithic Light (3D)")]
    [SerializeField] private GameObject neolithicLight;  // e.g. a point light or some 3D object


    ILocationProvider locationProvider;

    void Start()
    {
        Debug.Log("Compass Script Started");

        var mapManager = main.GetMapManager();
        locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;


        //get the "CurrentLocation" variable from the map manager
        var lastSeenLocation = main.GetVariable<LocationVariable>("LastSeenLocation");

        targetLocation = main.GetVariable<LocationVariable>("TargetLocation");

        Debug.Log("Target Location: " + targetLocation.name);

        //StartCoroutine(StartLocationService());
    }

    System.Collections.IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("Location services not enabled by user.");
            yield break;
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            Debug.LogWarning("Timed out while initializing location services.");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogWarning("Unable to determine device location.");
            yield break;
        }

        locationServiceStarted = true;
    }

    private void Update()
    {
        // If not running on a device, comment this out or handle gracefully
        // if (Input.location.status != LocationServiceStatus.Running) return;

        // 1) Get current user position (or your test position for debugging)

        //if(Input.location.status == LocationServiceStatus.Stopped)
        //{
        //    Debug.LogWarning("Location service stopped.");
        //    //disable this script 
        //    this.enabled = false;
        //    return;
        //}



        //convert the targetLocation Value Postion from a stirng lat,lon to a Vector2
        string[] latLon = targetLocation.Value.Position.Split(',');
        float lat = float.Parse(latLon[0]);
        float lon = float.Parse(latLon[1]);
        targetLatLon = new Vector2(lat, lon);

        LocationInfo currentData;

        if (Input.location.status == LocationServiceStatus.Stopped)
        {
            currentData = new LocationInfo();
        }
        else
        {
            currentData = Input.location.lastData;
        }

        
        var currentLatLonMapbox = locationProvider.CurrentLocation.LatitudeLongitude;

        

           Vector2 currentLatLon = new Vector2((float)currentLatLonMapbox.x, (float)currentLatLonMapbox.y);

        // 2) Calculate bearing from current to target
        float bearingToTarget = (float)CalculateBearing(currentLatLon, targetLatLon);

        // 3) Subtract device's heading if you want the phone orientation
        float deviceHeading = Input.compass.magneticHeading; // can be 0 in Editor
        float relativeAngle = bearingToTarget - deviceHeading;

        // 4) Typically 0°=up, rotate clockwise
        float targetAngle = -relativeAngle;

        // 5) Smoothly Lerp
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);

        // 6) Switch visuals by timePeriod
        switch (timePeriod)
        {
            case TimePeriod.Modern:
            case TimePeriod.MiddleAges:
                // Normal compass arrow
                if (compassImage != null)
                {
                    compassImage.rectTransform.localEulerAngles = new Vector3(0f, 0f, currentAngle);
                }
              //  break;

            //case TimePeriod.Neolithic:
                // Instead of rotating a UI arrow, place a real 3D light on the screen edge
                if (neolithicLight != null)
                {
                    float halfW = Screen.width * 0.5f;
                    float halfH = Screen.height * 0.5f;

                    float normalizedAngle = (currentAngle + 360f) % 360f;

                    // 1) We get that 2D center-based coordinate
                    Vector2 edgePosCentered = GetRectEdgePosition(normalizedAngle, halfW, halfH);
                   
                    // 2) Shift for actual Screen coords (0,0 at bottom-left)
                    float screenX = -edgePosCentered.x + halfW;
                    float screenY = edgePosCentered.y + halfH;

                    // 3) Decide how far from the camera we want the object
                    //    If you're doing a big 3D scene, pick a distance that
                    //    places the light in front of the camera, e.g. 2f.
                    float distanceFromCamera = 2f;

                    // 4) Convert to a point in front of camera
                    Vector2 screenPos = new Vector2(screenX, screenY);
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

                    // 5) Move your 3D light to that world position
                    worldPos.z = neolithicLight.transform.position.z; // Keep the Z position
                    neolithicLight.transform.position = worldPos;
                }
                break;
        }
    }

    // same bearing function as before
    public static double CalculateBearing(Vector2 fromLatLon, Vector2 toLatLon)
    {
        double fromLat = fromLatLon.x * Mathf.Deg2Rad;
        double fromLon = fromLatLon.y * Mathf.Deg2Rad;
        double toLat = toLatLon.x * Mathf.Deg2Rad;
        double toLon = toLatLon.y * Mathf.Deg2Rad;

        double y = Math.Sin(toLon - fromLon) * Math.Cos(toLat);
        double x = Math.Cos(fromLat) * Math.Sin(toLat)
                    - Math.Sin(fromLat) * Math.Cos(toLat) * Math.Cos(toLon - fromLon);

        double bearing = Math.Atan2(y, x) * Mathf.Rad2Deg;
        bearing = (bearing + 360.0) % 360.0;

        return bearing;
    }

    // your "edge of rectangle" function
    private Vector2 GetRectEdgePosition(float angleDeg, float halfW, float halfH)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float dx = Mathf.Sin(rad); // 0°=up => y=cos, x=sin
        float dy = Mathf.Cos(rad);

        float tMin = float.PositiveInfinity;

        // left edge
        if (dx < 0f)
        {
            float t = -halfW / dx;
            if (t > 0f && t < tMin) tMin = t;
        }
        // right edge
        else if (dx > 0f)
        {
            float t = halfW / dx;
            if (t > 0f && t < tMin) tMin = t;
        }

        // bottom edge
        if (dy < 0f)
        {
            float t = -halfH / dy;
            if (t > 0f && t < tMin) tMin = t;
        }
        // top edge
        else if (dy > 0f)
        {
            float t = halfH / dy;
            if (t > 0f && t < tMin) tMin = t;
        }

        if (tMin < float.PositiveInfinity)
        {
            float xPos = dx * tMin;
            float yPos = dy * tMin;
            return new Vector2(xPos, yPos);
        }

        return Vector2.zero;
    }
}
