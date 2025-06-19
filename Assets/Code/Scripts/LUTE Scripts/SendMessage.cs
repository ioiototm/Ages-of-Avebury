using LoGaCulture.LUTE;
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
        //GameObject content = GameObject.Find("ModernInboxCanvas/Scroll View/Viewport/Content");


        //create an instance of the mailDoc and edit the text fields
        //VisualElement mailElement = mailDoc.CloneTree();
        //if (mailElement != null) {
        //    mailElement.Q<TextElement>("FromField").text = from;

        //    Debug.Log(mailElement.Q<TextElement>("FromField").text);

        //    mailElement.Q<TextElement>("SubjectField").text = subject;
        //    mailElement.Q<TextElement>("EmailBody").text = message;
        //}

        //InitialiseEverything.modernScreen.GetComponent<ModernScreenScript>().addMessage(mailElement);

        //Continue();

        //return;


        GameObject content = InitialiseEverything._inboxCanvas.transform.Find("ModernInbox/Scroll View/Viewport/Content").gameObject;

        GameObject messageObject = Instantiate(messagePrefab, content.transform);
        messageObject.transform.SetSiblingIndex(0);


        //this spawns it in a content ui object, which is in a view port which is in a scroll view, so changes to the scroll view will affect the content object


        //the message has a "FromField/Text (TMP)" object
        // a "SubjectField/Text (TMP)" object
        // and a "Panel/ContentsField" object

        //Update each of these objects with the values from the order
        messageObject.transform.Find("FromField/FromField").GetComponent<TMPro.TextMeshProUGUI>().text = from;
        messageObject.transform.Find("SubjectField/SubjectField").GetComponent<TMPro.TextMeshProUGUI>().text = subject;
        messageObject.transform.Find("Panel/ContentsField").GetComponent<TMPro.TextMeshProUGUI>().text = message;



        //Debug.Log("Setting from to " + from + " subject to " + subject + " message to " + message);
        //get the BasicFlowEngine 
        BasicFlowEngine engine = GetEngine();



        BooleanVariable isInInbox = engine.GetVariable<BooleanVariable>("isInInbox");


        if (!isInInbox.Value)
        {
            //get the ModernFrame/FooterPanel/InboxButton/Notification object and enable it
            GameObject notification = GameObject.Find("ModernInterface/ModernFrame/FooterPanel/InboxButton/Notification");
            //enable the notification object
            notification.SetActive(true);
        }



        BooleanVariable unreadMessages = engine.GetVariable<BooleanVariable>("unreadMessages");
        //set it to true
        unreadMessages.Value = true;

        BooleanVariable newMessages = engine.GetVariable<BooleanVariable>("newMessages");
        //set it to true
        newMessages.Value = true;


        //check if it's true or false, if it is, then get the Notification object in the ModernFrame
        //if (boolVar != null)
        //{
        //    if (boolVar.Evaluate(ComparisonOperator.Equals, true))
        //    {
        //        GameObject notification = GameObject.Find("ModernFrame/FooterPanel/InboxButton/Notification");
        //        //enable the notification object
        //        notification.SetActive(false);

        //    }
        //    else
        //    {                 //if it's false, then get the Notification object in the ModernFrame
        //        GameObject notification = GameObject.Find("ModernFrame/FooterPanel/InboxButton/Notification");
        //        //disable the notification object
        //        notification.SetActive(true);
        //    }
        //}


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