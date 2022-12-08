[System.Serializable]
public class CharacterDataContainer
{
    public CharacterData[] characters;

    public static CharacterDataContainer Create(CharacterData[] _characters)
    {
        CharacterDataContainer container = new CharacterDataContainer();
        container.characters = _characters;
        return container;
    }
}
