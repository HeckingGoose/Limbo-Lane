[System.Serializable] // Tell Unity that this is a class
public class ProfileData
{
    public string name;
    public string version; // Define required variables
    public int currency;
    public string location;
    public Deck deck;
    public ObjectState[] enemyStates;
    public ObjectState[] locationStates;
    public static ProfileData Create(string profileName, string version, int currency, string location, Deck deck, ObjectState[] enemyStates, ObjectState[] locationStates) // Tell Unity which variables are used
    {
        ProfileData data = new ProfileData();
        data.name = profileName; // Create and return an instance of itself
        data.version = version;
        data.currency = currency;
        data.location = location;
        data.deck = deck;
        data.enemyStates = enemyStates;
        data.locationStates = locationStates;
        return data;
    }
}
