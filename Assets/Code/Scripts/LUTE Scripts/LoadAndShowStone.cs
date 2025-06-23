using LoGaCulture.LUTE;
using System.Collections;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Loads a random stone from online and shows the outline",
              "")]
[AddComponentMenu("")]
public class LoadAndShowStone : Order
{

    public GameObject SpawnStoneOutline(float width = 0.05f)
    {
        //get a random List<Vector3> from Compass rocks which is List<List<Vector3>>
        //get a random index 

        var randomIndex = Random.Range(0, Compass.rocks.Count);

        var pts = Compass.rocks[randomIndex];

        GameObject lineGO = new GameObject("RemoteStoneOutline");




        var lr = lineGO.AddComponent<LineRenderer>();


        lr.useWorldSpace = false;
        

        lr.positionCount = pts.Count;
        lr.SetPositions(pts.ToArray());
        lr.loop = true;
        lr.widthMultiplier = width;
        lr.material = new Material(Shader.Find("Sprites/Default"))
        {
            color = Color.white
        };

        // put it 1 m in front of camera so the player sees it
        //lineGO.transform.position = Camera.main.transform.position
        //                          + Camera.main.transform.forward * 1f;

        // optional: rotate flat to camera view
        //lineGO.transform.rotation = Quaternion.Euler(90, 0, 0);

        //set the scale to 0.11
        lineGO.transform.localScale = new Vector3(0.10f, 0.10f, 0.10f);

        //rotate the gameobject 90 on the z
        lineGO.transform.rotation = Quaternion.Euler(0, -90, -90);

        return lineGO;
    }

    public override void OnEnter()
    {

        SpawnStoneOutline();

       StartCoroutine(wait5Seconds());
        //continue the order
        //Continue();

    }

    IEnumerator wait5Seconds()
    {
        yield return new WaitForSeconds(5f);
        //continue the order
        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "Detects spin";
    }
}