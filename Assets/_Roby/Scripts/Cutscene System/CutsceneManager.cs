using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    [TitleGroup("Runtime")]
    [ShowInInspector]
    public PlayableDirector CurrentPlayingCutscene { get; set; }

    [TitleGroup("Runtime")]
    [ShowInInspector]
    public List<TrackBinder> TrackBinders { get; set; }

    [TitleGroup("Debug Functions")]
    [Button]
    public void PlayCutscene(PlayableDirector cutscenePrefab)
    {
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
    }   

    [TitleGroup("Debug Functions")]
    [Button]
    public void FindAllBinders()
    {
        TrackBinders = FindObjectsByType<TrackBinder>(FindObjectsSortMode.None).ToList();
    }
}
