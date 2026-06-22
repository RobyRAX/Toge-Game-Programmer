
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class CombatantBase : MonoBehaviour
{
    [TitleGroup("Current Value")]
    [ShowInInspector]
    public virtual float CurrentHp { get; set; }

    [TitleGroup("Current Value")]
    [ShowInInspector]
    public virtual float CurrentStamina { get; set; }

    [TitleGroup("Stat")]
    [ShowInInspector]
    [HideReferenceObjectPicker]
    [HideLabel]
    public virtual StatContainer_Runtime StatContainer { get; set; }
}