using UnityEngine;

public class CombatantClickTarget : MonoBehaviour
{
    public CombatantBase Combatant { get; private set; }

    public void Setup(CombatantBase combatant)
    {
        Combatant = combatant;
    }
}
