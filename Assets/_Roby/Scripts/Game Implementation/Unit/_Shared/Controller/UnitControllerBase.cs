using System;
using Cysharp.Threading.Tasks;
using RAXY.Animation;
using RAXY.Core;
using Sirenix.OdinInspector;
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
    }

    public async UniTask PreInit()
    {
    }
    #endregion

    public virtual string UnitName { get; }
    public virtual string UnitId => name;
    
    public UnitMovement MovementCont { get; set; }
    public AnimancerController AnimancerCont { get; set; }

    public virtual UnitAnimationClipsSO AnimationClips { get; }
    
    [TitleGroup("Exploration")]
    [ShowInInspector]
    public BrainExplorationBase Brain_Exploration { get; set; }

    [TitleGroup("Exploration")]
    [ShowInInspector]
    public UnitStateMachine_Exploration StateMachine_Exploration { get; set; }
}
