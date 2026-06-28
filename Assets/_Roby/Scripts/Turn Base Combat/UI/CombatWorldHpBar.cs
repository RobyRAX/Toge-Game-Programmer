using Sirenix.OdinInspector;
using TMPro;
using ToGaProTest.Shared;
using UnityEngine;
using UnityEngine.UI;

public class CombatWorldHpBar : MonoBehaviour
{
    public CombatantBase Combatant { get; private set; }

    [TitleGroup("UI Ref")]
    [SerializeField]
    Slider hpBarSlider;

    [TitleGroup("UI Ref")]
    [SerializeField]
    TextMeshProUGUI unitNameTmp;

    [TitleGroup("UI Ref")]
    [SerializeField]
    TextMeshProUGUI levelTmp;

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

        if (levelTmp != null)
            levelTmp.text = $"Lv {combatant.Level}";

        Refresh();
    }

    public void Refresh()
    {
        if (Combatant?.StatContainer == null)
            return;

        float maxHp = Combatant.StatContainer.GetTotalValue(StatAttribute.MaxHp);
        if (hpBarSlider != null)
        {
            hpBarSlider.maxValue = maxHp;
            hpBarSlider.value = Combatant.DisplayedHp;
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    void LateUpdate()
    {
        if (Camera.main == null)
            return;

        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }
}
