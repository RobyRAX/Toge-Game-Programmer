using System;
using UnityEngine;

public class CombatUnitController : UnitControllerBase
{
    public virtual CombatantBase CombatantCont { get; set; }

    public event Action OnAttacked;
    public void Invoke_OnAttacked() => OnAttacked?.Invoke();
}
