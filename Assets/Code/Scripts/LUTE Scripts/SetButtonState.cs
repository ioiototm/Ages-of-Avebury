using LoGaCulture.LUTE;
using System.Collections;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Button state",
              "Let's you enable or disable a button")]
[AddComponentMenu("")]
public class SetButtonState : Order
{

    [Tooltip("The button to set the state of.")]
    [SerializeField] private UnityEngine.UI.Button button;

    [Tooltip("If true, the button will be enabled, if false it will be disabled.")]
    [SerializeField] private bool enableButton = true;


    public override void OnEnter()
    {

        //This is called when the order starts executing
        if (button == null)
        {
            Debug.LogError("Button is not assigned in SetButtonState order.");
            Continue();
            return;
        }

        //Set the state of the button
        button.interactable = enableButton;

        //To continue or not depending on if this order takes care of everything or diffrent orders take care of the rest
        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order sets the state of the button to " + (enableButton ? "enabled" : "disabled") + ".";
    }
}