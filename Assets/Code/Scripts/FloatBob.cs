using UnityEngine;

/// <summary>
/// Makes the object gently bob on its local Y axis.
/// </summary>
public class FloatBob : MonoBehaviour
{
    [Header("Bobbing Settings")]
    [Tooltip("Peak distance above and below the start position (in Unity units).")]
    [SerializeField] private float amplitude = 0.25f;

    [Tooltip("How many full up-and-down cycles per second.")]
    [SerializeField] private float frequency = 1f;

    private Vector3 startPos;

    private void Awake()
    {
        // Cache the original position so we always bob relative to it.
        startPos = transform.localPosition;
    }

    private void Update()
    {
        //Sin outputs -1...1, so multiply by amplitude to scale the motion.
        float offset = Mathf.Sin(Time.time * Mathf.PI * 2f * frequency) * amplitude;

        // Apply only on the Y axis.
        transform.localPosition = startPos + Vector3.up * offset;
    }
}
