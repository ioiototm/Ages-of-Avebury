using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ARPhotoCapture : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The AR camera that renders the background + virtual content")]
    public Camera arCamera;

    [Tooltip("Optional: the root Canvas you want hidden during capture")]
    public Canvas uiCanvas;

    [Header("Capture Settings")]
    [Tooltip("Multiply screen resolution by this factor for the output image")]
    [Range(0.25f, 4f)] public float resolutionScale = 1.0f;

    [Tooltip("Optional: override width/height. If zero, uses Screen size * scale")]
    public int overrideWidth = 0;
    public int overrideHeight = 0;

    [Tooltip("Image format (PNG = lossless)")]
    public bool saveAsPng = true; // else JPG

    [Tooltip("JPG Quality (1-100)")]
    [Range(1, 100)] public int jpgQuality = 90;

    [Header("UX")]
    [Tooltip("Optional shutter sound")]
    public AudioSource shutterSfx;
    [Tooltip("Optional white flash overlay GameObject (enable briefly)")]
    public GameObject flashOverlay;

    bool _busy;

    public void TakePhoto()
    {
        if (_busy ) return;
        if(arCamera == null)
        {
            arCamera = GameObject.Find("XR-New/XR Origin (Mobile AR)/Camera Offset/Main Camera").GetComponent<Camera>();
        }
        

        StartCoroutine(CaptureRoutine());
    }

    IEnumerator CaptureRoutine()
    {
        _busy = true;

        // Hide UI (so buttons/overlays don't end up in the image)
        bool hadUI = uiCanvas && uiCanvas.enabled;
        if (hadUI) uiCanvas.enabled = false;

        // Optional: tiny delay lets layout/UI settle if you just clicked a button
        yield return null;

        // Calculate target dimensions
        int w = overrideWidth > 0 ? overrideWidth : Mathf.RoundToInt(Screen.width * resolutionScale);
        int h = overrideHeight > 0 ? overrideHeight : Mathf.RoundToInt(Screen.height * resolutionScale);

        // Create a RenderTexture and a Texture2D to store the pixels
        var rt = new RenderTexture(w, h, /*depth*/24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
        {
            antiAliasing = 1,
            useMipMap = false,
            autoGenerateMips = false
        };
        var prevTarget = arCamera.targetTexture;
        var prevActiveRT = RenderTexture.active;
        var prevClearFlags = arCamera.clearFlags;
        var prevRect = arCamera.rect;

        try
        {
            // Important: make sure camera renders full frame to RT
            arCamera.rect = new Rect(0, 0, 1, 1);
            arCamera.targetTexture = rt;

            // For AR, we usually keep ClearFlags = SolidColor or DepthOnly.
            // SolidColor is fine; background is provided by ARCameraBackground anyway.
            // Keep existing clear flags unless you need alpha. (See notes below.)
            // arCamera.clearFlags = CameraClearFlags.SolidColor;

            // Render one frame into the RT
            arCamera.Render();

            // Read pixels out
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply(false, false);

            // Save to disk
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"ARStone_{timestamp}.{(saveAsPng ? "png" : "jpg")}";
            string path = Path.Combine(Application.persistentDataPath, filename);
            byte[] bytes = saveAsPng ? tex.EncodeToPNG() : tex.EncodeToJPG(jpgQuality);
            File.WriteAllBytes(path, bytes);

            // UX: shutter + flash
            if (shutterSfx) shutterSfx.Play();
            if (flashOverlay) StartCoroutine(FlashRoutine());

            Debug.Log($"[ARPhotoCapture] Saved: {path}");

            // Clean up CPU copy
            Destroy(tex);
        }
        finally
        {
            // Restore state
            arCamera.targetTexture = prevTarget;
            arCamera.rect = prevRect;
            arCamera.clearFlags = prevClearFlags;
            RenderTexture.active = prevActiveRT;
            rt.Release();
            Destroy(rt);

            if (hadUI) uiCanvas.enabled = true;
            _busy = false;
        }
    }

    IEnumerator FlashRoutine()
    {
        flashOverlay.SetActive(true);
        yield return new WaitForSeconds(0.08f);
        flashOverlay.SetActive(false);
    }




}
