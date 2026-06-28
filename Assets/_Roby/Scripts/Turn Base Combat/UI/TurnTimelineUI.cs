using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class TurnTimelineUI : MonoBehaviour
{
    [TitleGroup("UI Ref")]
    [SerializeField]
    Image currentTurnUnitIconImg;

    [TitleGroup("UI Ref")]
    [SerializeField]
    List<TurnTimelineStepUI> steps;

    TurnBaseCombatManager manager;

    public void Setup(TurnBaseCombatManager manager)
    {
        this.manager = manager;
        Refresh();
    }

    public void Refresh()
    {
        if (manager == null)
            return;

        var current = manager.CurrentCombatant;
        if (currentTurnUnitIconImg != null)
        {
            currentTurnUnitIconImg.sprite = current?.CombatantInfo?.unitIcon;
            currentTurnUnitIconImg.enabled = currentTurnUnitIconImg.sprite != null;
        }

        var preview = manager.TurnTimeline?.PreviewSteps;
        if (preview == null || steps == null)
            return;

        for (int i = 0; i < steps.Count; i++)
        {
            var stepUi = steps[i];
            if (stepUi == null)
                continue;

            if (i < preview.Count)
            {
                stepUi.gameObject.SetActive(true);
                stepUi.Refresh(preview[i]);
            }
            else
            {
                stepUi.gameObject.SetActive(false);
            }
        }
    }
}
