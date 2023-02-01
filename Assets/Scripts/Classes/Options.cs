[System.Serializable] // Tell Unity that this is a class
public class Options
{
    //public int qualityLevel;
    public int resWidth;
    public int resHeight;
    public int refreshRate;
    public string windowedMode;
    public float renderScale;
    public bool vsync;
    public int maxTextureSize;
    public float volume; // Define variables
    public int linesPerFrame;
    public float charactersPerSecond;
    public float skipSpeed;
    public static Options Create(int resWidth, int resHeight, int refreshRate, string windowedMode, float renderScale, bool vsync, int maxTextureSize, float volume, int linesPerFrame, float charactersPerSecond, float skipSpeed) // Create and return an instance of itself
    {
        Options options = new Options();
        options.resWidth = resWidth;
        options.resHeight = resHeight;
        options.refreshRate = refreshRate;
        options.renderScale = renderScale;
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