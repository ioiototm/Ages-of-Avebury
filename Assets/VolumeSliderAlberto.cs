using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Controls master volume via a UI Slider (0–100) and plays assigned sounds when crossing specific slider values.
/// </summary>
public class VolumeSliderAlberto : MonoBehaviour
{
    [Tooltip("UI Slider that controls the game's master volume.")]
    public Slider volumeSlider;

    [Tooltip("AudioSource used to play feedback sounds.")]
    public AudioSource audioSource;

    [Tooltip("List of threshold values (0–100) and their associated AudioClips.")]
    public List<ThresholdSound> thresholdSounds = new List<ThresholdSound>();

    // Tracks the previous slider value to detect crossings
    private float previousValue;

    private void Awake()
    {
        if (volumeSlider == null)
        {
            Debug.LogError("VolumeSlider: No Slider assigned in the Inspector.");
            enabled = false;
            return;
        }
        if (audioSource == null)
        {
            Debug.LogError("VolumeSlider: No AudioSource assigned in the Inspector.");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        // Configure slider range
        volumeSlider.minValue = 0;
        volumeSlider.maxValue = 100;

        // Initialize slider and volume
        float initValue = AudioListener.volume * 100f;
        volumeSlider.value = initValue;
        previousValue = initValue;

        // Listen for changes
        volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    /// <summary>
    /// Called when the slider value is changed by the user.
    /// Plays sounds when crossing defined thresholds.
    /// </summary>
    /// <param name="sliderValue">Value between 0 and 100.</param>
    private void OnSliderValueChanged(float sliderValue)
    {
        // Update master volume
        AudioListener.volume = sliderValue / 100f;

        // Check each threshold
        foreach (var ts in thresholdSounds)
        {
            // If crossing threshold upwards
            if (previousValue < ts.threshold && sliderValue >= ts.threshold)
            {
                if (ts.clip != null)
                    audioSource.PlayOneShot(ts.clip);
            }
            // If crossing threshold downwards (optional):
            else if (previousValue > ts.threshold && sliderValue <= ts.threshold)
            {
                if (ts.clip != null)
                    audioSource.PlayOneShot(ts.clip);
            }
        }

        previousValue = sliderValue;
    }

    private void OnDestroy()
    {
        volumeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }
}

[System.Serializable]
public class ThresholdSound
{
    [Range(0, 100)]
    public float threshold;
    public AudioClip clip;
}
