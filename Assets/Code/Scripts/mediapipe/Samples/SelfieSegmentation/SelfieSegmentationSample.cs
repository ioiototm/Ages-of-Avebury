using TensorFlowLite;
using UnityEngine;
using UnityEngine.UI;
using TextureSource;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using LoGaCulture.LUTE.Logs;

[RequireComponent(typeof(VirtualTextureSource))]
public class SelfieSegmentationSample : MonoBehaviour
{
    [SerializeField]
    private RawImage outputView = null;

    [SerializeField]
    private SelfieSegmentation.Options options = default;

    private SelfieSegmentation segmentation;

    public GameObject xrObject;


    public GameObject currentStone;
  

    private void Start()
    {
        segmentation = new SelfieSegmentation(options);
        if (TryGetComponent(out VirtualTextureSource source))
        {
            source.OnTexture.AddListener(OnTextureUpdate);
        }

    }

    private void OnDestroy()
    {
        if (TryGetComponent(out VirtualTextureSource source))
        {
            source.OnTexture.RemoveListener(OnTextureUpdate);
        }
        segmentation?.Dispose();
    }

    private void DrawContour(List<Vector2> contour, Camera cam)
    {
        for (int i = 0; i < contour.Count; i++)
        {
            Vector2 p1 = contour[i];
            Vector2 p2 = contour[(i + 1) % contour.Count]; // Loop back to the start

            // Scale and move points into world space
            //Vector3 worldPos1 = cam.ViewportToWorldPoint(new Vector3(p1.x, p1.y, 1));
            //Vector3 worldPos2 = cam.ViewportToWorldPoint(new Vector3(p2.x, p2.y, 1));

            //Debug.DrawLine(worldPos1, worldPos2, Color.green, 0.1f);

            //multiply each point by 0.1
            p1 *= 0.04f;
            p2 *= 0.04f;


            //move it 10 to the left 
            p1.x -= 3;
            p2.x -= 3;


            Debug.DrawLine(new Vector3(p1.x, p1.y, -1), new Vector3(p2.x, p2.y, -1), Color.white, 0.1f);

            //print the first 10 points
            //if (i < 10)
            //{
            //    Debug.Log($"Point {i}: {p1}");
            //}

        }
    }

    private void DrawContourWithRenderer(List<Vector2> contour, Camera cam)
    {


        //get previous game object ContourLine if there is and delete it
        GameObject previousContourLine = GameObject.Find("ContourLine");
        if (previousContourLine != null)
        {
            Destroy(previousContourLine);
        }

        // Create a new GameObject to hold the LineRenderer
        GameObject lineObject = new GameObject("ContourLine");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        // Configure LineRenderer settings
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Use a simple material
        lineRenderer.widthMultiplier = 0.05f; // Line thickness
        lineRenderer.positionCount = contour.Count + 1; // Loop back to start

        // Set the line points
        for (int i = 0; i < contour.Count; i++)
        {
            Vector2 p = contour[i] * 0.04f; // Scale the point
            p.x -= 3; // Shift it to the left
            Vector3 worldPoint = new Vector3(p.x, p.y, -1);
            lineRenderer.SetPosition(i, worldPoint); // Add point to the LineRenderer
        }

        // Loop back to the start
        Vector2 firstPoint = contour[0] * 0.04f;
        firstPoint.x -= 3;
        lineRenderer.SetPosition(contour.Count, new Vector3(firstPoint.x, firstPoint.y, -1));
    }




    bool pressedButton = false;

    private void Update()
    {
        //if j key was pressed
        if (Input.GetKeyDown(KeyCode.J))
        {
            pressedButton = true;
        }
        
    }


    

    public void OnClick(Button button)
    {
        if (pressedButton == false)
        {
            pressedButton = true;

            pressedButtonObject = button;
            //disable the button
            button.interactable = false;

        }
    }

    private Vector3 TextureToWorldPoint(Vector2 texCoord, Camera camera, RenderTexture maskTex, float distance = 1f)
    {
        // Normalize texture coordinates (0..1)
        float normX = texCoord.x / maskTex.width;
        float normY = texCoord.y / maskTex.height;

        // Convert to viewport space (0..1), flipping Y
        Vector3 viewportPoint = new Vector3(normX, 1.0f - normY, distance);

        // Map viewport point to world space
        return camera.ViewportToWorldPoint(viewportPoint);
    }

    public float scale = 1.0f;

    public Material stoneMaterial;



    public bool hasMadeStone = false;

