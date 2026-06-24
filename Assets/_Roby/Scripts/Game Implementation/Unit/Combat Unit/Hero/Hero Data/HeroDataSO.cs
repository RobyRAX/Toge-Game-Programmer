using System;
using System.Collections.Generic;
using RAXY.Core.Addressable;
using RAXY.InventorySystem;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroDataSO", menuName = "RAXY/Unit/Hero/Hero Data")]
public class HeroDataSO : ScriptableObject, IItemEntry
{
    [SerializeField] 
    string heroId;

    [SerializeField]  
    string heroName;

    [SerializeField]  
    Sprite heroIcon;

    public GameObject heroPrefab;

    public string ItemId => heroId;
    public bool IsStackable => false;
    public string ItemName => heroName;
    public string ItemDescription => "";
    public string ItemAdditionalDescription => "";
    public Sprite ItemIcon => heroIcon;

    [TitleGroup("Animations")]
    [HideLabel]
    public UnitAnimationClipsSO AnimationClipsSO;

    [TitleGroup("Combat Data")]
    [HideLabel]
    public HeroCombatDataSO CombatDataSO;

    [TitleGroup("Stat Growth")]
    [HideLabel]
    public StatGrowth StatGrowth;
}