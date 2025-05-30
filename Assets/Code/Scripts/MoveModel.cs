using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MoveModel : MonoBehaviour
{

    public GameObject landscapeWithMaterial;
    public GameObject landscapeWithMaterialUnderside;
    public float someOffsetX = 0.42f;
    public float someOffsetY = -0.115f;
    public float scale = 0.09f;

    //public Button left, right, up, down;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        //left = GameObject.Find("Left").GetComponent<Button>();
        //right = GameObject.Find("Right").GetComponent<Button>();
        //up = GameObject.Find("Up").GetComponent<Button>();
        //down = GameObject.Find("Down").GetComponent<Button>();

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

        
        float offsetX = -gameObject.transform.position.x * scale + someOffsetX;
        float offsetY = gameObject.transform.position.z * scale + someOffsetY;

        //when this main object moves, set the offest so it follows the main object
        //get the material first
        Material materialUp = landscapeWithMaterial.GetComponent<Renderer>().material;

        //get the offset property 
        Vector2 offset = materialUp.GetVector("Vector2_E103CB4D");
        print(offset);


        //Debug.Log("OffsetX: " + offsetX + " OffsetY: " + offsetY);

        //make the buttons work by changing teh offsets
        //lambda expressions for the buttons
        //left.onClick.AddListener(() => Move(-0.0001f, 0f));
        //right.onClick.AddListener(() => Move(0.0001f, 0f));
        //up.onClick.AddListener(() => Move(0f, 0.0001f));
        //down.onClick.AddListener(() => Move(0f, -0.0001f));


        //calculate the new 

        offset.x = offsetX;
        offset.y = offsetY;

        //set the new offset
        materialUp.SetVector("Vector2_E103CB4D", offset);


        Material underside = landscapeWithMaterialUnderside.GetComponent<Renderer>().material;
        //get the offset property
        Vector2 offsetUnderside = underside.GetVector("Vector2_E103CB4D");
        //Debug.Log("Underside OffsetX: " + offsetUnderside.x + " Underside OffsetY: " + offsetUnderside.y);
        //calculate the new offset for the underside
        offsetUnderside.x = offsetX;
        offsetUnderside.y = offsetY; 
        underside.SetVector("Vector2_E103CB4D", offsetUnderside);

    }

    void Move(float x, float y)
    {
        Debug.LogWarning("Move called with x: " + x + " y: " + y);

        //get the material first
        Material material = landscapeWithMaterial.GetComponent<Renderer>().material;
        //get the offset property 
        Vector2 offset = material.GetVector("Vector2_E103CB4D");
        //calculate the new offset
        offset.x += x;
        offset.y += y;
        //set the new offset
        material.SetVector("Vector2_E103CB4D", offset);
    }
}
