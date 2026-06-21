using RAXY.InputSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveUnitBrainExplorationConfigSO", menuName = "RAXY/Unit/Brain/Active Unit Exploration Config")]
public class ActiveUnitBrainExplorationConfigSO : BrainExplorationConfigBaseSO
{
    public InputActionEventSO MoveInputActionSO;
    public InputActionEventSO SprintInputActionSO;
    public InputActionEventSO AttackInputActionSO;
}
