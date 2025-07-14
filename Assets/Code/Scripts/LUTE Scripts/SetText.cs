using TMPro;
using UnityEngine;

namespace LoGaCulture.LUTE
{
    [OrderInfo("AgesOfAvebury",
             "Set Text",
             "Sets text on an object with TextMeshPro")]
    [AddComponentMenu("")]
    public class SetText : Order
    {
        [Tooltip("Text to add to the info panel")]
        [TextArea(15, 20)]
        [SerializeField] protected string textToAdd;
       
        [Space]
        
        [Tooltip("A custom text info panel to use")]
        [SerializeField] protected TextMeshProUGUI textMeshPro;

        public override void OnEnter()
        {
            
            if(textMeshPro == null)
            {
                Debug.LogError("TextMeshProUGUI component is not assigned. Please assign it in the inspector.");
                Continue();
            }

            textMeshPro.text = textToAdd;
  
            textMeshPro.ForceMeshUpdate(); // Ensure the text is updated immediately


            Continue();
        }
    }
}
