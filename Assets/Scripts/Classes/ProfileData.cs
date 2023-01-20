[System.Serializable] // Tell Unity that this is a class
public class ProfileData
{
    public string name;
    public string version; // Define required variables
    public int currency;
    public int matchStartingCurrency;
    public string location;
    public string[] deck;
    public int handSize;
    public ObjectState[] enemyStates;
    public ObjectState[] locationStates;
    public static ProfileData Create(string profileName, string version, int currency, int matchStartingCurrency, string location, string[] deck, int handSize, ObjectState[] enemyStates, ObjectState[] locationStates) // Tell Unity which variables are used
    {
        ProfileData data = new ProfileData();
        data.name = profileName; // Create and return an instance of itself
        data.version = version;
        data.currency = currency;
        data.matchStartingCurrency = matchStartingCurrency;
        data.location = location;
        data.deck = deck;
        data.handSize = handSize;
        data.enemyStates = enemyStates;
        data.locationStates = locationStates;
        return data;
    }
}
