using System.Collections;
using System.Collections.Generic; // Reference required assemblies
using TMPro;
using UnityEngine;

public class PulseTextTransparency : MonoBehaviour
{
    [SerializeField]
    private float fadeSpeed = 0.1f;
    private float transparency = 1f; // Define variables
    private int direction = -1;
    private TMP_Text text;
    private void Start()
    {
        try // Try to run the below code
        {
            text = gameObject.GetComponent<TMP_Text>(); // Find a text component on the object this script is attached to
        }
        catch // If the above code fails to run
        {
            Debug.Log("Unable to find text component on '" + gameObject.name + "."); // Inform the Unity console that something went wrong
            text = null; // Set text to null
        }
    }
    private void Update()
    {
        if (gameObject.activeSelf && text != null) // If the object is active and text is not null
        {
            transparency = transparency + fadeSpeed * direction * Time.deltaTime; // Set transparency equal to the current transparency + fadespeed * direction * deltatime
            if (transparency >= 1f) // If transparency is more than 1
            {
                direction = -1; // Set direction to -1
            }
            if (transparency <= 0f) // If transparency is less than 0
            {
                direction = 1; // Set direction to 1
            }
            transparency = Mathf.Clamp(transparency, 0, 1); // Clamp transparency to between 0 and 1
            text.color = new Color(text.color.r, text.color.g, text.color.b, transparency); // Set the colour of the text equal to itself with the new alpha value
        }
    }
}