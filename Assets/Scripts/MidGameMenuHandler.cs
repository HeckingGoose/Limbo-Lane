using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MidGameMenuHandler : MonoBehaviour
{
    public bool disabled;
    [SerializeField]
    private GameObject globalEventHandler;
    [SerializeField]
    private GameObject loadGamePrompt;
    [SerializeField]
    private TMP_Dropdown loadGameDropdown;
    private GameObject self;
    private bool menuPressed = false;
    private void Start()
    {
        self = this.transform.GetChild(0).gameObject;
        FetchProfiles();
    }
    private void FetchProfiles()
    {
        #region Mess of code for loading profiles
        string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string[] profiles = Directory.GetFiles(documentsPath + @"\My Games\LimboLane\Profiles"); // Find all files in the profiles directory
        List<string> jsonFiles = new List<string>(); // Create a new list
        foreach (string file in profiles) // Loop through every file
        {
            string fileName = file.Split('\\')[file.Split('\\').Length - 1];
            string[] fileNameParts = fileName.Split('.'); // Split the file into a name and a file extension
            if (fileNameParts[fileNameParts.Length - 1] == "json") // If the extension is json
            {
                jsonFiles.Add(fileNameParts[0]); // Add the file name to the list of files
            }
        }
        profiles = new string[jsonFiles.Count]; // Create a new array of the same length as the list
        for (int i = 0; i < jsonFiles.Count; i++) // Populate the array with the values in the list
        {
            profiles[i] = jsonFiles[i];
        }
        List<Sprite> options = new List<Sprite>();
        foreach (string profile in profiles)
        {
            options.Add(null);
        }
        loadGameDropdown.options = new List<TMP_Dropdown.OptionData>();
        loadGameDropdown.AddOptions(options);
        for (int i = 0; i < profiles.Length; i++)
        {
            loadGameDropdown.options[i].text = profiles[i];
        }
        #endregion
    }
    private void Update()
    {
        if (Input.GetAxis("Menu") > 0 && !menuPressed && !disabled)
        {
            // handle switching menu
            menuPressed = true;
            if (self.activeSelf)
            {
                self.SetActive(false);
                globalEventHandler.SetActive(true);
            }
            else
            {
                self.SetActive(true);
                globalEventHandler.SetActive(false);
                FetchProfiles();
            }
        }
        else if (Input.GetAxis("Menu") == 0)
        {
            menuPressed = false;
        }
    }
    public void Resume()
    {
        self.SetActive(false);
        globalEventHandler.SetActive(true);
    }
    public void LoadGame()
    {
        disabled = true;
        self.SetActive(false);
        loadGamePrompt.SetActive(true);
    }
    public void BeginLoadGame()
    {
        PersistentVariables.profileName = loadGameDropdown.options[loadGameDropdown.value].text;
        ProfileData profile = JsonUtility.FromJson<ProfileData>(File.ReadAllText(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + @"\My Games\LimboLane\Profiles\" + loadGameDropdown.options[loadGameDropdown.value].text + ".json"));
        PersistentVariables.nextSceneName = profile.location;
        SceneManager.LoadScene("LoadingScreen"); // Load the loading scene
    }
    public void CancelLoadGame()
    {
        disabled = false;
        self.SetActive(true);
        loadGamePrompt.SetActive(false);
    }
    public void Return()
    {
        PersistentVariables.profileName = "";
        PersistentVariables.nextSceneName = "AlexHouse";
        SceneManager.LoadScene("LoadingScreen");
    }
}
