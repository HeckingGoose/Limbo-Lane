using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGamePrompt : MonoBehaviour
{
    // Define variables that need accessing from inspector
    [SerializeField]
    private Image alexImage;
    [SerializeField]
    private Sprite winSprite;
    [SerializeField]
    private Sprite loseSprite;
    [SerializeField]
    private TextMeshProUGUI winText;
    [SerializeField]
    private TextMeshProUGUI rewardsText;
    [SerializeField]
    private Image promptCharacter;
    [SerializeField]
    private TextMeshProUGUI xpText;
    [SerializeField]
    private Image xpImage;

    public void SetWinText(bool won) // Sets the winning text to either won or lost
    {
        switch (won) // Pick between whether the input is a win or a loss
        {
            case true: // If a win
                winText.text = "You Won!"; // Set the win text to You Won!
                alexImage.sprite = winSprite; // Set the sprite to a winning sprite
                break;
            case false: // If a lost
                winText.text = "You Lost!"; // Set the win text to You Lost!
                alexImage.sprite = loseSprite; // Set the sprite to a losing sprite
                break;
        }
    }
    public void SetRewards((int, string, string)[] rewards) // Sets the rewards text to a list of rewards stating how much of each reward was won
    {
        string outputText = "Rewards:"; // Define beginning of rewards text
        for (int i = 0; i < rewards.Length; i++) // Loop through every reward
        {
            if (i != rewards.Length - 1) // If this is not the last reward
            {
                outputText += "\n +" + rewards[i].Item1 + " " + rewards[i].Item2 + ","; // Add reward Item2 of amount Item1 to rewards text with a comma at the end
            }
            else // Otherwise
            {
                outputText += "\n +" + rewards[i].Item1 + " " + rewards[i].Item2; // Add reward Item2 of amount Item1 to rewards text with a comma at the end
            }
        }
        rewardsText.text = outputText; // Set the rewards text object to the rewards text value
    }
    public void SetXpBar(int xp, int maxXp) // Function for setting XP bar
    {
        xpImage.fillAmount = (xp * 1f) / (maxXp * 1f); // Set the image for the XP bar's fill amount to a percentage of the current xp over the xp to the next level
        xpText.text = "XP: " + xp.ToString() + " / " + maxXp.ToString(); // Set the xp text to XP: xp / maxxp
    }
}
