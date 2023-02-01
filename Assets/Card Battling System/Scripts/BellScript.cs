using UnityEngine;

public class BellScript : MonoBehaviour
{
    [HideInInspector]
    public bool bellClicked = false; // True when the bell has been clicked
    private bool mouseDown = false; // True when the mouse has been clicked
    private void Update() // Code in here is ran every frame
    {
        if (Input.GetAxis("PrimaryAction") > 0) // If the mouse is clicked
        {
            mouseDown = true; // Set mouseDown to true
        }
        else // If the mouse has not been clicked
        {
            mouseDown = false; // Set mouseDown to false
            bellClicked = false; // Set bellClicked to false
        }
    }
    private void OnMouseOver() // Code in here is ran while the mouse hovers over this object
    {
        if (!mouseDown && Input.GetAxis("PrimaryAction") > 0) // If the mouse is not held and the mouse is clicked
        {
            bellClicked = true; // Set bellClicked to true
        }
        else // Otherwise
        {
            bellClicked = false; // Set bellClicked to false
        }
    }
}
