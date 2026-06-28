using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class TurnTimelineStepUI : MonoBehaviour
{
    public TurnStep Step { get; private set; }

    [TitleGroup("UI Ref")]
    [SerializeField]
    [ListDrawerSettings(ShowIndexLabels = true)]
    List<GameObject> unitIconPivots;

    [TitleGroup("UI Ref")]
    [SerializeField]
    [ListDrawerSettings(ShowIndexLabels = true)]
    List<Image> unitIconImgs;

    [TitleGroup("Setting")]
    [SerializeField]
    float containUnitWidth = 50;

    [TitleGroup("Setting")]
    [SerializeField]
    float noContainUnitWidth = 15;

    RectTransform rectTransform;

    void Awake()
    {
        rectTransform = (RectTransform)transform;
    }

    public void Refresh(TurnStep step)
    {
        Step = step;
        bool hasCombatants = step.Combatants != null && step.Combatants.Count > 0;
        int combatantCount = hasCombatants ? step.Combatants.Count : 0;

        if (rectTransform != null)
        {
            float targetWidth = hasCombatants ? containUnitWidth : noContainUnitWidth;
            var size = rectTransform.sizeDelta;
            size.x = targetWidth;
            rectTransform.sizeDelta = size;
        }

        if (unitIconPivots == null)
            return;

        for (int i = 0; i < unitIconPivots.Count; i++)
        {
            var pivot = unitIconPivots[i];
            if (pivot == null)
                continue;

            if (!hasCombatants || i >= combatantCount)
            {
                pivot.SetActive(false);
                continue;
            }

            pivot.SetActive(true);

            if (unitIconImgs == null || i >= unitIconImgs.Count)
                continue;

            var iconImg = unitIconImgs[i];
            if (iconImg == null)
                continue;

            var combatant = step.Combatants[i];
            iconImg.sprite = combatant?.CombatantInfo?.unitIcon;
            iconImg.enabled = iconImg.sprite != null;
        }
    }
}
