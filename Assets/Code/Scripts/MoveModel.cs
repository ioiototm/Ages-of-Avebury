using UnityEditor;
using UnityEngine;

public class MoveModel : MonoBehaviour
{

    public GameObject landscapeWithMaterial;
    public float someOffsetX = 0.42f;
    public float someOffsetY = -0.115f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        //main object is at x -9.5 z -15.72
        //landscape offset is at x 0.42 y -0.115

        //main object is at x -18.5 z -15.72
        //landscape offset is at x 0.68 y -0.115

        //main object is at x -18.5 z -27.11
        //landscape offset is at x 0.68 y 0.6 (or 1.6 or 2.6 etc)

        float scale = 0.03f;
        float offsetX = -gameObject.transform.position.x * scale + someOffsetX;
        float offsetY = gameObject.transform.position.z * scale + someOffsetY;

        //when this main object moves, set the offest so it follows the main object
        //get the material first
        Material material = landscapeWithMaterial.GetComponent<Renderer>().material;

        //get the offset property 
        Vector2 offset = material.GetVector("Vector2_E103CB4D");
        //print(offset);


        //calculate the new 

        offset.x = offsetX;
        offset.y = offsetY;

        //set the new offset
        material.SetVector("Vector2_E103CB4D", offset);

    }
}
