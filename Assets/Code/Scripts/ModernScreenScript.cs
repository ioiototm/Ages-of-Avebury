using UnityEngine;
using UnityEngine.UIElements;

public class ModernScreenScript : MonoBehaviour
{

    public UIDocument modernUIDoc;

    public Button menuButton, mailButton, mapButton, scannerButton;

    public void addMessage(VisualElement mailDoc)
    {


        // Add it to #EmailList
        modernUIDoc.rootVisualElement.Q<VisualElement>("EmailList").Insert(0,mailDoc);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        menuButton = modernUIDoc.rootVisualElement.Q<Button>("MenuButton");
        mailButton = modernUIDoc.rootVisualElement.Q<Button>("MailButton");
        mapButton = modernUIDoc.rootVisualElement.Q<Button>("MapButton");
        scannerButton = modernUIDoc.rootVisualElement.Q<Button>("ScannerButton");

        menuButton.RegisterCallback<ClickEvent>(onMenuButtonClick); 

    }

    void onMenuButtonClick(ClickEvent evt)
    {
        Debug.Log("Menu Button Clicked");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
