using UnityEngine; // Import required assemblies

public class OysterInitialiseScript : MonoBehaviour
{
    private OysterCharacterScript characterScript; // Setup variables
    private bool mouseHeld = false;
    void FixedUpdate() // Fixed update is used as raycasts are part of Unity's physics system - which work best in a FixedUpdate function
    {
        if (Input.GetAxis("PrimaryAction") > 0 && mouseHeld == false) // Wait for LMB to be pressed, + if it isn't already pressed
        {
            mouseHeld = true;
            Ray raycast = Camera.main.ScreenPointToRay(Input.mousePosition); // Create a new raycast between the screen and game world
            RaycastHit hit; // Create an object to store the output of the raycast in
            if (Physics.Raycast(raycast, out hit, maxDistance: 3f)) // True if the raycast collides with an object
            {
                try // Attempt to find an OysterCharacterScript component, and if that succeeds then begin a conversation
                {
                    characterScript = hit.collider.gameObject.GetComponent<OysterCharacterScript>();
                    characterScript.StartSpeech();
                }
                catch // If the above part fails, then reset the characterScript variable to be empty
                {
                    characterScript = null;
                }
            }
        }
        else if (Input.GetAxis("PrimaryAction") == 0) // If the mouse has been released
        {
            mouseHeld = false; // Allow another conversation to start if mouse is clicked again
        }
    }
}
