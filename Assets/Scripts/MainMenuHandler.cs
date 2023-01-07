using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine; // Reference required assemblies
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour
{
    // Define variables for each part of each menu
    #region Menu containers
    [SerializeField]
    private GameObject mainMenu;
    [SerializeField]
    private GameObject options;
    #endregion
    #region New Game objects
    [SerializeField]
    private GameObject newGamePrompt;
    [SerializeField]
    private TMP_InputField newGameInput;
    [SerializeField]
    private OysterCharacterScript script;
    [SerializeField]
    private OysterInitialiseScript initialiseScript;
    private int newGameState = 0;
    #endregion // Define a lot of variables
    #region Load Game objects
    [SerializeField]
    private GameObject loadGamePrompt;
    [SerializeField]
    private TMP_Dropdown loadGameDropdown;
    private int loadGameState = 0;
    #endregion
    #region Graphics options objects
    [SerializeField]
    private TMP_Dropdown qualityDropdown;
    private string[] availableQualitySettings;
    #endregion
    #region Sound options objects
    [SerializeField]
    private Slider volumeSlider;
    #endregion
    #region Oyster options
    [SerializeField]
    private TMP_InputField linesPerFrameText;
    [SerializeField]
    private TMP_InputField charactersPerFrameText;
    #endregion
    #region Other
    [SerializeField]
    private GameObject HUD;
    [SerializeField]
    private ProfileHandler profileHandler;
    private GameObject blocker;
    #endregion
    void Start() // On script start
    {
        profileHandler.DoChecks(); // Ensure that the profile handler has done its checks
        #region Populate quality options
        availableQualitySettings = QualitySettings.names; // Find the names of the available quality options
        List<Sprite> options = new List<Sprite>(); // create a new list of sprites
        foreach (string str in availableQualitySettings)
        {
            options.Add(null); // For every available quality option add a null value to the list
        }
        qualityDropdown.AddOptions(options); // Add the previously made list to the quality dropdown options
        for (int i = 0; i < availableQualitySettings.Length; i++) // Loop through each option in the quality dropdown
        {
            qualityDropdown.options[i].text = availableQualitySettings[i]; // Set the text value of the current option equal to the corresponding quality name
        }
        qualityDropdown.value = QualitySettings.GetQualityLevel(); // Set the current value of the dropdown text to the current quality level
        #endregion
        #region Populate Oyster options
        linesPerFrameText.text = profileHandler.linesPerFrame.ToString(); // Set lines per frame equal to the value of linesperframe stored in profilehandler
        charactersPerFrameText.text = PersistentVariables.charactersPerSecond.ToString();
        #endregion
        #region Populate load game options
        string[] profiles = profileHandler.FindProfiles(); // Repeat the same as above to populate the load game options dropdown
        options = new List<Sprite>();
        foreach (string profile in profiles)
        {
            options.Add(null);
        }
        loadGameDropdown.AddOptions(options);
        for (int i = 0; i < profiles.Length; i++)
        {
            loadGameDropdown.options[i].text = profiles[i];
        }
        #endregion
        #region Set volume slider value
        volumeSlider.value = AudioListener.volume; // Set the audio slider value to the current volume
        #endregion
    }
    #region Main menu
    public void ForceNewGame()
    {
        newGameState = 2;
        NewGame();
    }
    public void NewGame()
    {
        switch (newGameState)
        {
            case 0: // show new game prompt
                blocker = CreateBlocker(new Vector2(-650, 0), new Vector2(620, 1080));
                newGamePrompt.SetActive(true);
                newGameState = 1;
                break;
            case 1: // create new save file
                bool success = profileHandler.SaveNewGame(newGameInput.text);
                if (success)
                {
                    // launch into game
                    newGameInput.text = "";
                    newGamePrompt.SetActive(false);
                    GameObject.Destroy(blocker);
                    blocker = null;
                    newGameState = 2;
                    NewGame();
                }
                else
                {
                    Debug.Log("This save file already exists!"); // Inform Unity that the save already exists
                }
                break;
            case 2: // launch into new game
                mainMenu.SetActive(false);
                initialiseScript.CallScript(script);
                newGameState = 0;
                break;
        }
    }
    public void CancelNewGame()
    {
        GameObject.Destroy(blocker); // Destroy the blocker and set the newGamePrompt to be hidden
        blocker = null;
        newGamePrompt.SetActive(false);
        newGameState = 0;
    }
    public void CancelLoadGame()
    {
        GameObject.Destroy(blocker); // Do the same as CancelNewGame but for loadGame related objects
        blocker = null;
        loadGamePrompt.SetActive(false);
        loadGameState = 0;
    }
    public void LoadGame()
    {
        switch (loadGameState)
        {
            case 0: // show load game prompt
                blocker = CreateBlocker(new Vector2(-650, 0), new Vector2(620, 1080));
                loadGamePrompt.SetActive(true);
                loadGameState = 1;
                break;
            case 1: // when load game prompt has been submitted
                mainMenu.SetActive(false);
                GameObject.Destroy(blocker);
                blocker = null;
                loadGameState = 0;
                loadGamePrompt.SetActive(false);
                profileHandler.LoadGame(loadGameDropdown.options[loadGameDropdown.value].text);
                break;
        }
    }
    public void Options()
    {
        // Switch menu to list of options
        mainMenu.SetActive(false);
        options.SetActive(true);
    }
    public void QuitGame()
    {
        Application.Quit(); // Quit the game
    }
    #endregion
    #region Options
    #region Graphics options
    public void SwitchGraphicsOptions()
    {
        string optionName = availableQualitySettings[qualityDropdown.value]; // Find the name of the selected graphics option
        for (int i = 0; i < availableQualitySettings.Length; i++) // Loop through the available graphics options
        {
            if (availableQualitySettings[i] == optionName) // If there is a match
            {
                QualitySettings.SetQualityLevel(i); // Set the current quality level to the value specified
            }
        }
    }
    #endregion
    #region Sound options
    public void SetVolume()
    {
        AudioListener.volume = volumeSlider.value; // Set the volume equal to the value of the slider
    }
    #endregion
    #region Oyster options

    #endregion
    #region Back button
    public void ReturnToMenu()
    {
        mainMenu.SetActive(true);
        options.SetActive(false);
    }
    #endregion
    #region Save button
    public void SaveOptions()
    {
        Options options = new Options();
        options.qualityLevel = QualitySettings.GetQualityLevel(); // Save every option to an Options object
        options.volume = AudioListener.volume;
        options.linesPerFrame = Convert.ToInt32(linesPerFrameText.text);
        options.charactersPerSecond = Convert.ToInt32(charactersPerFrameText.text);
        profileHandler.SaveOptions(options); // Pass options to profilehandler for it to then be saved externally
    }
    #endregion
    #endregion
    #region Misc
    private GameObject CreateBlocker(Vector2 position, Vector2 size)
    {
        GameObject output = new GameObject();
        output.name = "Blocker"; // Create a blocker object and set its parent
        output.transform.parent = HUD.transform;

        RectTransform outputRectTransform = output.AddComponent<RectTransform>();
        Canvas outputCanvas = output.AddComponent<Canvas>();
        Image outputImage = output.AddComponent<Image>(); // Add components to the object
        Button outputButton = output.AddComponent<Button>();
        GraphicRaycaster outputGraphicRayCaster = output.AddComponent<GraphicRaycaster>();

        outputCanvas.overrideSorting = true;
        outputCanvas.sortingOrder = 29999;
        outputRectTransform.localScale = new Vector3(1, 1, 1);
        outputRectTransform.sizeDelta = size; // Set default values and load size and position
        outputRectTransform.anchoredPosition = position;
        outputImage.color = new Color(255, 255, 255, 0);

        return output;
    }
    #endregion
}
