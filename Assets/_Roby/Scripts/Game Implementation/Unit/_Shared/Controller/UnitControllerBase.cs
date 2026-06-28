using System;
using Cysharp.Threading.Tasks;
using RAXY.Animation;
using RAXY.Core;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

public abstract class UnitControllerBase : MonoBehaviour, ISepObject
{
    #region ISepObject
    public GameObject GetGameObject => gameObject;

    public bool FirstInitDone { get; set; }
    public int Order { get; set; }
    public string SepGroup { get; set; }
    public bool UsePreInit { get; set; }

    public virtual async UniTask Init()
    {
        MovementCont = GetComponent<UnitMovement>();
        AnimancerCont = GetComponent<AnimancerController>();

        StateMachine_Exploration = new UnitStateMachine_Exploration(this);

        FirstInitDone = true;
    }

    public async UniTask PreInit()
    {
    }
    #endregion

    public virtual string UnitName { get; }
    public virtual string UnitId => name;
    
    public UnitMovement MovementCont { get; set; }
    public AnimancerController AnimancerCont { get; set; }

    bool explorationUpdatesEnabled = true;

    public virtual UnitAnimationClipsSO AnimationClips { get; set; }
    public virtual UnitDataSO UnitData => null;
    
    [TitleGroup("Exploration")]
    [ShowInInspector]
    public BrainExplorationBase Brain_Exploration { get; set; }

    [TitleGroup("Exploration")]
    [ShowInInspector]
    public UnitStateMachine_Exploration StateMachine_Exploration { get; set; }

    public void Setup_BrainExploration(BrainExplorationType brainType, BrainExplorationConfigBaseSO config)
    {
        if (brainType == BrainExplorationType.ActiveUnit)
        {
            Brain_Exploration = new ActiveUnitBrainExploration(this, 
                                                                config as 
                                                                ActiveUnitBrainExplorationConfigSO,
                                                                Camera.main.transform);

            if (StateMachine_Exploration != null)
                StateMachine_Exploration.Brain = Brain_Exploration;
        }
    }

    public void SetExplorationUpdatesEnabled(bool isEnabled)
    {
        explorationUpdatesEnabled = isEnabled;
    }

    protected virtual void Update()
    {
        if (!explorationUpdatesEnabled)
            return;

        Brain_Exploration?.Update();
        StateMachine_Exploration?.CurrentState?.Update();
    }

    protected virtual void OnDestroy()
    {
        Brain_Exploration?.OnDestroy();
    }
}
