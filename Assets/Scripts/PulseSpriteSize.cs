using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseSpriteSize : MonoBehaviour
{
    [SerializeField]
    private float maxScale;
    [SerializeField]
    private float timePeriod;
    private float currentTime;
    private string state;
    private Vector3 ogScale;
    private Vector3 finalScale;
    private void Start()
    {
        if (maxScale < 1) // Limit maxScale to be 1
        {
            maxScale = 1;
        }
        ogScale = this.transform.localScale; // Cache the original sprite size
        finalScale = this.transform.localScale * maxScale; // Calculate and store the max sprite size
        state = "increasing"; // Set the state to increasing
    }
    private void Update()
    {
        switch (state) // Pick which state the function is currently in
        {
            default: // State not recognised
                Debug.Log("State not recognised! (" + state + ")"); // Inform the Unity console that something went wrong
                break;
            case "increasing": // If the state is increasing
                if (currentTime < timePeriod) // If it is not time to switch state
                {
                    this.transform.localScale = Vector3.Lerp(ogScale, finalScale, currentTime / timePeriod); // Interpolate between og and final scale
                }
                else // Otherwiwse
                {
                    this.transform.localScale = finalScale; // Set scale to final scale
                    state = "decreasing"; // Set state to decreasing
                    currentTime = 0; // Reset time
                }
                break;
            case "decreasing": // If the state is decreasing
                if (currentTime < timePeriod) // If it is not time to switch state
                {
                    this.transform.localScale = Vector3.Lerp(finalScale, ogScale, currentTime / timePeriod); // Interpolate between final and og scale
                }
                else // Otherwise
                {
                    this.transform.localScale = ogScale; // Set scale to og scale
                    state = "increasing"; // Set state to increasing
                    currentTime = 0; // Reset time
                }
                break;
        }
        currentTime += Time.deltaTime; // Increment time
    } 
}