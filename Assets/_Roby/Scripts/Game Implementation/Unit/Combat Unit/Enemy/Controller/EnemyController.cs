using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyController : CombatUnitController
{
    public EnemyGroup EnemyGroup { get; set; }
    public EnemyDataSO enemyDataSO;
    public EnemyCombatant enemyCombatant;

    [TitleGroup("Brain")]
    [SerializeField]
    EnemyBrainExplorationConfigSO brainConfig;

    public override CombatantBase CombatantCont { get => enemyCombatant; }
    public override UnitDataSO UnitData => enemyDataSO;

    public override async UniTask Init()
    {
        await base.Init();

        InitDone = false;

        AnimationClips = enemyDataSO.AnimationClipsSO;

        SetHitboxSetting(GameplayConfig.Instance.ConfigSO.enemyHitboxSetting);

        enemyCombatant = GetComponent<EnemyCombatant>();
        enemyCombatant.Init(this);

        var config = brainConfig ?? GameplayConfig.Instance.ConfigSO.defaultEnemyBrainExplorationConfigSO;
        if (config != null && EnemyGroup != null && !EnemyGroup.IsCleared)
            Setup_EnemyBrainExploration(config);

        InitDone = true;
    }

    public void Setup_EnemyBrainExploration(EnemyBrainExplorationConfigSO config)
    {
        Brain_Exploration = new EnemyBrainExploration(this, config, EnemyGroup);

        if (StateMachine_Exploration != null)
            StateMachine_Exploration.Brain = Brain_Exploration;
    }

    protected override void OnHitboxHit(CombatUnitController target)
    {
        if (target is HeroController)
        {
            EnemyGroup?.StartCombatFromEnemyAttack(this);
            return;
        }

        base.OnHitboxHit(target);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (Brain_Exploration is EnemyBrainExploration enemyBrain)
            enemyBrain.DrawGizmos();
    }
#endif
}