    IEnumerator afterSceneLoad()
    {
        //wait 5 seconds
        yield return new WaitForSeconds(5);

        //gtet the obhject "Object Spawner" 
        GameObject objectSpawner = GameObject.Find("Object Spawner");

        //get the script ObjectSpawner
        ObjectSpawner objectSpawnerScript = objectSpawner.GetComponent<ObjectSpawner>();


        //set the scale of the game object to 0.04
        gameObject.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);

        //create a copy of the game object
        GameObject newGameObject = Instantiate(gameObject);


        //set the object prefabs of the first object to be the poly extruder
        objectSpawnerScript.objectPrefabs[0] = newGameObject;

        //disable original object
        gameObject.SetActive(false);


    }

    [SerializeField] private float shimmerAmplitude = 0.015f;   // max offset in world units
    [SerializeField] private float shimmerFrequency = 0.75f;     // cycles per second
    [SerializeField] private float shimmerNoiseScale = 0.8f;     // perlin sampling density


    Coroutine runningCoroutine = null;
    private bool forceFinishLine = false;   
    private bool pauseAfterFinish = false; 

    private IEnumerator DrawContourWithRenderer(
        List<Vector2> contour,
        Camera cam,
        int pointsPerFrame = 1,
        float frameDelay = 0.1f,
        float endPause = 1f)
    {
        if (runningCoroutine != null) yield break;
        runningCoroutine = StartCoroutine(Co());
        yield return runningCoroutine;
        runningCoroutine = null;
        yield break;

        IEnumerator Co()
        {
            GameObject prev = GameObject.Find("ContourLine");
            if (prev) Destroy(prev);

            var go = new GameObject("ContourLine");
            var lr = go.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.widthMultiplier = 0.05f;
            lr.loop = forceFinishLine;

            if (forceFinishLine)
            {
                lr.positionCount = contour.Count;
                for (int i = 0; i < contour.Count; i++)
                {
                    Vector2 p = contour[i] * 0.04f; p.x -= 3;
                    lr.SetPosition(i, new Vector3(p.x, p.y, -1f));
                }
            }
            else
            {
                for (int i = 0; i < contour.Count; ++i)
                {
                    lr.positionCount = i + 1;
                    Vector2 p = contour[i] * 0.04f; p.x -= 3;
                    lr.SetPosition(i, new Vector3(p.x, p.y, -1f));

                    if ((i + 1) % pointsPerFrame == 0)
                        yield return new WaitForSeconds(frameDelay);
                }
            }

            // Attach shimmer animator
            var animator = go.AddComponent<ContourLineAnimator>();
            animator.Initialize(lr, shimmerAmplitude, shimmerFrequency, shimmerNoiseScale);

            if (!forceFinishLine || pauseAfterFinish)
                yield return new WaitForSeconds(endPause);
        }
    }

    [SerializeField]
    bool firstStone = true;


    private Button pressedButtonObject;


    public StoneCreation stoneOrder = null;

    private void OnTextureUpdate(Texture texture)
    {   

        segmentation.Run(texture);

        RenderTexture maskTex = segmentation.GetResultTexture();

        Color32[] maskPixels = segmentation.ReadMaskPixels(maskTex);



        List<Vector2> contour = new List<Vector2>();

        //Debug.Log("Extracted contour: " + contour.Count + " points.");
        if (hasMadeStone)
        {

            //get the game object ContourLine and disable it if it exists
            GameObject contourLine = GameObject.Find("ContourLine");

            //if (contourLine != null)

            //    contourLine.SetActive(false);
            contour = ContourTracing.TraceContour(maskPixels, maskTex.width, maskTex.height);
            //DrawContourWithRenderer(contour, Camera.main);
            //contourDrawer.DrawContour(contour, Camera.main);

            if (runningCoroutine == null)
            {
                runningCoroutine = StartCoroutine(DrawContourWithRenderer(contour, Camera.main, -1));
            }

            this.gameObject.SetActive(false);

            return;
        }
        if (!hasMadeStone)
        {
            contour = ContourTracing.TraceContour(maskPixels, maskTex.width, maskTex.height);
            //DrawContourWithRenderer(contour, Camera.main);
            //contourDrawer.DrawContour(contour, Camera.main);

            if(runningCoroutine == null)
            {
                runningCoroutine = StartCoroutine(DrawContourWithRenderer(contour, Camera.main, 15));
            }


        }
        // 3) Marching Squares



        if (pressedButton)
        {

            if (contour.Count < 50)
            {
                pressedButton = false;
                pressedButtonObject.interactable = true;
                return;
            }


           

            //make it uninteractable
            pressedButtonObject.interactable = false;

            stoneOrder.CreateStone();


            pressedButton = false;

            //List<Vector2> contour = ContourTracing.TraceContour(maskPixels, maskTex.width, maskTex.height);
            //ExtractContour(maskPixels, maskTex.width, maskTex.height);
            int w = maskTex.width;
            int h = maskTex.height;


            //debug draw the contour


            forceFinishLine = true;
            pauseAfterFinish = true;

        
            hasMadeStone = true;


            //draw the contour one last time
            if (runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
                runningCoroutine = null;
            }

            // Fix for the error: Replace the incorrect call to `StartCoroutine` with the correct method signature.
            StartCoroutine(DrawContourWithRenderer(contour, Camera.main, -1, 0.05f, 0.5f));
            //StartCoroutine(DrawContourWithRenderer(contour, Camera.main));

            

            var stoneCreator = currentStone.GetComponent<StoneCreator>();

            var normalisedContour = NormalizeToBounds(contour, new Vector2(-4, -4), new Vector2(4, 4));

            stoneCreator.outlinePoints = normalisedContour;
            stoneCreator.GenerateSlab();

            var mesh = stoneCreator.GetComponent<MeshFilter>().mesh;

            //disable the stone for now
            stoneCreator.gameObject.SetActive(false);


            var mapCompletion = GameObject.Find("MapComplete").GetComponent<MapCompletion>();

            if(firstStone)
            {
                mapCompletion.createdStone1 = stoneCreator.gameObject;
            }
            else
            {
                mapCompletion.createdStone2 = stoneCreator.gameObject;
            }




                //set this script to false active
                //this.gameObject.SetActive(false);

                DontDestroyOnLoad(gameObject);


            //var lineRenderer = polyExtruderGO.GetComponent<LineRenderer>();

            //get the list of points
            //Vector3[] points = new Vector3[lineRenderer.positionCount + 1];



            //lineRenderer.GetPositions(points);

            //go through all the points and save them as a string in the format "x,y,z;x,y,z;etc"
            //string pointsString = string.Join(";", points.Select(p => $"{p.x},{p.y},{p.z}"));

            //ConnectionManager.Instance.SaveSharedVariable("stone1", "stone", pointsString);


            var meshData = MeshSerializer.ToBase64(mesh, normalisedContour);
            ConnectionManager.Instance.SaveSharedVariable("StoneComplete", "MeshAndOutlineBase64",meshData);
            TinySave.Instance.SaveStoneData(meshData, firstStone ? 1 : 2);


            //load next scene in the build
            //UnityEngine.SceneManagement.SceneManager.LoadScene(1);

            //StartCoroutine(afterSceneLoad());



            //enable the xr object
            //xrObject.SetActive(true);






            //move it to the centre of the camera
            //polyExtruderGO.transform.position = new Vector3(0, 0, 0);

            //for (int i = 0; i < contour.Count; i++)
            //{
            //    Vector2 p1 = contour[i];
            //    Vector2 p2 = contour[(i + 1) % contour.Count]; // Loop back to the start

            //    // Scale and move points into world space
            //    Vector3 worldPos1 = new Vector3(p1.x * scale, p1.y * scale, -3) + offset;
            //    Vector3 worldPos2 = new Vector3(p2.x * scale, p2.y * scale, -3) + offset;

            //    Debug.DrawLine(worldPos1, worldPos2, Color.green, 5f);

            //    //if(i < 10)
            //    //Debug.Log("Drawing Line from " + worldPos1 + " to " + worldPos2);
            //}

            //// 3. Use Marching Squares to extract line segments
            //var segments = MarchingSquaresHelper.GenerateContours(maskPixels, width, height);
            //Debug.Log($"Generated {segments.Count} line segments");

            //// 4. Build a single polygon from the segments
            //var polygon = PolygonBuilder.BuildPolygonFromSegments(segments);
            //Debug.Log($"Built polygon with {polygon.Count} vertices");

            //// 5. Triangulate the polygon to generate a 2D mesh
            //var triangleIndices = Triangulator.Triangulate(polygon);
            //Mesh flatMesh = Build2DMesh(polygon, triangleIndices);

            //// 6. Extrude the 2D mesh into a 3D shape
            //float thickness = 0.2f; // Adjust thickness as needed
            //Mesh extrudedMesh = MeshExtruder.ExtrudeMesh(flatMesh, thickness);

            //// 7. Create a GameObject to display the stone
            //CreateStoneObject(extrudedMesh);

        }

        //var segments = MarchingSquaresHelper.GenerateContours(maskPixels, w, h);
        //Debug.Log($"Got {segments.Count} line segments");

        //3) Scale and draw in Scene View
        //float scale = 0.01f; // Adjust as needed
        //Vector3 offset = new Vector3(-5, -5, 0); // Offset to center it in your view

        //Debug.Log("Countour count " + contour.Count);

        //for (int i = 0; i < contour.Count; i++)
        //{
        //    Vector2 p1 = contour[i];
        //    Vector2 p2 = contour[(i + 1) % contour.Count]; // Loop back to the start

        //    // Scale and move points into world space
        //    Vector3 worldPos1 = new Vector3(p1.x * scale, p1.y * scale, -3) + offset;
        //    Vector3 worldPos2 = new Vector3(p2.x * scale, p2.y * scale, -3) + offset;

        //    Debug.DrawLine(worldPos1, worldPos2, Color.green, 5f);

        //    //if(i < 10)
        //    //Debug.Log("Drawing Line from " + worldPos1 + " to " + worldPos2);
        //}

        //Vector3 offset = new Vector3(0, 0, 2); // 2 units in front of camera

        //foreach (var seg in segments)
        //{
        //    Vector3 p1 = offset + new Vector3(seg.Item1.x * scale, seg.Item1.y * scale, 0);
        //    Vector3 p2 = offset + new Vector3(seg.Item2.x * scale, seg.Item2.y * scale, 0);
        //    Debug.DrawLine(p1, p2, Color.green, 0.1f); // draws for 5 seconds
        //}


        //outputView.texture = segmentation.GetResultTexture();
    }

    public static List<Vector2> NormalizeToBounds(List<Vector2> input, Vector2 minTarget, Vector2 maxTarget)
    {
        // Step 1: Find original bounds
        Vector2 min = input[0];
        Vector2 max = input[0];
        foreach (var p in input)
        {
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }

        Vector2 inputSize = max - min;
        Vector2 targetSize = maxTarget - minTarget;

        // Avoid divide-by-zero if input shape is flat
        if (inputSize.x == 0) inputSize.x = 1;
        if (inputSize.y == 0) inputSize.y = 1;

        // Step 2: Scale and remap
        List<Vector2> result = new();
        foreach (var p in input)
        {
            Vector2 normalized = new Vector2(
                (p.x - min.x) / inputSize.x,
                (p.y - min.y) / inputSize.y
            );

            Vector2 mapped = new Vector2(
                minTarget.x + normalized.x * targetSize.x,
                minTarget.y + normalized.y * targetSize.y
            );

            result.Add(mapped);
        }

        return result;
    }

    private void GenerateBoxUVs(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];

            // Use the largest two components of the vertex for UVs (box projection)
            if (Mathf.Abs(v.x) > Mathf.Abs(v.y) && Mathf.Abs(v.x) > Mathf.Abs(v.z))
            {
                uvs[i] = new Vector2(v.y, v.z); // X-dominant
            }
            else if (Mathf.Abs(v.y) > Mathf.Abs(v.z))
            {
                uvs[i] = new Vector2(v.x, v.z); // Y-dominant
            }
            else
            {
                uvs[i] = new Vector2(v.x, v.y); // Z-dominant
            }
        }

        mesh.uv = uvs; // Assign UVs to the mesh
    }



    private void GenerateUVs(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];

        // Planar mapping: project texture onto the X-Y plane
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].y);
        }

        mesh.uv = uvs; // Assign UVs to the mesh
    }

    private void GenerateFixedSideUVs(Mesh mesh, float height)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Create a new UV array
        Vector2[] uvs = new Vector2[vertices.Length];

        // Find min and max Z for height mapping
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        foreach (var vertex in vertices)
        {
            if (vertex.z < minZ) minZ = vertex.z;
            if (vertex.z > maxZ) maxZ = vertex.z;
        }

        //float height = maxZ - minZ;

        // Track perimeter distances for the side vertices
        Dictionary<int, float> perimeterDistances = new Dictionary<int, float>();

        // Calculate perimeter distances for side vertices
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // Get the vertices for this triangle
            int v0 = triangles[i];
            int v1 = triangles[i + 1];
            int v2 = triangles[i + 2];

            Vector3 p0 = vertices[v0];
            Vector3 p1 = vertices[v1];
            Vector3 p2 = vertices[v2];

            // Check if this is a side face (two vertices share the same Z)
            if (Mathf.Abs(p0.z - p1.z) < 0.01f || Mathf.Abs(p1.z - p2.z) < 0.01f || Mathf.Abs(p2.z - p0.z) < 0.01f)
            {
                float d0 = Vector2.Distance(new Vector2(p0.x, p0.y), new Vector2(p1.x, p1.y));
                float d1 = Vector2.Distance(new Vector2(p1.x, p1.y), new Vector2(p2.x, p2.y));
                float d2 = Vector2.Distance(new Vector2(p2.x, p2.y), new Vector2(p0.x, p0.y));

                // Accumulate distances for each vertex
                if (!perimeterDistances.ContainsKey(v0)) perimeterDistances[v0] = 0;
                if (!perimeterDistances.ContainsKey(v1)) perimeterDistances[v1] = 0;
                if (!perimeterDistances.ContainsKey(v2)) perimeterDistances[v2] = 0;

                perimeterDistances[v1] += d0;
                perimeterDistances[v2] += d1;
                perimeterDistances[v0] += d2;
            }
        }

        // Normalize perimeter distances for UV mapping
        float totalPerimeter = 0f;
        foreach (var kvp in perimeterDistances)
        {
            totalPerimeter += kvp.Value;
        }

        foreach (var key in perimeterDistances.Keys.ToList())
        {
            perimeterDistances[key] /= totalPerimeter; // Normalize to [0, 1]
        }

        // Assign UVs for the sides
        for (int i = 0; i < vertices.Length; i++)
        {
            float u = perimeterDistances.ContainsKey(i) ? perimeterDistances[i] : 0f; // Use perimeter distance for U
            float v = (vertices[i].z - minZ) / height; // Map V based on Z height

            uvs[i] = new Vector2(u, v);
        }

        // Assign the new UVs back to the mesh
        mesh.uv = uvs;
    }





    // Function to combine all meshes in a GameObject
    void CombineMeshes(GameObject parentGO)
    {
        MeshFilter[] meshFilters = parentGO.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {


           


            combine[i].mesh = meshFilters[i].sharedMesh;
            
            
            
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;

            // Optionally disable the child GameObject after combining
            
            
            meshFilters[i].gameObject.SetActive(false);




        }

        // Create a new combined mesh
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        // Assign the combined mesh to the parent GameObject
        MeshFilter parentMeshFilter = parentGO.GetComponent<MeshFilter>();
        if (parentMeshFilter == null)
        {
            parentMeshFilter = parentGO.AddComponent<MeshFilter>();
        }
        parentMeshFilter.mesh = combinedMesh;


        // Add or get a MeshRenderer for the combined mesh
        if (parentGO.GetComponent<MeshRenderer>() == null)
        {
            parentGO.AddComponent<MeshRenderer>();
        }


        // Generate UVs for the combined mesh
        GenerateUVs(combinedMesh);
        //GenerateFixedSideUVs(combinedMesh,1f);

        //get the mesh renderer
        // MeshRenderer meshRenderer = parentGO.GetComponent<MeshRenderer>();
        //if (meshRenderer == null) 
        {

            //ScaleTopAndBottomLayers(combinedMesh, 0.2f,0.2f);

            //AddHeightNoise(combinedMesh, 1f, -2f);
        }


    }

    private void ScaleTopAndBottomLayers(Mesh mesh, float scaleFactorTop, float scaleFactorBottom)
    {
        Vector3[] vertices = mesh.vertices;

        // Find the top and bottom Z values
        float maxZ = float.MinValue;
        float minZ = float.MaxValue;
        foreach (var vertex in vertices)
        {
            if (vertex.z > maxZ) maxZ = vertex.z;
            if (vertex.z < minZ) minZ = vertex.z;
        }

        // Scale the top and bottom layers
        for (int i = 0; i < vertices.Length; i++)
        {
            if (Mathf.Abs(vertices[i].z - maxZ) < 0.01f) // Top layer
            {
                vertices[i] = new Vector3(
                    vertices[i].x * scaleFactorTop,
                    vertices[i].y * scaleFactorTop,
                    vertices[i].z
                );
            }
            else if (Mathf.Abs(vertices[i].z - minZ) < 0.01f) // Bottom layer
            {
                vertices[i] = new Vector3(
                    vertices[i].x * scaleFactorBottom,
                    vertices[i].y * scaleFactorBottom,
                    vertices[i].z
                );
            }
        }

        // Apply the updated vertices and recalculate normals
        mesh.vertices = vertices;
        mesh.RecalculateNormals(); // Ensures proper lighting/shading
    }


    private void ScaleTopLayer(Mesh mesh, float scaleFactor)
    {
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            // Scale only the top layer (e.g., vertices with z == maxZ)
            if (Mathf.Abs(vertices[i].z - mesh.bounds.max.z) < 0.01f)
            {
                vertices[i] = new Vector3(vertices[i].x * scaleFactor, vertices[i].y * scaleFactor, vertices[i].z);
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals(); // Recalculate normals for proper shading
    }

    private void AddHeightNoise(Mesh mesh, float noiseScale, float noiseStrength)
    {
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            // Apply noise only to the top surface (e.g., vertices with z == maxZ)
            if (Mathf.Abs(vertices[i].z - mesh.bounds.max.z) < 0.01f)
            {
                float noise = Mathf.PerlinNoise(vertices[i].x * noiseScale, vertices[i].y * noiseScale);
                vertices[i] += new Vector3(0, 0, noise * noiseStrength); // Adjust Z-axis based on noise
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals(); // Recalculate normals for proper lighting
    }

    List<Vector2> SortContourPoints(List<Vector2> points)
    {
        if (points.Count == 0) return points;

        List<Vector2> sortedPoints = new List<Vector2>();
        Vector2 currentPoint = points[0];
        sortedPoints.Add(currentPoint);
        points.RemoveAt(0);

        while (points.Count > 0)
        {
            Vector2 nearestPoint = FindNearestPoint(currentPoint, points);
            sortedPoints.Add(nearestPoint);
            points.Remove(nearestPoint);
            currentPoint = nearestPoint;
        }

        return sortedPoints;
    }


    List<Vector2> SortContourPointsByConcaveHull(List<Vector2> points, int k)
    {
        if (points.Count == 0) return points;

        // Find the concave hull
        List<Vector2> hull = ConcaveHull(points, k);

        // Sort the points based on the concave hull
        List<Vector2> sortedPoints = new List<Vector2>(hull);

        return sortedPoints;
    }

    List<Vector2> ConcaveHull(List<Vector2> points, int k)
    {
        if (points.Count <= 1) return points;

        // Ensure k is at least 3
        k = Mathf.Max(k, 3);

        List<Vector2> hull = new List<Vector2>();

        // Start with the leftmost point
        Vector2 startPoint = points[0];
        foreach (var point in points)
        {
            if (point.x < startPoint.x)
            {
                startPoint = point;
            }
        }

        hull.Add(startPoint);
        points.Remove(startPoint);

        Vector2 currentPoint = startPoint;
        while (points.Count > 0)
        {
            List<Vector2> nearestNeighbors = FindKNearestNeighbors(currentPoint, points, k);
            Vector2 nextPoint = nearestNeighbors[0];
            float minAngle = float.MaxValue;

            foreach (var neighbor in nearestNeighbors)
            {
                float angle = Vector2.SignedAngle(currentPoint - hull[hull.Count - 1], neighbor - currentPoint);
                if (angle < minAngle)
                {
                    minAngle = angle;
                    nextPoint = neighbor;
                }
            }

            if (nextPoint == startPoint)
            {
                break;
            }

            hull.Add(nextPoint);
            points.Remove(nextPoint);
            currentPoint = nextPoint;
        }

        return hull;
    }

    List<Vector2> FindKNearestNeighbors(Vector2 point, List<Vector2> points, int k)
    {
        List<Vector2> neighbors = new List<Vector2>(points);
        neighbors.Sort((a, b) => Vector2.Distance(point, a).CompareTo(Vector2.Distance(point, b)));
        return neighbors.GetRange(0, Mathf.Min(k, neighbors.Count));
    }

    List<Vector2> SortContourPointsByConvexHull(List<Vector2> points)
    {
        if (points.Count == 0) return points;

        // Find the convex hull
        List<Vector2> hull = ConvexHull(points);

        // Sort the points based on the convex hull
        List<Vector2> sortedPoints = new List<Vector2>(hull);

        return sortedPoints;
    }

    List<Vector2> ConvexHull(List<Vector2> points)
    {
        if (points.Count <= 1) return points;

        // Sort points by x-coordinate (in case of tie, by y-coordinate)
        points.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));

        List<Vector2> hull = new List<Vector2>();

        // Build lower hull
        foreach (var p in points)
        {
            while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
            {
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(p);
        }

        // Build upper hull
        int t = hull.Count + 1;
        for (int i = points.Count - 1; i >= 0; i--)
        {
            var p = points[i];
            while (hull.Count >= t && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
            {
                hull.RemoveAt(hull.Count - 1);
            }
            hull.Add(p);
        }

        hull.RemoveAt(hull.Count - 1); // Remove the last point because it's the same as the first one

        return hull;
    }


    Vector2 FindNearestPoint(Vector2 currentPoint, List<Vector2> points)
    {
        Vector2 nearestPoint = points[0];
        float minDistance = Vector2.Distance(currentPoint, nearestPoint);

        foreach (var point in points)
        {
            float distance = Vector2.Distance(currentPoint, point);
            if (distance < minDistance)
            {
                nearestPoint = point;
                minDistance = distance;
            }
        }

        return nearestPoint;
    }

    private void CreateStoneObject(Mesh stoneMesh)
    {
        GameObject stoneObj = new GameObject("Stone");
        stoneObj.AddComponent<MeshFilter>().mesh = stoneMesh;
        var renderer = stoneObj.AddComponent<MeshRenderer>();
        //renderer.material = stoneMaterial;

        // Position the stone in front of the camera for now
        stoneObj.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
        stoneObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f); // Scale it appropriately
    }

    List<Vector2> RamerDouglasPeucker(List<Vector2> points, float epsilon)
    {
        if (points.Count < 3)
            return points;

        int startIndex = 0;
        int endIndex = points.Count - 1;
        List<int> pointIndicesToKeep = new List<int> { startIndex, endIndex };

        while (points[startIndex] == points[endIndex])
        {
            endIndex--;
        }

        RamerDouglasPeuckerRecursive(points, startIndex, endIndex, epsilon, ref pointIndicesToKeep);

        List<Vector2> result = new List<Vector2>();
        pointIndicesToKeep.Sort();
        foreach (int index in pointIndicesToKeep)
        {
            result.Add(points[index]);
        }

        return result;
    }

    void RamerDouglasPeuckerRecursive(List<Vector2> points, int startIndex, int endIndex, float epsilon, ref List<int> pointIndicesToKeep)
    {
        float maxDistance = 0;
        int indexFarthest = 0;

        for (int i = startIndex + 1; i < endIndex; i++)
        {
            float distance = PerpendicularDistance(points[startIndex], points[endIndex], points[i]);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                indexFarthest = i;
            }
        }

        if (maxDistance > epsilon && indexFarthest != 0)
        {
            pointIndicesToKeep.Add(indexFarthest);

            RamerDouglasPeuckerRecursive(points, startIndex, indexFarthest, epsilon, ref pointIndicesToKeep);
            RamerDouglasPeuckerRecursive(points, indexFarthest, endIndex, epsilon, ref pointIndicesToKeep);
        }
    }

    float PerpendicularDistance(Vector2 point1, Vector2 point2, Vector2 point)
    {
        float area = Mathf.Abs(0.5f * (point1.x * point2.y + point2.x * point.y + point.x * point1.y - point2.x * point1.y - point.x * point2.y - point1.x * point.y));
        float bottom = Mathf.Sqrt(Mathf.Pow(point1.x - point2.x, 2) + Mathf.Pow(point1.y - point2.y, 2));
        float height = area / bottom * 2;

        return height;
    }

    private Mesh Build2DMesh(List<Vector2> polygon, List<int> triangleIndices)
    {
        Mesh mesh = new Mesh();

        // Convert polygon points to Vector3 vertices
        Vector3[] vertices = new Vector3[polygon.Count];
        for (int i = 0; i < polygon.Count; i++)
        {
            vertices[i] = new Vector3(polygon[i].x, polygon[i].y, 0f);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangleIndices.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    List<Vector2> ExtractContour(Color32[] maskPixels, int width, int height)
    {
        List<Vector2> contourPoints = new List<Vector2>();
        bool[,] visited = new bool[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;
                if (!visited[y, x] && maskPixels[i].r > 128) // If white and not visited
                {

                    ContourTracing.TraceContour(maskPixels, width, height);

                    //TraceContour(maskPixels, width, height, x, y, visited, contourPoints);
                    //TraceRegion(maskPixels, width, height, x, y, visited, contourPoints);
                    break; // Only trace one region (the person)
                }
            }
        }

        var simplified =  RamerDouglasPeucker(contourPoints,1.0f); // Sort points in order (clockwise or counter-clockwise
          
        return SortContourPointsByGiftWrapping(simplified);

    }

    List<Vector2> SortContourPointsByGiftWrapping(List<Vector2> points)
    {
        if (points.Count == 0) return points;

        List<Vector2> sortedPoints = new List<Vector2>();
        Vector2 startPoint = points[0];

        // Find the leftmost point
        foreach (var point in points)
        {
            if (point.x < startPoint.x)
            {
                startPoint = point;
            }
        }

        Vector2 currentPoint = startPoint;
        sortedPoints.Add(currentPoint);
        points.Remove(currentPoint);

        while (points.Count > 0)
        {
            Vector2 nextPoint = points[0];
            foreach (var point in points)
            {
                float crossProduct = Cross(currentPoint, nextPoint, point);
                if (crossProduct < 0 || (crossProduct == 0 && Vector2.Distance(currentPoint, point) > Vector2.Distance(currentPoint, nextPoint)))
                {
                    nextPoint = point;
                }
            }

            if (nextPoint == startPoint)
            {
                break;
            }

            sortedPoints.Add(nextPoint);
            points.Remove(nextPoint);
            currentPoint = nextPoint;
        }

        return sortedPoints;
    }

    float Cross(Vector2 o, Vector2 a, Vector2 b)
    {
        return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
    }

    void TraceContour(Color32[] maskPixels, int width, int height, int startX, int startY, bool[,] visited, List<Vector2> points)
    {
        int[,] directions = new int[,]
        {
        { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } // Right, Down, Left, Up
        };

        int x = startX;
        int y = startY;
        int dir = 0; // Start direction (right)

        do
        {
            points.Add(new Vector2(x, y));
            visited[y, x] = true;

            // Try to move in the current direction
            int nx = x + directions[dir, 0];
            int ny = y + directions[dir, 1];

            if (nx >= 0 && nx < width && ny >= 0 && ny < height && maskPixels[ny * width + nx].r > 128)
            {
                // Move to the next pixel
                x = nx;
                y = ny;
            }
            else
            {
                // Change direction (turn right)
                dir = (dir + 1) % 4;
            }
        } while (x != startX || y != startY && !visited[y, x]); // Stop when we return to the starting point
    }

    void TraceRegion(Color32[] maskPixels, int width, int height, int startX, int startY,
                     bool[,] visited, List<Vector2> points)
    {
        Queue<(int x, int y)> queue = new Queue<(int, int)>();
        queue.Enqueue((startX, startY));
        visited[startY, startX] = true;

        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            points.Add(new Vector2(cx, cy));

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = cx + dx, ny = cy + dy;
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                    int idx = ny * width + nx;
                    if (!visited[ny, nx] && maskPixels[idx].r > 128)
                    {
                        visited[ny, nx] = true;
                        queue.Enqueue((nx, ny));
                    }
                }
            }
        }
    }


}

