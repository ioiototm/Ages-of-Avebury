using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class TMP_LinkOpener : MonoBehaviour, IPointerClickHandler
{
    TMP_Text text;

    void Awake() => text = GetComponent<TMP_Text>();

    public void OnPointerClick(PointerEventData eventData)
    {
        // Which link did we tap?
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            text, eventData.position, eventData.pressEventCamera);

        if (linkIndex == -1) return;         // no link hit

        TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
        string id = linkInfo.GetLinkID();    // we stored the URL here

        Application.OpenURL(id);             // hands off to OS browser
    }
}
