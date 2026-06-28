using Sirenix.OdinInspector;
using TMPro;
using ToGaProTest.Shared;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUnitStatusUI : MonoBehaviour
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
    Slider staminaBarSlider;

    [TitleGroup("UI Ref")]
    [SerializeField]
    Image ultimateGaugeImg;

    [TitleGroup("UI Ref")]
    [SerializeField]
    GameObject currentTurnIndicator;

    public void Setup(CombatantBase combatant)
    {
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

        RefreshStats();
        SetCurrent(false);
    }

    public void RefreshStats()
    {
        if (Combatant?.StatContainer == null)
            return;

        float maxHp = Combatant.StatContainer.GetTotalValue(StatAttribute.MaxHp);
        if (hpBarSlider != null)
        {
            hpBarSlider.maxValue = maxHp;
            hpBarSlider.value = Combatant.DisplayedHp;
        }

        float maxStamina = Combatant.StatContainer.GetTotalValue(StatAttribute.MaxStamina);
        if (staminaBarSlider != null)
        {
            staminaBarSlider.maxValue = maxStamina;
            staminaBarSlider.value = Combatant.CurrentStamina;
        }

        if (ultimateGaugeImg != null)
        {
            if (Combatant is HeroCombatant hero)
            {
                ultimateGaugeImg.gameObject.SetActive(true);
                ultimateGaugeImg.fillAmount = hero.CurrentUltimateGauge / HeroCombatant.MaxUltimateGauge;
            }
            else
            {
                ultimateGaugeImg.gameObject.SetActive(false);
            }
        }
    }

    public void SetCurrent(bool isCurrent)
    {
        if (currentTurnIndicator != null)
            currentTurnIndicator.SetActive(isCurrent);
    }
}
