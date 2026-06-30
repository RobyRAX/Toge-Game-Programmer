using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RAXY.Dialogue;
using RAXY.Event;
using RAXY.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

public class CutsceneManager : Singleton<CutsceneManager>, INotificationReceiver
{
    [TitleGroup("Event")]
    public String2EventSO PlayTimelineSectionEventSO;

    [TitleGroup("Event")]
    public EventSO EndCutsceneEventSO;

    Coroutine _segmentCo;

    [TitleGroup("Runtime")]
    [ShowInInspector, ReadOnly]
    Cutscene _currentCutscenePrefab;

    [TitleGroup("Runtime")]
    [ShowInInspector]
    public Cutscene CurrentPlayingCutscene { get; set; }

    public PlayableDirector CurrentPlayableDirector => CurrentPlayingCutscene != null ? CurrentPlayingCutscene.PlayableDirector : null;

    [TitleGroup("Runtime")]
    [ShowInInspector]
    public List<TrackBinder> TrackBinders { get; set; }

    public event Action OnCutsceneStarted;
    public event Action OnCutsceneEnded;

    [TitleGroup("Debug Functions")]
    [Button]
    public void PlayCutscene(Cutscene cutscenePrefab)
    {
        if (cutscenePrefab == null)
        {
            Debug.LogWarning("PlayCutscene: cutscenePrefab is null.");
            return;
        }

        if (CurrentPlayingCutscene != null)
        {
            if (CurrentPlayingCutscene.gameObject.name == cutscenePrefab.gameObject.name)
            {
                return;
            }
        }
        
        _currentCutscenePrefab = cutscenePrefab;

        var clone = Instantiate(cutscenePrefab);
        clone.gameObject.name = cutscenePrefab.gameObject.name;
        clone.transform.position = Vector3.zero;
        clone.transform.eulerAngles = Vector3.zero;

        CurrentPlayingCutscene = clone;

        FindAllBinders();
        BindToTimeline();

        CurrentPlayableDirector.playOnAwake = false;
        CurrentPlayableDirector.extrapolationMode = DirectorWrapMode.Hold;
        CurrentPlayableDirector.Stop();

        var dialogueSO = CurrentPlayingCutscene.dialogueSO;
        if (dialogueSO != null)
        {
            if (CurrentPlayingCutscene.playDialogueOnStart)
                DialogueManager.Instance.PlayDialogueSO(dialogueSO);
            else
            {
                DialogueManager.Instance.PrepareDialogueSO(dialogueSO);
                DialogueManager.Instance.HideDialogue();
            }
        }

        PlayTimelineSectionEventSO.Unsubscribe(PlayTimelineSectionHandler);
        PlayTimelineSectionEventSO.Subscribe(PlayTimelineSectionHandler);
        EndCutsceneEventSO.Unsubscribe(EndCutsceneHandler);
        EndCutsceneEventSO.Subscribe(EndCutsceneHandler);

        double firstStop = CurrentPlayingCutscene.GetMarkerTime(CurrentPlayingCutscene.firstMarkerStop);
        PlayTimelineSegment(0, firstStop);

        OnCutsceneStarted?.Invoke();
    }   

    void PlayTimelineSectionHandler(String2 section)
    {
        if (CurrentPlayingCutscene == null)
            return;
        
        string startId = section.x;
        string endId = section.y;

        double startTime = CurrentPlayingCutscene.GetMarkerTime(startId);
        double endTime = CurrentPlayingCutscene.GetMarkerTime(endId);
        PlayTimelineSegment(startTime, endTime);
    }

    void EndCutsceneHandler()
    {
        EndCutscene();
    }

    public void PlayTimelineSegment(double startTime, double endTime)
    {
        if (_segmentCo != null)
            StopCoroutine(_segmentCo);

        _segmentCo = StartCoroutine(PlayTimelineSegmentCo(startTime, endTime));
    }
    IEnumerator PlayTimelineSegmentCo(double startTime, double endTime)
    {
        CurrentPlayableDirector.time = startTime;
        CurrentPlayableDirector.Evaluate();
        CurrentPlayableDirector.Play();

        while (CurrentPlayableDirector.time < endTime)
            yield return null;

        CurrentPlayableDirector.Pause();
    }

