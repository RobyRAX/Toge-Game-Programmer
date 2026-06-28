using System;
using UnityEngine;
using UnityEngine.UI;

public class DefeatScreenUI : MonoBehaviour
{
    public Button respawnBtn;

    public event Action OnRespawnClicked;

    public void Awake()
    {
        respawnBtn.onClick.AddListener(RespawnClickedHandler);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void RespawnClickedHandler()
    {
        OnRespawnClicked?.Invoke();
    }
}