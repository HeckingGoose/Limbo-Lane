using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainCardBattleHandler : MonoBehaviour
{
    #region Variables that can be accessed from inspector
    [SerializeField]
    private ProfileHandler profileHandler;
    [SerializeField]
    private SceneStateLoader sceneStateLoader;
    [SerializeField]
    private GameObject cardBattleHUD;
    [SerializeField]
    private GameObject gameEndPrompt;
    [SerializeField]
    private RectTransform HUD;
    [SerializeField] // Set up a lot of variables
    private GameObject cardPrefab;
    [SerializeField]
    private GameObject boardPrefab;
    [SerializeField]
    private GameObject cardInfoPrefab;
    [SerializeField]
    private EnemyScript enemy;
    [SerializeField]
    private GameObject board;
    [SerializeField]
    private BellScript bellScript;
    [SerializeField]
    private Sprite bellSprite;
    [SerializeField]
    private BellScript phischScript;
    [SerializeField]
    private Sprite phischPileSprite;
    [SerializeField]
    private GameObject playerDeckPile;
    [SerializeField]
    private DeckPileScript playerDeckPileScript;
    [SerializeField]
    private Sprite deckPileSprite;
    [SerializeField]
    private Material defaultCardMaterial;
    [SerializeField]
    private Material[] cardMaterials;
    [SerializeField]
    private Sprite defaultPortrait;
    [SerializeField]
    private Sprite[] characterPortraits;
    [SerializeField]
    private Sprite defaultCardPortrait;
    [SerializeField]
    private Sprite[] cardPortraits;
    #endregion
    #region Player Variables
    private Deck playerFullDeck;
    private Deck playerDeck; // Variables related specifically to the player
    private int playerHandSize;
    [HideInInspector]
    public int playerCurrency;
    private int health;
    private int maxHealth;
    private int level;
    private int xp;
    private int levelXp;
    #endregion
    #region Enemy Variables
    #endregion // Practically unused since enemy script contains most of these
    #region Other
    private System.Random random;
    private GameObject[,] boardCards;
    private GameObject mainCamera; // Unsorted variables
    private GameObject playerDeckObject;
    private List<GameObject> playerCardObjects;
    private GameObject selectedCard;
    private CardScript selectedCardScript;
    private EndGamePrompt gameEndPromptScript;
    private bool continueClicked = false;
    private bool gameEndFirstLoop = true;
    private int selectedCardIndex;
    private bool selecting = false;
    private float totalRange = 0.6f;
    private string currentTurn = "Setup";
    private bool inCardBattle = false;
    private bool mouseDown = false;
    private bool drawnPhisch = false;
    private Camera mainCameraCam;
    private bool bellClicked = false;
    private bool showingBellPrompt = false;
    private GameObject bellPrompt;
    private int currentID = 0;
    private bool showingPrompt = false;
    private GameObject createdPrompt = null;
    private RectTransform createdPromptSize;
    private int column = 0;
    private int waitingState = 0;
    private CardScript attackCard;
    private CardScript defendCard;
    private bool showingDeckSprite = false;
    private GameObject deckPrompt = null;
    private bool showingPhischSprite = false;
    private GameObject phischPrompt = null;
    private int bumpStateBy = 0;
    #endregion
    private void Start()
    {
        #region Sorting prompts
        gameEndPrompt.SetActive(false);
        gameEndPromptScript = gameEndPrompt.GetComponent<EndGamePrompt>();
        #endregion
        #region Setting up variables that cannot be setup outside of function
        random = new System.Random();
        boardCards = new GameObject[4, 4]; // Create a new array for boardCards
        mainCamera = Camera.main.gameObject; // Find the main camera gameObject in the scene
        mainCameraCam = Camera.main; // Find the main camera in the scene
        #endregion
    }
    private void Update()
    {
        #region Stay in card battle when in card battle
        if (inCardBattle) // If a card battle is supposed to be happening
        {
            CardBattle(); // Call the CardBattle method
        }
        #endregion
        #region Handle mouse input
        if (Input.GetAxis("PrimaryAction") == 0 && Input.GetAxis("SecondaryAction") == 0) // If there is no mouse input
        {
            mouseDown = false; // Set mouseDown to false
        }
        else // If there is  mouse input
        {
            mouseDown = true; // Set mouseDown to true
        }
        #endregion
    }
    public void StartCardBattle() // Public function for starting a card battle
    {
        #region Place game into card battle
        inCardBattle = true; // States that the script is currently in a card battle
        CardBattle();         // Calls the CardBattle method
        #endregion
    }
    private void CardBattle() // Main function for card battle
    {
        // Set inCardBattle to true
        if (enemy.ready)
        {
            switch (currentTurn.ToLower()) // Pick which turn it is
            {
                default: // If none of the below cases match the current turn
                    Debug.LogWarning("Turn type '" + currentTurn + "' not recognised."); // Inform the Unity console that the current turn does not exist
                    break;
                case "setup": // If the turn is 'setup', A.K.A: setting up the card battle
                    #region Load variables from persistent variables
                    if (PersistentVariables.profileLevel != -1)
                    {
                        level = PersistentVariables.profileLevel;
                    }
                    else
                    {
                        Debug.LogWarning("Unable to load player level! Defaulting to 1.");
                        level = 20;
                    }
                    if (PersistentVariables.profileXp != -1)
                    {
                        xp = PersistentVariables.profileXp;
                    }
                    else
                    {
                        Debug.LogWarning("Unable to load player xp! Defaulting to 0");
                        xp = 0;
                    }
                    if (PersistentVariables.matchStartingHealth != -1) // If persistant variables has this value set up
                    {
                        health = PersistentVariables.matchStartingHealth; // Load the value
                    }
                    else // Otherwise
                    {
                        Debug.LogWarning("Unable to load match starting health! Defaulting to 5"); // Inform the Unity console that default values are being used
                        health = 5; // Set a default value
                    }
                    maxHealth = health; // Set maxHealth to health
                    if (PersistentVariables.matchStartingCurrency != -1) // If persistant variables has this value set up
                    {
                        playerCurrency = PersistentVariables.matchStartingCurrency; // Load the value
                    }
                    else // Otherwise
                    {
                        Debug.LogWarning("Unable to load player starting currency! Defaulting to 5"); // Inform the Unity console that default values are being used
                        playerCurrency = 5; // Set a default value
                    }
                    if (PersistentVariables.handSize != -1) // If persistant variables had this value set up
                    {
                        playerHandSize = PersistentVariables.handSize; // Load the value
                    }
                    else // Otherwise
                    {
                        Debug.LogWarning("Unable to load player hand size! Defaulting to 7."); // Inform the Unity console that default values are being used
                        playerHandSize = 7; // Set a default value
                    }
                    if (PersistentVariables.profileName != "")// If a profile is loaded
                    {
                        // Load the player's cards and store them in their full deck
                        string[] deck = JsonUtility.FromJson<ProfileData>(File.ReadAllText(PersistentVariables.documentsPath + @"\My Games\LimboLane\Profiles\" + PersistentVariables.profileName + ".json")).deck;
                        int deckSize = deck.Length;
                        playerFullDeck = new Deck();
                        playerFullDeck.cards = new Card[deckSize];
                        for (int i = 0; i < deckSize; i++)
                        {
                            playerFullDeck.cards[i] = CreateCardData(deck[i]);
                        }
                    }
                    else // If no profile is loaded - intended for testing
                    {
                        Debug.LogWarning("No profile is currently loaded! Continuing with default deck."); // Inform the Unity console that no profile is loaded
                        playerFullDeck = new Deck(); // Create a new deck to store all of the player's cards
                        playerFullDeck.cards = new Card[10]; // Set the size of the full deck to the number of cards the player has
                        playerFullDeck.cards[0] = CreateCardData("Reaper");
                        playerFullDeck.cards[1] = CreateCardData("Phisch");
                        playerFullDeck.cards[2] = CreateCardData("Reaper");
                        playerFullDeck.cards[3] = CreateCardData("Reaper"); // Populate the full deck
                        playerFullDeck.cards[4] = CreateCardData("Reaper");
                        playerFullDeck.cards[5] = CreateCardData("Reaper");
                        playerFullDeck.cards[6] = CreateCardData("Reaper");
                        playerFullDeck.cards[7] = CreateCardData("Reaper");
                        playerFullDeck.cards[8] = CreateCardData("Reaper");
                        playerFullDeck.cards[9] = CreateCardData("Reaper");
                        // Make code for populating enemy deck
                    }
                    #endregion
                    #region Populate player deck
                    playerDeck = new Deck(); // Create a new deck to store the player's hand
                    playerDeck.cards = new Card[playerHandSize]; // Initialise the deck in playerDeck to the size of the player's hand
                    playerDeck = PopulateDeck(playerFullDeck, playerHandSize, playerDeck); // Call populate deck to fill the player's deck
                    playerDeckPileScript.RePile(playerFullDeck, playerDeckPile); // Call method to generate which cards are free
                    playerDeckPileScript.GeneratePile(playerDeckPile, cardMaterials); // Call method to generate pile object based on array generated above
                    #endregion
                    #region Setup the UI
                    UpdateHealth();
                    UpdateCurrency(playerCurrency, enemy.currency); // Call functions to load objects and values for the UI elements
                    SetPlayerPortrait("Alex");
                    SetEnemyPortrait(enemy.enemyName);
                    #endregion
                    #region Misc
                    cardBattleHUD.SetActive(true); // Enable the card battling HUD
                    currentTurn = "Enemy"; // Set the turn to Player
                    GenerateDeck(); // Generate the player's deck
                    continueClicked = false;
                    levelXp = Convert.ToInt32(((Math.Pow(Convert.ToDouble(level), 1.4) + 25) / 4) + 4);
                    #endregion
                    break;
                case "player": // If the current turn is player
                    #region Do player turn
                    bool hitThing = false; // Create a new bool called hitThing and set it to false
                    RaycastHit hit; // Create a new raycasthit object
                    Ray ray = mainCameraCam.ScreenPointToRay(Input.mousePosition); // Create a new raycast
                    if (Physics.Raycast(ray, out hit)) // If the raycast hits an object
                    {
                        if (hit.transform.tag == "PlayerBoardSpace") // If the object has no children
                        {
                            #region Board has no card on space and card is selected
                            if (hit.transform.childCount == 0 && selecting) // If the object is a board space on the player's side
                            {
                                hitThing = true; // Set hitThing to true
                                selectedCard.transform.position = new Vector3(hit.transform.position.x, hit.transform.position.y + 0.01f, hit.transform.position.z); // Move the player's selected card to the board space they clicked on
                                if (!mouseDown && Input.GetAxis("PrimaryAction") > 0 && playerCurrency >= selectedCardScript.cardData.cost) // If mouseDown is false and the player clicks
                                {
                                    mouseDown = true; // Set mouseDown to true
                                    #region Pick which space the card needs to be added to
                                    switch (hit.transform.name) // Pick which space the player is clicking on
                                    {
                                        default: // If the space is not recognised
                                            Debug.Log("Object '" + hit.transform.name + "' not recognised."); // Inform the Unity console that the space was not recognised
                                            boardCards[0, 0] = selectedCard;
                                            break;
                                        case "00":
                                            boardCards[2, 0] = selectedCard;
                                            break;
                                        case "01":
                                            boardCards[2, 1] = selectedCard; // This part pretty much explains itself
                                            break;
                                        case "02":
                                            boardCards[2, 2] = selectedCard;
                                            break;
                                        case "03":
                                            boardCards[2, 3] = selectedCard;
                                            break;
                                        case "10":
                                            boardCards[3, 0] = selectedCard;
                                            break;
                                        case "11":
                                            boardCards[3, 1] = selectedCard;
                                            break;
                                        case "12":
                                            boardCards[3, 2] = selectedCard;
                                            break;
                                        case "13":
                                            boardCards[3, 3] = selectedCard;
                                            break;
                                    }
                                    #endregion
                                    selectedCard.transform.parent = hit.transform; // Set the card's parent to the board space it is on
                                    playerCurrency -= selectedCardScript.cardData.cost; // Remove the card's cost from the player's currency
                                    UpdateCurrency(playerCurrency, enemy.currency); // update to use enemy currency
                                    #region Set all instances of the card to be in their 'on the board' state
                                    selectedCardScript.cardData.state = 2;
                                    foreach (Card card in playerFullDeck.cards) // Loop through every card in the player's fullDeck
                                    {
                                        if (card.ID == selectedCardScript.cardData.ID) // If the card ID matches the selected card's ID
                                        {
                                            card.state = 2; // Set the card's state to 2
                                        }
                                    }
                                    #endregion
                                    #region Remove all references to a selected card
                                    selectedCard = null; // Remove the reference to the card
                                    selectedCardScript = null;
                                    selecting = false; // Set selecting to false
                                    playerDeck.cards[selectedCardIndex] = null; // Remove reference to card in playerDeck
                                    playerCardObjects[selectedCardIndex] = null;
                                    selectedCardIndex = -1;
                                    #endregion
                                    #region Remove all null values from the player's deck and resize it
                                    int i = 0; // Define i and set it to 0
                                    foreach (Card card in playerDeck.cards) // Loop through every card in the player's deck
                                    {
                                        if (card != null) // If the card does exist
                                        {
                                            i++; // Add 1 to i
                                        }
                                    }
                                    Card[] temp = new Card[i]; // Create temp and set it equal to the length of cards that did exist in the player's deck
                                    int k = 0; // Define k and set it to 0
                                    for (i = 0; i < playerDeck.cards.Length; i++) // Loop through every index of playerDeck.cards
                                    {
                                        if (playerDeck.cards[i] != null) // If the card exists
                                        {
                                            temp[k] = playerDeck.cards[i]; // Set the kth index of temp equal to playerDeck.cards[i]
                                            k++; // Add 1 to k
                                        }
                                    }
                                    playerDeck.cards = temp; // Set the player's deck equal to temp
                                    #endregion
                                    GenerateDeck(); // Generate the player's deck again
                                }
                            }
                            #endregion
                            #region Board has card on space and card is selected
                            else if (hit.transform.childCount == 1 && selecting) // If the object has a singular child and a card is being selected
                            {
                                selectedCardScript.Select(); // Move the selected card back to its selecting position
                            }
                            #endregion
                            #region Card is potentially being slapped
                            else if (hit.transform.childCount == 1) // If the object has a child
                            {
                                hit.transform.GetChild(0).GetComponent<CardScript>().HandleSlapSprite(true); // Running this pretty much once per frame isn't great
                                if (Input.GetAxis("SecondaryAction") > 0 && !mouseDown) // If the mouse is clicked
                                {
                                    selectedCard = hit.transform.GetChild(0).gameObject; // Set selected card to the card that was clicked
                                    selectedCardScript = selectedCard.GetComponent<CardScript>(); // Set selected card script equal to the script on that card
                                    mouseDown = true; // Set mouseDown to true
                                    try // Try to run the below code
                                    {
                                        playerCurrency += selectedCardScript.cardData.cost; // Increase player currency by the card cost
                                        foreach (Card card in playerFullDeck.cards) // Loop through every card in the player's deck
                                        {
                                            if (card != null && card.ID == selectedCardScript.cardData.ID) // If the card ID matches that of the selected card
                                            {
                                                card.state = 3; // Set the card state to 3
                                            }
                                        }
                                        GameObject.Destroy(selectedCard); // Desctroy the card object
                                        selectedCard = null;
                                        selectedCardScript = null; // Reset all references to the selected card
                                        selectedCardIndex = -1;
                                        UpdateCurrency(playerCurrency, enemy.currency); // Update the player and enemy currency icons
                                    }
                                    catch // If the above code fails to run
                                    {
                                        Debug.Log("Unable to slap card."); // Inform the Unity console that something went wrong
                                    }
                                }
                            #endregion
                            }
                        }
                        else // If the raycast does not hit an object
                        {
                            try
                            {
                                selectedCardScript.Select(); // Move the selected card back to its selecting position
                            }
                            catch { }
                        }
                    }
                    else // If the raycast does not hit an object
                    {
                        try
                        {
                            selectedCardScript.Select(); // Move the selected card back to its selecting position
                        }
                        catch { }
                    }
                    #region Selecting a card
                    if (!mouseDown && Input.GetAxis("PrimaryAction") > 0 && !hitThing) // If the player clicks the mouse, mouseDown is false and the raycast did not hit a valid board space
                    {
                        selecting = false; // Set selecting to false
                        selectedCard = null; // Set selectedCard to null
                        selectedCardScript = null; // Set selectedCardScript to null
                        mouseDown = true; // Set mouseDown to true
                        if (playerCardObjects.Count != 0)
                        {
                            for (int i = 0; i < playerCardObjects.Count; i++) // Loop through every card in the player's deck
                            {
                                try // Try to run the below code
                                {
                                    CardScript script = playerCardObjects[i].GetComponent<CardScript>(); // Find the cardScript component of the card
                                    script.Unselect(); // Unselect the card
                                    if (script.mouseIsOver) // If the mouse is over the card
                                    {
                                        // move card into placing state
                                        script.Select(); // Set the card to its selected state
                                        selecting = true; // Set selecting to true
                                        selectedCard = playerCardObjects[i]; // Set selectedCard equal to the current card
                                        selectedCardIndex = i; // Set selectedCardIndex to the current index
                                        selectedCardScript = script; // Set the selectedCardScript to script
                                    }
                                }
                                catch // If the above code fails to run
                                {
                                    Debug.Log("Leaked card at index " + i); // Inform the Unity console that something has gone wrong
                                }
                            }
                        }
                        #region Handle if player full deck is clicked on
                        if (playerDeckPileScript.mouseOver == true) // If the mouse is over the player's deck pile
                        {
                            //
                            if (playerDeck.cards.Length < playerHandSize) // If the player's deck is not more than or equal to their hand size
                            {
                                Card newCard = playerDeckPileScript.DrawCard(playerFullDeck, playerDeckPile);
                                playerDeckPileScript.RePile(playerFullDeck, playerDeckPile); // Draw a card and then repile
                                playerDeckPileScript.GeneratePile(playerDeckPile, cardMaterials);
                                if (newCard != null) // If the card was drawn correctly
                                {
                                    Deck temp = new Deck();
                                    temp.cards = new Card[playerDeck.cards.Length + 1]; // Remake the player's deck to be +1 in length
                                    for (int i = 0; i < playerDeck.cards.Length; i++)
                                    {
                                        temp.cards[i] = playerDeck.cards[i];
                                    }
                                    playerDeck = temp;
                                    playerDeck.cards[playerDeck.cards.Length - 1] = newCard; // Add the new card to the player's deck
                                    GenerateDeck(); // Re-generate the player's deck object
                                }
                                else // Otherwise
                                {
                                    Debug.Log("No cards to draw"); // Inform the Unity console that there are no cards left
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                    #region Showing deck pile prompt
                    if (playerDeckPileScript.mouseOver)
                    {
                        Vector2 mousePosition = ConvertMouseToCanvasUnits(HUD);
                        switch (showingDeckSprite)
                        {
                            case false: // first loop of bell prompt needing to be shown
                                deckPrompt = CreatePrompt("DeckPrompt", deckPileSprite, new Vector2(256, 128), mousePosition, HUD);
                                showingDeckSprite = true;
                                break;
                            case true: // every other loop of the bell prompt needing to be shown
                                Vector2 offset = deckPrompt.GetComponent<RectTransform>().sizeDelta / 2;
                                deckPrompt.transform.localPosition = mousePosition - offset;
                                break;
                        }
                    }
                    else
                    {
                        showingDeckSprite = false;
                        GameObject.Destroy(deckPrompt);
                        deckPrompt = null;
                    }
                    #endregion
                    #region Showing Phisch pile prompt
                    if (phischScript.mouseOver)
                    {
                        Vector2 mousePosition = ConvertMouseToCanvasUnits(HUD);
                        switch (showingPhischSprite)
                        {
                            case false: // first loop of bell prompt needing to be shown
                                phischPrompt = CreatePrompt("PhischPrompt", phischPileSprite, new Vector2(256, 128), mousePosition, HUD);
                                showingPhischSprite = true;
                                break;
                            case true: // every other loop of the bell prompt needing to be shown
                                Vector2 offset = phischPrompt.GetComponent<RectTransform>().sizeDelta / 2;
                                phischPrompt.transform.localPosition = mousePosition - offset;
                                break;
                        }
                    }
                    else
                    {
                        showingPhischSprite = false;
                        GameObject.Destroy(phischPrompt);
                        phischPrompt = null;
                    }
                    #endregion
                    #region Handle if the phisch pile is clicked on
                    if (phischScript.bellClicked) // If the phisch pile is clicked
                    {
                        if (!drawnPhisch && Input.GetAxis("PrimaryAction") > 0) // If a phisch hasn't been drawn and the mouse is held
                        {
                            Deck temp = playerDeck; // Create a new temporary deck from the player's deck
                            playerDeck = new Deck(); // Reset the player's deck to a new deck
                            playerDeck.cards = new Card[temp.cards.Length + 1]; // Create a new array the size of temp + 1
                            for (int i = 0; i < temp.cards.Length; i++) // Loop through every card in temp
                            {
                                playerDeck.cards[i] = temp.cards[i]; // Add the card to the player's deck
                            }
                            playerDeck.cards[playerDeck.cards.Length - 1] = CreateCardData("Phisch"); // Add a phisch to the player's last deck index
                            drawnPhisch = true; // Set drawnPhisch to true
                            GenerateDeck(); // Re-generate the player's deck
                        }
                    }
                    #endregion
                    #region Handle if the bell is clicked on
                    if (bellScript.bellClicked && !bellClicked) // If the bell is clicked
                    {
                        // play a ding sound here
                        currentTurn = "ComputeTurn"; // Switch to the enemy's turn
                        drawnPhisch = false; // Set drawnPhisch to false
                        bellClicked = true;
                    }
                    if (bellScript.mouseOver)
                    {
                        Vector2 mousePosition = ConvertMouseToCanvasUnits(HUD);
                        switch (showingBellPrompt)
                        {
                            case false: // first loop of bell prompt needing to be shown
                                bellPrompt = CreatePrompt("BellPrompt", bellSprite, new Vector2 (256, 128), mousePosition, HUD);
                                showingBellPrompt = true;
                                break;
                            case true: // every other loop of the bell prompt needing to be shown
                                Vector2 offset = bellPrompt.GetComponent<RectTransform>().sizeDelta / 2;
                                bellPrompt.transform.localPosition = mousePosition - offset;
                                break;
                        }
                    }
                    else // when mouse is no longer over bell prompt
                    {
                        if (showingBellPrompt)
                        {
                            showingBellPrompt = false;
                            GameObject.Destroy(bellPrompt);
                            bellPrompt = null;
                        }
                    }
                    #endregion
                    #endregion
                    break;
                case "enemy": // If the current turn is enemy
                    #region Call enemy function and move to next turn
                    boardCards = enemy.ComputeTurn(boardCards);// pass script onto enemy script for turn
                    bellClicked = false; // Set bellClicked to false
                    currentTurn = "Player"; // Switch the turn to computeTurn
                    #endregion
                    break;
                case "computeturn":
                    #region Do turn
                    showingBellPrompt = false;
                    showingDeckSprite = false;
                    showingPhischSprite = false;
                    GameObject.Destroy(bellPrompt);
                    GameObject.Destroy(deckPrompt);
                    GameObject.Destroy(phischPrompt);
                    switch (waitingState) // Pick which state of animation the function is currently in
                    {
                        default: // If the state is not recognised
                            Debug.Log("Waiting state not recognised! Defaulting to 0."); // Inform the Unity console that something went wrong
                            waitingState = 0; // Set waitingState to 0
                            break;
                        case 0: // If card attacks need to be calculated
                            #region Calculate card attacks through black magic
                            //playerDeck = PopulateDeck(playerFullDeck, playerHandSize, playerDeck); // Populate the player deck <- figure out why this doesn't work
                            foreach (Card card in playerFullDeck.cards)
                            {
                                if (card.state == 3)
                                {
                                    card.state = 0;
                                }
                            }
                            playerDeckPileScript.RePile(playerFullDeck, playerDeckPile);
                            playerDeckPileScript.GeneratePile(playerDeckPile, cardMaterials);
                            GameObject playerFrontCard = null;
                            GameObject playerBackCard = null; // Setup some variables for later use
                            GameObject enemyFrontCard = null;
                            GameObject enemyBackCard = null;
                            int[] columnState = new int[4] { 0, 0, 0, 0 };
                            for (int row = 0; row < 4; row++) // Loop through every row
                            {
                                switch (row) // Switch between row
                                {
                                    default: // If none of the below cases match
                                        Debug.Log("Board space (" + row + "," + column + ") not recognised."); // Inform the Unity console that something went wrong
                                        break;
                                    case 0: // enemy back space
                                        if (boardCards[row, column] != null) // Check the space isn't empty
                                        {
                                            enemyBackCard = boardCards[row, column];
                                            //Debug.Log("Found enemy back card '" + enemyBackCard.name + ":" + enemyBackCard.ID + "' on space (" + (row - (column * 4)) + "," + column + ").");
                                            columnState[0] = 1;
                                        }
                                        else // Otherwise
                                        {
                                            enemyBackCard = null;
                                            columnState[0] = 0; // Set card to null
                                        }
                                        break;
                                    case 1: // enemy front space
                                        if (boardCards[row, column] != null) // Check the space isn't empty
                                        {
                                            enemyFrontCard = boardCards[row, column];
                                            //Debug.Log("Found enemy front card '" + enemyFrontCard.name + ":" + enemyFrontCard.ID + "' on space (" + (row - (column * 4)) + "," + column + ").");
                                            columnState[1] = 1;
                                        }
                                        else // Otherwise
                                        {
                                            enemyFrontCard = null;
                                            columnState[1] = 0; // Set card to null
                                        }
                                        break;
                                    case 2: // player front space
                                        if (boardCards[row, column] != null) // Check the space isn't empty
                                        {
                                            playerFrontCard = boardCards[row, column];
                                            //Debug.Log("Found player front card '" + playerFrontCard.name + ":" + playerFrontCard.ID + "' on space (" + (row - (column * 4)) + "," + column + ").");
                                            columnState[2] = 1;
                                        }
                                        else // Otherwise
                                        {
                                            playerFrontCard = null;
                                            columnState[2] = 0; // Set card to null
                                        }
                                        break;
                                    case 3: // player back space
                                        if (boardCards[row, column] != null) // Check the space isn't empty
                                        {
                                            playerBackCard = boardCards[row, column];
                                            //Debug.Log("Found player back card '" + playerBackCard.name + ":" + playerBackCard.ID + "' on space (" + (row - (column * 4)) + "," + column + ").");
                                            columnState[3] = 1;
                                        }
                                        else // Otherwise
                                        {
                                            playerBackCard = null;
                                            columnState[3] = 0; // Set card to null
                                        }
                                        break;
                                }
                            }
                            for (int i = 0; i < 4; i++)
                            {
                                if (boardCards[i, column] != null)
                                {
                                    CardScript cardScript = boardCards[i, column].GetComponent<CardScript>();
                                    if (cardScript.cardData.name.ToLower() == "phisch")
                                    {
                                        cardScript.cardData.cost++;
                                        cardScript.cardData.maxCost = cardScript.cardData.cost; // This doesn't need any health calculations as fish max hp is 1
                                        cardScript.UpdateCost();
                                    }
                                }
                            }
                            (attackCard, defendCard) = CalculateAttack(column, columnState, new GameObject[4] { enemyBackCard, enemyFrontCard, playerFrontCard, playerBackCard }); // Calculate attack again
                            GenerateDeck(); // Re-generate the deck
                            waitingState = 1;
                            #endregion
                            break;
                        case 1: // Handle card attacking animation
                            if (attackCard == null) // If there is no attacking card
                            {
                                waitingState = 2; // Move to the next state
                            }
                            else // Otherwise
                            {
                                if (attackCard.cardData.attack > 0) // If the card has an attack value
                                {
                                    waitingState = attackCard.DoAttack(2); // Run the attack animation until it finishes, and then increment waitingState
                                }
                                else // Otherwise
                                {
                                    waitingState = 2; // Move to the next state
                                }
                            }
                            break;
                        case 2: // Handle card attacking animation for defending card
                            if (defendCard == null) // If there is no defending card
                            {
                                waitingState = 3; // Move to the next state
                            }
                            else // Otherwise
                            {
                                if (defendCard.cardData.attack > 0) // If the defending card has an attack value
                                {
                                    waitingState = defendCard.DoAttack(3); // Run the attack animation until it finishes, and then increment waitingState
                                }
                                else // Otherwise
                                {
                                    waitingState = 3; // Move to the next state
                                }
                            }
                            break;
                        case 3: // Animating is done
                            // this is where the card battle ends
                            if (enemy.health <= 0) // If the enemy runs out of health
                            { // Player wins
                                currentTurn = "PlayerWin"; // Switch turn to playerWin
                                bumpStateBy = 2;
                            }
                            else if (health <= 0) // If the player health runs out of health
                            { // Enemy wins
                                currentTurn = "EnemyWin"; // Switch turn to enemyTurn
                                bumpStateBy = 1;
                            }
                            else
                            {
                                waitingState = 0; // Move back to the first state
                                if (column >= 3)
                                {
                                    currentTurn = "Enemy"; // Set turn to enemy
                                    column = 0;
                                }
                                else
                                {
                                    column++;
                                }
                            }
                            break;
                    }
                    #endregion
                    break;
                case "playerwin":
                    #region Show player winning screen
                    if (gameEndFirstLoop)
                    {
                        #region Calculate Xp gain
                        float multiplier = -1;
                        switch (enemy.type)
                        {
                            default: // basic enemy
                                multiplier = 1;
                                break;
                            case "boss":
                                multiplier = 2;
                                break;
                        }
                        int xpGain = Convert.ToInt32((levelXp / 10) * multiplier + 1);
                        #endregion
                        (int, string, string)[] rewards = new (int, string, string)[enemy.rewards.Length + 1];
                        rewards[0] = (xpGain, "XP", "XP");
                        for (int i = 0; i < enemy.rewards.Length; i++)
                        {
                            rewards[i + 1] = enemy.rewards[i];
                        }
                        xp += xpGain; // Handle leveling up here
                        #region Handle showing the prompt
                        gameEndPrompt.SetActive(true);
                        gameEndFirstLoop = false;
                        gameEndPromptScript.SetWinText(true);
                        gameEndPromptScript.SetRewards(rewards);
                        gameEndPromptScript.SetXpBar(xp, levelXp);
                        continueClicked = false;
                        #endregion
                        #region Update player profile
                        if (profileHandler != null && PersistentVariables.profileName != "")
                        {
                            ProfileData saveGame = profileHandler.FetchProfileData(PersistentVariables.profileName);
                            saveGame.level = level;
                            for (int i = 0; i < saveGame.locationStates.Length; i++)
                            {
                                if (saveGame.locationStates[i].name == saveGame.location)
                                {
                                    saveGame.locationStates[i].state += bumpStateBy;
                                }
                            }
                            foreach ((int, string, string) reward in rewards)
                            {
                                switch (reward.Item3.ToLower())
                                {
                                    default:
                                        Debug.LogWarning("Reward not recognised! (Amount: " + reward.Item1 +", Name: " + reward.Item2 + ", Type: " + reward.Item3 + ")");
                                        break;
                                    case "card":
                                        string[] temp = saveGame.deck;
                                        saveGame.deck = new string[temp.Length + reward.Item1];
                                        for (int i = 0; i < saveGame.deck.Length; i++)
                                        {
                                            if (i < temp.Length)
                                            {
                                                saveGame.deck[i] = temp[i];
                                            }
                                            else
                                            {
                                                saveGame.deck[i] = reward.Item2;
                                            }
                                        }
                                        break;
                                    case "currency":
                                        saveGame.currency += reward.Item1;
                                        break;
                                    case "xp":
                                        saveGame.xp += reward.Item1;
                                        break;
                                }
                            }
                            profileHandler.SaveGame(saveGame);
                            profileHandler.SetupPersistentVariables(saveGame, profileHandler.FetchOptions(), profileHandler.FetchDocumentsPath());
                        }
                        else
                        {
                            Debug.LogWarning("Missing reference to profileHandler.cs! Skipping updating profile.");
                        }
                        #endregion
                    }
                    if (continueClicked)
                    {
                        gameEndPrompt.SetActive(false);
                        currentTurn = "EndGame";
                        gameEndFirstLoop = true;
                    }
                    #endregion
                    break;
                case "enemywin":
                    #region Show enemy winning screen
                    if (gameEndFirstLoop)
                    {
                        #region Calculate Xp gain
                        float multiplier = -1;
                        switch (enemy.type)
                        {
                            default: // basic enemy
                                multiplier = 1;
                                break;
                            case "boss":
                                multiplier = 2;
                                break;
                        }
                        #endregion
                        int xpGain = Convert.ToInt32((levelXp / 10) * multiplier + 1);
                        xp += xpGain;
                        (int, string, string)[] rewards = new (int, string, string)[1] { (xpGain, "XP", "XP") };
                        #region Handle showing the prompt
                        gameEndPrompt.SetActive(true);
                        gameEndFirstLoop = false;
                        gameEndPromptScript.SetWinText(false);
                        gameEndPromptScript.SetRewards(rewards);
                        gameEndPromptScript.SetXpBar(xp, levelXp);
                        continueClicked = false;
                        #endregion
                        #region Update player profile
                        if (profileHandler != null && PersistentVariables.profileName != "")
                        {
                            ProfileData saveGame = profileHandler.FetchProfileData(PersistentVariables.profileName);
                            saveGame.level = level;
                            for (int i = 0; i < saveGame.locationStates.Length; i++)
                            {
                                if (saveGame.locationStates[i].name == saveGame.location)
                                {
                                    saveGame.locationStates[i].state += bumpStateBy;
                                }
                            }
                            foreach ((int, string, string) reward in rewards)
                            {
                                switch (reward.Item3.ToLower())
                                {
                                    default:
                                        Debug.Log("Reward not recognised!");
                                        break;
                                    case "card":
                                        string[] temp = saveGame.deck;
                                        saveGame.deck = new string[temp.Length + reward.Item1];
                                        for (int i = 0; i < saveGame.deck.Length; i++)
                                        {
                                            if (i < temp.Length)
                                            {
                                                saveGame.deck[i] = temp[i];
                                            }
                                            else
                                            {
                                                saveGame.deck[i] = reward.Item2;
                                            }
                                        }
                                        break;
                                    case "currency":
                                        saveGame.currency += reward.Item1;
                                        break;
                                    case "xp":
                                        saveGame.xp += reward.Item1;
                                        break;
                                }
                            }
                            profileHandler.SaveGame(saveGame);
                            profileHandler.SetupPersistentVariables(saveGame, profileHandler.FetchOptions(), profileHandler.FetchDocumentsPath());
                        }
                        else
                        {
                            Debug.LogWarning("Missing reference to profileHandler.cs! Skipping updating profile.");
                        }
                        #endregion
                    }
                    if (continueClicked)
                    {
                        gameEndPrompt.SetActive(false);
                        currentTurn = "EndGame";
                        gameEndFirstLoop = true;
                    }
                    #endregion
                    break;
                case "endgame":
                    inCardBattle = false; // Set inCardBattle to false
                    cardBattleHUD.SetActive(false); // Hide and remove any card battling objects
                    GameObject.Destroy(playerDeckObject);
                    // remove cards from the board here
                    try // Try to run the below code
                    {
                        // v Load profile v
                        ProfileData profile = JsonUtility.FromJson<ProfileData>(File.ReadAllText(PersistentVariables.documentsPath + @"\My Games\LimboLane\Profiles\" + PersistentVariables.profileName + ".json"));
                        foreach (ObjectState location in profile.locationStates) // Loop through every location
                        {
                            if (location.name == profile.location) // If the location name matches the profile location
                            {
                                location.state += bumpStateBy; // Bump locationState by 1
                            }
                        }
                    }
                    catch // If the above code fails to run
                    {
                        Debug.LogWarning("Unable to bump scene state!"); // Inform the Unity console that something went wrong
                    }
                    Vector3 position = board.transform.position;
                    Quaternion rotation = board.transform.rotation; // Store the position data for the board
                    Vector3 scale = board.transform.localScale;
                    GameObject.Destroy(board); // Destroy the board
                    board = Instantiate(boardPrefab, position, rotation); // Create a new instance of the board <- this needs changing
                    board.transform.localScale = scale;
                    try // Try to run the below code
                    {
                        sceneStateLoader.locationState += bumpStateBy;
                        sceneStateLoader.Run(); // Load the new scene state
                    }
                    catch // If the above code fails to run
                    {
                        Debug.LogWarning("Unable to continue since sceneStateLoader does not exist!"); // Inform the Unity console that something went wrong
                    }
                    currentTurn = "Setup"; // Set the currentTurn to setup
                    break;
            }
            if (currentTurn.ToLower() == "player" || currentTurn.ToLower() == "enemy" || currentTurn.ToLower() == "computeturn")
            {
                if (Input.mousePosition.x >= 0 && Input.mousePosition.y >= 0 && Input.mousePosition.x <= Screen.width && Input.mousePosition.y <= Screen.height) // check if mouse is actually in game window
                {
                    RaycastHit hitCard; // Create a new raycasthit object
                    Ray cardRay = mainCameraCam.ScreenPointToRay(Input.mousePosition); // Create a new raycast
                    if (Physics.Raycast(cardRay, out hitCard))
                    {
                        ShowCardInfo(hitCard); // figure out why this doesn't work properly
                    }
                    else
                    {
                        HideCardInfo();
                    }
                }
            }
        }
    }
    public void ContinueClicked()
    {
        continueClicked = true;
    }
    private GameObject CreatePrompt(string name, Sprite image, Vector2 size, Vector2 position, Transform parent)
    {
        GameObject output = new GameObject();
        output.AddComponent<CanvasRenderer>();
        Image outputImage = output.AddComponent<Image>();
        output.transform.SetParent(parent); // This block of code simply creates a new canvas sprite and parents it to the given canvas, it then returns that sprite
        outputImage.rectTransform.sizeDelta = size;
        outputImage.sprite = image;
        output.name = name;
        output.transform.localPosition = position;
        output.transform.localScale = new Vector3(1, 1, 1);
        return output;
    }
    private void ShowCardInfo(RaycastHit card) // !Function still not finished
    {
        if (card.transform.tag.ToLower() == "enemyboardspace" || card.transform.tag.ToLower() == "playerboardspace")
        {
            try
            {
                GameObject cardObject = card.transform.GetChild(0).gameObject;
                CardScript cardScript = cardObject.transform.GetComponent<CardScript>();
                if (cardScript.cardData.state == 2)
                {
                    // If the mouse is hovering over any card
                    Vector2 promptPosition = ConvertMouseToCanvasUnits(HUD);
                    switch (showingPrompt) // this code needs fixing, for some reason mouse position is not relative to game window
                    {
                        case true:
                            
                            if (Input.mousePosition.x > (Screen.width / 2))// mouse is on right side of screen
                            {
                                if (Input.mousePosition.y > (Screen.height / 2)) // mouse is at top of screen
                                {
                                    promptPosition = new Vector2(promptPosition.x - (createdPromptSize.rect.width / 2) - 1, promptPosition.y - (createdPromptSize.rect.height / 2) - 1);
                                }
                                else // mouse is at bottom of screen
                                {
                                    promptPosition = new Vector2(promptPosition.x - (createdPromptSize.rect.width / 2) - 1, promptPosition.y + (createdPromptSize.rect.height / 2) + 1);
                                }
                            }
                            else // mouse is on left side of screen
                            {
                                if (Input.mousePosition.y > (Screen.height / 2)) // mouse is at top of screen
                                {
                                    promptPosition = new Vector2(promptPosition.x + (createdPromptSize.rect.width / 2) + 1, promptPosition.y - (createdPromptSize.rect.height / 2) - 1);
                                }
                                else // mouse is at bottom of screen
                                {
                                    promptPosition = new Vector2(promptPosition.x + (createdPromptSize.rect.width / 2) + 1, promptPosition.y + (createdPromptSize.rect.height / 2) + 1);
                                }
                            }
                            
                            createdPrompt.transform.localPosition = promptPosition; // Set the prompt position
                            createdPrompt.transform.localScale = new Vector3(1, 1, 1); // Reset the scale since Unity sometimes does wacky stuff with scale for unknown reasons
                            break;
                        case false:
                            showingPrompt = true; // Set showingprompt to true
                            createdPrompt = Instantiate(cardInfoPrefab, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0, 0, 0))); // Create a new prompt
                            createdPromptSize = createdPrompt.GetComponent<RectTransform>();
                            createdPrompt.transform.SetParent(cardBattleHUD.transform); // Parent the prompt accordingly
                            createdPrompt.transform.localScale = new Vector3(1, 1, 1); // V Load card details into prompt V
                            createdPrompt.transform.Find("CardTitleDetails").Find("CardName").GetComponent<TextMeshProUGUI>().text = cardScript.cardData.name.ToString();
                            createdPrompt.transform.Find("CardTitleDetails").Find("Health").Find("HealthText").GetComponent<TextMeshProUGUI>().text = cardScript.cardData.health.ToString() + "/" + cardScript.cardData.maxHealth.ToString();
                            createdPrompt.transform.Find("CardTitleDetails").Find("Attack").Find("AttackText").GetComponent<TextMeshProUGUI>().text = cardScript.cardData.attack.ToString();
                            createdPrompt.transform.Find("CardTitleDetails").Find("Cost").Find("CostText").GetComponent<TextMeshProUGUI>().text = cardScript.cardData.cost.ToString() + "/" + cardScript.cardData.maxCost.ToString();
                            createdPrompt.transform.Find("CardAbilityDescription").Find("AbilityText").GetComponent<TextMeshProUGUI>().text = cardScript.cardData.description.ToString();
                            bool found = false;
                            foreach (Sprite portrait in cardPortraits) // Loop through every portrait sprite
                            {
                                if (portrait.name == cardScript.cardData.name) // If the name of the card matches the portrait name
                                {
                                    createdPrompt.transform.Find("CardSprite").GetComponent<Image>().sprite = portrait; // Set the card prompt portrait to the found portrait
                                    found = true; // Set found to true
                                }
                            }
                            if (!found) // If a portrait was not found
                            {
                                createdPrompt.transform.Find("CardSprite").GetComponent<Image>().sprite = defaultCardPortrait; // Use a default portrait
                            }
                            
                            if (Input.mousePosition.x > (Screen.width / 2))// mouse is on right side of screen
                            {
                                if (Input.mousePosition.y > (Screen.height / 2)) // mouse is at top of screen
                                {
                                    promptPosition = new Vector2(promptPosition.x - (createdPromptSize.rect.width / 2) - 1, promptPosition.y - (createdPromptSize.rect.height / 2) - 1);
                                }
                                else // mouse is at bottom of screen
                                {
                                    promptPosition = new Vector2(promptPosition.x - (createdPromptSize.rect.width / 2) - 1, promptPosition.y + (createdPromptSize.rect.height / 2) + 1);
                                }
                            }
                            else // mouse is on left side of screen
                            {
                                if (Input.mousePosition.y > (Screen.height / 2)) // mouse is at top of screen
                                {
                                    promptPosition = new Vector2(promptPosition.x + (createdPromptSize.rect.width / 2) + 1, promptPosition.y - (createdPromptSize.rect.height / 2) - 1);
                                }
                                else // mouse is at bottom of screen
                                {
                                    promptPosition = new Vector2(promptPosition.x + (createdPromptSize.rect.width / 2) + 1, promptPosition.y + (createdPromptSize.rect.height / 2) + 1);
                                }
                            }
                            
                            createdPrompt.transform.localPosition = promptPosition; // Set prompt to correct position
                            createdPrompt.transform.localScale = new Vector3(1, 1, 1); // Re-scale prompt to prevent Unity shenanigans
                            break;
                    }
                }
            }
            catch // If the above code fails to run
            {
                Debug.LogWarning("Unable to find card!"); // Inform the Unity console that something went wrong
            }
        }
    }
    private Vector2 ConvertMouseToCanvasUnits(RectTransform canvas)
    {
        Vector2 offset = new Vector2(canvas.rect.width / 2, canvas.rect.height / 2); // Calculate (in canvas units) the offset from the mouse origin to the canvas origin
        Vector2 mouseToCanvasScale = new Vector2(canvas.rect.width / Screen.width, canvas.rect.height / Screen.height); // Calculate a ratio between the canvas size and screen size
        Vector2 output = new Vector2(Input.mousePosition.x * mouseToCanvasScale.x - offset.x, Input.mousePosition.y * mouseToCanvasScale.y - offset.y); // Combine mouse position, scale and offset to calculate mouse position relative to canvas
        return output; // Return calculated position
    }
    private void HideCardInfo() // Function for removing card info prompt
    {
        if (showingPrompt) // If there is a prompt
        {
            try // Try to run the below code
            {
                GameObject.Destroy(createdPrompt); // Destroy the prompt
                showingPrompt = false; // Set all relevant variables to default values
                createdPrompt = null;
                createdPromptSize = new RectTransform();
            }
            catch // If the above code fails to run
            {
                Debug.Log("Unable to find prompt in scene."); // Inform the Unity console that something went wrong
            }
        }
    }
    private (Card, Card) DoAttack(Card attackingCard, Card defendingCard) // Function for handling attacks between two cards
    {
        int damageDealt = attackingCard.attack; // Define useful values for later
        bool crit = false;
        switch (attackingCard.ability) // Check which ability the attacking card has
        {
            default: // If the ability is not recognised
                Debug.Log("Ability " + attackingCard.ability + " not recognised on attacking card."); // Inform the Unity console that something went wrong
                break;
            case 0: // 25% chance to crit
                if (random.Next(0,100) < 25) // 25% chance of being true
                {
                    damageDealt = damageDealt * 2; // Double damage dealt
                    crit = true; // Set crit to true
                    //Debug.Log("Did crit");
                }
                break;
        }
        switch (defendingCard.ability) // CHeck which ability the defending card has
        {
            default: // If the ability is not recognised
                Debug.Log("Ability " + defendingCard.ability + " not recognised on defending card."); // Inform the Unity console that something went wrong
                break;
            case 2: // reflect damage
                if (random.Next(0, 100) < 10) // 10% chance of running
                {
                    defendingCard.health += damageDealt; // Add any damage dealt back to the defending card's health
                    attackingCard.health -= damageDealt; // Damage the attacking card with its own damage
                    //Debug.Log("Reflected damage");
                }
                break;
            case 3: // immune to crits
                if (crit) // If the card critted
                {
                    //Debug.Log("Blocked crit");
                    damageDealt = damageDealt / 2; // Half the damage so it is only a regular amount of damage
                }
                break;
        }
        defendingCard.health -= damageDealt; // Actually remove health from defending card equal to the damageDealt value
        return (attackingCard, defendingCard); // Return attacking and defending card
    }
    private (CardScript, CardScript) CalculateAttack(int column, int[] columnState, GameObject[] columnCardsObj) // This function is just a large collection of conditions for setting which cards are supposed to attack which
    {
        string state = "";
        CardScript attackingCard = null;
        CardScript defendingCard = null;
        foreach (int i in columnState) // Loop through every value in columnState
        {
            state += i.ToString(); // Convert value to string and add it to state
        }
        CardScript[] columnCards = new CardScript[columnCardsObj.Length];
        for (int i = 0; i < columnCards.Length; i++) // Loop through all four cards give
        {
            if (columnCardsObj[i] != null) // If there is a card
            {
                columnCards[i] = columnCardsObj[i].GetComponent<CardScript>(); // Find and reference the card script component
            }
            else // Otherwise
            {
                columnCards[i] = null; // Leave the card script as null
            }
        }
        switch (state) // Pick which state the board is in
        {
            default:
                //Debug.Log("No cards on board at column " + column + " so nothing happened. (" + state + ")");
                break;
            case "0001": // player has back card
                //Debug.Log("Player back card cannot reach enemy player on column " + column + " so nothing happened. (" + state + ")");
                if (columnCards[3].cardData.attack > 0)
                {
                    columnCards[3].ShowNada();
                }
                break;
            case "0010": // player has front card
                //Debug.Log("Player front card can reach enemy player on column " + column + " so enemy player takes damage. (" + state + ")");
                enemy.health -= columnCards[2].cardData.attack;
                attackingCard = columnCards[2];
                break;
            case "0100": // enemy has front card
                //Debug.Log("Enemy front card can reach player player on column " + column + " so player player takes damage. (" + state + ")");
                health -= columnCards[1].cardData.attack;
                attackingCard = columnCards[1];
                break;
            case "1000": // enemy has back card
                //Debug.Log("Enemy back card cannot reach player player on column " + column + " so nothing happened. (" + state + ")");
                if (columnCards[0].cardData.attack > 0)
                {
                    columnCards[0].ShowNada();
                }
                break;
            case "0011": // player has front and back card
                //Debug.Log("Player front card can reach enemy player and player back card is blocked by front card on column " + column + " so the enemy player takes damage. (" + state + ")");
                enemy.health -= columnCards[2].cardData.attack;
                attackingCard = columnCards[2];
                break;
            case "0101": // player has back card and enemy has front card
                //Debug.Log("Player has back card that can reach enemy front card on column " + column + " so player back card and enemy front card hit each other (" + state + ")");
                (columnCards[3].cardData, columnCards[1].cardData) = DoAttack(columnCards[3].cardData, columnCards[1].cardData);
                if (columnCards[1].cardData.health > 0)
                {
                    (columnCards[1].cardData, columnCards[3].cardData) = DoAttack(columnCards[1].cardData, columnCards[3].cardData);
                    defendingCard = columnCards[1];
                }
                attackingCard = columnCards[3];
                break;
            case "1001": // player has back card and enemy has back card
                //Debug.Log("Player has back card that can reach enemy back card on column " + column + " so player back card and enemy back card hit each other. (" + state + ")");
                (columnCards[3].cardData, columnCards[0].cardData) = DoAttack(columnCards[3].cardData, columnCards[0].cardData);
                if (columnCards[0].cardData.health > 0)
                {
                    (columnCards[0].cardData, columnCards[3].cardData) = DoAttack(columnCards[0].cardData, columnCards[3].cardData);
                    defendingCard = columnCards[0];
                }
                attackingCard = columnCards[3];
                break;
            case "0110": // player has front card and enemy has front card
                //Debug.Log("Player has front card that can reach enemy front card on column " + column + " so the player front card and enemy front card hit each other. (" + state + ")");
                (columnCards[2].cardData, columnCards[1].cardData) = DoAttack(columnCards[2].cardData, columnCards[1].cardData);
                if (columnCards[1].cardData.health > 0)
                {
                    (columnCards[1].cardData, columnCards[2].cardData) = DoAttack(columnCards[1].cardData, columnCards[2].cardData);
                    defendingCard = columnCards[1];
                }
                attackingCard = columnCards[2];
                break;
            case "1010": // player has front card and enemy has back card
                //Debug.Log("Player has front card that can reach enemy back card on column " + column + " so the player front card and enemy back card hit each other. (" + state + ")");
                (columnCards[2].cardData, columnCards[0].cardData) = DoAttack(columnCards[2].cardData, columnCards[0].cardData);
                if (columnCards[0].cardData.health > 0)
                {
                    (columnCards[0].cardData, columnCards[2].cardData) = DoAttack(columnCards[0].cardData, columnCards[2].cardData);
                    defendingCard = columnCards[0];
                }
                attackingCard = columnCards[2];
                break;
            case "1100": // enemy has front and back card
                //Debug.Log("Enemy has front card that can reach player and enemy has back card blocked by front card on column " + column + " so the enemy front card damages the player. (" + state + ")");
                health -= columnCards[1].cardData.attack;
                attackingCard = columnCards[1];
                break;
            case "0111": // player has front and back card and enemy has front card
                //Debug.Log("Player has front card that can reach enemy front card and player has back card that is blocked by front card on column " + column + " so the player front card and enemy front card damage each other. (" + state + ")");
                (columnCards[2].cardData, columnCards[1].cardData) = DoAttack(columnCards[2].cardData, columnCards[1].cardData);
                if (columnCards[1].cardData.health > 0)
                {
                    (columnCards[1].cardData, columnCards[2].cardData) = DoAttack(columnCards[1].cardData, columnCards[2].cardData);
                    defendingCard = columnCards[1];
                }
                attackingCard = columnCards[2];
                break;
            case "1011": // player has front and back card and enemy has back card
                //Debug.Log("Player has front card that can reach enemy back card and player has back card that is blocked by front card on column " + column + " so the player front card and enemy back card hit each other. (" + state + ")");
                (columnCards[2].cardData, columnCards[0].cardData) = DoAttack(columnCards[2].cardData, columnCards[0].cardData);
                if (columnCards[0].cardData.health > 0)
                {
                    (columnCards[0].cardData, columnCards[2].cardData) = DoAttack(columnCards[0].cardData, columnCards[2].cardData);
                    defendingCard = columnCards[0];
                }
                attackingCard = columnCards[2];
                break;
            case "1101": // enemy has front and back card and player has back card
                //Debug.Log("Player has back card that can reach enemy front card and enemy back card is blocked by front card on column " + column + " so the player back card and enemy front card hit each other. (" + state + ")");
                (columnCards[3].cardData, columnCards[1].cardData) = DoAttack(columnCards[3].cardData, columnCards[1].cardData);
                if (columnCards[1].cardData.health > 0)
                {
                    (columnCards[1].cardData, columnCards[3].cardData) = DoAttack(columnCards[1].cardData, columnCards[3].cardData);
                    defendingCard = columnCards[1];
                }
                attackingCard = columnCards[3];
                break;
            case "1110": // enemy has front and back card and player has front card
                //Debug.Log("Player has front card that can reach enemy front card and enemy has back card blocked by front card on column " + column + " so the enemy front card and player front card hit each other. (" + state + ")");
                (columnCards[2].cardData, columnCards[1].cardData) = DoAttack(columnCards[2].cardData, columnCards[1].cardData);
                if (columnCards[1].cardData.health > 0)
                {
                    (columnCards[1].cardData, columnCards[2].cardData) = DoAttack(columnCards[1].cardData, columnCards[2].cardData);
                    defendingCard = columnCards[1];
                }
                attackingCard = columnCards[2];
                break;
            case "1111": // enemy has front and back card and player has front and back card
                //Debug.Log("Player has front card that can reach enemy front card and enemy has back card blocked by front card and player has back card blocked by front card on column " + column + " so the enemy front card and player front card hit each other. (" + state + ")");
                (columnCards[2].cardData, columnCards[1].cardData) = DoAttack(columnCards[2].cardData, columnCards[1].cardData);
                if (columnCards[1].cardData.health > 0)
                {
                    (columnCards[1].cardData, columnCards[2].cardData) = DoAttack(columnCards[1].cardData, columnCards[2].cardData);
                    defendingCard = columnCards[1];
                }
                attackingCard = columnCards[2];
                break;
                
        }
        for (int j = 0; j < columnCardsObj.Length; j++) // Loop through every card again
        {
            if (columnCardsObj[j] != null) // If there is a card
            {
                columnCardsObj[j].transform.Find("Health").GetComponent<TextMeshPro>().text = columnCards[j].cardData.health.ToString();
                float maxCost = columnCards[j].cardData.maxCost; // Update all of the card's values
                float maxHealth = columnCards[j].cardData.maxHealth;
                float health = columnCards[j].cardData.health;
                columnCards[j].cardData.cost = Convert.ToInt32((maxCost * (health / maxHealth)));
                columnCardsObj[j].transform.Find("Cost").GetComponent<TextMeshPro>().text = columnCards[j].cardData.cost.ToString();
                if (columnCards[j].cardData.health <= 0) // If the card has run out of health
                {
                    if (columnCardsObj[j] == attackCard) // If the card is attacking
                    {
                        attackCard = null; // Remove reference to attack card
                    }
                    else if (columnCardsObj[j] == defendCard) // If the card is defending
                    {
                        defendCard = null; // Remove reference to defend card
                    }
                    GameObject.Destroy(columnCardsObj[j]); // Destroy the card that has run out of health
                    columnCardsObj[j] = null; // Remove reference to the card
                    if (j < 2) // Card is enemy card
                    {
                        foreach (Card card in enemy.fullDeck.cards) // Search through enemy full deck
                        {
                            if (columnCards[j].cardData.ID == card.ID) // If destroyed card is found
                            {
                                card.state = 0; // Reset card to be in fullDeck
                            }
                        }
                        playerCurrency += columnCards[j].cardData.maxCost;
                        UpdateCurrency(playerCurrency, enemy.currency);
                    }
                    else // Card is friendly card
                    {
                        foreach (Card card in playerFullDeck.cards) // Search through player full deck
                        {
                            if (columnCards[j].cardData.ID == card.ID) // If destroyed card is found
                            {
                                card.state = 0; // Reset card to be in fullDeck
                            }
                        }
                        enemy.currency += columnCards[j].cardData.maxCost;
                        UpdateCurrency(playerCurrency, enemy.currency);
                    }
                }
            }
        }
        UpdateHealth(); // Call the UpdateHealth function
        return (attackingCard, defendingCard); // Return the attacking and defending cards
    }
    private void GenerateDeck()
    {
        #region Delete old deck object
        try // Try to run the below code
        {
            GameObject.Destroy(playerDeckObject); // Destroy the player's deck object
        }
        catch // If the above code fails to run
        {
            Debug.Log("Unable to destroy player deck object."); // Inform the Unity console that something went wrong
        }
        #endregion
        #region Create Deck object
        playerDeckObject = new GameObject(); // Create a new gameObject
        playerDeckObject.name = "PlayerDeck"; // Name it PlayerDeck
        playerDeckObject.transform.parent = mainCamera.transform; // Set the deck's parent to the mainCamera
        playerDeckObject.transform.localPosition = new Vector3(0f, -0.17f, 0.45f); // Set the deck's position to the stated value
        playerDeckObject.transform.localRotation = Quaternion.Euler(new Vector3(80f, -180f, 0f)); // Set the deck's rotation to the stated value
        playerDeckObject.transform.localScale = new Vector3(1, 1, 1); // Set the deck's scale to 1
        #endregion
        #region Create player card objects
        if (playerDeck.cards.Length > 0) // If the player's deck has cards
        {
            float cardSpacing = totalRange / (playerDeck.cards.Length - 1); // Calculate the space between each card
            float currentPos = totalRange / 2; // Calculate the current position for adding cards
            playerCardObjects = new List<GameObject>(); // Create a new list of gameObjects
            if (playerDeck.cards.Length > 3) // If the player's deck is more than 3 cards in length
            {
                foreach (Card card in playerDeck.cards) // Loop through every card in the player's deck
                {
                    playerCardObjects.Add(CreateCard(card, new Vector3(currentPos, 0f, 0f), playerDeckObject)); // Add a card object to the deck that is created by the CreateCard function
                    currentPos = currentPos - cardSpacing; // Move currentPos by cardSpacing
                }
            }
            #region Special cases for small decks
            else if (playerDeck.cards.Length == 3) // These below cases are for when the player's deck is too small for the above code to look correct
            { // They are simply a set of pre-defined positions for where each card should go
                playerCardObjects.Add(CreateCard(playerDeck.cards[0], new Vector3(-0.14f, 0, 0), playerDeckObject));
                playerCardObjects.Add(CreateCard(playerDeck.cards[1], new Vector3(0, 0, 0), playerDeckObject));
                playerCardObjects.Add(CreateCard(playerDeck.cards[2], new Vector3(0.14f, 0, 0), playerDeckObject));
            }
            else if (playerDeck.cards.Length == 2)
            {
                playerCardObjects.Add(CreateCard(playerDeck.cards[0], new Vector3(-0.075f, 0, 0), playerDeckObject));
                playerCardObjects.Add(CreateCard(playerDeck.cards[1], new Vector3(0.075f, 0, 0), playerDeckObject));
            }
            else if (playerDeck.cards.Length == 1)
            {
                playerCardObjects.Add(CreateCard(playerDeck.cards[0], new Vector3(0, 0, 0), playerDeckObject));
            }
            #endregion
        }
        else
        {
            Debug.Log("Player deck size is less than 1.");
        }
        #endregion
    }
    public GameObject CreateCard(Card card, Vector3 position, GameObject deck)
    {
        #region Create Instance of card
        GameObject output = Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0, 0, 0))); // Create a new instance of a card object
        output.transform.parent = deck.transform; // Set the card's parent to the given deck
        output.transform.localPosition = position; // Set the card's position to position
        output.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -10)); // Set the card's rotation to localRotation
        output.transform.localScale = new Vector3(1, 1, 1); // Set the card's scale to 1
        CardScript outputScript = output.GetComponent<CardScript>(); // Find and set the cardData component of the card to card
        outputScript.cardData = card;
        outputScript.board = board.transform;
        MeshRenderer outputMesh = output.GetComponent<MeshRenderer>(); // Cache the meshrenderer of the card
        #endregion
        #region Swap card material for required material
        outputMesh.material = defaultCardMaterial;
        foreach (Material material in cardMaterials)
        {
            if (material.name == card.materialName)
            {
                outputMesh.material = material;
            }
        }
        #endregion
        #region Load card details
        try // Try to run the below code
        {
            output.transform.GetChild(0).GetComponent<TextMeshPro>().text = card.health.ToString(); // load health
            output.transform.GetChild(1).GetComponent<TextMeshPro>().text = card.attack.ToString(); // load attack
            output.transform.GetChild(2).GetComponent<TextMeshPro>().text = card.cost.ToString(); // load cost
            output.transform.GetChild(3).GetComponent<TextMeshPro>().text = card.name; // load name
            output.transform.GetChild(4).GetComponent<TextMeshPro>().text = card.description; // load description
        }
        catch // If the above code fails to run
        {
            Debug.Log("Card details for " + card.name + " are missing!"); // Inform the Unity console that something went wrong
        }
        #endregion
        return output; // Return the created card
    }
    public Card CreateCardData(string cardName)
    {
        #region Create card object and set its ID
        Card outputCard = new Card(); // Create a new card
        outputCard.ID = currentID;
        currentID++;
        #endregion
        #region Load card data based on name
        switch (cardName.ToLower()) // Create card data based on the provided name
        {
            default: // If no names match
                Debug.Log("Card name not recognised."); // Inform the Unity console that the card does not exist
                break;
            case "reaper": // If the name is reaper
                outputCard.name = "Reaper";
                outputCard.ability = 0;
                outputCard.description = "25% chance to deal a critical hit when attacking.";
                outputCard.materialName = "Reaper";
                outputCard.health = 2;
                outputCard.cost = 2;
                outputCard.attack = 2;
                outputCard.state = 0;
                break;
            case "phisch": // If the name is phisch
                outputCard.name = "Phisch";
                outputCard.ability = 1;
                outputCard.description = "Appreciates in price by one chip every turn.";
                outputCard.materialName = "Phisch";
                outputCard.health = 1;
                outputCard.cost = 0;
                outputCard.attack = 0;
                outputCard.state = 0;
                break;
            case "dusk": // If the name is dusk
                outputCard.name = "Dusk";
                outputCard.ability = 2;
                outputCard.description = "10% chance to reflect damage when being hit.";
                outputCard.materialName = "Dusk";
                outputCard.health = 3;
                outputCard.cost = 3;
                outputCard.attack = 1;
                outputCard.state = 0;
                break;
            case "rock": // If the name is rock
                outputCard.name = "Rock";
                outputCard.ability = 3;
                outputCard.description = "Immune to critical hits.";
                outputCard.materialName = "Rock";
                outputCard.health = 6;
                outputCard.cost = 2;
                outputCard.attack = 0;
                outputCard.state = 0;
                break;
        }
        outputCard.maxCost = outputCard.cost;
        outputCard.maxHealth = outputCard.health;
        #endregion
        return outputCard; // Return the created card
    }
    public Deck PopulateDeck(Deck fullDeck, int handSize, Deck currentDeck)
    {
        #region Check how many spare spaces there are in deck and how many spare cards there are in fullDeck
        Deck tempDeck = currentDeck;
        currentDeck = new Deck();
        if (handSize > tempDeck.cards.Length)
        {
            currentDeck.cards = new Card[handSize];
            for (int i = 0; i < tempDeck.cards.Length; i++)
            {
                currentDeck.cards[i] = tempDeck.cards[i];
            }
        }
        else
        {
            currentDeck.cards = tempDeck.cards;
        }
        int l = 0; // Create l and set it to 0
        foreach (Card card in fullDeck.cards) // Loop through every card in fullDeck
        {
            if (card.state == 0) // If the card is not in the player's hand or on the board
            {
                l++; // Add 1 to l
            }
        }
        int m = 0;
        foreach (Card card in currentDeck.cards)
        {
            if (card == null)
            {
                m++;
            }
        }
        #endregion
        #region If there are more sparce cards in fullDeck than spare spaces in deck
        if (l > m) // if the number of free cards in fullDeck is more than or equal to the number of free cards in deck
        {
            System.Random random = new System.Random(); // Create a new random
            List<int> usedIndexes = new List<int>(); // Create a list of used indexes
            for (int i = 0; i < currentDeck.cards.Length; i++) // Loop through every index of currentDeck
            {
                if (currentDeck.cards[i] == null) // If there is a card missing
                {
                    // add a card
                    //int index = random.Next(fullDeck.cards.Length); // Randomize a new index
                    //while (usedIndexes.Contains(index) || (fullDeck.cards[index].state == 1 || fullDeck.cards[index].state == 2)) // Loop until a unique index is found
                    //{
                    //    index = random.Next(fullDeck.cards.Length); // Randomize a new index
                    //}
                    List<int> freeIndexes = new List<int>();
                    for (int n = 0; n < fullDeck.cards.Length; n++)
                    {
                        if (fullDeck.cards[n] != null)
                        {
                            if (fullDeck.cards[n].state == 0 && !usedIndexes.Contains(n))
                            {
                                freeIndexes.Add(n);
                            }
                        }
                        else
                        {
                            Debug.Log("Null card in fullDeck, this should not happen!");
                        }
                    }
                    int index = freeIndexes[random.Next(0, freeIndexes.Count)];
                    currentDeck.cards[i] = fullDeck.cards[index]; // Add the selected card to currentDeck
                    fullDeck.cards[index].state = 1;
                    usedIndexes.Add(index); // Add the index to usedIndexes
                }
            }
        }
        #endregion
        #region If there are more spare spaces in deck than spare cards in fullDeck
        else // If the current deck is larger than l
        {
            if (l > 0)
            {
                Debug.Log("Cannot fully populate deck when fullDeck is less than the currentDeck!"); // Inform the Unity console that something has gone very wrong
                int i = 0;
                Card[] temp = new Card[currentDeck.cards.Length - m + l]; // Code below here simply fills in currentDeck with every card that is left in fullDeck
                for (int k = 0; k < currentDeck.cards.Length; k++)
                {
                    if (currentDeck.cards[k] != null)
                    {
                        temp[k] = currentDeck.cards[k];
                        i = k;
                    }
                }
                for (int j = 0; j < fullDeck.cards.Length; j++)
                {
                    if (fullDeck.cards[j].state == 0)
                    {
                        i++;
                        temp[i] = fullDeck.cards[j];
                        fullDeck.cards[j].state = 1;
                    }
                }
                currentDeck.cards = temp;
            }
            else
            {
                Debug.Log("There are no free cards in fullDeck");
                int i = 0;
                foreach (Card card in currentDeck.cards)
                {
                    if (card != null)
                    {
                        i++;
                    }
                }
                Card[] temp = new Card[i];
                for (int j = 0; j < currentDeck.cards.Length; j++)
                {
                    if (currentDeck.cards[j] != null)
                    {
                        temp[j] = currentDeck.cards[j];
                    }
                }
                currentDeck.cards = temp;
            }
        }
        #endregion
        return currentDeck; // Return currentDeck
    }
    #region Functions for updating the UI
    public void UpdateCurrency(int playerCurrency, int enemyCurrency) // Finds the enemy and player currency text and updates them with provided values
    {
        cardBattleHUD.transform.Find("PlayerInfo").Find("Currency").Find("CurrencyText").GetComponent<TextMeshProUGUI>().text = playerCurrency.ToString();
        cardBattleHUD.transform.Find("EnemyInfo").Find("Currency").Find("CurrencyText").GetComponent<TextMeshProUGUI>().text = enemyCurrency.ToString();
    }
    private void UpdateHealth()
    {
        float h = health;
        float maxH = maxHealth;
        cardBattleHUD.transform.Find("PlayerInfo").Find("Health").Find("HealthText").GetComponent<TextMeshProUGUI>().text = health.ToString();
        cardBattleHUD.transform.Find("PlayerInfo").Find("Health").Find("Health").GetComponent<Image>().fillAmount = h / maxH;
        enemy.UpdateHealth(cardBattleHUD);
    }
    #region Functions for handling portraits
    private void SetPlayerPortrait(string name)
    {
        SetPortrait(name, "PlayerInfo"); // Set the player's portrait to the portrait of name
    }
    private void SetEnemyPortrait(string name)
    {
        SetPortrait(name, "EnemyInfo"); // Set the enemy's portrait to the portrait of name
    }
    private void SetPortrait(string name, string parentName)
    {
        bool success = false; // Create succcess and make it false
        #region Try to find and set the specified portrait
        foreach (Sprite portrait in characterPortraits) // Loop through every portrait in the array of portraits
        {
            if (portrait.name == name) // If the name of the portrait matches name
            {
                Transform sprite = cardBattleHUD.transform.Find(parentName).Find("Icon"); // Find the icon that needs changing
                if (sprite != null) // If the icon was found
                {
                    sprite.GetComponent<Image>().sprite = portrait; // Set the icon to the correct portrait
                    success = true; // Set success to true
                }
                else // If the icon was not found
                {
                    Debug.Log("Unable to find icon for " + parentName); // Inform the Unity console that something went wrong
                }
            }
        }
        #endregion
        #region If the portrait could not be set
        if (!success) // If the script failed in any way
        {
            Debug.Log("Unable to find requested portrait! (" + name + ")"); // Inform the Unity console that the portrait could not be found
            cardBattleHUD.transform.Find(parentName).Find("Icon").GetComponent<Image>().sprite = defaultPortrait; // Set the icon to the default icon
        }
        #endregion
    }
    #endregion
    #endregion
}
