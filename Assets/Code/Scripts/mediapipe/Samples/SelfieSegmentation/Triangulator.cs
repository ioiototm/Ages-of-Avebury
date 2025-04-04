using System.Collections.Generic;
using UnityEngine;

public static class Triangulator
{
    /// <summary>
    /// Ear-clipping triangulation: input is a list of polygon vertices in order
    /// (clockwise or counterclockwise). Output is a list of triangle indices.
    /// </summary>
    public static List<int> Triangulate(List<Vector2> points)
    {
        List<int> indices = new List<int>();

        // create a list of vertex indices
        List<int> V = new List<int>();
        for (int i = 0; i < points.Count; i++) V.Add(i);

        // Choose orientation: check if polygon is clockwise or CCW
        float area = PolygonArea(points);
        bool clockwise = area > 0;

        int count = 2 * points.Count;
        for (int v = points.Count - 1; points.Count > 2;)
        {
            if ((count--) <= 0) break; // we have an error => possibly non-simple polygon

            // next, previous, current
            int prev = (v + points.Count - 1) % points.Count;
            int curr = v;
            int next = (v + 1) % points.Count;

            if (Snip(points, prev, curr, next, points.Count, clockwise))
            {
                // clip ear
                indices.Add(V[prev]);
                indices.Add(V[curr]);
                indices.Add(V[next]);

                // remove v from the polygon
                V.RemoveAt(v);
                points.RemoveAt(v);

                // fix index
                v = (v + points.Count - 1) % points.Count;
            }
            else
            {
                v = (v + 1) % points.Count;
            }
        }
        return indices;
    }

    private static float PolygonArea(List<Vector2> points)
    {
        float area = 0;
        for (int i = 0; i < points.Count; i++)
        {
            int j = (i + 1) % points.Count;
            area += points[i].x * points[j].y - points[j].x * points[i].y;
        }
        return area * 0.5f;
    }

    private static bool Snip(List<Vector2> points, int u, int v, int w, int n, bool clockwise)
    {
        Vector2 A = points[u];
        Vector2 B = points[v];
        Vector2 C = points[w];

        if (Mathf.Epsilon > Mathf.Abs(Cross(B - A, C - A))) return false; // area = 0 => no ear

        // if any other point lies inside this triangle => can't snip
        for (int p = 0; p < n; p++)
        {
            if (p == u || p == v || p == w) continue;
            if (PointInTriangle(A, B, C, points[p])) return false;
        }

        return true;
    }

    private static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private static bool PointInTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float c0 = Cross(B - A, P - A);
        float c1 = Cross(C - B, P - B);
        float c2 = Cross(A - C, P - C);

        bool d0 = (c0 >= 0);
        bool d1 = (c1 >= 0);
        bool d2 = (c2 >= 0);
        return (d0 == d1) && (d1 == d2);
    }
}
