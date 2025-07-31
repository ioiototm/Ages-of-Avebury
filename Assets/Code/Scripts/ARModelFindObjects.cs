using UnityEngine;
using System.Collections.Generic;

public class ARModelFindObjects : MonoBehaviour
{
    private Camera mainCamera;
    private HashSet<GameObject> foundObjects = new HashSet<GameObject>();
    private const string LookableTag = "LookableObject";

    [Tooltip("The maximum distance from the camera to an object to be considered 'found'.")]
    [SerializeField] private float maxDistance = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Find XR Origin (Mobile AR)/Camera Offset/Main Camera
        GameObject mainCameraObject = GameObject.Find("XR Origin (Mobile AR)/Camera Offset/Main Camera");



        mainCameraObject.AddComponent<SphereCollider>();

        mainCamera = mainCameraObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCamera == null) return;

        // Create a ray from the center of the camera's view
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Perform the raycast with a maximum distance
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // Check if the object hit has the "LookableObject" tag
            if (hit.collider.CompareTag(LookableTag))
            {
                GameObject lookedAtObject = hit.collider.gameObject;

                // Check if we haven't already found this object
                if (!foundObjects.Contains(lookedAtObject))
                {
                    Debug.Log($"Found object: {lookedAtObject.name} at a distance of {hit.distance}");
                    foundObjects.Add(lookedAtObject);

                    GameObject found = GameObject.Find("Found");
                    found.GetComponent<TMPro.TextMeshProUGUI>().text += lookedAtObject.name + "\n";


                    // Optional: Provide feedback that the object was found
                    // For example, disable the object or change its material
                    // lookedAtObject.SetActive(false); 
                }
            }
        }
    }
}
