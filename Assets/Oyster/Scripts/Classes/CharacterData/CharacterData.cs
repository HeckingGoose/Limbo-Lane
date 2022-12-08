[System.Serializable]
public class CharacterData
{
    public string name;
    public string altName;
    public string colour;

    public static CharacterData Create(string _name, string _altName, string _colour)
    {
        CharacterData data = new CharacterData();
        data.name = _name;
        data.altName = _altName;
        data.colour = _colour;
        return data;
    }
}
