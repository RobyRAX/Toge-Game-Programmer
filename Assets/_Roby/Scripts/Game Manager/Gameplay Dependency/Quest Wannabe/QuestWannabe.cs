using System;
using System.Collections.Generic;
using RAXY.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestWannabe : Singleton<QuestWannabe>
{
    public GameObject marker;
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "questId")]
    public List<QuestStepEntry> questStepEntries;

    [TitleGroup("Runtime")]
    [ShowInInspector]
    public QuestStepEntry CurrentQuest { get; set; }

}

[Serializable]
public class QuestStep_Runtime
{
    public QuestStepEntry entry;
    public bool isRunning;
}

[Serializable]
public class QuestStepEntry
{
    public string questId;
    public string questDesc;
    public string sceneLocation;
    public Vector3 markerPosition;
    public float markerRadius = 2;

#if UNITY_EDITOR
    [TitleGroup("Helper")]
    [ShowInInspector]
    [OnValueChanged("OnHelperChanged")]
    Transform helperTransform;

    void OnHelperChanged()
    {
        sceneLocation = SceneManager.GetActiveScene().name;
        markerPosition = helperTransform.position;

        helperTransform = null;
    }
#endif
}