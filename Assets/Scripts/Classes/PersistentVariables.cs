[System.Serializable]
public class PersistentVariables // Tell Unity that this is a class
{
    public static string nextSceneName; // Create variables that persist across scenes
    public static string profileName; // Name of current profile being used
    public static int charactersPerSecond; // Number of characters to add to screen per second for AddSmoothText
    public static string documentsPath = ""; // Path to system documents folder
}