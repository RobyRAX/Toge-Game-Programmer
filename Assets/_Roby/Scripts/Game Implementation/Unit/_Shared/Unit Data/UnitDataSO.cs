using RAXY.Animation;
using Sirenix.OdinInspector;
using ToGaProTest.Shared;
using UnityEngine;

public abstract class UnitDataSO : ScriptableObject
{
    [TitleGroup("General")]
    [SerializeField]
    protected string unitId;

    [TitleGroup("General")]
    [SerializeField]
    protected string unitName;

    [TitleGroup("General")]
    [SerializeField]
    protected Sprite unitIcon;

    public string UnitName => unitName;
    public Sprite UnitIcon => unitIcon;

    [TitleGroup("General")]
    public GameObject unitPrefab;

    [TitleGroup("Attack")]
    [HideLabel]
    public AnimationClipSet Attack_Exploration;

    [TitleGroup("Attack")]
    [SuffixLabel("seconds")]
    public float timeToExecuteHitbox;

#if UNITY_EDITOR
    
    [TitleGroup("Attack")]
    [Button]
    public void SyncToAnimationLength()
    {
        var clip = Attack_Exploration?.AnimationClip;
        if (clip == null)
        {
            Debug.LogWarning($"[{name}] Attack_Exploration has no AnimationClip to sync.", this);
            return;
        }

        float speed = Mathf.Abs(Attack_Exploration.speed);
        if (speed < 0.0001f)
            speed = 1f;

        timeToExecuteHitbox = clip.length / speed;

        UnityEditor.EditorUtility.SetDirty(this);
    }

#endif

    [TitleGroup("Animations")]
    [HideLabel]
    public UnitAnimationClipsSO AnimationClipsSO;

    public abstract CombatDataBaseSO CombatDataSO { get; }

    [PropertyOrder(2)]
    [TitleGroup("Stat Growth")]
    [HideLabel]
    public StatGrowth StatGrowth;
}
