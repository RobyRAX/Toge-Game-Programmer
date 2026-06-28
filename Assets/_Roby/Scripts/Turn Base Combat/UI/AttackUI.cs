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

        RefreshInteractable();
    }

    public void RefreshInteractable()
    {
        if (button == null || manager == null || attack == null)
            return;

        button.interactable = manager.CanAffordAttack(manager.CurrentCombatant, attack);
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
