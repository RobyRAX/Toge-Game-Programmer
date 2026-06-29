using Sirenix.OdinInspector;
using TMPro;
using ToGaProTest.Shared;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUnitUI : MonoBehaviour
{
    public CombatantBase Combatant { get; private set; }

    [TitleGroup("UI Ref")]
    [SerializeField]
    TextMeshProUGUI unitNameTmp;

    [TitleGroup("UI Ref")]
    [SerializeField]
    Image unitIconImg;
    
    [TitleGroup("UI Ref")]
    [SerializeField]
    Slider hpBarSlider;

    [TitleGroup("UI Ref")]
    [SerializeField]
    GameObject currentMainUnitIndicator;

    public void Setup(CombatantBase combatant)
    {
        Teardown();

        Combatant = combatant;

        if (combatant == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (unitNameTmp != null)
            unitNameTmp.text = combatant.CombatantInfo?.unitName ?? combatant.name;

        if (unitIconImg != null)
        {
            unitIconImg.sprite = combatant.CombatantInfo?.unitIcon;
            unitIconImg.enabled = unitIconImg.sprite != null;
        }

        combatant.OnStatsChanged += HandleStatsChanged;

        RefreshStats();
        SetCurrentMainUnit(false);
    }

    public void RefreshStats()
    {
        if (Combatant?.StatContainer == null)
            return;

        if (hpBarSlider != null)
        {
            hpBarSlider.maxValue = Combatant.StatContainer.GetTotalValue(StatAttribute.MaxHp);
            hpBarSlider.value = Combatant.CurrentHp;
        }
    }

    public void SetCurrentMainUnit(bool isCurrent)
    {
        if (currentMainUnitIndicator != null)
            currentMainUnitIndicator.SetActive(isCurrent);
    }

    public void Teardown()
    {
        if (Combatant != null)
            Combatant.OnStatsChanged -= HandleStatsChanged;

        Combatant = null;
    }

    void HandleStatsChanged(CombatantBase combatant) => RefreshStats();

    void OnDestroy()
    {
        Teardown();
    }
}
