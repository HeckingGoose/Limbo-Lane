[System.Serializable] // Tell Unity that this is a class
public class Deck
{
    public Card[] cards; // Define required variables
    public static Deck Create(Card[] cards) // Tell Unity which variables are used
    {
        Deck deck = new Deck();
        deck.cards = cards; // Create and return an instance of itself
        return deck;
    }
}