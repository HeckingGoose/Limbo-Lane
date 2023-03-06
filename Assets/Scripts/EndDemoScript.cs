using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndDemoScript : MonoBehaviour
{
    private void Update()
    {
        if (Input.anyKey)
        {
            if (Input.GetAxis("SkipText") == 0)
            {
                try
                {
                    string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments); // Define documents path
                    ProfileData profileData = JsonUtility.FromJson<ProfileData>(File.ReadAllText(documentsPath + @"\My Games\LimboLane\Profiles\" + PersistentVariables.profileName + ".json"));
                    profileData.location = "AlexHouse";
                    foreach(ObjectState location in profileData.locationStates)
                    {
                        location.state = 0;
                    }
                    File.WriteAllText(documentsPath + @"\My Games\LimboLane\Profiles\" + profileData.name + ".json", JsonUtility.ToJson(profileData));
                }
                catch (Exception e)
                {
                    Debug.Log("Something went wrong! (" + e + ")");
                }
                PersistentVariables.profileLocation = "AlexHouse";
                PersistentVariables.profileName = "";
                PersistentVariables.nextSceneName = "AlexHouse";
                SceneManager.LoadScene("LoadingScreen");
            }
        }
    }
}
