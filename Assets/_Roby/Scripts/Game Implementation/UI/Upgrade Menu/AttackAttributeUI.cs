using Sirenix.OdinInspector;
using TMPro;
using ToGaProTest.Shared;
using UnityEngine;

public class AttackAttributeUI : MonoBehaviour
{
    [TitleGroup("UI Ref")]
    [SerializeField]
    TextMeshProUGUI attributeNameTmp;

    [TitleGroup("UI Ref")]
    [SerializeField]
    TextMeshProUGUI currentValueTmp;

    [TitleGroup("UI Ref")]
    [SerializeField]
    TextMeshProUGUI nextValueTmp;

    [TitleGroup("UI Ref")]
    [SerializeField]
    GameObject nextValueRoot;

    public void Setup(
        AttributeEntry entry,
        int currentLevel,
        int previewLevel,
        int maxTalentLevel,
        bool hasAvailableTalentPoints,
        IStatEntryProvider statEntryProvider)
    {
        if (entry == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (attributeNameTmp != null)
            attributeNameTmp.text = string.IsNullOrEmpty(entry.attributeName) ? entry.attributeId : entry.attributeName;

        var currentProfile = GetProfileAtLevel(entry, currentLevel);
        var nextProfile = GetProfileAtLevel(entry, previewLevel);

        if (currentValueTmp != null)
            currentValueTmp.text = FormatDamageProfile(currentProfile, entry.multiplierAttribute, statEntryProvider);

        bool canPreviewNextLevel = previewLevel <= maxTalentLevel && currentLevel < maxTalentLevel;
        bool showNext = hasAvailableTalentPoints && canPreviewNextLevel;

        if (nextValueRoot != null)
            nextValueRoot.SetActive(showNext);

        if (nextValueTmp != null)
            nextValueTmp.text = showNext
                ? FormatDamageProfile(nextProfile, entry.multiplierAttribute, statEntryProvider)
                : string.Empty;
    }

    static DamageProfile GetProfileAtLevel(AttributeEntry entry, int level)
    {
        if (entry?.damageProfiles == null || entry.damageProfiles.Count == 0)
            return null;

        int index = level - 1;
        if (index < 0 || index >= entry.damageProfiles.Count)
            return null;

        return entry.damageProfiles[index];
    }

    static string FormatDamageProfile(DamageProfile profile, StatAttribute multiplierAttribute, IStatEntryProvider statEntryProvider)
    {
        if (profile == null)
            return "-";

        int flat = Mathf.RoundToInt(profile.flatDamage);
        int multiplier = Mathf.RoundToInt(profile.multiplierDamage);

        if (flat == 0 && multiplier == 0)
            return "-";

        if (multiplier == 0)
            return flat.ToString();

        string abbr = ResolveStatAbbreviation(statEntryProvider, multiplierAttribute);
        string multiplierText = string.IsNullOrEmpty(abbr)
            ? $"{multiplier}%"
            : $"{multiplier}% {abbr}";

        return flat == 0
            ? multiplierText
            : $"{flat} + {multiplierText}";
    }

    static string ResolveStatAbbreviation(IStatEntryProvider provider, StatAttribute attr)
    {
        var entry = FindStatEntry(provider?.StatEntries, attr);
        if (!string.IsNullOrEmpty(entry?.abbreviation))
            return entry.abbreviation;

        return attr.ToString();
    }

    static StatEntry FindStatEntry(System.Collections.Generic.List<StatEntry> entries, StatAttribute attr)
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
