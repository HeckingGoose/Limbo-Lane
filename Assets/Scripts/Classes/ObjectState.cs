[System.Serializable] // Tell Unity that this is a class
public class ObjectState
{
    public string name; // Define required variables
    public int state;
    public static ObjectState Create(string name, int state) // Tell Unity which variables are actually used
    {
        ObjectState objectState= new ObjectState();
        objectState.name = name; // Create and return an instance of itself
        objectState.state = state;
        return objectState;
    }
}
