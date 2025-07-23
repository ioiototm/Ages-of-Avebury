using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class LocationClickHandler : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI messageText;    // Assign in Inspector
    public Animator animator;              // Assign in Inspector
    public GameObject cantGetThereObject;  // Assign in Inspector (formerly found by name)

    private int clickCount = 0;

    public void OnPointerClick(PointerEventData eventData)
    {
        clickCount++;

        if (clickCount == 1)
        {
            StartCoroutine(updateText("I can't get there either!"));
        }
        else if (clickCount == 2)
        {
            messageText.text = "I can't get there!";

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

    private IEnumerator updateText(string text)
    {
        //wait 1 second
        yield return new WaitForSeconds(1f);
        messageText.text = text;
        
    }

    private IEnumerator PlayAnimationAndUpdateText()
    {
        yield return new WaitForSeconds(3f); // Wait before starting animation

        animator.Play("ICGTClose");

        // Wait for animation to finish
        float duration = GetAnimationLength("ICGTClose");
        yield return new WaitForSeconds(duration + 3);


        messageText.text = "I can't get there!";

        if (cantGetThereObject != null)
        {
            cantGetThereObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameObject 'ICantGetThere' not assigned.");
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
