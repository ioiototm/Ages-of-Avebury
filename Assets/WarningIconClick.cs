using UnityEngine;
using UnityEngine.UI;

public class WarningIconClick : MonoBehaviour
{
    public Animator animator;   // Reference to the Animator
    public Button WarningIcon;  // Reference to the Button

    void Start()
    {
        // Ensure the button has an onClick event listener
        WarningIcon.onClick.AddListener(OnWarningIconClick);
    }

    void OnWarningIconClick()
    {
        Debug.Log("Warning Icon Clicked and current state is " + animator.GetCurrentAnimatorStateInfo(0).ToString());

        // Check if the Animator is currently playing the Open Idle animation
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("ICGTOpenIdle"))
        {

            Debug.Log("Playing Close Animation");

            // If it's playing Open Idle, play the closing animation'
            animator.Play("ICGTClose");
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("ICGTCloseIdle"))
        {

            Debug.Log("Playing Open Animation");

            // Otherwise, play the open animation - this SHOULD BE ICGTOpen but it's not because Unity reasons, so if it bugs out in future then try amending to that.
            animator.Play("ICantGetThereAnim");
        }
    }
}