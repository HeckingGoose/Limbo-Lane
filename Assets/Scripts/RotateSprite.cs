using UnityEngine; // Reference required assemblies

public class RotateSprite : MonoBehaviour
{
    [SerializeField]
    private float maxAngle = 20;
    [SerializeField]
    private float timePeriod = 1;
    private GameObject sprite; // Define variables
    private Quaternion ogRotation;
    private float currentTime = 0;
    private int state = 0;
    private Quaternion currentRotation;
    private Quaternion maxLeft;
    private Quaternion maxRight;
    // States:
    // 0 - sprite has begun rotating to the left
    // 1 - sprite has rotated max to left and is now rotating to the right
    // 2 - sprite has rotated max to right and is now rotating to the left

    private void Start()
    {
        sprite = gameObject;
        timePeriod = timePeriod / 2; // Set variable values, timePeriod begins at half as first state is half a rotation
        ogRotation = sprite.transform.rotation;
        currentRotation = ogRotation;
#pragma warning disable CS0618 // Type or member is obsolete
        Vector3 tempRotation = currentRotation.ToEuler() * 180 / Mathf.PI; // Translate quaternion rotation to a vector3
#pragma warning restore CS0618 // Type or member is obsolete

        maxLeft = Quaternion.Euler(new Vector3(tempRotation.x, tempRotation.y, tempRotation.z - maxAngle)); // Calculate the max right and left rotation
        maxRight = Quaternion.Euler(new Vector3(tempRotation.x, tempRotation.y, tempRotation.z + maxAngle));
    }
    private void Update()
    {
        currentTime += Time.deltaTime; // Add the current time between frames to currentTime
        if (currentTime >= timePeriod) // If currentTime is more than the intended waiting time
        {
            currentTime = 0; // Set currentTime to 0
            switch (state) // Compare state against the below cases
            {
                default: // If state does not match any of the below cases
                    Debug.Log("State does not exist!"); // Inform the Unity console that something went wrong
                    break;
                case 0: // If state is 0
                    state = 1; // Set state to 1
                    timePeriod = timePeriod * 2; // Set the timePeriod to be twice of itself
                    break;
                case 1: // If state is 1
                    state = 2; // Set state to 2
                    break;
                case 2: // If state is 2
                    state = 1; // Set state to 1
                    break;
            }
        }
        else // If currentTime is not more than the intended waiting time
        {
            switch (state) // Compare state against the below cases
            {
                default: // If state does not match any of the below cases
                    Debug.Log("State does not exist!"); // Inform the Unity console that something went wrong
                    break;
                case 0: // If state is 0
                    currentRotation = Quaternion.Lerp(ogRotation, maxLeft, currentTime / timePeriod); // Rotate sprite between the start rotation and max left rotation
                    break;
                case 1: // If state is 1
                    currentRotation = Quaternion.Lerp(maxLeft, maxRight, currentTime / timePeriod); // Rotate sprite between the max left and max right
                    break;
                case 2: // If state is 2
                    currentRotation = Quaternion.Lerp(maxRight, maxLeft, currentTime / timePeriod); // Rotate sprite between the max right and max left
                    break;
            }
        }
        sprite.transform.rotation = currentRotation; // Apply the rotation previously calculated to the sprite
    }
}
