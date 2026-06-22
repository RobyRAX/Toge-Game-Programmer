using ToGaProTest.Shared;
using UnityEngine;

public class TestCombatant : CombatantBase
{
    public GameplayConfigSO GameplayConfigSO;

    void Awake()
    {
        StatContainer = new StatContainer_Runtime(GameplayConfigSO);

        CurrentHp = StatContainer.GetTotalValue(StatAttribute.MaxHp);
        CurrentStamina = StatContainer.GetTotalValue(StatAttribute.MaxStamina);
        IsAlive = true;
    }
}
