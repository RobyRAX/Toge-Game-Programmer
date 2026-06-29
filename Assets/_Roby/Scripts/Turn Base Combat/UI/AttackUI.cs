using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttackUI : MonoBehaviour
{
    [SerializeField]
    Button button;

    [SerializeField]
    TextMeshProUGUI attackNameTmp;

    [SerializeField]
    Image attackIconImg;

    [SerializeField]
    TextMeshProUGUI staminaCostTmp;

    [SerializeField]
    GameObject lockedOverlay;

    [SerializeField]
    Image gaugeFillImg;

    [SerializeField]
    Color affordableStaminaCostColor = Color.white;

    [SerializeField]
    Color unaffordableStaminaCostColor = new Color(1f, 0.35f, 0.35f, 1f);

    [SerializeField]
    Color affordableUltimateCostColor = new Color(1f, 0.85f, 0.35f, 1f);

    [SerializeField]
    Color unaffordableUltimateCostColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    Attack_Runtime attack;
    TurnBaseCombatManager manager;

    public void Setup(TurnBaseCombatManager manager, Attack_Runtime attack)
    {
        this.manager = manager;
        this.attack = attack;

        if (attack?.AttackSO != null)
        {
            if (attackNameTmp != null)
                attackNameTmp.text = attack.AttackSO.GetDisplayName();

            if (attackIconImg != null)
            {
                attackIconImg.sprite = attack.AttackSO.displayIcon;
                attackIconImg.enabled = attackIconImg.sprite != null;
            }
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }

        RefreshCostDisplay();
        RefreshInteractable();
    }

    public void RefreshInteractable()
    {
        if (button == null || manager == null || attack == null)
            return;

        bool canAfford = manager.CanAffordAttack(manager.CurrentCombatant, attack);
        button.interactable = canAfford;

        if (lockedOverlay != null)
            lockedOverlay.SetActive(attack.IsUltimateAttack && !canAfford);

        RefreshGaugeFill();
        RefreshCostDisplay();
    }

    void RefreshGaugeFill()
    {
        if (gaugeFillImg == null)
            return;

        bool showGauge = attack != null && attack.IsUltimateAttack;
        gaugeFillImg.gameObject.SetActive(showGauge);

        if (!showGauge || manager?.CurrentCombatant is not HeroCombatant hero)
            return;

        gaugeFillImg.fillAmount = hero.CurrentUltimateGauge / HeroCombatant.MaxUltimateGauge;
    }

    void RefreshCostDisplay()
    {
        if (staminaCostTmp == null || attack == null)
            return;

        bool canAfford = manager != null &&
                         manager.CanAffordAttack(manager.CurrentCombatant, attack);

        if (attack.IsUltimateAttack)
        {
            staminaCostTmp.text = "ULT";
            staminaCostTmp.color = canAfford
                ? affordableUltimateCostColor
                : unaffordableUltimateCostColor;
            return;
        }

        int cost = attack.StaminaCost;
        if (cost <= 0)
        {
            staminaCostTmp.text = "";
            return;
        }

        staminaCostTmp.text = $"{cost} STA";
        staminaCostTmp.color = canAfford
            ? affordableStaminaCostColor
            : unaffordableStaminaCostColor;
    }

    void OnClick()
    {
        if (manager == null || attack == null)
            return;

        manager.SubmitSelectedAttack(attack);
    }

    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveAllListeners();
    }
}
