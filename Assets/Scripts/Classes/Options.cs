[System.Serializable] // Tell Unity that this is a class
public class Options
{
    public int qualityLevel;
    public float volume; // Define variables
    public int linesPerFrame;
    public int charactersPerSecond;
    public static Options Create(int qualityLevel, float volume, int linesPerFrame, int charactersPerSecond) // Create and return an instance of itself
    {
        Options options = new Options();
        options.qualityLevel = qualityLevel;
        options.volume = volume; // Set given values to class variables
        options.linesPerFrame = linesPerFrame;
        options.charactersPerSecond = charactersPerSecond;
        return options; // Return self
    }
}