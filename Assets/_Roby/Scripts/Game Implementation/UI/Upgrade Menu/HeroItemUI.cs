using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class HeroItemUI : MonoBehaviour
{
    [TitleGroup("UI Ref")]
    [SerializeField]
    Image heroIconImg;

    [TitleGroup("UI Ref")]
    [SerializeField]
    Button button;

    [TitleGroup("UI Ref")]
    [SerializeField]
    GameObject selectedIndicator;

    Action onClick;

    public void Setup(ItemInstance_Hero hero, Action onClick)
    {
        Teardown();

        this.onClick = onClick;

        if (hero?.heroDataSO == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (heroIconImg != null)
        {
            heroIconImg.sprite = hero.heroDataSO.ItemIcon;
            heroIconImg.enabled = heroIconImg.sprite != null;
        }

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedIndicator != null)
            selectedIndicator.SetActive(selected);
    }

    void HandleClick() => onClick?.Invoke();

    public void Teardown()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);

        onClick = null;
    }

    void OnDestroy()
    {
        Teardown();
    }
}
