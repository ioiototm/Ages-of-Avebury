// OutlinePlaneMetaballTest.cs
// -----------------------------------------------------------
// Minimal experiment: take a 2D outline, fill its interior with a grid of
// metaballs on a single Z?plane (plus a wafer?thin top & bottom layer), then
// run Marching Cubes.
// Purpose: visual sanity?check that placing metaballs *inside* the silhouette
// actually produces a contiguous flat-ish mesh.
// -----------------------------------------------------------
// *Attach* this to an empty GameObject with MeshFilter + MeshRenderer
// (MeshCollider optional).
// Requires: Scrawk's MarchingCubes.cs (namespace MarchingCubesProject).
// -----------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using MarchingCubesProject;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class StoneCreator : MonoBehaviour
{
    [Header("2D Outline")]
    public List<Vector2> outlinePoints = new();
    [Range(1, 10)] public int outlineDensity = 1;

    [Header("Grid Fill inside outline")]
    [Tooltip("Number of samples per axis inside outline bounds")] public int fillResolution = 32;
    [Range(0.05f, 2f)] public float metaballRadius = 0.2f;
    [Range(0.05f, 2f)] public float outlineMetaballRadius = 0.3f;
    [Range(0.1f, 5f)] public float fillDensity = 1.0f;
    public bool includeOutlinePoints = true; // New option to include outline points
    public bool includeInteriorPoints = true; // New option to include interior points
    public bool includeFilledFaces = true; // Option to create filled front/back faces

    [Header("Marching Cubes")]
    [Range(0.05f, 1f)] public float isoLevel = 0.5f;

    [Header("Thickness (for wafer extrude)")]
    [Tooltip("Actual Z thickness of the generated slab (world units)")]
    public float slabThickness = 0.05f;

    [Header("Debug")] public bool autoUpdate = true;
    public bool debugMode = false;

    private MeshFilter mf;

    private void Awake() => mf = GetComponent<MeshFilter>();

    private void OnEnable() => GenerateSlab();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (autoUpdate && isActiveAndEnabled)
            UnityEditor.EditorApplication.delayCall += () => { if (this) GenerateSlab(); };
    }
