using System;
using Sirenix.OdinInspector;
using TMPro;
using ToGaProTest.Shared;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeTalentUI : MonoBehaviour
{
    [TitleGroup("UI Ref")]
    [SerializeField]
    Button selectButton;

    [TitleGroup("UI Ref")]
    [SerializeField]
    Button upgradeButton;

    [TitleGroup("UI Ref")]
    [SerializeField]
    Image talentIconImg;

    [TitleGroup("UI Ref")]
    [SerializeField]
    TextMeshProUGUI talentLevelTmp;

    [TitleGroup("UI Ref")]
    [SerializeField]
    GameObject selectedIndicator;

    ItemInstance_Hero hero;
    HeroTalentType talentType;
    Action<HeroTalentType> onSelected;
    Action onUpgraded;

    public void Setup(
        ItemInstance_Hero hero,
        HeroTalentType type,
        Talent talentData,
        string displayName,
        Action<HeroTalentType> onSelected,
        Action onUpgraded)
    {
        Teardown();

        this.hero = hero;
        talentType = type;
        this.onSelected = onSelected;
        this.onUpgraded = onUpgraded;

        if (talentIconImg != null)
        {
            talentIconImg.sprite = talentData?.talentIcon;
            talentIconImg.enabled = talentIconImg.sprite != null;
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(HandleSelect);
            selectButton.onClick.AddListener(HandleSelect);
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(HandleUpgrade);
            upgradeButton.onClick.AddListener(HandleUpgrade);
        }

        Refresh(talentData);
    }

    public void Refresh(Talent talentData)
    {
        int level = HeroProgression.GetTalentLevel(hero, talentType);

        if (talentLevelTmp != null)
            talentLevelTmp.text = $"Lv. {level}";

        if (upgradeButton != null)
            upgradeButton.interactable = HeroProgression.CanUpgradeTalent(hero, talentType);
    }

    public void SetSelected(bool selected)
    {
        if (selectedIndicator != null)
            selectedIndicator.SetActive(selected);
    }

    void HandleSelect() => onSelected?.Invoke(talentType);

    void HandleUpgrade()
    {
        if (!HeroProgression.TryUpgradeTalent(hero, talentType))
            return;

        onUpgraded?.Invoke();
    }

    public void Teardown()
    {
        if (selectButton != null)
            selectButton.onClick.RemoveListener(HandleSelect);

        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(HandleUpgrade);

        hero = null;
        onSelected = null;
        onUpgraded = null;
    }

    void OnDestroy()
    {
        Teardown();
    }
}
