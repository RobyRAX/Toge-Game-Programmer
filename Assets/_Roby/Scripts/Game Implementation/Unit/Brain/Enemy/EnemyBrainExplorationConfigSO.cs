using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBrainExplorationConfigSO", menuName = "RAXY/Unit/Brain/Enemy Exploration Config")]
public class EnemyBrainExplorationConfigSO : BrainExplorationConfigBaseSO
{
    [TitleGroup("Detection")]
    public float chaseRadius = 8f;

    [TitleGroup("Detection")]
    public float attackRadius = 2f;

    [TitleGroup("Patrol")]
    [SuffixLabel("seconds")]
    public float patrolIdleDuration = 2f;

    [TitleGroup("Patrol")]
    public float patrolArrivalThreshold = 0.5f;

    [TitleGroup("Chase")]
    public bool useSprintWhileChasing = true;
}
