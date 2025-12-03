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
    public bool savedStone1 = false, savedStone2 = false;

    [SerializeField]
    public bool destroyedStone3 = false, savedStone3 = false;


    [SerializeField] public static List<DecisionMedieval.StoneDecision> decisions = new List<DecisionMedieval.StoneDecision>();

    [SerializeField]
    public bool southQuadrant = false;

    [SerializeField]
    public GameObject bakery, cottage, church;

    [SerializeField]
    GameObject beamOfLight;

    [SerializeField]
    public bool hasStones = false;

    public static bool alreadyLoaded=false;

    public void SyncFromTinySave()
    {
        var ts = TinySave.Instance != null ? TinySave.Instance : FindObjectOfType<TinySave>();

        gameCompleted = ts != null ? ts.IsGameCompleted() : TinySave.GetGameCompletionFlag();

        bool prefetchedStonesAvailable = Compass.meshesAndOutlines != null && Compass.meshesAndOutlines.Count > 0;
        hasStones = prefetchedStonesAvailable || TinySave.HasAnyPlayerCreatedStones;

        TinySave.StoneDecisionCollection collection = ts != null
            ? ts.LoadStoneMedieval()
            : TinySave.LoadStoneMedievalFromPrefs();

        if (collection?.decisions != null)
        {
            decisions = collection.decisions
                .Where(d => d?.stoneDecision != null)
                .Select(d => d.stoneDecision)
                .ToList();
        }
        else
        {
            decisions = new List<DecisionMedieval.StoneDecision>();
        }

        ApplyStoneOutcomeFlags();
        EnsurePlayerStonePrefabs();

        Debug.Log($"MapCompletion: Saved stones - slot1:{TinySave.HasPlayerStoneData(1)} slot2:{TinySave.HasPlayerStoneData(2)} slot3:{TinySave.HasPlayerStoneData(3)}");
    }

    private void ApplyStoneOutcomeFlags()
    {
        destroyedStone1 = TinySave.StoneWasDestroyed(1);
        destroyedStone2 = TinySave.StoneWasDestroyed(2);
        destroyedStone3 = TinySave.StoneWasDestroyed(3);

        savedStone1 = TinySave.StoneWasSaved(1);
        savedStone2 = TinySave.StoneWasSaved(2);
        savedStone3 = TinySave.StoneWasSaved(3);
    }

    private void EnsurePlayerStonePrefabs()
    {
        createdStone1 = ResolvePlayerStone(createdStone1, 1, "stone 1");
        createdStone2 = ResolvePlayerStone(createdStone2, 2, "stone 2");
        foundStone = ResolvePlayerStone(foundStone, 3, "stone 3");

        if (!hasStones)
        {
            hasStones = createdStone1 != null || createdStone2 != null || foundStone != null;
        }
    }

    private GameObject ResolvePlayerStone(GameObject current, int slot, string label)
    {
        bool hasData = TinySave.HasPlayerStoneData(slot);
        bool needsReload = current == null || current.GetComponent<MeshFilter>()?.sharedMesh == null;

        if (!hasData)
        {
            return current; // nothing saved for this slot
        }

        if (!needsReload)
        {
            return current; // current reference already contains a mesh
        }

        var prefab = TinySave.GetSavedStonePrefab(slot, stoneMaterial, stoneOutlineMaterial);
        if (prefab == null)
        {
            Debug.LogWarning($"MapCompletion: Player {label} data exists but prefab failed to reconstruct.");
        }
        else
        {
            Debug.Log($"MapCompletion: Reloaded player {label} from saved data.");
        }

        return prefab;
    }

    private GameObject SpawnBuildingAt(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject prefab = cottage != null ? cottage : bakery; // prefer cottage
        if (prefab == null) return null;
        var building = Instantiate(prefab, position, rotation * Quaternion.Euler(0, 180, 0), parent);
        building.name = prefab.name + "_FromStoneDecision";
        return building;
    }

    public IEnumerator wait10AndCreateStones(float seconds)
    {
               yield return new WaitForSeconds(seconds);

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

        hasStones = stones.Count > 0;
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
                EnsurePlayerStonePrefabs();

                if(southQuadrant)
                {
                    //if it's 81 or 87
                    if(child.name.Contains("81") || child.name.Contains("87"))
                    {

                        if (createdStone1 == null) continue;
                        if (createdStone2 == null) continue;

                        if (child.name.Contains("81"))
                        {
                            if (destroyedStone1)
                            {
                                SpawnBuildingAt(sphere.transform.position, Quaternion.identity, sphere.transform);
                                continue;
                            }
                            if (createdStone1 == null) continue;

                            //spawn the createdStone1 prefab at the position of the sphere
                            GameObject stoneCopy = Instantiate(createdStone1, sphere.transform.position, Quaternion.identity, sphere.transform);
                            stoneCopy.name = "Created Stone 1 " + child.name; // Name the stone copy
                            stoneCopy.transform.localScale = Vector3.one * 0.05f; // Scale down the stone copy
                            stoneCopy.SetActive(true); // Activate the stone copy

                            //spawn a beam of light at the position of the stone, above it
                            GameObject beam = Instantiate(beamOfLight, stoneCopy.transform.position + Vector3.up * 1f, Quaternion.identity, stoneCopy.transform);
                            beam.name = "Beam of Light " + child.name; // Name the beam of light
                            beam.transform.localScale = new Vector3(10, 150, 10);

                            //beam.transform.localScale = Vector3.one * 0.1f; // Scale down the beam of light
                            beam.SetActive(true); // Activate the beam of light

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
                            if (destroyedStone2)
                            {
                                SpawnBuildingAt(sphere.transform.position, Quaternion.identity, sphere.transform);
                                continue;
                            }
                            if (createdStone2 == null) continue;

                            //spawn the createdStone2 prefab at the position of the sphere
                            GameObject stoneCopy = Instantiate(createdStone2, sphere.transform.position, Quaternion.identity, sphere.transform);
                            stoneCopy.name = "Created Stone 2 " + child.name; // Name the stone copy
                            stoneCopy.transform.localScale = Vector3.one * 0.05f; // Scale down the stone copy
                            stoneCopy.SetActive(true); // Activate the stone copy
                                                       //rotate the stone copy to face the centre of the map

                            //spawn a beam of light at the position of the stone, above it
                            GameObject beam = Instantiate(beamOfLight, stoneCopy.transform.position + Vector3.up * 1f, Quaternion.identity, stoneCopy.transform);
                            beam.name = "Beam of Light " + child.name; // Name the beam of light
                            beam.transform.localScale = new Vector3(10, 150, 10);

                            //beam.transform.localScale = Vector3.one * 0.1f; // Scale down the beam of light
                            beam.SetActive(true); // Activate the beam of light

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

                            if (destroyedStone1)
                            {
                                SpawnBuildingAt(sphere.transform.position, Quaternion.identity, sphere.transform);
                                continue;
                            }
                            if (createdStone1 == null) continue;

                            //spawn the createdStone1 prefab at the position of the sphere
                            GameObject stoneCopy = Instantiate(createdStone1, sphere.transform.position, Quaternion.identity, sphere.transform);
                            stoneCopy.name = "Created Stone 1 " + child.name; // Name the stone copy
                            stoneCopy.transform.localScale = Vector3.one * 0.05f; // Scale down the stone copy
                            stoneCopy.SetActive(true); // Activate the stone copy
                                                       //rotate the stone copy to face the centre of the map

                            //spawn a beam of light at the position of the stone, above it
                            GameObject beam = Instantiate(beamOfLight, stoneCopy.transform.position + Vector3.up * 1f, Quaternion.identity, stoneCopy.transform);
                            beam.name = "Beam of Light " + child.name; // Name the beam of light
                            //beam.transform.localScale = Vector3.one * 0.9f; // Scale down the beam of light
                            beam.transform.localScale = new Vector3(10, 150, 10);

                            beam.SetActive(true); // Activate the beam of light
                            if (centreOfMap != null)
                            {
                                Vector3 directionToCentre = (centreOfMap.transform.position - stoneCopy.transform.position).normalized;
                                Quaternion lookRotation = Quaternion.LookRotation(directionToCentre);
                                stoneCopy.transform.rotation = lookRotation;
                            }
                        }
                        else if (child.name.Contains("53"))
                        {
                            if (destroyedStone2)
                            {
                                SpawnBuildingAt(sphere.transform.position, Quaternion.identity, sphere.transform);
                                continue;
                            }
                            if (createdStone2 == null) continue;

                            //spawn the createdStone2 prefab at the position of the sphere
                            GameObject stoneCopy = Instantiate(createdStone2, sphere.transform.position, Quaternion.identity, sphere.transform);
                            stoneCopy.name = "Created Stone 2 " + child.name; // Name the stone copy
                            stoneCopy.transform.localScale = Vector3.one * 0.05f; // Scale down the stone copy
                            stoneCopy.SetActive(true); // Activate the stone copy
                                                       //rotate the stone copy to face the centre of the map

                            //spawn a beam of light at the position of the stone, above it
                            GameObject beam = Instantiate(beamOfLight, stoneCopy.transform.position + Vector3.up * 1f, Quaternion.identity, stoneCopy.transform);
                            beam.name = "Beam of Light " + child.name; // Name the beam of light
                            //beam.transform.localScale = Vector3.one * 0.1f; // Scale down the beam of light
                            beam.transform.localScale = new Vector3(10, 150, 10);
                            beam.SetActive(true); // Activate the beam of light
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
                    // Special handling: third stone decision applied once
                    if (!foundStoneCreated && (savedStone3 || destroyedStone3))
                    {
                        if (destroyedStone3)
                        {
                            SpawnBuildingAt(sphere.transform.position, Quaternion.identity, sphere.transform);
                            foundStoneCreated = true;
                            continue;
                        }
                        else if (savedStone3 && foundStone != null)
                        {
                            GameObject stoneCopy = Instantiate(foundStone, sphere.transform.position, Quaternion.identity, sphere.transform);
                            stoneCopy.name = "Found Stone 3 " + child.name;
                            stoneCopy.transform.localScale = Vector3.one * 0.05f;
                            stoneCopy.SetActive(true);

                            GameObject beam = Instantiate(beamOfLight, stoneCopy.transform.position + Vector3.up * 1f, Quaternion.identity, stoneCopy.transform);
                            beam.name = "Beam of Light " + child.name;
                            //beam.transform.localScale = Vector3.one * 0.1f;
                            beam.transform.localScale = new Vector3(10, 150, 10);

                            beam.SetActive(true);

                            if (centreOfMap != null)
                            {
                                Vector3 directionToCentre = (centreOfMap.transform.position - stoneCopy.transform.position).normalized;
                                Quaternion lookRotation = Quaternion.LookRotation(directionToCentre);
                                stoneCopy.transform.rotation = lookRotation;
                            }

                            foundStoneCreated = true;
                            continue;
                        }
                    }

                    int randomIndex = Random.Range(0, stones.Count);
                    GameObject stoneCopy2 = Instantiate(stones[randomIndex], sphere.transform.position, Quaternion.identity,sphere.transform);
                    stoneCopy2.name = "Stone Copy " + child.name; // Name the stone copy
                    stoneCopy2.transform.localScale = Vector3.one * 0.05f; // Scale down the stone copy

                    stoneCopy2.SetActive(true); // Activate the stone copy

                    //rotate the stone copy to face the centre of the map
                    if (centreOfMap != null)
                    {
                        Vector3 directionToCentre = (centreOfMap.transform.position - stoneCopy2.transform.position).normalized;
                        Quaternion lookRotation = Quaternion.LookRotation(directionToCentre);
                        stoneCopy2.transform.rotation = lookRotation;
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
        SyncFromTinySave();
        StartCoroutine(InitializeStoneFlow());

        DontDestroyOnLoad(this); // Ensure this script persists across scenes
    }

    IEnumerator InitializeStoneFlow()
    {
        yield return EnsureSharedStonesAvailable();
        yield return wait10AndCreateStones(0.25f);
    }

    IEnumerator EnsureSharedStonesAvailable()
    {
        var tinySave = TinySave.Instance != null ? TinySave.Instance : FindObjectOfType<TinySave>();
        if (tinySave == null)
        {
            yield break;
        }

        bool completed = false;
        tinySave.EnsureSharedStoneCache(75, _ => completed = true);

        float timeout = 10f;
        while (!completed && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }
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
