using UnityEngine;

[OrderInfo("Menu",
             "Popup Menu",
             "Creates a popup menu icon which displays a menu when clicked; the menu is populated by the orders on this node but only supports specific orders")]
[AddComponentMenu("")]
public class PopupMenuAOA : GenericButton
{
    [Header("Popup Menu Settings")]
    [Tooltip("A custom Menu display to use to display this popup menu")]
    [SerializeField] protected Popup popupWindow;
    [Tooltip("If true, the popup menu will be redrawn each time it is opened")]
    [SerializeField] protected bool allowRedraw = false;

    public Popup SetPopupWindow { get { return popupWindow; } set { popupWindow = value; } }

    private bool drawn = false;

    public override void OnEnter()
    {
        if (drawn && !allowRedraw)
        {
            Continue();
            return;
        }

        if (SetPopupWindow != null)
        {
            Popup.ActivePopupWindow = SetPopupWindow;
        }

        var popupWindow = Popup.GetPopupWindow();
        var popupIcon = SetupButton();

        if (popupWindow != null)
        {
            var orders = ParentNode.OrderList;
            popupIcon.SetPopupWindow(popupWindow);
            UnityEngine.Events.UnityAction action = () =>
            {
                popupWindow.OpenClose();
            };
            if (orders.Count > 0)
            {
                popupWindow.SetOrders(orders);
                popupWindow.CreateMenuGUI();
            }
            SetAction(popupIcon, action);
        }

        drawn = true;

        Continue();
    }
}