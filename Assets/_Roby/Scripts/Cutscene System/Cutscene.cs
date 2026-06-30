using System;
using System.Collections.Generic;
using System.Linq;
using RAXY.Dialogue;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(PlayableDirector))]
public class Cutscene : MonoBehaviour
{
    public PlayableDirector PlayableDirector { get; set; }

    public DialogueSO dialogueSO;
    public bool playDialogueOnStart;

    [TitleGroup("Marker")]
    [ShowInInspector]
    public TimelineAsset Timeline
    {
        get
        {
            if (PlayableDirector != null)
                return PlayableDirector.playableAsset as TimelineAsset;
            else
                return null;
        }
    }

    [TitleGroup("Marker")]
    [ReadOnly]
    public List<MarkerTimeId> markers;

    public List<string> MarkerIds { get; set; }

    void OnValidate()
    {
        if (PlayableDirector == null)
            PlayableDirector = GetComponent<PlayableDirector>();
    }

    void Awake()
    {
        PlayableDirector = GetComponent<PlayableDirector>();
    }

    [TitleGroup("Marker")]
    [Button]
    void GetMarkers()
    {
        markers.Clear();

        if (Timeline == null)
        {
            Debug.LogWarning("Timeline not assigned.");
            return;
        }

        // Collect all IdMarkers across all tracks
        foreach (var output in Timeline.outputs)
        {
            if (output.sourceObject is not TrackAsset track)
                continue;

            foreach (var marker in track.GetMarkers())
            {
                if (marker is IdMarker idMarker)
                    markers.Add(new MarkerTimeId(idMarker.id, idMarker));
            }
        }

        // Add start and end markers once
        markers.Add(new MarkerTimeId(MarkerTimeId.START_MARKER, 0));
        markers.Add(new MarkerTimeId(MarkerTimeId.END_MARKER, Timeline));

        // Sort once after collecting everything
        markers = markers.OrderBy(m => m.Time).ToList();

        // Build MarkerIds in the same order
        MarkerIds = new List<string>();
        foreach (var marker in markers)
            MarkerIds.Add(marker.id);
    }
}
