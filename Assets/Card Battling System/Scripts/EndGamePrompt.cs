using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGamePrompt : MonoBehaviour
{
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

    public void SetWinText(bool won)
    {
        switch (won)
        {
            case true:
                winText.text = "You Won!";
                alexImage.sprite = winSprite;
                break;
            case false:
                winText.text = "You Lost!";
                alexImage.sprite = loseSprite;
                break;
        }
    }
    public void SetRewards((int, string, string)[] rewards)
    {
        string outputText = "Rewards:";
        for (int i = 0; i < rewards.Length; i++)
        {
            if (i != rewards.Length - 1)
            {
                outputText += "\n +" + rewards[i].Item1 + " " + rewards[i].Item2 + ",";
            }
            else
            {
                outputText += "\n +" + rewards[i].Item1 + " " + rewards[i].Item2;
            }
        }
        rewardsText.text = outputText;
    }
    public void SetXpBar(int xp, int maxXp)
    {
        xpImage.fillAmount = (xp * 1f) / (maxXp * 1f);
        xpText.text = "XP: " + xp.ToString() + " / " + maxXp.ToString();
    }
}
