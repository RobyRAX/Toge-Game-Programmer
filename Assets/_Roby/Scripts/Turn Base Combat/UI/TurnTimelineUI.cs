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

    public void Setup(TurnBaseCombatManager manager)
    {
        
    }
}
