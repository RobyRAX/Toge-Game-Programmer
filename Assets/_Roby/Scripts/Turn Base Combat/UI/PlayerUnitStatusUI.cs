using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUnitStatusUI : MonoBehaviour
{
    public CombatantBase Combatant { get; set; }

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
    }

    public void SetCurrent(bool isCurrent)
    {
        
    }
}
