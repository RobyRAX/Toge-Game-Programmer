using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

public class CombatCameraDirector : MonoBehaviour
{
    [TitleGroup("References")]
    public CinemachineCamera combatCamera;

    [TitleGroup("References")]
    public CinemachineTargetGroup targetGroup;

    [TitleGroup("Target Group")]
    [MinValue(0)]
    public float memberWeight = 1f;

    [TitleGroup("Target Group")]
    [MinValue(0)]
    public float memberRadius = 1.5f;

    [TitleGroup("Camera Placement")]
    [Tooltip("Offset relatif terhadap anchor slot. Z = arah ke musuh (negatif = di belakang hero), Y = naik, X = samping.")]
    public Vector3 positionOffset = new Vector3(0f, 3f, -8f);

    [TitleGroup("Camera Placement")]
    public float lookAtHeightOffset = 1.5f;

    Transform[] cachedHeroSlots;
    Transform[] cachedEnemySlots;
    Transform cachedAnchor;

    public void Setup(Transform[] heroSlots, Transform[] enemySlots)
    {
        cachedHeroSlots = heroSlots;
        cachedEnemySlots = enemySlots;

        RebuildTargetGroup(heroSlots, enemySlots);
        FocusOnDefault();
    }

    public void RebuildTargetGroup(Transform[] heroSlots, Transform[] enemySlots)
    {
        if (targetGroup == null)
        {
            Debug.LogWarning($"{nameof(CombatCameraDirector)}: {nameof(targetGroup)} is null.");
            return;
        }

        targetGroup.Targets.Clear();
        AddSlotsToTargetGroup(heroSlots);
        AddSlotsToTargetGroup(enemySlots);
        targetGroup.DoUpdate();
    }

    public void FocusOnDefault()
    {
        cachedAnchor = GetFirstValidSlot(cachedHeroSlots);
        RepositionCamera();
    }

    public void FocusOnCombatant(CombatantBase combatant)
    {
        if (combatant == null || !combatant.HasFormationSlot)
        {
            Debug.LogWarning($"{nameof(CombatCameraDirector)}: combatant is null or has no formation slot. Falling back to default.");
            FocusOnDefault();
            return;
        }

        FocusOnSlot(combatant.FormationSlot);
    }

    public void FocusOnSlot(Transform slot)
    {
        if (slot == null)
        {
            Debug.LogWarning($"{nameof(CombatCameraDirector)}: slot is null.");
            return;
        }

        cachedAnchor = slot;
        RepositionCamera();
    }

    [Button]
    public void RepositionCamera()
    {
        if (combatCamera == null)
        {
            Debug.LogWarning($"{nameof(CombatCameraDirector)}: {nameof(combatCamera)} is null.");
            return;
        }

        Transform anchor = cachedAnchor != null ? cachedAnchor : GetFirstValidSlot(cachedHeroSlots);
        if (anchor == null)
        {
            Debug.LogWarning($"{nameof(CombatCameraDirector)}: no valid hero slot to anchor to. Call {nameof(Setup)} first.");
            return;
        }

        if (!TryGetSlotsCenter(cachedEnemySlots, out Vector3 enemyCenter))
        {
            Debug.LogWarning($"{nameof(CombatCameraDirector)}: no valid enemy slot for facing reference.");
            return;
        }

        // Keep a consistent battle-facing direction (hero slots -> enemy slots axis) regardless of which slot we anchor to.
        Vector3 heroCenter = TryGetSlotsCenter(cachedHeroSlots, out Vector3 hc) ? hc : anchor.position;

        Vector3 flatDir = enemyCenter - heroCenter;
        flatDir.y = 0f;

        if (flatDir.sqrMagnitude < 0.0001f)
            flatDir = anchor.forward;

        flatDir.Normalize();

        Quaternion facing = Quaternion.LookRotation(flatDir, Vector3.up);
        Vector3 camPos = anchor.position + facing * positionOffset;
        Vector3 lookTarget = (anchor.position + enemyCenter) * 0.5f + Vector3.up * lookAtHeightOffset;
        Vector3 lookDir = lookTarget - camPos;

        if (lookDir.sqrMagnitude < 0.0001f)
            lookDir = flatDir;

        combatCamera.transform.SetPositionAndRotation(camPos, Quaternion.LookRotation(lookDir, Vector3.up));
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        if (cachedHeroSlots == null || cachedEnemySlots == null)
            return;

        RepositionCamera();
    }

    void AddSlotsToTargetGroup(Transform[] slots)
    {
        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot == null)
                continue;

            targetGroup.AddMember(slot, memberWeight, memberRadius);
        }
    }

    static Transform GetFirstValidSlot(Transform[] slots)
    {
        if (slots == null)
            return null;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                return slots[i];
        }

        return null;
    }

    static bool TryGetSlotsCenter(Transform[] slots, out Vector3 center)
    {
        center = Vector3.zero;
        if (slots == null)
            return false;

        int count = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                continue;

            center += slots[i].position;
            count++;
        }

        if (count == 0)
            return false;

        center /= count;
        return true;
    }
}
