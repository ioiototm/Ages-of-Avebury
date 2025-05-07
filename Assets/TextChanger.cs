using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class LocationClickHandler : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI messageText;  // Assign in Inspector
    public Animator animator;            // Assign in Inspector

    private int clickCount = 0;

    public void OnPointerClick(PointerEventData eventData)
    {
        clickCount++;

        if (clickCount == 1)
        {
            messageText.text = "Location updated!";
        }
        else if (clickCount == 2)
        {
            messageText.text = "Location skipped!";

            if (animator != null)
            {
                StartCoroutine(PlayAnimationAndUpdateText());
            }
            else
            {
                Debug.LogWarning("Animator not assigned.");
            }

            clickCount = 0; // Reset for next cycle
        }
    }

    private IEnumerator PlayAnimationAndUpdateText()
    {
        animator.Play("ICGTClose");

        // Wait for animation to finish
        float duration = GetAnimationLength("ICGTClose");
        yield return new WaitForSeconds(duration);

        messageText.text = "I can't get there!";

        // Find and hide the GameObject named "ICantGetThere"
        GameObject target = GameObject.Find("ICantGetThere");
        if (target != null)
        {
            target.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameObject 'ICantGetThere' not found.");
        }
    }

    private float GetAnimationLength(string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 1f;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == stateName)
                return clip.length;
        }

        Debug.LogWarning($"Animation '{stateName}' not found. Defaulting to 1 second.");
        return 1f;
    }
}
