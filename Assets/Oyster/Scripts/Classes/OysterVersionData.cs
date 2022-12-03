[System.Serializable] // Tells Unity that this is a class
public class OysterVersionData
{
    public string name;
    public string packageVersion; // Variables to be stored within the class
    public string[] supportedCommands;

    public static OysterVersionData Create(string _name, string _packageVersion, string[] _supportedCommands) // Constructor so that Unity knows what variables are stored within the class
    {
        OysterVersionData oysterVersionData = new OysterVersionData();
        oysterVersionData.name = _name;
        oysterVersionData.packageVersion = _packageVersion; // Creates and returns an instance of itself
        oysterVersionData.supportedCommands = _supportedCommands;
        return oysterVersionData;
    }
}
