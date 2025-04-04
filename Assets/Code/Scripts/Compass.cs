using UnityEngine;
using LoGaCulture.LUTE;
using System;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    public BasicFlowEngine main;

    private bool locationServiceStarted = false;

    [SerializeField] RawImage compassImage;

    private float currentAngle = 0f; // We'll lerp toward this

    // Example target location (tokyo)
    [Header("Target Location (Lat, Lon)")]
    [SerializeField] private Vector2 targetLatLon = new Vector2(35.6895f, 139.6917f);

    [Header("Smoothing")]
    [SerializeField] private float rotationSpeed = 2f;

    void Start()
    {
        Debug.Log("Compass Script Started");
        StartCoroutine(StartLocationService());
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
        // If not running on a device with location, bail
        if (Input.location.status != LocationServiceStatus.Running)
            return;

        // 1) Get current position
        var currentData = Input.location.lastData;
        Vector2 currentLatLon = new Vector2(currentData.latitude, currentData.longitude);

        // 2) Calculate bearing from current to target
        float bearingToTarget = (float)CalculateBearing(currentLatLon, targetLatLon);

        // 3) If you want the arrow to account for the phone's orientation:
        float deviceHeading = Input.compass.magneticHeading; // can be 0 in Editor
        float relativeAngle = bearingToTarget - deviceHeading;

        // 4) Decide how to apply that angle to your UI:
        //    Typically, 0° on a top-facing compass means "North = up".
        //    We'll invert so that a positive angle rotates clockwise:
        float targetAngle = -relativeAngle;

        // 5) Smoothly Lerp from currentAngle to targetAngle
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);

        // 6) Apply rotation to the UI compass
        compassImage.rectTransform.localEulerAngles = new Vector3(0f, 0f, currentAngle);
    }

    // fromLatLon & toLatLon as: (latitude, longitude)
    public static double CalculateBearing(Vector2 fromLatLon, Vector2 toLatLon)
    {
        // Convert degrees to radians
        double fromLat = fromLatLon.x * Mathf.Deg2Rad;
        double fromLon = fromLatLon.y * Mathf.Deg2Rad;
        double toLat = toLatLon.x * Mathf.Deg2Rad;
        double toLon = toLatLon.y * Mathf.Deg2Rad;

        double y = Math.Sin(toLon - fromLon) * Math.Cos(toLat);
        double x = Math.Cos(fromLat) * Math.Sin(toLat)
                 - Math.Sin(fromLat) * Math.Cos(toLat) * Math.Cos(toLon - fromLon);

        double bearing = Math.Atan2(y, x);
        bearing = bearing * Mathf.Rad2Deg;

        // Convert range from [-180,180] to [0,360]
        bearing = (bearing + 360.0) % 360.0;

        return bearing;
    }
}
