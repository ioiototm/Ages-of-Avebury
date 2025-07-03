using LoGaCulture.LUTE;
using System.Collections;
using System.Linq;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Loads a random stone from online and shows the outline",
              "")]
[AddComponentMenu("")]
public class LoadAndShowStone : Order
{
    [SerializeField]
    Camera mainCamera;
    public GameObject SpawnStoneOutline(float width = 0.05f)
    {
        //get a random List<Vector3> from Compass rocks which is List<List<Vector3>>
        //get a random index 

        var randomIndex = Random.Range(0, Compass.rocks.Count);

        var pts = Compass.rocks[randomIndex];

        GameObject lineGO = new GameObject("RemoteStoneOutline");

        // --- Begin Changes ---
                
        // Calculate bounds of the points
        var bounds = new Bounds(pts[0], Vector3.zero);
        for (int i = 1; i < pts.Count; i++)
        {
            bounds.Encapsulate(pts[i]);
        }

        var center = bounds.center;
        var size = bounds.size;

        // Normalize points by centering them
        var normalizedPts = pts.Select(p => p - center).ToList();

        // Find the maximum distance from the center to scale correctly
        float maxDist = 0f;
        foreach (var p in normalizedPts)
        {
            if (p.magnitude > maxDist)
            {
                maxDist = p.magnitude;
            }
        }

        // Normalize all points by the max distance
        if (maxDist > 0)
        {
            for (int i = 0; i < normalizedPts.Count; i++)
            {
                normalizedPts[i] /= maxDist;
            }
        }


        //remove the last point
        normalizedPts.RemoveAt(normalizedPts.Count - 1);

        var lr = lineGO.AddComponent<LineRenderer>();


        lr.useWorldSpace = false;
        
        // To close the loop, add the first point at the end
        lr.positionCount = normalizedPts.Count;
        lr.SetPositions(normalizedPts.ToArray());
        lr.loop = true; // Set loop to true to properly close the outline

        lr.widthMultiplier = width;
        lr.material = new Material(Shader.Find("Sprites/Default"))
        {
            color = Color.white
        };

        // put it 5m in front of camera so the player sees it
        float distance = 5f;
        lineGO.transform.position = mainCamera.transform.position
                                  + mainCamera.transform.forward * distance;

        // Rotate to face the camera, then rotate 90 degrees on the local X axis
        lineGO.transform.rotation = mainCamera.transform.rotation * Quaternion.Euler(90, 0, 0);

        //set the scale to fit the screen
        float screenHeight = 2.0f * distance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float scale = screenHeight * 0.4f; // Use 40% of the screen height for a better fit
        lineGO.transform.localScale = new Vector3(scale, scale, scale);

        // --- End Changes ---

        return lineGO;
    }

    public override void OnEnter()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        SpawnStoneOutline();

       StartCoroutine(wait5Seconds());
        //continue the order
        //Continue();

    }

    IEnumerator wait5Seconds()
    {
        yield return new WaitForSeconds(5f);
        //destroy the outline
        var outline = GameObject.Find("RemoteStoneOutline");   
        if (outline != null)
        {
            Destroy(outline);
        }

        //continue the order
        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "Detects spin";
    }
}