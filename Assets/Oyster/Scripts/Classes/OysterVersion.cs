using UnityEngine;

[System.Serializable]
public class OysterVersion
{
    public OysterVersionData oysterVersion;

    public static OysterVersionData CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<OysterVersionData>(jsonString);
    }
}
