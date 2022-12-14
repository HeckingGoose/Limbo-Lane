using UnityEngine;

[System.Serializable]
public class ObjectAndPosition
{
    public GameObject gameObject;
    public Vector3[] oldState;
    
    public static ObjectAndPosition Create(GameObject _gameObject, Vector3[] _oldState)
    {
        ObjectAndPosition objectAndPosition = new ObjectAndPosition();
        objectAndPosition.gameObject = _gameObject;
        objectAndPosition.oldState = _oldState;
        return objectAndPosition;
    }
}
