using LoGaCulture.LUTE;
using System.Collections;
using UnityEngine;


[OrderInfo("AgesOfAvebury",
              "Detects when the user spins",
              "Detects spinnign and activates the events provoded")]
[AddComponentMenu("")]
public class SpinDetector : Order
{

    [SerializeField]
    private float spinThreshold = 360.0f; // The minimum angle to consider a spin

    [SerializeField]
    private float spinAccumulation = 0.0f; // Accumulated spin angle

    [SerializeField]
    private bool isSpinning = false; // Flag to indicate if currently spinning

    //list of events to trigger when spinning starts and stops
    [SerializeField]

    public override void OnEnter()
    {

        if (SystemInfo.supportsGyroscope)
        {
            // Enable the gyroscope
            Input.gyro.enabled = true;


            // Start the spin detection coroutine
            StartCoroutine(SpinDetection());
        }
        else
        {
            Debug.LogWarning("Gyroscope not supported on this device.");
            Continue();
        }


        // Start a coroutine to continue the order after 1 minute if the user doesn't spin
        StartCoroutine(continueAfterAMinute());
        // Continue();
    }


    IEnumerator SpinDetection()
    {

        yield return new WaitForSeconds(2f); // Wait for a moment to ensure gyroscope is ready
        while (true)
        {
            // Get the gyroscope rotation rate
            Vector3 rotationRate = Input.gyro.rotationRateUnbiased;
            // Accumulate the spin angle
            float horizontalRotation = Mathf.Abs(rotationRate.y) * Mathf.Rad2Deg;

            if (horizontalRotation > 50f) // Adjust threshold as needed
            {
                spinAccumulation += horizontalRotation * Time.deltaTime;
            }
            else
            {
                spinAccumulation = 0.0f; // Reset if not spinning

            }

            Debug.Log($"Spin Accumulation: {spinAccumulation} degrees");

            // Check if the accumulated spin exceeds the threshold
            if (spinAccumulation >= spinThreshold)
            {
                if (!isSpinning)
                {
                    isSpinning = true;
                    Debug.Log("User is spinning!");
                    // Trigger your spin event here
                    // For example, you can raise an event or call a method

                    Continue();

                    //exit the coroutine 
                    yield break;
                }
            }
            else
            {
                if (isSpinning)
                {
                    isSpinning = false;
                    Debug.Log("User stopped spinning.");
                    // Trigger your stop spin event here
                }
            }

            yield return null; // Wait for the next frame
        }
    }


    IEnumerator continueAfterAMinute()
    {
        yield return new WaitForSeconds(90f); // Wait for 1 minute
        Continue(); // Continue the order after 1 minute
    }
    public override string GetSummary()
    {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "Detects spin";
    }
}