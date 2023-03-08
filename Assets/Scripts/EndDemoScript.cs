using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndDemoScript : MonoBehaviour
{
    private void Update()
    {
        if (Input.anyKey) // If any button is pressed
        {
            if (Input.GetAxis("SkipText") == 0) // If lctrl or rctrl are not held or pressed
            {
                try // Try to run the below code
                {
                    string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments); // Define documents path
                    ProfileData profileData = JsonUtility.FromJson<ProfileData>(File.ReadAllText(documentsPath + @"\My Games\LimboLane\Profiles\" + PersistentVariables.profileName + ".json")); // Load profile
                    profileData.location = "AlexHouse"; // Set the profile's location to Alex's house
                    foreach(ObjectState location in profileData.locationStates) // Loop through every location
                    {
                        location.state = 0; // Set the location state to 0
                    }
                    File.WriteAllText(documentsPath + @"\My Games\LimboLane\Profiles\" + profileData.name + ".json", JsonUtility.ToJson(profileData)); // Commit the changes to the save file
                }
                catch (Exception e) // If the code fails to run
                {
                    Debug.Log("Something went wrong! (" + e + ")"); // Inform the Unity console that something went wrong
                }
                PersistentVariables.profileLocation = "AlexHouse";
                PersistentVariables.profileName = ""; // Set default values for a few persistent variables
                PersistentVariables.nextSceneName = "AlexHouse";
                SceneManager.LoadScene("LoadingScreen"); // Begin loading the loading screen
            }
        }
    }
}
