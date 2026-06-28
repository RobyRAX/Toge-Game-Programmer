using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CombatUnitController : UnitControllerBase
{
    public virtual CombatantBase CombatantCont { get; set; }

    float hitboxRadius = 1f;
    float hitboxForwardOffset = 1f;
    float hitboxVerticalOffset = 1f;
    LayerMask hitboxTargetMask = ~0;

    public event Action OnAttacked;
    public void Invoke_OnAttacked() => OnAttacked?.Invoke();

    public void SetHitboxSetting(HitboxSetting hitboxSetting)
    {
        hitboxRadius = hitboxSetting.hitboxRadius;
        hitboxForwardOffset = hitboxSetting.hitboxForwardOffset;
        hitboxVerticalOffset = hitboxSetting.hitboxVerticalOffset;
        hitboxTargetMask = hitboxSetting.hitboxTargetMask;
    }

    public virtual void ActivateHitbox()
    {
        Vector3 center = GetHitboxCenter();

        Collider[] overlaps = Physics.OverlapSphere(center,
                                                    hitboxRadius,
                                                    hitboxTargetMask,
                                                    QueryTriggerInteraction.Ignore);
        if (overlaps.Length <= 0)
            return;

        var hitTargets = new HashSet<CombatUnitController>();

        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider col = overlaps[i];
            if (col == null)
                continue;

            var targetCont = col.GetComponentInParent<CombatUnitController>();
            if (targetCont == null || targetCont == this)
                continue;

            // Satu unit bisa punya lebih dari satu collider, jadi pastikan
            // tiap target hanya diproses sekali per aktivasi hitbox.
            if (hitTargets.Add(targetCont))
                OnHitboxHit(targetCont);
        }
    }

    protected virtual void OnHitboxHit(CombatUnitController target)
    {
        target?.Invoke_OnAttacked();
    }

    Vector3 GetHitboxCenter()
    {
        return transform.position
             + transform.forward * hitboxForwardOffset
             + Vector3.up * hitboxVerticalOffset;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        Gizmos.DrawWireSphere(GetHitboxCenter(), hitboxRadius);
    }
#endif
}

[Serializable]
public class HitboxSetting
{
    public float hitboxRadius;
    public float hitboxForwardOffset;
    public float hitboxVerticalOffset;
    public LayerMask hitboxTargetMask;
}