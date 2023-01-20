[System.Serializable]
public class PersistentVariables // Tell Unity that this is a class
{
    public static string nextSceneName = ""; // Create variables that persist across scenes
    #region Data for current profile
    public static string profileName = ""; // Name of current profile being used
    public static string profileVersion = "";
    public static int profileCurrency = -1;
    public static int matchStartingCurrency = -1;
    public static string profileLocation = "";
    public static int handSize = -1;
    // Any non-built in objects, such as Deck cannot be referenced like this
    #endregion
    public static float charactersPerSecond = -1; // Number of characters to add to screen per second for AddSmoothText
    public static int linesPerFrame = -1; // Max number of lines to be processed per frame in Oyster
    public static float skipSpeed = -1; // The speed at which lines can be skipped through in Oyster
    public static string documentsPath = ""; // Path to system documents folder
}