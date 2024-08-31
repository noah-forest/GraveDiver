using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BattleDifficulty
{
    easy,
    medium,
    hard,
    superHard
}

public class BattleHazard : BoatHazard
{
    [SerializeField]
    BattleDifficulty battleDifficulty;
    [SerializeField]
    GameObject[] difficultyVisuals;
    public void Start()
    {
        if (battleDifficulty != BattleDifficulty.easy && difficultyVisuals.Length > (int)battleDifficulty - 1)
        {
            difficultyVisuals[(int)battleDifficulty - 1].SetActive(true);
        }
    }

    public override void InteractWithBoat(BoatMaster boat)
    {
        base.InteractWithBoat(boat);
        boat.controller.StopMovement();
        boat.gameManager.battleManager.nextBattleDifficulty = battleDifficulty;
        boat.gameManager.startShopTransition.Invoke();
        Destroy(gameObject);
    }
}