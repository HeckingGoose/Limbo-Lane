using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement; // Import required assemblies
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    private Image loadingProgressBar;
    [SerializeField] // Define variables and make certain ones viewable in the inspector
    private GameObject loadingText;
    public AsyncOperation sceneLoader; // Set this variable to be able to be accessed from other scripts
    private string documentsPath;
    private void Start()
    {
        documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments); // Define documents path
        // begin loading
        try // Try to run the below code
        {
            sceneLoader = SceneManager.LoadSceneAsync(PersistentVariables.nextSceneName); // Begin loading the next scene asynchronously
            sceneLoader.completed += SceneLoader_Completed; // I'm not even sure how adding this makes it work here, but it just does
            sceneLoader.allowSceneActivation = false; // Prevent it from auto loading to the next scene
        }
        catch // If the above code fails to run
        {
            Debug.Log("Unable to load scene '" + PersistentVariables.nextSceneName + "' as it does not exist. Loading back to main menu"); // Inform Unity that something went wrong
            PersistentVariables.nextSceneName = "MainMenu"; // Default to loading back to the main menu
            sceneLoader = SceneManager.LoadSceneAsync(PersistentVariables.nextSceneName);
        }
    }
    private void Update()
    {
        if (sceneLoader.progress >= 0.8) // If the scene is 80% loaded or more
        {
            loadingProgressBar.fillAmount = 1; // Set the progress bar to be full
            if (!loadingText.activeSelf) // If the loading finished text is not active
            {
                loadingText.SetActive(true); // Set the loading finished text to be active
            }
            if (Input.anyKey) // If any key is pressed
            {
                loadingText.SetActive(false); // Set the loading finished text to false
                sceneLoader.allowSceneActivation = true; // Allow the next scene to finish loading
                // Change location in save file
                try // Try to run the below code
                {
                    ProfileData profileData = JsonUtility.FromJson<ProfileData>(File.ReadAllText(documentsPath + @"\My Games\LimboLane\Profiles\" + PersistentVariables.profileName + ".json")); // Load profile data
                    profileData.location = PersistentVariables.nextSceneName; // Set the name of the profile's next location
                    File.WriteAllText(documentsPath + @"\My Games\LimboLane\Profiles\" + PersistentVariables.profileName + ".json", JsonUtility.ToJson(profileData)); // Write new location to save file
                }
                catch // If the above code fails
                {
                    Debug.Log("Unable to update profile with new location!"); // Inform the Unity console that something went wrong
                }
            }
        }
        else // Otherwise
        {
            loadingProgressBar.fillAmount = sceneLoader.progress; // Set the progress bar equal to the current progress of the scene loader
        }
    }
    private void SceneLoader_Completed(AsyncOperation result)
    {
        // I'm not sure how this function does help, but from my testing it does, soooo it's gonna stay
    }
}
