using System.Collections.Generic;
using System.IO;
using UnityEngine; // Reference required assemblies
using UnityEngine.SceneManagement;

public class ProfileHandler : MonoBehaviour
{
    [SerializeField]
    private string[] enemies;
    [SerializeField]
    private string[] locations;
    [SerializeField]
    private MainMenuHandler mainMenuHandler;
    private string documentsPath; // Define variables
    public bool checksDone = false;
    public int linesPerFrame = 5;
    private void Start()
    {
        DoChecks(); // Do required checks
    }
    public bool SaveNewGame(string profileName)
    {
        if (File.Exists(documentsPath + @"\My Games\LimboLane\Profiles\" + profileName + ".json")) // Check if the save game exists
        {
            return false; // Return false if the file exists
        }
        else // If the save does not already exist
        {
            ProfileData newProfile = CreateDefaultProfile(profileName); // Create a new default profile
            try // Try to run the below code
            {
                PersistentVariables.profileName = profileName;
                File.WriteAllText(documentsPath + @"\My Games\LimboLane\Profiles\" + profileName + ".json", JsonUtility.ToJson(newProfile)); // Write the data to the specified file
                return true; // Return true
            } 
            catch // If the above code fails to run
            {
                Debug.Log("File name contains invalid characters!"); // Inform the Unity console that the filename contains invalid characters
                return false;
            }
        }
    }
    public void LoadGame(string profileName)
    {
        // Add Try statement to catch when file does not exist (even though file should always exist based on how this is entered)
        ProfileData profileData = JsonUtility.FromJson<ProfileData>(File.ReadAllText(documentsPath + @"\My Games\LimboLane\Profiles\" + profileName + ".json")); // Load the profile data from a file
        PersistentVariables.profileName = profileName;
        switch (profileData.location) // Compare the location to the below cases
        {
            default:
                Debug.Log("Location not recognised.");
                break;
            case "AlexHouse":
                // load and run tutorial from AlexHouse
                mainMenuHandler.ForceNewGame();
                break;
            case "AlyxRoom":
                // load and run game from AlyxHouse
                PersistentVariables.nextSceneName = "AlyxRoom";
                SceneManager.LoadScene("LoadingScreen"); // Load the loading scene
                break;
        }
    }
    public void SaveOptions(Options options)
    {
        File.WriteAllText(documentsPath + @"\My Games\LimboLane\options.json", JsonUtility.ToJson(options)); // Write options to options file
    }
    public void LoadOptions()
    {
        try // Try to run the below code
        {
            Options options = JsonUtility.FromJson<Options>(File.ReadAllText(documentsPath + @"\My Games\LimboLane\options.json")); // Read the options file
            QualitySettings.SetQualityLevel(options.qualityLevel);
            AudioListener.volume = options.volume; // Load required data
            PersistentVariables.charactersPerSecond = options.charactersPerSecond;
            linesPerFrame = options.linesPerFrame;
        }
        catch
        {
            Debug.Log("options.json is not formatted correctly, resetting options.json to default values."); // Inform the Unity console that something went wrong
            File.Delete(documentsPath + @"\My Games\LimboLane\options.json"); // Delete the options file
            DoChecks(); // Run checks to remake the options file
        }
    }
    public void DoChecks()
    {
        documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments); // Define documents path
        PersistentVariables.documentsPath = documentsPath;
        #region Ensure file structure exists
        // Sanity check for if files actually exist
        if (!Directory.Exists(documentsPath + @"\My Games\LimboLane")) // If the main directory does not exist
        {
            // Create directory
            Directory.CreateDirectory(documentsPath + @"\My Games\LimboLane");
            Debug.Log("Directory LimboLane does not exist! Creating directory...");
        }
        if (!Directory.Exists(documentsPath + @"\My Games\LimboLane\Profiles")) // If the profiles directory does not exist
        {
            // Create directory
            Directory.CreateDirectory(documentsPath + @"\My Games\LimboLane\Profiles");
            Debug.Log("Directory LimboLane\\Profiles does not exist! Creating directory...");
        }
        if (!File.Exists(documentsPath + @"\My Games\LimboLane\options.json")) // If the options file does not exist
        {
            // Create file
            Options options = new Options();
            options.qualityLevel = QualitySettings.GetQualityLevel();
            options.volume = AudioListener.volume;
            options.linesPerFrame = 5;
            options.charactersPerSecond = 2;
            File.WriteAllText(documentsPath + @"\My Games\LimboLane\options.json", JsonUtility.ToJson(options));
            Debug.Log("File options.json does not exist! Creating file...");
        }
        #endregion
        LoadOptions(); // Load options from options file
        checksDone = true; // Set checks done to true
    }
    private ProfileData CreateDefaultProfile(string profileName)
    {
        ProfileData profile = new ProfileData();
        profile.name = profileName;
        profile.version = "1.0";
        profile.location = "AlexHouse";
        profile.currency = 0;
        profile.deck = new Deck(); // Defines a bunch of default values for a profile and then returns the created profile
        profile.enemyStates = new ObjectState[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            ObjectState enemyState = new ObjectState();
            enemyState.name = enemies[i];
            enemyState.state = 0;
            profile.enemyStates[i] = enemyState;
        }
        profile.locationStates = new ObjectState[locations.Length];
        for (int i = 0; i < locations.Length; i++)
        {
            ObjectState locationState = new ObjectState();
            locationState.name = locations[i];
            locationState.state = 0;
            profile.locationStates[i] = locationState;
        }
        return profile;
    }
    public string[] FindProfiles()
    {
        string[] files = Directory.GetFiles(documentsPath + @"\My Games\LimboLane\Profiles"); // Find all files in the profiles directory
        List<string> jsonFiles = new List<string>(); // Create a new list
        foreach (string file in files) // Loop through every file
        {
            string fileName = file.Split('\\')[file.Split('\\').Length - 1];
            string[] fileNameParts = fileName.Split('.'); // Split the file into a name and a file extension
            if (fileNameParts[fileNameParts.Length - 1] == "json") // If the extension is json
            {
                jsonFiles.Add(fileNameParts[0]); // Add the file name to the list of files
            }
        }
        files = new string[jsonFiles.Count]; // Create a new array of the same length as the list
        for (int i = 0; i < jsonFiles.Count; i++) // Populate the array with the values in the list
        {
            files[i] = jsonFiles[i];
        }
        return files; // Return the array
    }
}
