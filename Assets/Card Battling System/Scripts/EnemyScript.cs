using UnityEngine;
using UnityEngine.UI;

public class EnemyScript : MonoBehaviour
{
    #region Public variables for main script
    public string enemyName;
    [HideInInspector]
    public int currency;
    [HideInInspector] // Setup variables that mainScript can see for handling card battle
    public bool ready = false;
    #endregion
    [SerializeField]
    private MainCardBattleHandler mainScript;
    [SerializeField]
    private Image healthBar; // Setup variables
    private float health;
    private int handSize;
    private Deck fullDeck;
    private Deck deck;
    private GameObject deckObject;

    private void Start()
    {
        deckObject = new GameObject(); // Create a new deck object for the enemy (this is temporary code)
        switch (enemyName.ToLower()) // Pick which enemy data to load
        {
            default: // If enemyName matches none of the cases
                Debug.Log("Enemy name not recognised!"); // Inform the Unity console that something went wrong
                break;
            case "dawn": // If the enemyName is dawn
                currency = 5;
                health = 10;
                handSize = 3;
                fullDeck = new Deck();
                deck = new Deck();
                deck.cards = new Card[handSize];
                fullDeck.cards = new Card[10];
                fullDeck.cards[0] = mainScript.CreateCardData("Reaper");
                fullDeck.cards[1] = mainScript.CreateCardData("Reaper");
                fullDeck.cards[2] = mainScript.CreateCardData("Reaper");
                fullDeck.cards[3] = mainScript.CreateCardData("Dusk"); // Setup values and deck for character
                fullDeck.cards[4] = mainScript.CreateCardData("Dusk");
                fullDeck.cards[5] = mainScript.CreateCardData("Dusk");
                fullDeck.cards[6] = mainScript.CreateCardData("Rock");
                fullDeck.cards[7] = mainScript.CreateCardData("Rock");
                fullDeck.cards[8] = mainScript.CreateCardData("Rock");
                fullDeck.cards[9] = mainScript.CreateCardData("Dusk");
                deck = mainScript.PopulateDeck(fullDeck, handSize, deck);
                break;
        }
        ready = true; // Set ready to true
    }
    public GameObject[ , ] ComputeTurn(GameObject[ , ] board) // Can be called externally when it is enemy's turn
    {
        switch (enemyName.ToLower()) // Pick which enemy AI needs to be used
        {
            default: // If enemyName matches none of the below cases
                Debug.Log("Enemy name not recognised!"); // Inform the Unity console that something went wrong
                return board; // Return board
            case "dawn": // If the enemyName is dawn
                System.Random random = new System.Random(); // Create a new Random
                // v Create a new Card that is randomly picked from the enemy's deck v
                GameObject cardToPlace = mainScript.CreateCard(deck.cards[random.Next(0, deck.cards.Length)], new Vector3 (0, 0, 0), deckObject);
                int boardy = random.Next(0, 1); // 0 is front, 1 is back, randomize a y co-ordinate for the card
                int boardx = random.Next(0, 4); // Randomize an x co-ordinate for the card
                board[boardx, 1 - boardy] = cardToPlace; // Place the card on the board object
                return board; // Return the board
        }
    }
}
