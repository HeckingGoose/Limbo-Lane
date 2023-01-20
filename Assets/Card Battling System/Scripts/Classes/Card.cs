using UnityEditor;

[System.Serializable] // Tell Unity that this is a class
public class Card
{
    public string name;
    public string description;
    public string materialName;
    public int health;
    public int attack;
    public int state; // 0 - in fullDeck, 1 - in currentDeck, 2 - onBoard
    //public int abilityType; // Define the required variables
    //public int ability;
    //public int canBeDamaged;
    public int cost;
    public static Card Create(string name, string description, string materialName, int health, int attack, int cost, int state) // Tell Unity which variables the class uses
    {
        Card card = new Card();
        card.name = name;
        card.description = description;
        card.materialName = materialName;
        card.health = health;
        card.attack = attack;
        //card.abilityType = abilityType; // Create and return an instance of itself
        //card.ability = ability;
        //card.canBeDamaged= canBeDamaged;
        card.cost = cost;
        card.state = state;
        return card;
    }
}
