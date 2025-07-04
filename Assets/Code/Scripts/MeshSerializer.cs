using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

public static class MeshSerializer
{
    public static string ToBase64(Mesh mesh, List<Vector2> outline)
    {
        StoneDTO dto = new StoneDTO
        {
            mesh = MeshToDTO(mesh),
            outline = OutlineToArray(outline)
        };

        string json = JsonUtility.ToJson(dto);
        byte[] gzJson = Compress(json);
        return Convert.ToBase64String(gzJson);
    }

    public static void FromBase64(
        string b64,
        out Mesh mesh,
        out List<Vector2> outline)
    {
        string json = Decompress(Convert.FromBase64String(b64));
        StoneDTO dto = JsonUtility.FromJson<StoneDTO>(json);

        mesh = DTOToMesh(dto.mesh);
        outline = ArrayToOutline(dto.outline);
    }

    /* ---------- helpers ---------- */

    static MeshDTO MeshToDTO(Mesh m)
    {
        var dto = new MeshDTO
        {
            v = new float[m.vertexCount * 3],
            n = new float[m.vertexCount * 3],
            t = m.triangles
        };
        for (int i = 0; i < m.vertexCount; i++)
        {
            Vector3 v = m.vertices[i];
            dto.v[i * 3] = v.x; dto.v[i * 3 + 1] = v.y; dto.v[i * 3 + 2] = v.z;

            Vector3 n = m.normals[i];
            dto.n[i * 3] = n.x; dto.n[i * 3 + 1] = n.y; dto.n[i * 3 + 2] = n.z;
        }
        return dto;
    }

    static Mesh DTOToMesh(MeshDTO d)
    {
        int vCount = d.v.Length / 3;
        Vector3[] verts = new Vector3[vCount];
        Vector3[] normals = new Vector3[vCount];

        for (int i = 0; i < vCount; i++)
        {
            verts[i] = new Vector3(
                d.v[i * 3], d.v[i * 3 + 1], d.v[i * 3 + 2]);

            normals[i] = new Vector3(
                d.n[i * 3], d.n[i * 3 + 1], d.n[i * 3 + 2]);
        }

        Mesh m = new Mesh
        {
            vertices = verts,
            normals = normals,
            triangles = d.t
        };
        m.RecalculateBounds();
        return m;
    }

    static float[] OutlineToArray(List<Vector2> o)
    {
        float[] arr = new float[o.Count * 2];
        for (int i = 0; i < o.Count; i++)
        {
            arr[i * 2] = o[i].x;
            arr[i * 2 + 1] = o[i].y;
        }
        return arr;
    }

    static List<Vector2> ArrayToOutline(float[] arr)
    {
        var list = new List<Vector2>(arr.Length / 2);
        for (int i = 0; i < arr.Length; i += 2)
            list.Add(new Vector2(arr[i], arr[i + 1]));
        return list;
    }

    static byte[] Compress(string s)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(mso, System.IO.Compression.CompressionLevel.Optimal, true))
            msi.CopyTo(gs);
        return mso.ToArray();
    }

    static string Decompress(byte[] gz)
    {
        using var msi = new MemoryStream(gz);
        using var gs = new GZipStream(msi, CompressionMode.Decompress);
        using var sr = new StreamReader(gs);
        return sr.ReadToEnd();
    }


    [System.Serializable]
    public class MeshDTO            // already familiar
    {
        public float[] v;           // vertices  x0,y0,z0...
        public int[] t;           // triangles
        public float[] n;           // normals
    }

    [System.Serializable]
    public class StoneDTO          
    {
        public MeshDTO mesh;
        public float[] outline;     // x0,y0, x1,y1 ...
    }

}