using LoGaCulture.LUTE;
using Mapbox.Unity.Location;
using Mapbox.Utils;
using System.Collections;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Stone Creation",
              "Sets up the Stone Creation")]
[AddComponentMenu("")]
public class StoneCreation : Order
{

    [SerializeField]
    SelfieSegmentationSample selfieSegmentationSample;

    [SerializeField]
    GameObject makeStoneButton;

    [SerializeField]
    GameObject stoneObject;

    [SerializeField, Range(1, 2)]
    int stoneSlot = 1;


    //a function that will be called when the user has pressed the button and created the stone
    public void CreateStone()
    {
     


        StartCoroutine(waitForABit());
    }

    IEnumerator waitForABit()
    {




        yield return new WaitForSeconds(6f);

        // Add your stone creation logic here
        Debug.Log("Stone Created!");
        makeStoneButton.SetActive(false);

        selfieSegmentationSample.gameObject.SetActive(false);

    

        GameObject contourLine = GameObject.Find("ContourLine");
        contourLine.SetActive(false);


        //GameObject stoneObject = GameObject.Find("StoneObject");
        //stoneObject.SetActive(false);

        //get the current location
        //LocationRandomiser.Instance.SetLastSeenAndTargetToFirstNPC();
        Vector2d playerLocation = LocationProviderFactory.Instance.DefaultLocationProvider.CurrentLocation.LatitudeLongitude;


        //

        Continue();
    }


    public override void OnEnter()
    {

        if (selfieSegmentationSample == null)
        {
            selfieSegmentationSample = GameObject.Find("SelfieSegmentationSample").GetComponent<SelfieSegmentationSample>();
            selfieSegmentationSample.stoneOrder = this;
            selfieSegmentationSample.ConfigureStoneSlot(stoneSlot);
            return;
        }


        selfieSegmentationSample.gameObject.SetActive(true);
        selfieSegmentationSample.currentStone = stoneObject;
        selfieSegmentationSample.stoneOrder = this;
        selfieSegmentationSample.ConfigureStoneSlot(stoneSlot);

        makeStoneButton.SetActive(true);
        //To continue or not depending on if this order takes care of everything or diffrent orders take care of the rest
        //Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order sets up the Stone Creation";
    }
}