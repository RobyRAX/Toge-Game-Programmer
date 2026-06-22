using System.Collections.Generic;
using ToGaProTest.Shared;
using UnityEngine;

public interface IStatDataModifierProvider
{
    public List<StatAttributeWithValue> StatAttributeModifiers { get; }
}
