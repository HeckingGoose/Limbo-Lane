[System.Serializable] // Tell Unity that this is a class
public class CharacterData
{
    public string name;
    public string altName; // Define variables that make up the class
    public string colour;

    public static CharacterData Create(string name, string altName, string colour) // Tell Unity which variables make up the class
    {
        CharacterData data = new CharacterData();
        data.name = name; // Create and return an instance of itself
        data.altName = altName;
        data.colour = colour;
        return data;
    }
}
