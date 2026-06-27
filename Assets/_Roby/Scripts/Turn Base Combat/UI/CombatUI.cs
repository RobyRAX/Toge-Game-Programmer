using Sirenix.OdinInspector;
using UnityEngine;

public class CombatUI : MonoBehaviour
{
    public PlayerUnitListUI partyListUI;
    public TurnTimelineUI turnTimelineUI;
    public TurnPhaseTimelineUI turnPhaseTimelineUI;

    [TitleGroup("Attack Ref")]
    [SerializeField]
    Transform attackContainer;

    [TitleGroup("Attack Ref")]
    [SerializeField]
    AttackUI attackUiPrefab;

    [TitleGroup("Runtime")]

    public void Setup()
    {
        
    }
}
