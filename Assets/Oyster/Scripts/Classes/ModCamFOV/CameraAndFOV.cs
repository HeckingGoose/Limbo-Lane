using UnityEngine;
[System.Serializable] // Tell Unity that this is a class
public class CameraAndFOV
{
    public Camera camera; // Setup variables
    public float fov;
    public static CameraAndFOV Create(Camera camera, float fov) // Tell Unity which variables are part of the class
    {
        CameraAndFOV cameraAndFOV= new CameraAndFOV();
        cameraAndFOV.camera= camera; // Create and return an instance of itself
        cameraAndFOV.fov= fov;
        return cameraAndFOV;
    }
}
