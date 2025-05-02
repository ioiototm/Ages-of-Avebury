using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FitSpriteToScreen : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr.sprite == null)
        {
            Debug.LogError("No sprite found on SpriteRenderer.");
            return;
        }

        transform.localScale = Vector3.one;

        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;

        float worldScreenHeight = Camera.main.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * Screen.width / Screen.height;

        Vector3 scale = transform.localScale;
        scale.x = worldScreenWidth / width;
        scale.y = worldScreenHeight / height;

        transform.localScale = scale;
    }
}
