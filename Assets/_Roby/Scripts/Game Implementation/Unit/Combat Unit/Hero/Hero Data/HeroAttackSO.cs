using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroAttackSO", menuName = "RAXY/Unit/Hero/Attack SO")]
public class HeroAttackSO : CombatAttackBaseSO
{
    [TitleGroup("Damage Profile")]
    [HideLabel]
    public HeroDamageProfileProvider damageProfileProvider;

    public override DamageProfileWithAttribute DamageProfile => null;

#if UNITY_EDITOR
    [TitleGroup("Editor")]
    public HeroCombatDataSO heroCombatDataSO_editorData;

#endif
}

