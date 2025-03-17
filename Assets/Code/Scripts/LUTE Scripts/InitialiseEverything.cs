using UnityEngine;

[OrderInfo("AgesOfAvebury",
              "InitialiseEverything",
              "Initialises all variables needed for the game")]
[AddComponentMenu("")]
public class InitialiseEverything : Order
{


    [SerializeField]
    private static GameObject messagePrefab;
 
    public static GameObject inboxCanvas;

  
    public static GameObject menuCanvas;

    public static GameObject mapCanvas;


    public static GameObject modernScreen;

    public override void OnEnter()
    {
        //get the ModernInboxCanvas/Scroll View/Viewport/Content object and spawn the message prefab as a child
        //inboxCanvas = GameObject.Find("ModernInboxCanvas");
       // menuCanvas = GameObject.Find("ModernMenuCanvas");
        //mapCanvas = GameObject.Find("ModernMapCanvas");

        modernScreen = GameObject.Find("ModernScreen");


        //disable the inbox and enable the menu
        //inboxCanvas.SetActive(false);
       // menuCanvas.SetActive(true);
        //mapCanvas.SetActive(false);

        //GameObject messageObject = Instantiate(messagePrefab, content.transform);

        //the message has a "FromField/Text (TMP)" object
        // a "SubjectField/Text (TMP)" object
        // and a "Panel/ContentsField" object

        //Update each of these objects with the values from the order
        //messageObject.transform.Find("FromField/Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>().text = from;
        //messageObject.transform.Find("SubjectField/Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>().text = subject;
        //messageObject.transform.Find("Panel/ContentsField").GetComponent<TMPro.TextMeshProUGUI>().text = message;


        //this code gets executed as the order is called
        //some orders may not lead to another node so you can call continue if you wish to move to the next order after this one   
        Continue();
    }

    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "This order will initialise eveyrthing needed";
    }
}