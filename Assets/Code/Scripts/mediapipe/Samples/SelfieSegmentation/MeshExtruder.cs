using System.Collections.Generic;
using UnityEngine;

public static class MeshExtruder
{
    public static Mesh ExtrudeMesh(Mesh flatMesh, float thickness)
    {
        // 1) Get front vertices (original)
        Vector3[] frontVerts = flatMesh.vertices;
        // 2) Duplicate + offset in Z for back face
        Vector3[] backVerts = new Vector3[frontVerts.Length];
        for (int i = 0; i < frontVerts.Length; i++)
        {
            backVerts[i] = frontVerts[i] + new Vector3(0f, 0f, thickness);
        }

        // Combine them: front + back
        Vector3[] allVerts = new Vector3[frontVerts.Length + backVerts.Length];
        frontVerts.CopyTo(allVerts, 0);
        backVerts.CopyTo(allVerts, frontVerts.Length);

        // Build triangles
        //  - front face uses the original flatMesh.triangles
        //  - back face: same triangles but offset by frontVerts.Length, reversed winding
        int[] frontTris = flatMesh.triangles;
        int[] backTris = new int[frontTris.Length];
        for (int i = 0; i < frontTris.Length; i += 3)
        {
            backTris[i] = frontTris[i] + frontVerts.Length;
            backTris[i + 1] = frontTris[i + 2] + frontVerts.Length;
            backTris[i + 2] = frontTris[i + 1] + frontVerts.Length;
        }

        // Then build the side walls by connecting each vertex i in front to i in back
        // We'll generate quads (two triangles each)
        // each pair of adjacent vertices in the polygon plus their back-face equivalents
        List<int> sideTris = new List<int>();
        int vertCount = frontVerts.Length;
        for (int i = 0; i < vertCount; i++)
        {
            int next = (i + 1) % vertCount; // loop around
                                            // front: i, next
                                            // back: i+vertCount, next+vertCount

            // two triangles forming a quad
            sideTris.Add(i);
            sideTris.Add(next);
            sideTris.Add(i + vertCount);

            sideTris.Add(next);
            sideTris.Add(next + vertCount);
            sideTris.Add(i + vertCount);
        }

        // Combine everything
        Mesh extrudedMesh = new Mesh();
        extrudedMesh.vertices = allVerts;

        int[] finalTris = new int[frontTris.Length + backTris.Length + sideTris.Count];
        frontTris.CopyTo(finalTris, 0);
        backTris.CopyTo(finalTris, frontTris.Length);
        sideTris.ToArray().CopyTo(finalTris, frontTris.Length + backTris.Length);

        extrudedMesh.triangles = finalTris;
        extrudedMesh.RecalculateNormals();
        extrudedMesh.RecalculateBounds();

        return extrudedMesh;
    }

}