#endif

    [ContextMenu("Generate Slab")]
    public void GenerateSlab()
    {
        if (outlinePoints == null || outlinePoints.Count < 3) { mf.sharedMesh = null; return; }

        // 1. Determine outline bounding box
        float xmin = float.PositiveInfinity, xmax = float.NegativeInfinity;
        float ymin = float.PositiveInfinity, ymax = float.NegativeInfinity;
        foreach (var p in outlinePoints)
        {
            if (p.x < xmin) xmin = p.x;
            if (p.x > xmax) xmax = p.x;
            if (p.y < ymin) ymin = p.y;
            if (p.y > ymax) ymax = p.y;
        }

        // Add padding so MC sees air outside silhouette
        float pad = Mathf.Max(metaballRadius, outlineMetaballRadius) * 2.0f;
        xmin -= pad; xmax += pad; ymin -= pad; ymax += pad;

        // Increase Z resolution to better capture the thin slice
        float zPad = Mathf.Max(metaballRadius, outlineMetaballRadius) * 2.0f;

        float zmin = -slabThickness * 0.5f - zPad;
        float zmax = slabThickness * 0.5f + zPad;

        // Resolution: keep roughly the same voxel size as X/Y
        int zRes = Mathf.Max(6, Mathf.CeilToInt(fillResolution * (zmax - zmin) / (xmax - xmin)));

        int xRes = fillResolution;
        int yRes = fillResolution;

        float dx = (xmax - xmin) / (xRes - 1);
        float dy = (ymax - ymin) / (yRes - 1);
        float dz = (zmax - zmin) / (zRes - 1);

        // 2. Build metaball list
        var metaballs = new List<(Vector3 position, float radius, float strength)>();
        float midZ = (zmin + zmax) * 0.5f;
        float frontZ = -slabThickness * 0.5f;
        float backZ = slabThickness * 0.5f;
        
        // Add metaballs on outline points
        if (includeOutlinePoints)
        {
            for (int i = 0; i < outlinePoints.Count; i++)
            {
                Vector2 current = outlinePoints[i];
                Vector2 next = outlinePoints[(i + 1) % outlinePoints.Count];
                
                // Add points along the outline segments
                for (int j = 0; j < outlineDensity; j++)
                {
                    float t = j / (float)outlineDensity;
                    Vector2 point = Vector2.Lerp(current, next, t);
                    
                    // Add metaballs on both front and back face edges
                    metaballs.Add((new Vector3(point.x, point.y, frontZ), outlineMetaballRadius, 1.2f));
                    metaballs.Add((new Vector3(point.x, point.y, backZ), outlineMetaballRadius, 1.2f));
                }
            }
        }
        
        // Create a triangulation of the interior for better face coverage
        if (includeFilledFaces)
        {
            // Create metaballs covering the entire face with some randomization for rocky appearance
            float faceMetaballRadius = outlineMetaballRadius * 0.8f; // Slightly smaller for more texture
            int faceDetailLevel = Mathf.Max(20, fillResolution); // Higher resolution grid
            
            float stepX = (xmax - xmin - pad*2) / faceDetailLevel;
            float stepY = (ymax - ymin - pad*2) / faceDetailLevel;
            
            // Generate a semi-random offset grid for more organic appearance
            System.Random rnd = new System.Random(42); // Fixed seed for consistency
            
            // Create a dense grid of metaballs on both front and back faces
            for (int xi = 0; xi <= faceDetailLevel; xi++)
            {
                for (int yi = 0; yi <= faceDetailLevel; yi++)
                {
                    // Add subtle randomness for rocky appearance
                    float jitterX = (float)rnd.NextDouble() * 0.5f * stepX;
                    float jitterY = (float)rnd.NextDouble() * 0.5f * stepY;
                    
                    Vector2 point = new Vector2(
                        xmin + pad + xi * stepX + jitterX - stepX * 0.25f,
                        ymin + pad + yi * stepY + jitterY - stepY * 0.25f
                    );
                    
                    if (PointInPolygon(point, outlinePoints))
                    {
                        // Randomize strength and radius slightly for rocky effect
                        float radiusVar = (float)rnd.NextDouble() * 0.4f + 0.8f; // 0.8-1.2 multiplier
                        float strengthVar = (float)rnd.NextDouble() * 0.5f + 0.8f; // 0.8-1.3 multiplier
                        
                        // Add stronger metaballs directly on the front/back faces
                        metaballs.Add((new Vector3(point.x, point.y, frontZ), 
                                       faceMetaballRadius * radiusVar, 1.3f * strengthVar));
                        metaballs.Add((new Vector3(point.x, point.y, backZ), 
                                       faceMetaballRadius * radiusVar, 1.3f * strengthVar));
                    }
                }
            }
            
            // Add stronger metaballs at key points for stability
            // At the centroid
            Vector2 centroid = Vector2.zero;
            foreach (var point in outlinePoints)
                centroid += point;
            centroid /= outlinePoints.Count;
            
            metaballs.Add((new Vector3(centroid.x, centroid.y, frontZ), faceMetaballRadius * 2, 2.0f));
            metaballs.Add((new Vector3(centroid.x, centroid.y, backZ), faceMetaballRadius * 2, 2.0f));
            
            // Add a few random larger blobs for rockier appearance
            for (int i = 0; i < 5; i++)
            {
                // Random point within the shape
                float rx = xmin + (float)rnd.NextDouble() * (xmax - xmin);
                float ry = ymin + (float)rnd.NextDouble() * (ymax - ymin);
                Vector2 rPoint = new Vector2(rx, ry);
                
                if (PointInPolygon(rPoint, outlinePoints))
                {
                    float blobSize = faceMetaballRadius * ((float)rnd.NextDouble() * 1.5f + 1.0f);
                    metaballs.Add((new Vector3(rPoint.x, rPoint.y, frontZ), blobSize, 1.7f));
                    metaballs.Add((new Vector3(rPoint.x, rPoint.y, backZ), blobSize, 1.7f));
                }
            }
        }
        
        // Add metaballs inside the volume to connect front and back
        if (includeInteriorPoints)
        {
            int densityFactor = Mathf.CeilToInt(1f / fillDensity);
            
            for (int xi = 0; xi < xRes; xi += densityFactor)
            {
                float x = xmin + xi * dx;
                for (int yi = 0; yi < yRes; yi += densityFactor)
                {
                    float y = ymin + yi * dy;
                    if (!PointInPolygon(new Vector2(x, y), outlinePoints)) continue;
                    
                    // Add metaball in the middle to connect front and back
                    metaballs.Add((new Vector3(x, y, midZ), metaballRadius, 1.0f));
                }
            }
        }

        // 3. Sample scalar field
        int resX = xRes;
        int resY = yRes;
        int resZ = zRes;
        float[,,] field = new float[resX, resY, resZ];
        
        // Sample the field using the metaballs
        for (int xi = 0; xi < resX; xi++)
        {
            for (int yi = 0; yi < resY; yi++)
            {
                for (int zi = 0; zi < resZ; zi++)
                {
                    Vector3 p = new Vector3(
                        xmin + xi * dx,
                        ymin + yi * dy,
                        zmin + zi * dz);
                    
                    float f = 0f;
                    foreach (var (position, radius, strength) in metaballs)
                    {
                        float d = Vector3.Distance(p, position);
                        float contribution = strength * Mathf.Exp(-(d * d) / (radius * radius));
                        f += contribution;
                    }
                    
                    field[xi, yi, zi] = f;
                    
                    // Extra boost for front/back face grid points to ensure continuity
                    Vector2 point2D = new Vector2(p.x, p.y);
                    bool isInside = PointInPolygon(point2D, outlinePoints);
                    
                    if (isInside && includeFilledFaces)
                    {
                        // Boost field values near the front and back faces
                        float frontDist = Mathf.Abs(p.z - frontZ);
                        float backDist = Mathf.Abs(p.z - backZ);
                        
                        if (frontDist < dz * 0.8f || backDist < dz * 0.8f)
                        {
                            // Apply stronger boost near edges to ensure closing
                            bool isNearEdge = false;
                            float edgeThreshold = Mathf.Max(dx, dy) * 3.0f;
                            
                            foreach (var edgePoint in outlinePoints)
                            {
                                if (Vector2.Distance(point2D, edgePoint) < edgeThreshold)
                                {
                                    isNearEdge = true;
                                    break;
                                }
                            }
                            
                            float boost = isNearEdge ? 1.5f : 1.2f;
                            field[xi, yi, zi] = Mathf.Max(field[xi, yi, zi], isoLevel * boost);
                        }
                    }
                }
            }
        }

        // 4. Marching Cubes
        var mc = new MarchingCubes(isoLevel);
        var verts = new List<Vector3>();
        var tris = new List<int>();
        mc.Generate(field, verts, tris);
        if (verts.Count == 0) { mf.sharedMesh = null; return; }

        // 5. Voxel?space ? local coords
        for (int i = 0; i < verts.Count; i++)
            verts[i] = new Vector3(
                xmin + verts[i].x * dx,
                ymin + verts[i].y * dy,
                zmin + verts[i].z * dz);

        // 6. Flip triangles to fix normals facing outwards
        for (int i = 0; i < tris.Count; i += 3)
        {
            int temp = tris[i];
            tris[i] = tris[i + 1];
            tris[i + 1] = temp;
        }

        // 7. Build mesh
        var mesh = new Mesh
        {
            indexFormat = verts.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16
        };
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        //Vector2[] uvs = new Vector2[verts.Count];

        //// pick the widest axis-pair so the texture isn’t squashed
        //Vector3 size = mesh.bounds.size;
        //bool xyIsWider = size.x >= size.z;   // simple heuristic

        //float minU = xyIsWider ? xmin : zmin;
        //float maxU = xyIsWider ? xmax : zmax;
        //float minV = ymin;
        //float maxV = ymax;

        //for (int i = 0; i < verts.Count; i++)
        //{
        //    Vector3 p = verts[i];
        //    float u = xyIsWider
        //              ? Mathf.InverseLerp(xmin, xmax, p.x)  // U = X
        //              : Mathf.InverseLerp(zmin, zmax, p.z); // U = Z
        //    float v = Mathf.InverseLerp(ymin, ymax, p.y);   // V = Y

        //    // optional tiling factor
        //    const float TILE = 3.0f;  // stone repeats 3× across each slab
        //    uvs[i] = new Vector2(u * TILE, v * TILE);
        //}
        //mesh.uv = uvs;


        mf.sharedMesh = mesh;
        
        if (debugMode)
        {
            Debug.Log($"Generated mesh with {verts.Count} vertices, {tris.Count/3} triangles");
        }
    }

    // -------------------------------------------------- Helpers
    private static bool PointInPolygon(Vector2 p, List<Vector2> poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            Vector2 pi = poly[i];
            Vector2 pj = poly[j];
            if (((pi.y > p.y) != (pj.y > p.y)) &&
                p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x)
                inside = !inside;
        }
        return inside;
    }
}
