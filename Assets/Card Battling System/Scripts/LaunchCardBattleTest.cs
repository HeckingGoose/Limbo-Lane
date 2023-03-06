using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchCardBattleTest : MonoBehaviour
{
    [SerializeField]
    private MainCardBattleHandler mainCardBattleHandler;
    void Start()
    {
        mainCardBattleHandler.StartCardBattle();
    }
}
// Test script that simply starts a card battle on scene start, to help test the card battling system