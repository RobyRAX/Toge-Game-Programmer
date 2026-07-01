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

    public void Setup(AttributeEntry entry, int currentLevel, int previewLevel, int maxTalentLevel)
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
            currentValueTmp.text = FormatDamageProfile(currentProfile);

        bool showNext = previewLevel <= maxTalentLevel && currentLevel < maxTalentLevel;

        if (nextValueRoot != null)
            nextValueRoot.SetActive(showNext);

        if (nextValueTmp != null)
            nextValueTmp.text = showNext ? FormatDamageProfile(nextProfile) : string.Empty;
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

    static string FormatDamageProfile(DamageProfile profile)
    {
        if (profile == null)
            return "-";

        return $"{Mathf.RoundToInt(profile.flatDamage)} + {Mathf.RoundToInt(profile.multiplierDamage)}%";
    }
}
