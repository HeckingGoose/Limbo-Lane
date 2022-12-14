[System.Serializable] // Tell Unity that this is a class
public class CharacterData
{
    public string name;
    public string altName; // Define variables that make up the class
    public string colour;

    public static CharacterData Create(string _name, string _altName, string _colour) // Tell Unity which variables make up the class
    {
        CharacterData data = new CharacterData();
        data.name = _name; // Create and return an instance of itself
        data.altName = _altName;
        data.colour = _colour;
        return data;
    }
}
