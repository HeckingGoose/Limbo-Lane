using UnityEngine; // Reference required assemblies

[System.Serializable] // Tell Unity that this is a class
public class ObjectAndPosition
{
    public GameObject gameObject; // Setup required variables
    public Vector3[] oldState;
    
    public static ObjectAndPosition Create(GameObject _gameObject, Vector3[] _oldState) // Tell Unity which variables this class uses
    {
        ObjectAndPosition objectAndPosition = new ObjectAndPosition();
        objectAndPosition.gameObject = _gameObject; // Create and return an instance of itself
        objectAndPosition.oldState = _oldState;
        return objectAndPosition;
    }
}
