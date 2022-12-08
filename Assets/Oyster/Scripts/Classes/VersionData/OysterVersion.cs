using UnityEngine;

[System.Serializable] // Tell Unity that this is a class
public class OysterVersion
{
    public OysterVersionData oysterVersion; // This class acts as a container for the OysterVersionData data type, so that it matches JSON formatting, meaning this is the only required variable

    public static OysterVersionData CreateFromJSON(string jsonString) // Ensure that this is always loaded in memory
    {
        return JsonUtility.FromJson<OysterVersionData>(jsonString); // Return the input, but converted to this class type
    }
}