    [TitleGroup("Debug Functions")]
    [Button]
    public void RestartCutscene()
    {
        if (_currentCutscenePrefab == null)
        {
            Debug.LogWarning("RestartCutscene: no cutscene prefab cached.");
            return;
        }

        var prefab = _currentCutscenePrefab;
        EndCutscene();
        PlayCutscene(prefab);
    }

    [TitleGroup("Debug Functions")]
    [Button]
    public void EndCutscene()
    {
        if (CurrentPlayingCutscene == null)
            return;

        PlayTimelineSectionEventSO.Unsubscribe(PlayTimelineSectionHandler);
        EndCutsceneEventSO.Unsubscribe(EndCutsceneHandler);

        if (_segmentCo != null)
        {
            StopCoroutine(_segmentCo);
            _segmentCo = null;
        }

        Destroy(CurrentPlayingCutscene.gameObject);
        CurrentPlayingCutscene = null;
    }

    [TitleGroup("Debug Functions")]
    [Button]
    public void FindAllBinders()
    {
        TrackBinders = FindObjectsByType<TrackBinder>(FindObjectsSortMode.None).ToList();
    }

    public void BindToTimeline()
    {
        if (CurrentPlayingCutscene == null)
        {
            Debug.LogWarning("BindToTimeline: CurrentPlayingCutscene is null.");
            return;
        }

        if (CurrentPlayableDirector.playableAsset == null)
        {
            Debug.LogWarning("BindToTimeline: CurrentPlayingCutscene has no playableAsset.");
            return;
        }

        if (TrackBinders == null || TrackBinders.Count == 0)
        {
            Debug.LogWarning("BindToTimeline: No TrackBinders found.");
            return;
        }

        var outputs = CurrentPlayableDirector.playableAsset.outputs;

        foreach (var binder in TrackBinders)
        {
            if (binder == null || binder.trackBinds == null)
                continue;

            foreach (var entry in binder.trackBinds)
            {
                var track = FindTrackByName(outputs, entry.trackName);
                if (track == null)
                {
                    Debug.LogWarning($"BindToTimeline: Track '{entry.trackName}' not found on timeline.");
                    continue;
                }

                var target = GetBindTarget(binder, entry.trackBindType);
                if (target == null)
                {
                    Debug.LogWarning($"BindToTimeline: No bind target for track '{entry.trackName}' ({entry.trackBindType}).");
                    continue;
                }

                CurrentPlayableDirector.SetGenericBinding(track, target);
            }
        }

        // Bind marker tracks ke CutsceneManager supaya INotification (OnNotify) kepanggil.
        foreach (var output in outputs)
        {
            if (output.sourceObject is MarkerTrack markerTrack)
                CurrentPlayableDirector.SetGenericBinding(markerTrack, this);
        }

        CurrentPlayableDirector.RebuildGraph();
        CurrentPlayableDirector.Evaluate();
    }

    static TrackAsset FindTrackByName(IEnumerable<PlayableBinding> outputs, string trackName)
    {
        foreach (var output in outputs)
        {
            if (output.sourceObject is TrackAsset track && track.name == trackName)
                return track;
        }

        return null;
    }

    static Object GetBindTarget(TrackBinder binder, TrackBindType bindType)
    {
        return bindType switch
        {
            TrackBindType.Animation => binder.animator,
            TrackBindType.Cinemachine => binder.cinemachineBrain,
            _ => null
        };
    }

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is StartDialogueMarker marker)
        {
            if (marker.inputMethod == StartDialogueMarker.InputMethod.Index)
                PlayDialogueSet(marker.dialogueSetIndex);
            else
                PlayDialogueSet(marker.dialogueSetId);
        }
        else if (notification is PauseTimelineMarker)
        {
            if (CurrentPlayableDirector != null)
                CurrentPlayableDirector.Pause();
        }
    }

    public void PlayDialogueSet(int index)
    {
        DialogueManager.Instance.PlayDialogueSet(index);
    }

    public void PlayDialogueSet(string id)
    {
        DialogueManager.Instance.PlayDialogueSet(id);
    }
}
