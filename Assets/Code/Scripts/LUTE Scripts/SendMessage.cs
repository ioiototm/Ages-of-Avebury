using UnityEngine;

[OrderInfo("AgesOfAvebury",
              "Send Message",
              "Sends a message to the UI")]
[AddComponentMenu("")]
public class SendMessage : Order
{

    [SerializeField]
    private string from;

    [SerializeField]
    private string subject;

    [SerializeField]
    private string message;

    [SerializeField]
    private GameObject messagePrefab;


    public override void OnEnter()
    {
        //get the ModernInboxCanvas/Scroll View/Viewport/Content object and spawn the message prefab as a child
        GameObject content = GameObject.Find("ModernInboxCanvas/Scroll View/Viewport/Content");

        GameObject messageObject = Instantiate(messagePrefab, content.transform);

        //the message has a "FromField/Text (TMP)" object
        // a "SubjectField/Text (TMP)" object
        // and a "Panel/ContentsField" object

        //Update each of these objects with the values from the order
        messageObject.transform.Find("FromField/Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>().text = from;
        messageObject.transform.Find("SubjectField/Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>().text = subject;
        messageObject.transform.Find("Panel/ContentsField").GetComponent<TMPro.TextMeshProUGUI>().text = message;


        //this code gets executed as the order is called
        //some orders may not lead to another node so you can call continue if you wish to move to the next order after this one   
        Continue();
    }

    public override string GetSummary()
  {
 //you can use this to return a summary of the order which is displayed in the inspector of the order
      return "This order will send a message to the UI with the following details: " + from + " " + subject + " " + message;
    }
}