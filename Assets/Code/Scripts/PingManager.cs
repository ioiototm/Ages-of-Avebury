using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;                      // For the Button
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PingScanner : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [Tooltip("Prefab to drop on the detected plane (your ripple / ping).")]
    public GameObject pingPrefab;

    public Button scanButton;             
    public ARRaycastManager raycastMgr;   
    public ARPlaneManager planeMgr;      

    [Header("Placement Options")]
    public bool continuousUntilHit = true;  
    public PlaneAlignment desiredAlignment = PlaneAlignment.HorizontalUp; 



    void Awake()
    {
        if (scanButton != null)
            scanButton.onClick.AddListener(BeginScan);
    }

    void BeginScan()
    {
        
        if (continuousUntilHit)
            StartCoroutine(ScanLoop());
        else
            TrySingleScan();
    }

    IEnumerator ScanLoop()
    {

        scanButton.interactable = false;

        while (!TrySingleScan())
        {
            yield return null;            
        }

        scanButton.interactable = true;
    }

    bool TrySingleScan()
    {
       
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        var hits = new List<ARRaycastHit>();
        if (raycastMgr.Raycast(screenCenter, hits, TrackableType.Planes))
        {
       
            var hit = hits[0];

  
            if (planeMgr != null && planeMgr.GetPlane(hit.trackableId).alignment != desiredAlignment)
                return false;

            SpawnPing(hit.pose);
            return true;
        }
        return false;
    }

    void SpawnPing(Pose pose)
    {
        //spawn it at the hit pose pointing up
        GameObject ping = Instantiate(pingPrefab, pose.position, pose.rotation);
        //set the x rotation to 90 degrees
        ping.transform.rotation = Quaternion.Euler(90, ping.transform.rotation.eulerAngles.y, ping.transform.rotation.eulerAngles.z);


    }

    private void Update()
    {
        //check if the arraycast manager is null
        if (raycastMgr == null)
        {
            raycastMgr = GameObject.Find("XR Origin (Mobile AR)").GetComponent<ARRaycastManager>();
        }

        //check if the plane manager is null
        if (planeMgr == null)
        {
            planeMgr = GameObject.Find("XR Origin (Mobile AR)").GetComponent<ARPlaneManager>();
        }
    }

}
