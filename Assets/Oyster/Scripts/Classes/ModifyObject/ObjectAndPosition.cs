using UnityEngine; // Reference required assemblies

[System.Serializable] // Tell Unity that this is a class
public class ObjectAndPosition
{
    public GameObject gameObject; // Setup required variables
    public Vector3[] oldState;
    
    public static ObjectAndPosition Create(GameObject gameObject, Vector3[] oldState) // Tell Unity which variables this class uses
    {
        ObjectAndPosition objectAndPosition = new ObjectAndPosition();
        objectAndPosition.gameObject = gameObject; // Create and return an instance of itself
        objectAndPosition.oldState = oldState;
        return objectAndPosition;
    }
}
