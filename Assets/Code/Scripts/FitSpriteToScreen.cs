using UnityEngine;

public class FitSpriteToScreen : MonoBehaviour
{
    [Tooltip("Manual adjustment factor to get the correct size")]
    public Vector2 sizeMultiplier = new Vector2(1.96f, 2.15f); // Calculated from 11/5.625 and 21.5/10

    void Start()
    {
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        if (spriteRenderers.Length == 0)
        {
            Debug.LogError("No SpriteRenderers found in the children of this GameObject.");
            return;
        }

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr.sprite == null)
            {
                Debug.LogError($"No sprite found on SpriteRenderer in GameObject: {sr.gameObject.name}");
                continue;
            }

            // Calculate the size of the sprite renderer so it tiles nicely
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No main camera found in the scene.");
                continue;
            }

            // Get screen dimensions in world units
            float screenHeight = 2f * mainCamera.orthographicSize;
            float screenWidth = screenHeight * mainCamera.aspect;

            // Apply the size multiplier to get the correct tiling size
            float adjustedWidth = screenWidth * sizeMultiplier.x;
            float adjustedHeight = screenHeight * sizeMultiplier.y;

            // Set the sprite's size with the corrected values
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(adjustedWidth, adjustedHeight);

            Debug.Log($"Set sprite size to: {sr.size} (screen dimensions: {screenWidth} x {screenHeight})");
        }
    }
}
