[System.Serializable] // Tell Unity that this is a class
public class OysterConversationsContainer
{
    public OysterConversation[] container; // Variable that is stored within the class
    public static OysterConversationsContainer Create(OysterConversation[] _container) // Constructor so that Unity knows what variables are stored within the class
    {
        OysterConversationsContainer conversations = new OysterConversationsContainer();
        conversations.container = _container; // Creates and returns an instance of itself
        return conversations;
    }
}
