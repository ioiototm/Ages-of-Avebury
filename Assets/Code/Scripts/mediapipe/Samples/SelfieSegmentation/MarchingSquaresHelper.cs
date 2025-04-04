using System.Collections.Generic;
using UnityEngine;

public static class MarchingSquaresHelper
{
    /// <summary>
    /// Generates a list of line segments that form the boundary/contour
    /// of a black-and-white mask using the Marching Squares algorithm.
    /// </summary>
    /// <param name="mask">The Color32 array of the mask (white = foreground, black = background)</param>
    /// <param name="width">Texture width</param>
    /// <param name="height">Texture height</param>
    /// <returns>A list of line segments, each segment is two Vector2s in texture space</returns>
    public static List<(Vector2, Vector2)> GenerateContours(Color32[] mask, int width, int height)
    {
        List<(Vector2, Vector2)> segments = new List<(Vector2, Vector2)>();

        // Iterate over each cell (x, y) = top-left corner of that 2x2 block
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                // Determine the 4 corners' "value" (0 or 1)
                int topLeft = IsWhite(mask, x, y, width);
                int topRight = IsWhite(mask, x + 1, y, width);
                int bottomRight = IsWhite(mask, x + 1, y + 1, width);
                int bottomLeft = IsWhite(mask, x, y + 1, width);

                // Build the case index [TL, TR, BR, BL]
                // bit 0 (1) = top-left, bit 1 (2) = top-right, bit 2 (4) = bottom-right, bit 3 (8) = bottom-left
                int cellIndex = (topLeft << 0) | (topRight << 1) | (bottomRight << 2) | (bottomLeft << 3);

                // Based on the cellIndex, add edges
                if (cellIndex == 0 || cellIndex == 15)
                {
                    // all corners 0 or all corners 1 = no boundary crosses
                    continue;
                }

                // We'll use "midpoint" on each edge of the square
                // For example, (x + 0.5f, y) is the midpoint on the top edge
                // There's a well-known lookup for each case, but let's do a minimal switch approach

                var edgePoints = new List<(Vector2, Vector2)>();

                // We find up to 2 edges for each case:
                // We'll define local helper to get the midpoint of each edge.

                Vector2 topMid = new Vector2(x + 0.5f, y);
                Vector2 rightMid = new Vector2(x + 1, y + 0.5f);
                Vector2 bottomMid = new Vector2(x + 0.5f, y + 1);
                Vector2 leftMid = new Vector2(x, y + 0.5f);

                // We can define a small function that checks if a corner is different from its neighbor
                // But let's do a quick switch on cellIndex:

                switch (cellIndex)
                {
                    case 1:  // 0001 => Only top-left is 1 => edges between topMid & leftMid
                        segments.Add((topMid, leftMid));
                        break;
                    case 2:  // 0010 => Only top-right => topMid & rightMid
                        segments.Add((topMid, rightMid));
                        break;
                    case 3:  // 0011 => top-left, top-right => rightMid & leftMid
                        segments.Add((rightMid, leftMid));
                        break;
                    case 4:  // 0100 => Only bottom-right => rightMid & bottomMid
                        segments.Add((rightMid, bottomMid));
                        break;
                    case 5:  // 0101 => top-left, bottom-right => topMid & rightMid, leftMid & bottomMid
                        segments.Add((topMid, rightMid));
                        segments.Add((leftMid, bottomMid));
                        break;
                    case 6:  // 0110 => top-right, bottom-right => topMid & bottomMid
                        segments.Add((topMid, bottomMid));
                        break;
                    case 7:  // 0111 => top-left, top-right, bottom-right => leftMid & bottomMid
                        segments.Add((leftMid, bottomMid));
                        break;
                    case 8:  // 1000 => Only bottom-left => leftMid & bottomMid
                        segments.Add((leftMid, bottomMid));
                        break;
                    case 9:  // 1001 => top-left, bottom-left => topMid & bottomMid
                        segments.Add((topMid, bottomMid));
                        break;
                    case 10: // 1010 => top-right, bottom-left => topMid & leftMid, rightMid & bottomMid
                        segments.Add((topMid, leftMid));
                        segments.Add((rightMid, bottomMid));
                        break;
                    case 11: // 1011 => top-left, top-right, bottom-left => rightMid & bottomMid
                        segments.Add((rightMid, bottomMid));
                        break;
                    case 12: // 1100 => bottom-right, bottom-left => rightMid & leftMid
                        segments.Add((rightMid, leftMid));
                        break;
                    case 13: // 1101 => top-left, bottom-right, bottom-left => topMid & rightMid
                        segments.Add((topMid, rightMid));
                        break;
                    case 14: // 1110 => top-right, bottom-right, bottom-left => topMid & leftMid
                        segments.Add((topMid, leftMid));
                        break;
                        // case 0 or 15 => no boundary
                }
            }
        }

        return segments;
    }

    private static int IsWhite(Color32[] mask, int x, int y, int width)
    {
        // Ensure we don't go out of bounds
        if (x < 0 || y < 0 || x >= width) return 0;
        int index = y * width + x;
        // If mask is white => return 1, else 0
        return (mask[index].r > 128) ? 1 : 0;
    }
}
