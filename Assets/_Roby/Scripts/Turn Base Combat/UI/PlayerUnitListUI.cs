using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitListUI : MonoBehaviour
{
    public PlayerUnitStatusUI unitA;
    public PlayerUnitStatusUI unitB;
    public PlayerUnitStatusUI unitC;

    public void Setup(TurnBaseCombatManager manager)
    {
        if (manager.PlayerCombatants[0] != null)
            unitA.Setup(manager.PlayerCombatants[0]);
        else   
            unitA.gameObject.SetActive(false); 

        if (manager.PlayerCombatants[0] != null)
            unitA.Setup(manager.PlayerCombatants[0]);
        else   
            unitA.gameObject.SetActive(false); 
        
        if (manager.PlayerCombatants[0] != null)
            unitA.Setup(manager.PlayerCombatants[0]);
        else   
            unitA.gameObject.SetActive(false); 
    }

    public void SetCurrentPlayerUnit()
    {
        
    }
}
