[System.Serializable] // Tell Unity that this is a class
public class CharacterDataContainer
{
    public CharacterData[] characters; // Variable that makes up the class

    public static CharacterDataContainer Create(CharacterData[] _characters) // Tell Unity which variables make up the class
    {
        CharacterDataContainer container = new CharacterDataContainer();
        container.characters = _characters; // Create and return an instance of itself
        return container;
    }
}
