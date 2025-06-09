using LoGaCulture.LUTE;
using Mapbox.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class HiddenItemScanner : MonoBehaviour
{
    public event Action<GameObject> OnItemDiscovered;

    [Header("Item Settings")]
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private LUTELocationInfo targetLocation;
    [SerializeField] private float detectionRadius = 1000f; // in meters
    [SerializeField] private float discoveryRadius = 10f; // in meters - when to spawn the item

    [Header("Ping Settings")]
    [SerializeField] private GameObject pingPrefab;
    [SerializeField] private GameObject directionalPingPrefab; // Ping that points toward item
    [SerializeField] private float pingLifetime = 3f;
    [SerializeField] private float directionalPingDelay = 0.7f; // Delay before showing directional pings
    [SerializeField] private float directionalPingDistance = 1f; // Distance to place directional pings

    [Header("AR Settings")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private PlaneAlignment desiredAlignment = PlaneAlignment.HorizontalUp;

    [Header("UI Elements")]
    [SerializeField] private Button scanButton;
    [SerializeField] private Text feedbackText;
    [SerializeField] private Animator scannerAnimator;

    // Private variables
    private BasicFlowEngine flowEngine;
    private Vector2d targetLatLon;
    private bool itemDiscovered = false;
    private bool isWithinDetectionRange = false;


    private int pingAmmount = 0;

    private bool isActive = false;

    public void SetItemToSpawn(GameObject item)
    {
        objectToSpawn = item;
        itemDiscovered = false;
    }

    public void SetIsActive(bool active)
    {
        isActive = active;
        if (isActive)
        {
            // Reset item discovered state
            itemDiscovered = false;
            isWithinDetectionRange = false;
            // Reset feedback text
            if (feedbackText != null)
            {
                feedbackText.text = "Tap Scan to begin searching";
            }
        }
    }


    public void SetLocation(LUTELocationInfo location)
    {
        targetLocation = location;
        if (targetLocation != null)
        {
            string position = targetLocation.Position;
            string[] latLon = position.Split(',');
            if (latLon.Length == 2 && double.TryParse(latLon[0], out double lat) &&
                double.TryParse(latLon[1], out double lon))
            {
                targetLatLon = new Vector2d(lat, lon);
                Debug.Log($"Target location set to: {targetLatLon.x}, {targetLatLon.y}");
            }
            else
            {
                Debug.LogError("Invalid target location format!");
            }
        }
    }

    void Start()
    {
        // Initialize flowEngine reference
        flowEngine = GameObject.Find("BasicFlowEngine")?.GetComponent<BasicFlowEngine>();
        if (flowEngine == null)
        {
            Debug.LogError("BasicFlowEngine not found!");
        }

        // Set up button listener
        if (scanButton != null)
        {
            scanButton.onClick.AddListener(Scan);
        }

        // Initialize target location
        if (targetLocation != null)
        {
            string position = targetLocation.Position;
            string[] latLon = position.Split(',');

            if (latLon.Length == 2 && double.TryParse(latLon[0], out double lat) &&
                double.TryParse(latLon[1], out double lon))
            {
                targetLatLon = new Vector2d(lat, lon);
                Debug.Log($"Target location set to: {targetLatLon.x}, {targetLatLon.y}");
            }
            else
            {
                Debug.LogError("Invalid target location format!");
            }
        }
        else
        {
            Debug.LogError("Target location not assigned!");
        }

        // Set up AR components if not assigned
        SetupARComponents();

        if (feedbackText != null)
        {
            feedbackText.text = "Tap Scan to begin searching";
        }
    }

    public void skipToNeolithic()
    {
        // Execute the node to skip to Neolithic
        var flowEngine = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>();
        flowEngine.ExecuteNode("Change to Neolithic");
    }

    void Update()
    {


        //if key pressed is r, execute node called Change to Neolithic
        if (Input.GetKeyDown(KeyCode.R))
        {
            var flowEngine = GameObject.Find("BasicFlowEngine").GetComponent<BasicFlowEngine>();
            //call 
            flowEngine.ExecuteNode("Change to Neolithic");

        }
        if (itemDiscovered || targetLocation == null)
            return;
        if (!isActive)
            return;


        //check if the arraycast manager is null
        if (raycastManager == null)
        {
            raycastManager = GameObject.Find("XR Origin (Mobile AR)").GetComponent<ARRaycastManager>();
        }

        //check if the plane manager is null
        if (planeManager == null)
        {
            planeManager = GameObject.Find("XR Origin (Mobile AR)").GetComponent<ARPlaneManager>();
        }

        // Check if within detection range
        CheckDistanceToTarget();
    }

    private void CheckDistanceToTarget()
    {
        // Get current location from MapManager
        if (flowEngine != null)
        {
            var mapManager = flowEngine.GetMapManager();
            if (mapManager != null)
            {
                Vector2d currentLocation = mapManager.TrackerPos();

                // Calculate distance to target
                double distanceInMeters = CalculateDistance(currentLocation, targetLatLon);

                // Update detection state
                bool wasInRange = isWithinDetectionRange;
                isWithinDetectionRange = distanceInMeters <= detectionRadius;

                wasInRange = true;


                // If we just entered range, notify the user
                if (isWithinDetectionRange && !wasInRange && feedbackText != null)
                {
                    feedbackText.text = "Something nearby... Try scanning!";
                }
                else if (!isWithinDetectionRange && feedbackText != null)
                {
                    feedbackText.text = "No signals detected in this area";
                }

                // Check if we're close enough to discover the item
                if (distanceInMeters <= discoveryRadius && !itemDiscovered)
                {
                    if (feedbackText != null)
                    {
                        feedbackText.text = "Very close! Scan to discover!";
                    }
                }
            }
        }
    }

    public void Scan()
    {
        if (itemDiscovered)
            return;


        pingAmmount++;

        Debug.Log($"Ping ammount: {pingAmmount}");

        if (pingAmmount > 10)
        {
            pingAmmount = 0;


            Debug.Log("Ping ammount reset");

            //spawn the item anyway
            StartCoroutine(DiscoverItem());
            return;
        }

        // Get current location and device heading
        var mapManager = flowEngine.GetMapManager();
        Vector2d currentLocation = mapManager.TrackerPos();
        float deviceHeading = Input.compass.trueHeading;

        // Calculate distance to target
        double distanceInMeters = CalculateDistance(currentLocation, targetLatLon);
        //distanceInMeters = 2;

        isWithinDetectionRange = true;

        //TODO: look into this 
        //bool locationMet = location.locationRef.Evaluate(ComparisonOperator.Equals, null);

        if (distanceInMeters <= discoveryRadius)
        {

            if (!isActive)
            {
                return;
            }
            // We're close enough to discover the item!
            if (pingAmmount > 3)
            {
                StartCoroutine(DiscoverItem());
            }
        }
        else if (isWithinDetectionRange && isActive)
        {


            // We're in detection range but not close enough to discover
            // Determine direction to target
            string direction = DetermineDirection(currentLocation, targetLatLon, deviceHeading);

            // Play animation if available
            PlayScanAnimation(direction);

            // Place ping on AR plane
            PlacePingOnPlane(direction);

            // Update feedback text
            if (feedbackText != null)
            {
                feedbackText.text = $"Signal detected {direction}! Move that way and scan again.";
            }
        }
        else
        {
            // Just place a standard ping with no direction
            PlacePingOnPlane(null);

            if (feedbackText != null)
            {
                feedbackText.text = "No signals detected in this area";
            }
        }
    }

    private IEnumerator DiscoverItem()
    {
        itemDiscovered = true;

        if (feedbackText != null)
        {
            feedbackText.text = "Item found!";
        }

        // Place a ping on the plane
        PlacePingOnPlane(null);

        // Wait a moment for dramatic effect
        yield return new WaitForSeconds(1.5f);

        // Spawn the hidden item on the AR plane
        Pose itemPose = GetCenterScreenPose();
        if (itemPose != null && objectToSpawn != null)
        {
            GameObject spawnedObject = Instantiate(objectToSpawn, itemPose.position, itemPose.rotation);

            // Adjust orientation if needed
            //spawnedObject.transform.rotation = Quaternion.Euler(-90, spawnedObject.transform.rotation.eulerAngles.y, 0);

      

            if (feedbackText != null)
            {
                feedbackText.text = $"You found: {objectToSpawn.name}!";
            }

            // Optionally notify the BasicFlowEngine that the item was discovered
            if (flowEngine != null)
            {
                // Example: Update location status or execute a node
                targetLocation.LocationStatus = LocationStatus.Visited;
                // flowEngine.ExecuteNode("ItemDiscovered");


            }

            // Notify listeners about the discovered item
            OnItemDiscovered?.Invoke(spawnedObject);
        }
        else
        {
            Debug.LogError("Failed to place item - no valid AR pose found or objectToSpawn is null");
            if (feedbackText != null)
            {
                feedbackText.text = "Error placing item. Try scanning a flat surface.";
            }
            itemDiscovered = false; // Allow retry
        }
    }

    private void PlacePingOnPlane(string direction)
    {
        StartCoroutine(PlacePingCoroutine(direction));
    }

    private IEnumerator PlacePingCoroutine(string direction)
    {
        Pose pose = GetCenterScreenPose();
        if (pose != null)
        {
            // Start with placing the main ping
            GameObject mainPing;

            // Use regular ping for the main ping
            mainPing = Instantiate(pingPrefab, pose.position, pose.rotation);
            mainPing.transform.rotation = Quaternion.Euler(90, mainPing.transform.rotation.eulerAngles.y, 0);

            // Destroy main ping after lifetime
            Destroy(mainPing, pingLifetime);

            // Wait a moment before showing directional pings
            yield return new WaitForSeconds(directionalPingDelay);

            // Now place a directional ping if we have a direction
            if (direction != null)
            {
                // Get current location and calculate bearing to target
                var mapManager = flowEngine.GetMapManager();
                Vector2d currentLocation = mapManager.TrackerPos();
                double bearingToTarget = CalculateBearing(currentLocation, targetLatLon);
                float deviceHeading = Input.compass.trueHeading;

                // Calculate relative angle (similar to Compass.cs)
                float relativeAngle = (float)bearingToTarget - deviceHeading;
                float normalizedAngle = (relativeAngle + 360f) % 360f;

                // Convert angle to radians for position calculation
                float rad = normalizedAngle * Mathf.Deg2Rad;

                // Calculate direction vector (similar to Compass's GetRectEdgePosition logic)
                float dx = Mathf.Sin(rad);
                float dz = Mathf.Cos(rad);
                Vector3 directionVector = new Vector3(dx, 0, dz).normalized;

                // Place the ping at specified distance in the direction of the target
                Vector3 pingPosition = pose.position + directionVector * directionalPingDistance;

                // Instantiate the directional ping
                GameObject directionalPing = Instantiate(directionalPingPrefab, pingPosition, pose.rotation);

                // Orient the ping to point toward the target
                directionalPing.transform.LookAt(new Vector3(pose.position.x, pingPosition.y, pose.position.z));
                directionalPing.transform.Rotate(90, 0, 0); // Adjust based on your model orientation

                // Destroy the directional ping after its lifetime
                Destroy(directionalPing, pingLifetime - directionalPingDelay);
            }
        }

        yield return null;
    }

    private Pose GetCenterScreenPose()
    {
        if (raycastManager == null)
            return new Pose();

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        var hits = new List<ARRaycastHit>();

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.Planes))
        {
            var hit = hits[0];

            // Check for desired plane alignment if needed
            if (planeManager != null && planeManager.GetPlane(hit.trackableId).alignment != desiredAlignment)
            {
                return new Pose();
            }

            return hit.pose;
        }

        return new Pose();
    }

    private void PlayScanAnimation(string direction)
    {
        if (scannerAnimator == null)
            return;

        switch (direction)
        {
            case "left":
                scannerAnimator.Play("ScanLeftAnimation");
                break;

            case "right":
                scannerAnimator.Play("ScanRightAnimation");
                break;

            case "ahead":
                scannerAnimator.Play("ScanForwardAnimation");
                break;

            default:
                scannerAnimator.Play("ScanAnimation");
                break;
        }
    }

    private void SetupARComponents()
    {
        // Find AR components if not assigned
        if (raycastManager == null)
        {
            raycastManager = FindObjectOfType<ARRaycastManager>();
        }

        if (planeManager == null)
        {
            planeManager = FindObjectOfType<ARPlaneManager>();
        }
    }

    public double CalculateDistance(Vector2d from, Vector2d to)
    {
        // Haversine formula for calculating distance between two coordinates
        double R = 6371e3; // Earth radius in meters
        double f1 = from.x * Mathf.Deg2Rad;
        double f2 = to.x * Mathf.Deg2Rad;
        double df = (to.x - from.x) * Mathf.Deg2Rad;
        double dl = (to.y - from.y) * Mathf.Deg2Rad;

        double a = Math.Sin(df / 2) * Math.Sin(df / 2) +
                   Math.Cos(f1) * Math.Cos(f2) *
                   Math.Sin(dl / 2) * Math.Sin(dl / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c; // Distance in meters
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

        // Define thresholds for direction determination
        float threshold = 30f; // degrees

        if (angleDifference > threshold)
        {
            return "right";
        }
        else if (angleDifference < -threshold)
        {
            return "left";
        }
        else
        {
            return "ahead";
        }
    }
}
