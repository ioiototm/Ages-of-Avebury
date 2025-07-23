using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace LoGaCulture.LUTE
{
    [OrderInfo("AgesOfAvebury",
             "Set the dots alpha for AR",
             "")]
    [AddComponentMenu("")]
    public class SetDotsAlpha : Order
    {
        [SerializeField] private float mAlpha = 0.8f;
        [SerializeField] private Material material;


        private void OnDestroy()
        {
            //set material back to 0.8f
            if (material != null)
            {
                var color = material.GetColor("_TexTintColor");
                color.a = 0.8f;
                material.SetColor("_TexTintColor", color);
            }
        }
        public override void OnEnter()
        {
            ARPlaneManager planeManager = FindFirstObjectByType<ARPlaneManager>();

            

            if (planeManager != null)
            {

                //go through each trackable in the plane manager and set the opacity of the _TextTintColor material in the MeshRenderer to 0.2f

                foreach (var trackable in planeManager.trackables)
                {
                    var meshRenderer = trackable.GetComponent<MeshRenderer>();
                    var color = meshRenderer.sharedMaterials[0].GetColor("_TexTintColor");
                    color.a = mAlpha;

                    meshRenderer.sharedMaterials[0].SetColor("_TexTintColor", color);


                }
            }

            // Set the alpha of the material
            if (material != null)
            {
                var color = material.GetColor("_TexTintColor");
                color.a = mAlpha;
                material.SetColor("_TexTintColor", color);
            }


            Continue();
        }
    }
}
