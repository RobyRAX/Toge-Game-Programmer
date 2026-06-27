using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnPhaseTimelineUI : MonoBehaviour
{
    [TitleGroup("UI Ref")]
    [SerializeField]
    [ListDrawerSettings(ShowIndexLabels = true)]
    List<PhaseUiRef> phaseUiRefs;

    [TitleGroup("Setting")]
    [SerializeField]
    Color finishedColor = Color.black;

    [TitleGroup("Setting")]
    [SerializeField]
    Color currentColor = Color.white;

    [TitleGroup("Setting")]
    [SerializeField]
    Color upcomingColor = Color.grey;

    public void Setup(TurnBaseCombatManager manager)
    {
        
    }

    public void SetPhase(TurnBaseCombatPhase phase)
    {
        
    }
}

[Serializable]
public class PhaseUiRef
{
    public Image phaseIconImg;
    public TextMeshProUGUI phaseNameTmp;
    public GameObject currentPhaseIndicator;
}
