#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.IO;

public class TakeScreenshot : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Capture Screenshot from Game Screen (during play)")]   // Cmd/Ctrl+Shift+L
    private static void DoCapture()
    {
        // Spawn a helper that lives for one frame
        var helper = new GameObject("__ScreenGrabber").AddComponent<TakeScreenshot>();
        helper.hideFlags = HideFlags.HideAndDontSave;
    }
#endif

    private IEnumerator Start()
    {
#if UNITY_EDITOR
        // Choose save path
        string path = EditorUtility.SaveFilePanel("Save Screenshot", "", "screenshot.jpg", "jpg");
        if (string.IsNullOrEmpty(path)) { DestroyImmediate(gameObject); yield break; }
#endif
        // Wait until *everything* has rendered this frame
        yield return new WaitForEndOfFrame();

        // Grab the Game view exactly as-is
        Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
        File.WriteAllBytes(path, tex.EncodeToJPG());

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
        Debug.Log($"Screenshot saved to {path}  ({tex.width}×{tex.height})");

        Destroy(tex);
        DestroyImmediate(gameObject);            // Clean-up helper
    }
}
