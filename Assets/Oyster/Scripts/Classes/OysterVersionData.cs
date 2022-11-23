[System.Serializable]
public class OysterVersionData
{
    public string name;
    public string packageVersion;
    public string[] supportedCommands;

    public static OysterVersionData Create(string _name, string _packageVersion, string[] _supportedCommands)
    {
        OysterVersionData oysterVersionData = new OysterVersionData();
        oysterVersionData.name = _name;
        oysterVersionData.packageVersion = _packageVersion;
        oysterVersionData.supportedCommands = _supportedCommands;
        return oysterVersionData;
    }
}
