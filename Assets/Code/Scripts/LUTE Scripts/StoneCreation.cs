using LoGaCulture.LUTE;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Stone Creation",
              "Sets up the Stone Creation")]
[AddComponentMenu("")]
public class StoneCreation : Order
{

    [SerializeField]
    SelfieSegmentationSample selfieSegmentationSample;



    public override void OnEnter()
    {

        if (selfieSegmentationSample == null)
        {
            selfieSegmentationSample = GameObject.Find("SelfieSegmentationSample").GetComponent<SelfieSegmentationSample>();
            return;
        }

        selfieSegmentationSample.gameObject.SetActive(true);
        //To continue or not depending on if this order takes care of everything or diffrent orders take care of the rest
        //Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order sets up the Stone Creation";
    }
}