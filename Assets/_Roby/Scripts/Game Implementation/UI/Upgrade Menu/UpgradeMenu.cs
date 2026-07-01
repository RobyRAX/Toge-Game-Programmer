using System.Collections.Generic;
using RAXY.InputSystem;
using Sirenix.OdinInspector;
using TMPro;
using ToGaProTest.Shared;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeMenu : MonoBehaviour
{
#if UNITY_EDITOR
    [TitleGroup("Editor")]
    [EnumToggleButtons]
    [HideLabel]
    public UpgradeMenuTab menuTab;
    
    public bool IsOverviewTab => menuTab == UpgradeMenuTab.Overview;
    public bool IsTalentTab => menuTab == UpgradeMenuTab.Talent;
#endif

    [TitleGroup("UI Ref")]
    [SerializeField]
    [LabelText("Hero Item Container")]
    Transform heroItemContainer;

    [TitleGroup("UI Ref")]
    [SerializeField]
    [LabelText("Overview Btn")]
    Button overviewBtn;

    [TitleGroup("UI Ref")]
    [SerializeField]
    [LabelText("Talent Btn")]
    Button talentBtn;

    [TitleGroup("UI Ref")]
    [SerializeField]
    [LabelText("Close Btn")]
    Button closeBtn;

    [TitleGroup("UI Ref")]
    [SerializeField]
    [LabelText("Overview Root")]
    Transform overviewRoot;

    [TitleGroup("UI Ref")]
    [SerializeField]
    [LabelText("Talent Root")]
    Transform talentRoot;

    [TitleGroup("Prefab Ref")]
    [SerializeField]
    [LabelText("Hero Item UI Prefab")]
    HeroItemUI heroItemUiPrefab;

    [TitleGroup("Overview UI Ref")]
    [SerializeField]
    [LabelText("Hero Name Tmp")]
    [ShowIf("@IsOverviewTab")]
    TextMeshProUGUI overview_heroNameTmp;

    [TitleGroup("Overview UI Ref")]
    [SerializeField]
    [LabelText("Level Tmp")]
    [ShowIf("@IsOverviewTab")]
    TextMeshProUGUI overview_levelTmp;

    [TitleGroup("Overview UI Ref")]
    [SerializeField]
    [LabelText("Exp Slider")]
    [ShowIf("@IsOverviewTab")]
    Slider overview_expSlider;

    [TitleGroup("Overview UI Ref")]
    [SerializeField]
    [LabelText("Exp Tmp")]
    [ShowIf("@IsOverviewTab")]
    TextMeshProUGUI overview_expTmp;

    [TitleGroup("Overview UI Ref")]
    [SerializeField]
    [LabelText("Normal Attack Talent UI")]
    [ShowIf("@IsOverviewTab")]
    [PropertySpace(5)]
    TalentUI overview_talentNormalAttackUI;

    [TitleGroup("Overview UI Ref")]
    [SerializeField]
    [LabelText("Skill Talent UI")]
    [ShowIf("@IsOverviewTab")]
    TalentUI overview_talentSkillUI;

    [TitleGroup("Overview UI Ref")]
    [SerializeField]
    [LabelText("Ultimate Talent UI")]
    [ShowIf("@IsOverviewTab")]
    TalentUI overview_talentUltimateUI;

    [TitleGroup("Overview UI Ref")]
    [SerializeField]
    [LabelText("Max HP Stat UI")]
    [ShowIf("@IsOverviewTab")]
    [PropertySpace(5)]
    StatUI overview_maxHpStatUI;

    [TitleGroup("Overview UI Ref")]
    [SerializeField]
    [LabelText("Attack Stat UI")]
    [ShowIf("@IsOverviewTab")]
    StatUI overview_attackStatUI;

    [TitleGroup("Overview UI Ref")]
    [SerializeField]
    [LabelText("Defense Stat UI")]
    [ShowIf("@IsOverviewTab")]
    StatUI overview_defenseStatUI;

    [TitleGroup("Overview UI Ref")]
    [SerializeField]
    [LabelText("Max Stamina Stat UI")]
    [ShowIf("@IsOverviewTab")]
    StatUI overview_maxStaminaStatUI;

    [TitleGroup("Overview UI Ref")]
    [SerializeField]
    [LabelText("Stamina Regen Stat UI")]
    [ShowIf("@IsOverviewTab")]
    StatUI overview_staminaRegenStatUI;

    [TitleGroup("Overview UI Ref")]
    [SerializeField]
    [LabelText("Attack Speed Stat UI")]
    [ShowIf("@IsOverviewTab")]
    StatUI overview_attackSpeedStatUI;

    [TitleGroup("Talent UI Ref")]
    [SerializeField]
    [LabelText("Normal Attack Talent UI")]
    [ShowIf("@IsTalentTab")]
    UpgradeTalentUI talent_talentNormalAttackUI;

    [TitleGroup("Talent UI Ref")]
    [SerializeField]
    [LabelText("Skill Talent UI")]
    [ShowIf("@IsTalentTab")]
    UpgradeTalentUI talent_talentSkillUI;

    [TitleGroup("Talent UI Ref")]
    [SerializeField]
    [LabelText("Ultimate Talent UI")]
    [ShowIf("@IsTalentTab")]
    UpgradeTalentUI talent_talentUltimateUI;

    [TitleGroup("Talent UI Ref")]
    [SerializeField]
    [LabelText("Available Talent Point Tmp")]
    [ShowIf("@IsTalentTab")]
    TextMeshProUGUI availableTalentPointTmp;
    
    [TitleGroup("Talent UI Ref")]
    [SerializeField]
    [LabelText("Attack Attribute Container")]
    [ShowIf("@IsTalentTab")]
    Transform talent_attackAttributeContainer;

    [TitleGroup("Talent Prefab Ref")]
    [SerializeField]
    [LabelText("Attack Attribute UI Prefab")]
    [ShowIf("@IsTalentTab")]
    AttackAttributeUI talent_attackAttributeUiPrefab;

    GameplayManager manager;
    readonly List<HeroItemUI> heroSlots = new();
    readonly List<AttackAttributeUI> attackAttributeSlots = new();
    readonly List<ItemInstance_Hero> heroInstances = new();

    ItemInstance_Hero selectedHero;
    HeroTalentType selectedTalentType = HeroTalentType.NormalAttack;
    UpgradeMenuTab activeTab = UpgradeMenuTab.Overview;

    InputActionEventSO closeInputEventSO;
    int openedOnFrame = -1;
    bool suspendedHeroesForMenu;
    bool hidExploreUiForMenu;

    public bool IsOpen => gameObject.activeSelf;

    public void Setup(GameplayManager manager, InputActionEventSO closeInputEventSO)
    {
        Teardown();

        this.manager = manager;
        this.closeInputEventSO = closeInputEventSO;

        HeroProgression.OnHeroChanged -= HandleHeroChanged;
        HeroProgression.OnHeroChanged += HandleHeroChanged;

        if (overviewBtn != null)
        {
            overviewBtn.onClick.RemoveListener(ShowOverviewTab);
            overviewBtn.onClick.AddListener(ShowOverviewTab);
        }

        if (talentBtn != null)
        {
            talentBtn.onClick.RemoveListener(ShowTalentTab);
            talentBtn.onClick.AddListener(ShowTalentTab);
        }

        if (closeBtn != null)
        {
            closeBtn.onClick.RemoveListener(Close);
            closeBtn.onClick.AddListener(Close);
        }

        BuildHeroList();
        ShowOverviewTab();

        if (heroInstances.Count > 0)
            SelectHero(heroInstances[0]);
        else
            RefreshSelectedHero();
    }

    public void Open()
    {
        if (manager != null && manager.CurrentState != GameplayState.Explore)
            return;

        openedOnFrame = Time.frameCount;
        gameObject.SetActive(true);

        closeInputEventSO?.Unsubscribe(CloseInputHandler);
        closeInputEventSO?.Subscribe(CloseInputHandler);

        SetExplorationPaused(true);
        RefreshSelectedHero();
    }

    public void Close()
    {
        closeInputEventSO?.Unsubscribe(CloseInputHandler);
        SetExplorationPaused(false);
        gameObject.SetActive(false);
    }

    void CloseInputHandler(InputContext ctx)
    {
        if (!IsOpen)
            return;

        if (Time.frameCount == openedOnFrame)
            return;

        Close();
    }

    void SetExplorationPaused(bool paused)
    {
        if (manager == null)
            return;

        if (paused)
        {
            hidExploreUiForMenu = manager.exploreUI != null && manager.exploreUI.gameObject.activeSelf;
            if (hidExploreUiForMenu)
                manager.exploreUI.Hide();

            suspendedHeroesForMenu = manager.MainHero != null && manager.MainHero.enabled;
            if (!suspendedHeroesForMenu || manager.SpawnedHeroDict == null)
                return;

            foreach (var hero in manager.SpawnedHeroDict.Values)
                hero?.SetSuspend(true);

            return;
        }

        if (suspendedHeroesForMenu && manager.SpawnedHeroDict != null)
        {
            foreach (var hero in manager.SpawnedHeroDict.Values)
                hero?.SetSuspend(false);

            suspendedHeroesForMenu = false;
        }

        if (hidExploreUiForMenu && manager.CurrentState == GameplayState.Explore)
            manager.exploreUI?.Show();

        hidExploreUiForMenu = false;
    }

    public void Teardown()
    {
        closeInputEventSO?.Unsubscribe(CloseInputHandler);

        HeroProgression.OnHeroChanged -= HandleHeroChanged;

        if (overviewBtn != null)
            overviewBtn.onClick.RemoveListener(ShowOverviewTab);

        if (talentBtn != null)
            talentBtn.onClick.RemoveListener(ShowTalentTab);

        if (closeBtn != null)
            closeBtn.onClick.RemoveListener(Close);

        ClearHeroList();
        ClearAttackAttributeList();

        talent_talentNormalAttackUI?.Teardown();
        talent_talentSkillUI?.Teardown();
        talent_talentUltimateUI?.Teardown();

        if (IsOpen)
            SetExplorationPaused(false);

        closeInputEventSO = null;
        manager = null;
        selectedHero = null;
    }

    void BuildHeroList()
    {
        ClearHeroList();
        heroInstances.Clear();

        if (manager?.partyIds == null || heroItemContainer == null || heroItemUiPrefab == null)
            return;

        foreach (var heroId in manager.partyIds)
        {
            if (string.IsNullOrEmpty(heroId))
                continue;

            if (manager.SpawnedHeroDict == null ||
                !manager.SpawnedHeroDict.ContainsKey(heroId))
                continue;

            var hero = InventoryManager.Instance?.GetInstanceHero(heroId);
            if (hero == null)
                continue;

            heroInstances.Add(hero);

            var slot = Instantiate(heroItemUiPrefab, heroItemContainer);
            var capturedHero = hero;
            slot.Setup(hero, () => SelectHero(capturedHero));
            heroSlots.Add(slot);
        }
    }

    void ClearHeroList()
    {
        foreach (var slot in heroSlots)
        {
            if (slot != null)
            {
                slot.Teardown();
                Destroy(slot.gameObject);
            }
        }

        heroSlots.Clear();
    }

    void ClearAttackAttributeList()
    {
        foreach (var slot in attackAttributeSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }

        attackAttributeSlots.Clear();
    }

    void SelectHero(ItemInstance_Hero hero)
    {
        selectedHero = hero;

        for (int i = 0; i < heroInstances.Count; i++)
        {
            if (heroSlots.Count <= i)
                break;

            heroSlots[i]?.SetSelected(heroInstances[i] == hero);
        }

        RefreshSelectedHero();
    }

    void ShowOverviewTab()
    {
        activeTab = UpgradeMenuTab.Overview;

        if (overviewRoot != null)
            overviewRoot.gameObject.SetActive(true);

        if (talentRoot != null)
            talentRoot.gameObject.SetActive(false);
    }

    void ShowTalentTab()
    {
        activeTab = UpgradeMenuTab.Talent;

        if (overviewRoot != null)
            overviewRoot.gameObject.SetActive(false);

        if (talentRoot != null)
            talentRoot.gameObject.SetActive(true);

        RefreshTalentTab();
    }

    void HandleHeroChanged(ItemInstance_Hero hero)
    {
        if (hero == null)
            return;

        if (selectedHero == hero)
            RefreshSelectedHero();
    }

    void RefreshSelectedHero()
    {
        if (selectedHero == null)
            return;

        if (activeTab == UpgradeMenuTab.Overview)
            RefreshOverviewTab();
        else
            RefreshTalentTab();
    }

    void RefreshOverviewTab()
    {
        var hero = selectedHero;
        var combatData = hero.heroDataSO?.heroCombatDataSO;
        var statContainer = hero.GetStatContainer(true);
        var config = GameplayConfig.Instance;

        if (overview_heroNameTmp != null)
            overview_heroNameTmp.text = hero.heroDataSO?.ItemName ?? string.Empty;

        if (overview_levelTmp != null)
            overview_levelTmp.text = $"Lv. {hero.level}";

        var (currentExp, requiredExp) = HeroProgression.GetExpProgress(hero);
        bool isMaxLevel = GameplayConfig.Instance?.ConfigSO?.IsMaxLevel(hero.level) ?? false;

        if (overview_expSlider != null)
        {
            overview_expSlider.maxValue = isMaxLevel ? 1f : requiredExp;
            overview_expSlider.value = isMaxLevel ? 1f : currentExp;
        }

        if (overview_expTmp != null)
        {
            overview_expTmp.text = isMaxLevel
                ? "MAX"
                : $"{currentExp} / {requiredExp}";
        }

        if (statContainer != null && config != null)
            RefreshOverviewStats(statContainer, config);

        overview_talentNormalAttackUI?.Setup(
            combatData?.NormalAttackTalent,
            hero.NormalAttackTalentLevel);

        overview_talentSkillUI?.Setup(
            combatData?.SkillTalent,
            hero.SkillTalentLevel);

        overview_talentUltimateUI?.Setup(
            combatData?.UltimateTalent,
            hero.UltimateTalentLevel);
    }

    void RefreshOverviewStats(StatContainer_Runtime statContainer, IStatEntryProvider provider)
    {
        overview_maxHpStatUI?.Setup(statContainer, provider);
        overview_attackStatUI?.Setup(statContainer, provider);
        overview_defenseStatUI?.Setup(statContainer, provider);
        overview_maxStaminaStatUI?.Setup(statContainer, provider);
        overview_staminaRegenStatUI?.Setup(statContainer, provider);
        overview_attackSpeedStatUI?.Setup(statContainer, provider);
    }

    void RefreshTalentTab()
    {
        var hero = selectedHero;
        var combatData = hero?.heroDataSO?.heroCombatDataSO;

        if (availableTalentPointTmp != null)
            availableTalentPointTmp.text = hero?.availableTalentPoints.ToString() ?? "0";

        SetupUpgradeTalentUI(
            talent_talentNormalAttackUI,
            hero,
            HeroTalentType.NormalAttack,
            combatData?.NormalAttackTalent,
            "Normal Attack");

        SetupUpgradeTalentUI(
            talent_talentSkillUI,
            hero,
            HeroTalentType.Skill,
            combatData?.SkillTalent,
            "Skill");

        SetupUpgradeTalentUI(
            talent_talentUltimateUI,
            hero,
            HeroTalentType.Ultimate,
            combatData?.UltimateTalent,
            "Ultimate");

        RefreshTalentSelectionIndicators();
        RefreshAttackAttributes();
    }

    void SetupUpgradeTalentUI(
        UpgradeTalentUI ui,
        ItemInstance_Hero hero,
        HeroTalentType type,
        Talent talentData,
        string displayName)
    {
        if (ui == null)
            return;

        ui.Setup(hero, type, talentData, displayName, HandleTalentSelected, RefreshTalentTab);
        ui.SetSelected(type == selectedTalentType);
    }

    void HandleTalentSelected(HeroTalentType type)
    {
        selectedTalentType = type;
        RefreshTalentSelectionIndicators();
        RefreshAttackAttributes();
    }

    void RefreshTalentSelectionIndicators()
    {
        talent_talentNormalAttackUI?.SetSelected(selectedTalentType == HeroTalentType.NormalAttack);
        talent_talentSkillUI?.SetSelected(selectedTalentType == HeroTalentType.Skill);
        talent_talentUltimateUI?.SetSelected(selectedTalentType == HeroTalentType.Ultimate);
    }

    void RefreshAttackAttributes()
    {
        ClearAttackAttributeList();

        if (selectedHero == null ||
            talent_attackAttributeContainer == null ||
            talent_attackAttributeUiPrefab == null)
            return;

        var combatData = selectedHero.heroDataSO?.heroCombatDataSO;
        var talentData = GetTalentData(combatData, selectedTalentType);
        var attackAttribute = talentData?.AttackAttribute;

        if (attackAttribute?.attributes == null)
            return;

        int currentLevel = HeroProgression.GetTalentLevel(selectedHero, selectedTalentType);
        int previewLevel = currentLevel + 1;
        int maxTalentLevel = GameplayConfig.Instance?.ConfigSO?.maxTalentLevel ?? 1;

        foreach (var entry in attackAttribute.attributes)
        {
            if (entry == null)
                continue;

            var ui = Instantiate(talent_attackAttributeUiPrefab, talent_attackAttributeContainer);
            ui.Setup(entry, currentLevel, previewLevel, maxTalentLevel);
            attackAttributeSlots.Add(ui);
        }
    }

    static Talent GetTalentData(HeroCombatDataSO combatData, HeroTalentType type)
    {
        if (combatData == null)
            return null;

        return type switch
        {
            HeroTalentType.NormalAttack => combatData.NormalAttackTalent,
            HeroTalentType.Skill => combatData.SkillTalent,
            HeroTalentType.Ultimate => combatData.UltimateTalent,
            _ => null
        };
    }

    void OnDestroy()
    {
        Teardown();
    }
}

public enum UpgradeMenuTab
{
    Overview, Talent
}
