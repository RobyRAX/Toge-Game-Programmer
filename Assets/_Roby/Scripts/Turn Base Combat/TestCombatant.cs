using UnityEngine;

public class TestCombatant : CombatantBase
{
    public GameplayConfigSO GameplayConfigSO;

    void Awake()
    {
        StatContainer = new StatContainer_Runtime(GameplayConfigSO);
    }
}
