[System.Serializable] // Tells Unity that this is a class
public class OysterVersionData
{
    public string name;
    public string packageVersion; // Variables to be stored within the class
    public string[] supportedCommands;

    public static OysterVersionData Create(string name, string packageVersion, string[] supportedCommands) // Constructor so that Unity knows what variables are stored within the class
    {
        OysterVersionData oysterVersionData = new OysterVersionData();
        oysterVersionData.name = name;
        oysterVersionData.packageVersion = packageVersion; // Creates and returns an instance of itself
        oysterVersionData.supportedCommands = supportedCommands;
        return oysterVersionData;
    }
}
