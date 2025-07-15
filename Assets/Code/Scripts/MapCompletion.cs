using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using UnityEngine;

public class MapCompletion : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    [SerializeField]
    public GameObject mapWithEmpty;


    [SerializeField]
    public bool completeMap = false;


    [SerializeField]
    public bool gameCompleted = false;

    [SerializeField]
    Material stoneMaterial;

    [SerializeField]
    Material stoneOutlineMaterial;

    [SerializeField]
    public List<GameObject> stones = new List<GameObject>();

    [SerializeField]
    public GameObject createdStone1, createdStone2, foundStone;
    [SerializeField]
    public bool destroyedStone1 = false, destroyedStone2 = false, foundStoneCreated = false;

    [SerializeField]
    public bool southQuadrant = false;

    [SerializeField]
    GameObject bakery, cottage, church;

    public static bool alreadyLoaded=false;

    IEnumerator wait10AndCreateStones()
    {
               yield return new WaitForSeconds(5f);

        //go thorugh each Compass.rocks rock, and normalise and create the stones

        int id = 0;


        foreach (var stoneMesh in Compass.meshesAndOutlines)
        {
            GameObject stoneObject = new GameObject("Stone " + id);
            id++;

            MeshFilter meshFilter = stoneObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = stoneMesh.mesh;

            MeshRenderer meshRenderer = stoneObject.AddComponent<MeshRenderer>();
            //add both materials to the mesh renderer
            meshRenderer.sharedMaterials = new Material[] { stoneMaterial, stoneOutlineMaterial };

            stoneObject.SetActive(false); // Initially set to inactive

            //stoneObject.AddComponent<KeepOne>(); // Ensure this stone persists across scenes
            DontDestroyOnLoad(stoneObject); // Ensure the stone persists across scenes

            stones.Add(stoneObject);

        }

        alreadyLoaded = true; // Set the flag to true after loading the stones

        //foreach (var rock in Compass.rocks)
        //{
        //    if (rock == null || rock.Count == 0)
        //    {
        //        Debug.LogWarning("Rock or its outline points are null or empty.");
        //        continue;
        //    }
        //    //CreateStoneFromOutline(rock.outlinePoints);

        //    if (id % 3 == 0)
        //        yield return null;

        //    GameObject stoneObject = new GameObject("Stone " + id);
        //    id++;



        //    var sc = stoneObject.AddComponent<StoneCreator>();

        //    //convert List<Vector3> to List<Vector2>, by taking the x and z coordinates
        //    var outlinePoints = rock.Select(p => new Vector2(p.x, p.z)).ToList();

        //    var normalisedContour = SelfieSegmentationSample.NormalizeToBounds(outlinePoints, new Vector2(-4, -4), new Vector2(4, 4));

        //    sc.outlinePoints = normalisedContour;

        //    sc.fillResolution = 12;
        //    sc.metaballRadius = 0.55f;
        //    sc.outlineMetaballRadius = 0.309f;
        //    sc.fillDensity = 1.19f;
        //    sc.isoLevel = 0.055f;
        //    sc.slabThickness = 0.68f;

        //    sc.autoUpdate = false;

        //    sc.GenerateSlab();


        //    sc.GetComponent<MeshRenderer>().material = stoneMaterial;

        //    stoneObject.SetActive(false); // Initially set to inactive


        //    stones.Add(stoneObject);

        //    Debug.Log($"Created stone {stoneObject.name} with {outlinePoints.Count} points.");

        //}





    }

    IEnumerator spawnMap()
    {
        //GameObject map = Instantiate(mapWithEmpty, Vector3.zero, Quaternion.identity);
        GameObject mapObject = mapWithEmpty.transform.GetChild(1).gameObject;


        completeMap = false; // Reset the flag after spawning
                             //this gameobject has a child transform called "New Stones" and under it, all child transforms
                             //go through each one, and spawn a sphere at the position of each child transform
        Transform newStones = mapObject.transform.Find("New Stones");

        int id = 0;
        GameObject centreOfMap = mapObject.transform.Find("Centre")?.gameObject;
        if (newStones != null)
        {
            foreach (Transform child in newStones)
            {
                GameObject sphere = new GameObject(child.name);
                sphere.transform.position = child.position;
                sphere.transform.localScale = Vector3.one * 0.1f; // Scale down the sphere
                sphere.name = child.name; // Name the sphere after the child transform
                sphere.transform.parent = child; // Set parent to New Stones

                id++;

                if(southQuadrant)
                {
                    //if it's 81 or 87
                    if(child.name.Contains("81") || child.name.Contains("87"))
                    {

                        if (createdStone1 == null) continue;
                        if (createdStone2 == null) continue;

                        if (child.name.Contains("81"))
                        {
                            //spawn the createdStone1 prefab at the position of the sphere
                            GameObject stoneCopy = Instantiate(createdStone1, sphere.transform.position, Quaternion.identity, sphere.transform);
                            stoneCopy.name = "Created Stone 1 " + child.name; // Name the stone copy
                            stoneCopy.transform.localScale = Vector3.one * 0.05f; // Scale down the stone copy
                            stoneCopy.SetActive(true); // Activate the stone copy

                            //rotate the stone copy to face the centre of the map
                            if (centreOfMap != null)
                            {
                                Vector3 directionToCentre = (centreOfMap.transform.position - stoneCopy.transform.position).normalized;
                                Quaternion lookRotation = Quaternion.LookRotation(directionToCentre);
                                stoneCopy.transform.rotation = lookRotation;
                            }
                        }
                        else if (child.name.Contains("87"))
                        {
                            //spawn the createdStone2 prefab at the position of the sphere
                            GameObject stoneCopy = Instantiate(createdStone2, sphere.transform.position, Quaternion.identity, sphere.transform);
                            stoneCopy.name = "Created Stone 2 " + child.name; // Name the stone copy
                            stoneCopy.transform.localScale = Vector3.one * 0.05f; // Scale down the stone copy
                            stoneCopy.SetActive(true); // Activate the stone copy
                            //rotate the stone copy to face the centre of the map
                            if (centreOfMap != null)
                            {
                                Vector3 directionToCentre = (centreOfMap.transform.position - stoneCopy.transform.position).normalized;
                                Quaternion lookRotation = Quaternion.LookRotation(directionToCentre);
                                stoneCopy.transform.rotation = lookRotation;
                            }
                        }

                        continue;
                    }
                }
                else
                {
                    //if it's 61 or 53
                    if (child.name.Contains("61") || child.name.Contains("53"))
                    {

                        if (createdStone1 == null) continue;
                        if (createdStone2 == null) continue;

                        if (child.name.Contains("61"))
                        {



                            //spawn the createdStone1 prefab at the position of the sphere
                            GameObject stoneCopy = Instantiate(createdStone1, sphere.transform.position, Quaternion.identity, sphere.transform);
                            stoneCopy.name = "Created Stone 1 " + child.name; // Name the stone copy
                            stoneCopy.transform.localScale = Vector3.one * 0.05f; // Scale down the stone copy
                            stoneCopy.SetActive(true); // Activate the stone copy
                            //rotate the stone copy to face the centre of the map
                            if (centreOfMap != null)
                            {
                                Vector3 directionToCentre = (centreOfMap.transform.position - stoneCopy.transform.position).normalized;
                                Quaternion lookRotation = Quaternion.LookRotation(directionToCentre);
                                stoneCopy.transform.rotation = lookRotation;
                            }
                        }
                        else if (child.name.Contains("53"))
                        {
                            //spawn the createdStone2 prefab at the position of the sphere
                            GameObject stoneCopy = Instantiate(createdStone2, sphere.transform.position, Quaternion.identity, sphere.transform);
                            stoneCopy.name = "Created Stone 2 " + child.name; // Name the stone copy
                            stoneCopy.transform.localScale = Vector3.one * 0.05f; // Scale down the stone copy
                            stoneCopy.SetActive(true); // Activate the stone copy
                            //rotate the stone copy to face the centre of the map
                            if (centreOfMap != null)
                            {
                                Vector3 directionToCentre = (centreOfMap.transform.position - stoneCopy.transform.position).normalized;
                                Quaternion lookRotation = Quaternion.LookRotation(directionToCentre);
                                stoneCopy.transform.rotation = lookRotation;
                            }
                        }

                        continue; // Skip these spheres in the north quadrant
                    }
                }

                //pick a random stone from the stones, spawn a copy of it at the position of the sphere
                if (stones.Count > 0)
                {
                    int randomIndex = Random.Range(0, stones.Count);
                    GameObject stoneCopy = Instantiate(stones[randomIndex], sphere.transform.position, Quaternion.identity,sphere.transform);
                    //stoneCopy.transform.parent = sphere.transform; // Set parent to the sphere
                    stoneCopy.name = "Stone Copy " + child.name; // Name the stone copy
                                                                 //set scale to 0.05f
                    stoneCopy.transform.localScale = Vector3.one * 0.05f; // Scale down the stone copy

                    stoneCopy.SetActive(true); // Activate the stone copy

                    //rotate the stone copy to face the centre of the map
                    if (centreOfMap != null)
                    {
                        Vector3 directionToCentre = (centreOfMap.transform.position - stoneCopy.transform.position).normalized;
                        Quaternion lookRotation = Quaternion.LookRotation(directionToCentre);
                        stoneCopy.transform.rotation = lookRotation;
                    }

                    Debug.Log("Spawned stone "+ child.name);
                }
                else
                {
                    Debug.LogWarning("No stones available to spawn.");
                }

                Debug.Log($"Spawned sphere at {child.position} with name {child.name}");

                yield return new WaitForSeconds(0.1f); // Yield to avoid freezing the main thread
            }
        }
        else
        {
            Debug.LogError("New Stones transform not found in the map prefab.");
        }

        Transform newBuildings = mapObject.transform.Find("Buildings");

        if (newBuildings != null)
        {

            //go through each child transform in newBuildings and spawn a building prefab at the position of each child transform
            //2% chance for a bakery, 98% chance for a cottage, no church

            foreach (Transform child in newBuildings)
            {
                GameObject buildingPrefab = null;
                // Randomly choose a building type
                float randomValue = Random.Range(0f, 100f);
                if (randomValue < 2f) // 2% chance for bakery
                {
                    buildingPrefab = bakery;
                }
                else // 98% chance for cottage
                {
                    buildingPrefab = cottage;
                }
                if (buildingPrefab != null)
                {
                    //instantiate it, and do the child rotation and flip y to 180 from what it is
                    //basically, the child rotation plus a rotation of 180 degrees on the y-axis
                    GameObject buildingInstance = Instantiate(buildingPrefab, child.position, child.rotation * Quaternion.Euler(0, 180, 0));
                    buildingInstance.transform.parent = child; // Set parent to the child transform
                    Debug.Log($"Spawned {buildingInstance.name} at {child.position}");
                }
                else
                {
                    Debug.LogWarning("Building prefab is not assigned.");
                }
                yield return new WaitForSeconds(0.1f);  // Yield to avoid freezing the main thread
            }
        }
    }

    void Start()
    {
        StartCoroutine(wait10AndCreateStones());

        DontDestroyOnLoad(this); // Ensure this script persists across scenes
    }

    // Update is called once per frame
    void Update()
    {

        if(completeMap)
        {

            //spawn the map with empty
            if (mapWithEmpty != null)
            {
                if (gameCompleted)
                {

                    StartCoroutine(spawnMap());
                    completeMap = false; // Reset the flag after spawning
                }
            }
            else
            {
                Debug.LogError("Map with empty prefab is not assigned.");
            }
        }

    }
}
