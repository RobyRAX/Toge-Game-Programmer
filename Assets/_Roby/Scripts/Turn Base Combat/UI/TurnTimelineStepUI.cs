using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class TurnTimelineStepUI : MonoBehaviour
{
    public TurnStep Step { get; set; }

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

    public void Setup(TurnStep step)
    {
        Step = step;
    }
}
