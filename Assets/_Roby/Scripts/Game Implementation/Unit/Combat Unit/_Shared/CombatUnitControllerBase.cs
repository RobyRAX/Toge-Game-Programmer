using System;
using UnityEngine;

public class CombatUnitController : UnitControllerBase
{
    [SerializeField]
    UnitAnimationClipsSO animationClips;
    public override UnitAnimationClipsSO AnimationClips => animationClips;
    public virtual CombatantBase CombatantCont { get; set; }

    public event Action OnAttacked;
    public void Invoke_OnAttacked() => OnAttacked?.Invoke();
}
