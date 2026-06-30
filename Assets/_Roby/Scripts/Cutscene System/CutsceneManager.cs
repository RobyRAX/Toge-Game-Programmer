using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RAXY.Dialogue;
using RAXY.Event;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CutsceneManager : MonoBehaviour, INotificationReceiver
{
    [TitleGroup("Event")]
    public Vector2EventSO PlayTimelineSectionEventSO;

    [TitleGroup("Runtime")]
    [ShowInInspector]
    public Cutscene CurrentPlayingCutscene { get; set; }

    public PlayableDirector CurrentPlayableDirector => CurrentPlayingCutscene != null ? CurrentPlayingCutscene.PlayableDirector : null;

    [TitleGroup("Runtime")]
    [ShowInInspector]
    public List<TrackBinder> TrackBinders { get; set; }

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
        
        var clone = Instantiate(cutscenePrefab);
        clone.gameObject.name = cutscenePrefab.gameObject.name;
        clone.transform.position = Vector3.zero;
        clone.transform.eulerAngles = Vector3.zero;

        CurrentPlayingCutscene = clone;

        FindAllBinders();
        BindToTimeline();

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
    }   

    void PlayTimelineSectionHandler(Vector2 section)
    {
        
    }

    public void PlayTimelineSegment(double startTime, double endTime)
    {
        StartCoroutine(PlayTimelineSegmentCo(startTime, endTime));
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
    public void EndCutscene()
    {
        if (CurrentPlayingCutscene == null)
            return;

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