// Animates a LineRenderer by subtly offsetting each point over time around its original position.
internal class ContourLineAnimator : MonoBehaviour
{
    private LineRenderer _lr;
    private Vector3[] _basePositions;
    private float _amplitude;
    private float _frequency;
    private float _noiseScale;
    private float _seed;

    // Cache to distribute phase along the contour for nicer motion
    private float[] _phaseOffsets;

    public void Initialize(LineRenderer lr, float amplitude, float frequency, float noiseScale)
    {
        _lr = lr;
        _amplitude = Mathf.Max(0f, amplitude);
        _frequency = Mathf.Max(0f, frequency);
        _noiseScale = Mathf.Max(0.001f, noiseScale);
        _seed = Random.value * 1000f;

        _basePositions = new Vector3[_lr.positionCount];
        _lr.GetPositions(_basePositions);

        _phaseOffsets = new float[_basePositions.Length];
        for (int i = 0; i < _phaseOffsets.Length; i++)
        {
            // Spread phases around the loop to avoid uniform movement
            _phaseOffsets[i] = (float)i / Mathf.Max(1, _phaseOffsets.Length - 1);
        }
    }

    private void Update()
    {
        if (_lr == null || _basePositions == null || _basePositions.Length == 0)
            return;

        float t = Time.time * _frequency * 2f * Mathf.PI;

        // Recompute positions each frame with small offsets
        for (int i = 0; i < _basePositions.Length; i++)
        {
            Vector3 basePos = _basePositions[i];

            // Sample perlin noise to derive local direction, then modulate by a sin for breathing
            float n0 = Mathf.PerlinNoise(_seed + basePos.x * _noiseScale, _seed + basePos.y * _noiseScale);
            float n1 = Mathf.PerlinNoise(_seed + (basePos.x + 10f) * _noiseScale, _seed + (basePos.y + 10f) * _noiseScale);

            // Build a stable small direction vector
            Vector2 dir = new Vector2(n0 - 0.5f, n1 - 0.5f);
            if (dir.sqrMagnitude < 1e-4f)
                dir = Vector2.up;
            dir.Normalize();

            // Gentle time-based modulation with phase offset per point
            float phase = t + _phaseOffsets[i] * Mathf.PI * 2f;
            float breath = Mathf.Sin(phase) * 0.5f + 0.5f; // 0..1
            float offsetMag = breath * _amplitude;

            Vector3 offset = new Vector3(dir.x, dir.y, 0f) * offsetMag;
            _lr.SetPosition(i, basePos + offset);
        }
    }
}

