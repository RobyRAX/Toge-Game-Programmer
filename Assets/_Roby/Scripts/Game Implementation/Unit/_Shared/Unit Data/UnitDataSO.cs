using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

public abstract class UnitDataSO : ScriptableObject
{
    [SerializeField]
    protected string unitId;

    [SerializeField]
    protected string unitName;

    [SerializeField]
    protected Sprite unitIcon;

    public GameObject unitPrefab;

    [TitleGroup("Animations")]
    [HideLabel]
    public UnitAnimationClipsSO AnimationClipsSO;

    public abstract CombatDataBaseSO CombatDataSO { get; }

    [PropertyOrder(2)]
    [TitleGroup("Stat Growth")]
    [HideLabel]
    public StatGrowth StatGrowth;
}
