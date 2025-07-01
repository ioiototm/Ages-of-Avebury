using System.Collections;
using UnityEngine;
using UnityEngine.Android;

public class PermissionManager : MonoBehaviour
{
    //public GameObject playButton; // Disable until permissions are granted

    void Start()
    {
       // playButton.SetActive(false); // Or interactable = false if using a Button
        StartCoroutine(CheckPermissions());
    }

    private IEnumerator CheckPermissions()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            Permission.RequestUserPermission(Permission.Camera);

        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            Permission.RequestUserPermission(Permission.FineLocation);

        // Wait a few frames to give system dialog time to pop up & respond
        yield return new WaitForSeconds(1.0f);

        // Poll until both permissions are accepted (hacky but reliable)
        while (!Permission.HasUserAuthorizedPermission(Permission.Camera) ||
               !Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            yield return null;
        }

        // Now safe to enable "Play"
        //playButton.SetActive(true);
    }
}
