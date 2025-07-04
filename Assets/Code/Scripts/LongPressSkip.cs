using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongPressSkip : MonoBehaviour,
                             IPointerDownHandler,
                             IPointerUpHandler,
                             IPointerExitHandler
{
    [Tooltip("Seconds to hold before triggering")]
    public float holdTime = 5f;

    

    //game objhect and function to call when long press is triggered so that it shows in the editor
    public UnityEvent OnLongPress = new UnityEvent();

    float _timer;
    bool _holding;

    void Update()
    {
        if (!_holding) return;

        _timer += Time.unscaledDeltaTime;           // UI feels better unscaled
       

        if (_timer >= holdTime)
        {
            _holding = false;
          

            OnLongPress.Invoke();  // Trigger the long press event
        }
    }

    public void OnPointerDown(PointerEventData e)
    {
        _holding = true;
        _timer = 0f;
       
    }

    public void OnPointerUp(PointerEventData e) => Cancel();
    public void OnPointerExit(PointerEventData e) => Cancel();

    void Cancel()
    {
        _holding = false;
        
    }
}
