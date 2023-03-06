using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine; // Reference required assemblies
using UnityEngine.Rendering.Universal;
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
    //[SerializeField]
    //private TMP_Dropdown qualityDropdown;
    [SerializeField]
    private TMP_Dropdown resolutionsDropdown;
    private Resolution[] resolutions;
    private int resWidth;
    private int resHeight;
    private int refreshRate;
    [SerializeField]
    private TMP_Dropdown windowModeDropdown;
    [SerializeField]
    private Toggle vsyncToggle;
    [SerializeField]
    private Slider maxTextureSizeSlider;
    [SerializeField]
    private Slider renderScaleSlider;
    [SerializeField]
    private TextMeshProUGUI renderScaleText;
    [SerializeField]
    private UniversalRenderPipelineAsset renderPipelineAsset;
    [SerializeField]
    private TextMeshProUGUI maxTextureSizeText;
    //private string[] availableQualitySettings;
    private string windowMode;
    private bool vsync;
    private int maxTextureSize;
    private float renderScale;
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
    [SerializeField]
    private Slider skipSpeedSlider;
    #endregion
    #region Other
    [SerializeField]
    private GameObject HUD;
    [SerializeField]
    private ProfileHandler profileHandler;
    [SerializeField]
    private MidGameMenuHandler midGameMenuHandler;
    private GameObject blocker;
    #endregion
    void Start() // On script start
    {
        profileHandler.DoChecks(); // Ensure that the profile handler has done its checks
        resolutions = Screen.resolutions;
        List<Sprite> temp = new List<Sprite>();
        foreach (Resolution resolution in resolutions)
        {
            temp.Add(null);
            temp.Add(null);
        }
        resolutionsDropdown.AddOptions(temp);
        resHeight = profileHandler.resHeight;
        resWidth = profileHandler.resWidth;
        refreshRate = profileHandler.refreshRate;
        renderScale = profileHandler.renderScale;
        renderScaleSlider.value = profileHandler.renderScale;
        renderPipelineAsset.renderScale = renderScale;
        renderScaleText.text = Convert.ToInt32(renderScale * 100).ToString() + "%";
        for (int i = 0; i < resolutions.Length * 2; i += 2)
        {
            resolutionsDropdown.options[i].text = resolutions[i / 2].width.ToString() + " x " + resolutions[i / 2].height.ToString() + " @ " + resolutions[i / 2].refreshRate;
            resolutionsDropdown.options[i + 1].text = resolutions[i / 2].height.ToString() + " x " + resolutions[i / 2].width.ToString() + " @ " + resolutions[i / 2].refreshRate;
            if (resolutions[i / 2].width == resWidth && resolutions[i / 2].height == resHeight && resolutions[i / 2].refreshRate == refreshRate)
            {
                resolutionsDropdown.value = i;
            }
            else if (resolutions[i / 2].width == resHeight && resolutions[i / 2].height == resWidth && resolutions[i / 2].refreshRate == refreshRate)
            {
                resolutionsDropdown.value = i + 1;
            }
        }
        windowMode = profileHandler.windowedMode;
        vsync = profileHandler.vsync;
        vsyncToggle.isOn = vsync;
        maxTextureSize = profileHandler.maxTextureSize;
        maxTextureSizeText.text = Convert.ToInt32(100 / (maxTextureSize + 1)).ToString() + "%";
        maxTextureSizeSlider.value = maxTextureSize;
        for (int i = 0; i < windowModeDropdown.options.Count; i++)
        {
            if (windowModeDropdown.options[i].text.ToLower() == windowMode.ToLower())
            {
                windowModeDropdown.value = i;
            }
        }
        /*
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
        */
        #region Populate Oyster options
        linesPerFrameText.text = profileHandler.linesPerFrame.ToString(); // Set lines per frame equal to the value of linesperframe stored in profilehandler
        charactersPerFrameText.text = PersistentVariables.charactersPerSecond.ToString();
        skipSpeedSlider.value = PersistentVariables.skipSpeed;
        #endregion
        #region Populate load game options
        string[] profiles = profileHandler.FindProfiles(); // Repeat the same as above to populate the load game options dropdown
        List<Sprite> options = new List<Sprite>();
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
    public void CancelNewGame()
    {
        GameObject.Destroy(blocker); // Destroy the blocker and set the newGamePrompt to be hidden
        blocker = null;
        newGameState = 0;
        newGameInput.text = "";
        newGamePrompt.SetActive(false);
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
                int successState = profileHandler.SaveNewGame(newGameInput.text);
                switch (successState)
                {
                    default:
                        Debug.Log("State '" + successState.ToString() + "' not recognised.");
                        break;
                    case 0: // success
                            // launch into game
                        newGameInput.text = "";
                        newGamePrompt.SetActive(false);
                        GameObject.Destroy(blocker);
                        blocker = null;
                        newGameState = 2;
                        NewGame();
                        break;
                    case 1: // File already exists
                        Debug.Log("This save file already exists!"); // Inform Unity that the save already exists

                        // add code to create an error box and then a blocker

                        break;
                    case 2: // File name contains invalid characters
                        Debug.Log("This save file contains invalid characters"); // Inform Unity that the save file contains invalid characters

                        // add code to create an error box and then a blocker

                        break;
                }
                break;
            case 2: // launch into new game
                mainMenu.SetActive(false);
                initialiseScript.CallScript(script);
                newGameState = 0;
                midGameMenuHandler.disabled = false;
                break;
        }
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
                midGameMenuHandler.disabled = false;
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
    /*
    public void SwitchQualityLevel()
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
    */
    public void SwitchResolution()
    {
        string tempString = resolutionsDropdown.options[resolutionsDropdown.value].text;
        string resolutionString = "";
        foreach (Char chr in tempString)
        {
            if (chr != ' ')
            {
                resolutionString += chr;
            }
        }
        Vector2 resolution = new Vector2();
        int refreshRate = 0;
        int state = 0;
        string temp = "";
        bool failed = false;
        for (int i = 0; i < resolutionString.Length; i++)
        {
            switch (state)
            {
                default:
                    Debug.Log("State " + state + " not recognised.");
                    break;
                case 0: // reading width
                    if (resolutionString[i] == 'x')
                    {
                        //i++;
                        state++;
                        try
                        {
                            resolution.x = Convert.ToInt32(temp);
                        }
                        catch
                        {
                            Debug.Log("Something went wrong! (" + temp + ")");
                            failed = true;
                        }
                        temp = "";
                    }
                    else
                    {
                        temp += resolutionString[i];
                    }
                    break;
                case 1: // reading height
                    if (resolutionString[i] == '@')
                    {
                       // i++;
                        state++;
                        try
                        {
                            resolution.y = Convert.ToInt32(temp);
                        }
                        catch
                        {
                            Debug.Log("Something went wrong! (" + temp + ")");
                            failed = true;
                        }
                        temp = "";
                    }
                    else
                    {
                        temp += resolutionString[i];
                    }
                    break;
                case 2: // reading refresh rate
                    temp += resolutionString[i];
                    break;
            }
        }
        try
        {
            refreshRate = Convert.ToInt32(temp);
        }
        catch
        {
            Debug.Log("Something went wrong! (" + temp + ")");
            failed = true;
        }
        temp = "";
        if (!failed)
        {
            switch (profileHandler.windowedMode.ToLower())
            {
                default:
                    Debug.Log("Window mode not recognised.");
                    break;
                case "borderless window":
                    Screen.SetResolution(Convert.ToInt32(resolution.x), Convert.ToInt32(resolution.y), FullScreenMode.FullScreenWindow, refreshRate);
                    break;
                case "exclusive fullscreen":
                    Screen.SetResolution(Convert.ToInt32(resolution.x), Convert.ToInt32(resolution.y), FullScreenMode.ExclusiveFullScreen, refreshRate);
                    break;
                case "windowed":
                    Screen.SetResolution(Convert.ToInt32(resolution.x), Convert.ToInt32(resolution.y), FullScreenMode.Windowed, refreshRate);
                    break;
            }
            resWidth = Convert.ToInt32(resolution.x);
            resHeight = Convert.ToInt32(resolution.y);
            refreshRate = Convert.ToInt32(refreshRate);
        }
    }
    public void SwitchMaxTextureSize()
    {
        maxTextureSize = Convert.ToInt32(maxTextureSizeSlider.value); // Convert the texture size slider to an integer
        QualitySettings.masterTextureLimit = maxTextureSize; // Set the texturelimit to maxTextureSize
        maxTextureSizeText.text = Convert.ToInt32(100 / (maxTextureSize + 1)).ToString() + "%"; // Calculate the percentage resolution of textures and set the text of maxTextureSizeText to that
    }
    public void SwitchRenderScale()
    {
        renderScale = renderScaleSlider.value;
        renderPipelineAsset.renderScale = renderScale;
        renderScaleText.text = Convert.ToInt32(renderScale * 100).ToString() + "%";
    }
    public void SwitchWindowMode()
    {
        switch (windowModeDropdown.options[windowModeDropdown.value].text.ToLower()) // Pick which window mode is being set
        {
            default: // If none of the below modes match
                Debug.Log("Window mode '" + windowModeDropdown.options[windowModeDropdown.value].text + "' not recognised."); // Inform the Unity console that something went wrong
                break;
            case "borderless window": // If the window mode is borderless window
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow; // Set the window mode to borderless window
                windowMode = "Borderless Window"; // Store a string referencing the current window mode
                break;
            case "exclusive fullscreen": // If the window mode is exclusive fullscreen
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; // Set the window mode to exclusive fullscreen (on some platforms this defaults back to borderless window)
                windowMode = "Exclusive Fullscreen"; // Store a string referencing the current window mode
                break;
            case "windowed": // If the window mode is windowed
                Screen.fullScreenMode = FullScreenMode.Windowed; // Set the window mode to windowed
                windowMode = "Windowed"; // Store a string referencing the current window mode
                break;
        }
        profileHandler.windowedMode = windowMode;
    }
    public void SwitchVsync()
    {
        if (vsyncToggle.isOn) // If the toggle is currently in a ticked state
        {
            vsync = true; // Set the reference to vsync to true
            QualitySettings.vSyncCount = 1; // Set vsync to 1 (might be smart to implement other vsync modes)
        }
        else // If the toggle is currently not in a ticked state
        {
            vsync = false; // Set the reference to vsync to false
            QualitySettings.vSyncCount = 0; // Set vsync to 0
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
    public void ChangeLinesPerFrame()
    {
        try // Try to run the below code
        {
            PersistentVariables.linesPerFrame = Convert.ToInt32(linesPerFrameText.text); // Set linesPerFrame equal to the linesPerFrameText text converted to an integer
        }
        catch // If the above code fails to run
        {
            Debug.Log("Failed to convert " + linesPerFrameText.text + " to an integer."); // Inform the Unity console that something went wrong
        }
    }
    public void ChangeCharactersPerSecond()
    {
        try // Try to run the below code
        {
            PersistentVariables.charactersPerSecond = float.Parse(charactersPerFrameText.text); // Set charactersPerSecond equal to charactersPerFrameText text as a float
        }
        catch // If the above code fails to run
        {
            Debug.Log("Unable to convert " + charactersPerFrameText.text + " to an integer."); // Inform the Unity console that something went wrong
        }
    }
    public void ChangeSkipSpeed()
    {
        PersistentVariables.skipSpeed = skipSpeedSlider.value; // Set the skipSpeed value equal to the value stored within skipSpeedSlider
    }
    #endregion
    #region Back button
    public void ReturnToMenu()
    {
        mainMenu.SetActive(true); // Switches the menu state back to being in the main menu
        options.SetActive(false);
    }
    #endregion
    #region Save button
    public void SaveOptions()
    {
        Options options = new Options();
        //options.qualityLevel = QualitySettings.GetQualityLevel(); // Save every option to an Options object
        options.windowedMode = windowMode;
        options.vsync = vsync;
        options.resWidth = resWidth;
        options.resHeight = resHeight;
        options.refreshRate = refreshRate;
        options.renderScale = renderScale;
        options.volume = AudioListener.volume;
        options.linesPerFrame = Convert.ToInt32(linesPerFrameText.text);
        options.charactersPerSecond = float.Parse(charactersPerFrameText.text);
        options.skipSpeed = skipSpeedSlider.value;
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
