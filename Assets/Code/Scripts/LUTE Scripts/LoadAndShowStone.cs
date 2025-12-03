using LoGaCulture.LUTE;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TriangleNet;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Load Stone From Online",
              "")]
[AddComponentMenu("")]
public class LoadAndShowStone : Order
{
    [SerializeField]
    Camera mainCamera;

    [SerializeField]
    int waitTime = 5;


    [SerializeField]
    StoneCreator stoneCreator;
    public GameObject SpawnStoneOutline(float width = 0.05f)
    {
        //get a random List<Vector3> from Compass rocks which is List<List<Vector3>>
        //get a random index 

        //if there are no elements, return null

        if(Compass.meshesAndOutlines.Count <= 0)
        {
            return null;
        }


        var randomIndex = Random.Range(0, Compass.meshesAndOutlines.Count);

        Debug.Log("Random Index for Random rock is: " + randomIndex);

        var pts = Compass.meshesAndOutlines[randomIndex];

        GameObject lineGO = new GameObject("RemoteStoneOutline");

        // --- Begin Changes ---
                
        // Calculate bounds of the points
        var bounds = new Bounds(pts.outline[0], Vector3.zero);
        for (int i = 1; i < pts.outline.Count; i++)
        {
            bounds.Encapsulate(pts.outline[i]);
        }

        var center = bounds.center;

        var centerVec2 = new Vector2(center.x, center.z);
        var size = bounds.size;

        // Normalize points by centering them
        var normalizedPts = SelfieSegmentationSample.NormalizeToBounds(pts.outline, new Vector2(-1, -1), new Vector2(1, 1));


        //remove the last point
        normalizedPts.RemoveAt(normalizedPts.Count - 1);

        var lr = lineGO.AddComponent<LineRenderer>();


        lr.useWorldSpace = false;
        
        // To close the loop, add the first point at the end
        lr.positionCount = normalizedPts.Count;

        List<Vector3> normalizedPtsVec3 = new List<Vector3>();

        //make normalizedPts a List<Vector3>, where y is 1
        for (int i = 0; i < normalizedPts.Count; i++)
        {
            normalizedPtsVec3.Add(new Vector3(normalizedPts[i].x, 1f, normalizedPts[i].y));
        }
        lr.SetPositions(normalizedPtsVec3.ToArray());
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

        // Rotate to face the camera, then rotate -90 degrees on the local X axis
        lineGO.transform.rotation = mainCamera.transform.rotation * Quaternion.Euler(-90, 0, 0);

        //set the scale to fit the screen
        float screenHeight = 2.0f * distance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float scale = screenHeight * 0.4f; // Use 40% of the screen height for a better fit
        lineGO.transform.localScale = new Vector3(scale, scale, scale);

        // --- End Changes ---

        stoneCreator.outlinePoints = normalizedPts;
        //stoneCreator.GenerateSlab();

        //get the mesh filter of this object, and set it to the random mesh from Compass.meshesAndOutlines
        MeshFilter meshFilter = stoneCreator.GetComponent<MeshFilter>();
        stoneCreator.enabled = false;
        if (meshFilter != null)
        {
            meshFilter.sharedMesh = Compass.meshesAndOutlines[randomIndex].mesh;
        }
        else
        {
            Debug.LogError("MeshFilter component not found on the GameObject.");
        }

        //disable the mesh renderer so it doesn't show
        stoneCreator.gameObject.SetActive(false);


        var mapCompletion = GameObject.Find("MapComplete").GetComponent<MapCompletion>();

        //if (firstStone)
        {
            mapCompletion.foundStone = stoneCreator.gameObject;
        }

        //serialize the stone to base64
        var base64Stone = MeshSerializer.ToBase64(meshFilter.sharedMesh, stoneCreator.outlinePoints);

        var tinySave = TinySave.Instance != null ? TinySave.Instance : FindObjectOfType<TinySave>();
        if (tinySave != null)
        {
            tinySave.SaveStoneData(base64Stone, 3);
            Debug.Log("LoadAndShowStone: Saved social stone data (slot 3).");
        }
        else
        {
            Debug.LogError("LoadAndShowStone: TinySave not found; social stone mesh not persisted.");
        }


        return lineGO;
    }

    public override void OnEnter()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        var spawned = SpawnStoneOutline();

        if (spawned == null)
        {
            Continue();
        }
        else
        {

            StartCoroutine(wait5Seconds());
            //continue the order
            //Continue();
        }
    }

    IEnumerator wait5Seconds()
    {
        yield return new WaitForSeconds(waitTime);
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
        return "Loads a random stone from online and shows the outline";
    }
}