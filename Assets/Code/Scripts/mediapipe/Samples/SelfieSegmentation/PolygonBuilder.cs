using System.Collections.Generic;
using UnityEngine;

public static class PolygonBuilder
{
    public static List<Vector2> BuildPolygonFromSegments(List<(Vector2 start, Vector2 end)> segments)
    {
        // 1) Make a dictionary from start -> end
        //    Each start can map to exactly one end if you have a single continuous loop.
        //    If multiple loops exist, you'll see branching, so you'd handle that carefully.

        Dictionary<Vector2, Vector2> chain = new Dictionary<Vector2, Vector2>();
        foreach (var seg in segments)
        {
            if (!chain.ContainsKey(seg.start))
            {
                chain[seg.start] = seg.end;
            }
            else
            {
                // if there's a conflict, you might need more robust handling
            }
        }

        // 2) Start with the first segment
        var first = segments[0];
        List<Vector2> polygon = new List<Vector2>();
        polygon.Add(first.start);

        // 3) Follow the chain until we loop back
        Vector2 current = first.end;
        while (current != first.start)
        {
            polygon.Add(current);
            current = chain[current];
        }

        // polygon now has an ordered ring of vertices
        return polygon;
    }

}
