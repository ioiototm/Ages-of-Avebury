using LoGaCulture.LUTE;
using Mapbox.Unity.Location;
using Mapbox.Utils;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;


public class Glitching : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] public LocationVariable portalLocation; // Your LocationVariable (GPS)
    [SerializeField] private Material glitchMat;              // The fullscreen glitch canvas material

    [Header("Distance Settings (metres)")]
    [SerializeField] private float startEffectAt = 50f;   //  this = effect begins
    [SerializeField] private float activateAt = 5f;       //  this = full activation

    [Header("Shader Property Names")]
    [SerializeField] private string scanProp = "_ScanLinesStrength";
    [SerializeField] private string flickerProp = "_FlickerStrength";
    [SerializeField] private string noiseProp = "_NoiseAmount";
    [SerializeField] private string glitchProp = "_Glitchiness";

    [Header("Glitch Ranges")]
    [SerializeField] private float scanMin = -5f;
    [SerializeField] private float scanMax = 1f;
    [SerializeField] private float flickerMax = 10f;
    [SerializeField] private float noiseMax = 10f;
    [SerializeField] private float glitchMax = 10f;

    [Header("Optional Event")]
    public UnityEvent OnActivate; // hook up particle burst, scene load, etc.


    [SerializeField]
    public bool running = false;

    private bool _activated;

    
    public BasicFlowEngine flowEngine; 
    private LUTEMapManager _mapManager;

    private void Start()
    {
        _mapManager = flowEngine.GetMapManager();


        glitchMat.SetFloat(scanProp, -5f);
    }

    void Update()
    {
        if (!portalLocation || !glitchMat) return;


        if(!running) return; // Check if the script is running

        // Get player location (make sure this is set up in your GPS system)

        //use the Mapbox location service to get the player location
        //Vector2d playerLocation = _mapManager.TrackerPos();

        //get the current position of the player
        Vector2d playerLocation = LocationProviderFactory.Instance.DefaultLocationProvider.CurrentLocation.LatitudeLongitude;

        //LocationProviderFactory.Instance.DefaultLocationProvider.CurrentLocation.LatitudeLongitude;


        //print the player location and portal location
        //Debug.Log("Player location: " + playerLocation + " Portal location: " + portalLocation.Position);

        // Extract portal GPS coordinates
        if (!TryParsePortalPosition(out float portalLat, out float portalLon)) return;

        // Calculate distance using the Haversine formula (in metres)
        float distance = HaversineDistance((float)playerLocation.x, (float)playerLocation.y, portalLat, portalLon);

        //Debug.Log($"Distance to portal: {distance}m");

        // 0 when far, 1 when hugging the portal
        float t = Mathf.InverseLerp(startEffectAt, 0f, distance);

        // Shader properties adjust based on distance
        float scan = Mathf.Lerp(scanMin, scanMax, t);
        float flicker = flickerMax * t;
        float noise = noiseMax * t;
        float glitch = glitchMax * t;

        // Feed the shader
        glitchMat.SetFloat(scanProp, scan);
        glitchMat.SetFloat(flickerProp, flicker);
        glitchMat.SetFloat(noiseProp, noise);
        glitchMat.SetFloat(glitchProp, glitch);

        //if the distance is under 15 meters, print it
        if (distance <= 15f)
        {
            Debug.Log("Distance to portal: " + distance);
        }

        // Trigger once when super-close
        if (!_activated && distance <= activateAt)
        {
            _activated = true;
            OnActivate?.Invoke();

            BasicFlowEngine flowEngine = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>();
            flowEngine.ExecuteNode("Change to Neolithic");
            var portalCompleted = flowEngine.GetVariable<LocationVariable>("Portal5");
            if (portalCompleted != null)
            {
                portalCompleted.Value.LocationStatus = LocationStatus.Completed;
            }


            //disable this game object
            gameObject.SetActive(false);

        }
    }

    private bool TryParsePortalPosition(out float latitude, out float longitude)
    {
        latitude = longitude = 0;
        if (string.IsNullOrEmpty(portalLocation.Value.Position))
        {
            Debug.LogWarning("Portal location is not set.");
            return false;
        }

        string[] parts = portalLocation.Value.Position.Split(',');
        if (parts.Length != 2) return false;

        if (float.TryParse(parts[0].Trim(), out latitude) && float.TryParse(parts[1].Trim(), out longitude))
        {
            return true;
        }

        Debug.LogWarning("Invalid portal GPS coordinates.");
        return false;
    }

    // Haversine formula for GPS distance calculation
    private float HaversineDistance(float lat1, float lon1, float lat2, float lon2)
    {
        const float R = 6371000f; // Radius of Earth in meters
        float latRad1 = Mathf.Deg2Rad * lat1;
        float latRad2 = Mathf.Deg2Rad * lat2;
        float dLat = Mathf.Deg2Rad * (lat2 - lat1);
        float dLon = Mathf.Deg2Rad * (lon2 - lon1);

        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(latRad1) * Mathf.Cos(latRad2) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);

        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        return R * c; // Distance in meters
    }


    //on disable, set the glitch mat to 0
    private void OnDisable()
    {
        if (glitchMat)
        {
            glitchMat.SetFloat(scanProp, -1);
            glitchMat.SetFloat(flickerProp, -1);
            glitchMat.SetFloat(noiseProp, -1);
            glitchMat.SetFloat(glitchProp, -1);
        }
    }
}
