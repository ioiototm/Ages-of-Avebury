using UnityEngine;
using System.Collections.Generic;

public static class ContourTracing
{
    public static List<Vector2> TraceContour(Color32[] maskPixels, int width, int height)
    {
        List<Vector2> contourPoints = new List<Vector2>();

        // Find a starting point (e.g., top-left white pixel)
        int startX = 0;
        int startY = 0;
        bool foundStart = false;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (maskPixels[index].r > 128) // Found a white pixel
                {
                    startX = x;
                    startY = y;
                    foundStart = true;
                    break;
                }
            }
            if (foundStart) break;
        }

        if (!foundStart)
        {
            Debug.LogError("No white pixels found in the mask.");
            return contourPoints; // Return empty list if no starting point found
        }

        // Perform contour tracing
        Trace(maskPixels, width, height, startX, startY, contourPoints);

        return contourPoints;
    }

    private static void Trace(Color32[] maskPixels, int width, int height, int startX, int startY, List<Vector2> points)
    {
        int currentX = startX;
        int currentY = startY;

        // Clockwise neighbor offsets
        int[,] neighborOffsets = new int[8, 2]
        {
            { 0, -1 },   // Up
            { 1, -1 },   // Up-Right
            { 1, 0 },    // Right
            { 1, 1 },    // Down-Right
            { 0, 1 },    // Down
            { -1, 1 },   // Down-Left
            { -1, 0 },   // Left
            { -1, -1 }   // Up-Left
        };

        int prevDirection = 0; // Start with an arbitrary direction
        points.Add(new Vector2(startX, startY)); // Add starting point

        while (true)
        {
            bool foundNext = false;
            int nextX = currentX;
            int nextY = currentY;

            // Check neighbors in a clockwise direction starting from the previous direction
            for (int i = 0; i < 8; i++)
            {
                int direction = (prevDirection + i) % 8; // Wrap around
                int dx = neighborOffsets[direction, 0];
                int dy = neighborOffsets[direction, 1];

                nextX = currentX + dx;
                nextY = currentY + dy;

                if (nextX >= 0 && nextX < width && nextY >= 0 && nextY < height &&
                    maskPixels[nextY * width + nextX].r > 128) // Valid white pixel
                {
                    foundNext = true;
                    prevDirection = (direction + 6) % 8; // Set next search start (backtrack prevention)
                    break;
                }
            }

            // If no valid neighbor is found, the contour is complete
            if (!foundNext)
            {
                Debug.Log("No next point found. Contour tracing finished.");
                break;
            }

            // Add the next point to the contour
            if (nextX == startX && nextY == startY && points.Count > 1)
            {
                Debug.Log("Returned to starting point. Contour tracing complete.");
                break;
            }

            points.Add(new Vector2(nextX, nextY));
            //Debug.DrawLine(new Vector3(currentX, currentY, 0), new Vector3(nextX, nextY, 0), Color.red, 5f);

            // Update current position
            currentX = nextX;
            currentY = nextY;
        }
    }
}
