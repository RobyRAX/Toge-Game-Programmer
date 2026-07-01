using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using ToGaProTest.Shared;
using UnityEngine;
using UnityEngine.UI;

public class StatUI : MonoBehaviour
{
    [TitleGroup("Config")]
    [SerializeField]
    [LabelText("Stat Attribute")]
    [ValueDropdown(nameof(GetNonPercentStatAttributes))]
    StatAttribute statAttribute;

    [TitleGroup("UI Ref")]
    [SerializeField]
    Image statIconImg;

    [TitleGroup("UI Ref")]
    [SerializeField]
    TextMeshProUGUI statNameTmp;

    [TitleGroup("UI Ref")]
    [SerializeField]
    TextMeshProUGUI statValueTmp;

    // [TitleGroup("UI Ref")]
    // [SerializeField]
    // TextMeshProUGUI statDetailTmp;

    public StatAttribute StatAttribute => statAttribute;

    public bool IsNonPercentStat =>
        statAttribute != StatAttribute.None &&
        !ToGaProTestShared.StatPercentAttributes.Contains(statAttribute);

    public void Setup(StatContainer_Runtime container, IStatEntryProvider provider)
    {
        if (container == null || !IsNonPercentStat)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        ApplyDisplay(container.GetTotalValue(statAttribute), provider);
    }

    public void Setup(StatAttribute attr, float value, IStatEntryProvider provider)
    {
        if (attr != StatAttribute.None)
            statAttribute = attr;

        if (!IsNonPercentStat)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        ApplyDisplay(value, provider);
    }

    void ApplyDisplay(float value, IStatEntryProvider provider)
    {
        var entry = FindStatEntry(provider?.StatEntries, statAttribute);

        if (statNameTmp != null)
            statNameTmp.text = entry?.statName ?? statAttribute.ToString();

        // if (statDetailTmp != null)
        // {
        //     bool hasDetail = !string.IsNullOrEmpty(entry?.statDetail);
        //     statDetailTmp.gameObject.SetActive(hasDetail);
        //     statDetailTmp.text = hasDetail ? entry.statDetail : string.Empty;
        // }

        if (statIconImg != null)
        {
            statIconImg.sprite = entry?.statIcon;
            statIconImg.enabled = statIconImg.sprite != null;
        }

        if (statValueTmp != null)
            statValueTmp.text = FormatValue(value, statAttribute, entry);
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

    static IEnumerable<StatAttribute> GetNonPercentStatAttributes()
    {
        return ToGaProTestShared.MainStatAttributes;
    }
}
