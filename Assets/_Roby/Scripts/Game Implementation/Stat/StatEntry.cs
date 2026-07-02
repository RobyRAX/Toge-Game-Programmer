
using System;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

[Serializable]
public class StatEntry
{
    public StatAttribute attribute;
    public float defaultValue;
    public bool isRound;

    [Title("Details")]
    public string statName;
    public string statDetail;
    public string abbreviation;
    public Sprite statIcon;
}