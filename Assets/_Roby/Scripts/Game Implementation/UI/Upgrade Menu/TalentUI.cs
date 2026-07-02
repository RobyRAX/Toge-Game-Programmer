using Sirenix.OdinInspector;
using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TalentUI : MonoBehaviour
{
    [TitleGroup("UI Ref")]
    [SerializeField]
    Button button;

    [TitleGroup("UI Ref")]
    [SerializeField]
    Image talentIconImg;

    // [TitleGroup("UI Ref")]
    // [SerializeField]
    // TextMeshProUGUI talentNameTmp;

    [TitleGroup("UI Ref")]
    [SerializeField]
    TextMeshProUGUI talentLevelTmp;

    public void Setup(Talent talentData, int level, Action onClick = null)
    {
        if (talentIconImg != null)
        {
            talentIconImg.sprite = talentData?.talentIcon;
            talentIconImg.enabled = talentIconImg.sprite != null;
        }

        // if (talentNameTmp != null)
        //     talentNameTmp.text = displayName ?? string.Empty;

        if (talentLevelTmp != null)
        {
            int maxLevel = GameplayConfig.Instance?.ConfigSO?.maxTalentLevel ?? 1;
            talentLevelTmp.text = $"Lv. {level}/{maxLevel}";
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.interactable = onClick != null;
            if (onClick != null)
                button.onClick.AddListener(() => onClick.Invoke());
        }
    }
}
