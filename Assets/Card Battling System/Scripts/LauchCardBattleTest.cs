using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LauchCardBattleTest : MonoBehaviour
{
    [SerializeField]
    private MainCardBattleHandler mainCardBattleHandler;
    void Start()
    {
        mainCardBattleHandler.StartCardBattle();
    }
}
