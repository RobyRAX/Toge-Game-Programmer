using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using ToGaProTest.Shared;
using UnityEngine;
using UnityEngine.UI;

public class StatUI : MonoBehaviour
{
    [TitleGroup("UI Ref")]
    [SerializeField]
    Image statIconImg;

    [TitleGroup("UI Ref")]
    [SerializeField]
    TextMeshProUGUI statNameTmp;

    [TitleGroup("UI Ref")]
    [SerializeField]
    TextMeshProUGUI statValueTmp;

    public void Setup(StatAttribute attr, float value, IStatEntryProvider provider)
    {
        if (attr == StatAttribute.None || ToGaProTestShared.StatPercentAttributes.Contains(attr))
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        var entry = FindStatEntry(provider?.StatEntries, attr);

        if (statNameTmp != null)
            statNameTmp.text = entry?.statName ?? attr.ToString();

        if (statIconImg != null)
        {
            statIconImg.sprite = entry?.statIcon;
            statIconImg.enabled = statIconImg.sprite != null;
        }

        if (statValueTmp != null)
            statValueTmp.text = FormatValue(value, attr, entry);
    }

    static string FormatValue(float value, StatAttribute attr, StatEntry entry)
    {
        bool round = entry?.isRound ?? ToGaProTestShared.ShouldRoundStat_Static(attr);
        return round ? Mathf.RoundToInt(value).ToString() : value.ToString("0.#");
    }

    static StatEntry FindStatEntry(List<StatEntry> entries, StatAttribute attr)
    {
        if (entries == null)
            return null;

        foreach (var entry in entries)
        {
            if (entry != null && entry.attribute == attr)
                return entry;
        }

        return null;
    }
}
