[System.Serializable] // Tell Unity that this is a class
public class Options
{
    //public int qualityLevel;
    public string windowedMode;
    public bool vsync;
    public int maxTextureSize;
    public float volume; // Define variables
    public int linesPerFrame;
    public float charactersPerSecond;
    public float skipSpeed;
    public static Options Create(string windowedMode, bool vsync, int maxTextureSize, float volume, int linesPerFrame, float charactersPerSecond, float skipSpeed) // Create and return an instance of itself
    {
        Options options = new Options();
        //options.qualityLevel = qualityLevel;
        options.maxTextureSize= maxTextureSize;
        options.windowedMode = windowedMode;
        options.vsync = vsync;
        options.volume = volume; // Set given values to class variables
        options.linesPerFrame = linesPerFrame;
        options.charactersPerSecond = charactersPerSecond;
        options.skipSpeed = skipSpeed;
        return options; // Return self
    }
}