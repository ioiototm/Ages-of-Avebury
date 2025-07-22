// ImageFadeOnClick.cs
// -------------------
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Fades multiple UI Images on click, then deactivates specified GameObjects smoothly when the fade is complete.
/// Attach this script to the same GameObject that has the Button component (e.g., a clickable UI element).
/// </summary>
[RequireComponent(typeof(Button))]
public class ImageFadeOnClick : MonoBehaviour
{
    [Tooltip("UI Image components to fade out.")]
    public Image[] imagesToFade;

    [Tooltip("GameObjects to deactivate after the fade completes.")]
    public GameObject[] objectsToDeactivate;

    [Tooltip("Duration of the fade in seconds.")]
    public float fadeDuration = 1f;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (imagesToFade == null || imagesToFade.Length == 0)
            Debug.LogError("ImageFadeOnClick: No Images assigned to fade.");
        if (objectsToDeactivate == null || objectsToDeactivate.Length == 0)
            Debug.LogError("ImageFadeOnClick: No GameObjects assigned to deactivate.");

        // Ensure all images start fully opaque
        foreach (var img in imagesToFade)
        {
            if (img != null)
            {
                Color c = img.color;
                img.color = new Color(c.r, c.g, c.b, 1f);
            }
        }
    }

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // Disable further clicks
        button.interactable = false;
        StartCoroutine(FadeAndDeactivate());
    }

    private IEnumerator FadeAndDeactivate()
    {
        float elapsed = 0f;
        Color[] originals = new Color[imagesToFade.Length];
        for (int i = 0; i < imagesToFade.Length; i++)
            originals[i] = imagesToFade[i]?.color ?? Color.clear;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float alpha = Mathf.Lerp(1f, 0f, t);
            for (int i = 0; i < imagesToFade.Length; i++)
            {
                var img = imagesToFade[i];
                if (img != null)
                {
                    Color o = originals[i];
                    img.color = new Color(o.r, o.g, o.b, alpha);
                }
            }
            yield return null;
        }

        // Finalize transparency
        for (int i = 0; i < imagesToFade.Length; i++)
        {
            var img = imagesToFade[i];
            if (img != null)
            {
                Color o = originals[i];
                img.color = new Color(o.r, o.g, o.b, 0f);
            }
        }

        // Deactivate the specified objects
        foreach (var go in objectsToDeactivate)
        {
            if (go != null)
                go.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClick);
    }
}
