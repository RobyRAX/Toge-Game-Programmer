using System;
using System.Collections.Generic;
using RAXY.Utility;
using Sirenix.OdinInspector;
using UnityEngine;

public class TurnBaseCombatManager : Singleton<TurnBaseCombatManager>
{
    [TitleGroup("Formation")]
    public int maxMemberPerTeam;

    [TitleGroup("Formation")]
    public int distancePerTeamUnit;

    [TitleGroup("Formation")]
    public int twoTeamDistance;

    [TitleGroup("Formation")]
    public Transform formationRoot;

    [TitleGroup("Formation")]
    public Transform heroRoot;

    [TitleGroup("Formation")]
    public Transform enemyRoot;

    [HorizontalGroup("Formation/Pos")]
    [ListDrawerSettings(ShowIndexLabels = true, Expanded = true)]
    public Transform[] HeroPositions;

    [HorizontalGroup("Formation/Pos")]
    [ListDrawerSettings(ShowIndexLabels = true, Expanded = true)]
    public Transform[] EnemyPositions;

    [FoldoutGroup("Formation/Gizmos")]
    public bool drawFormationGizmos = true;

    [FoldoutGroup("Formation/Gizmos")]
    [MinValue(0)]
    public float gizmoSlotRadius = 0.25f;

    [FoldoutGroup("Formation/Gizmos")]
    [MinValue(0)]
    public float gizmoRootRadius = 0.35f;
    
    [TitleGroup("Formation")]
    [Button]
    void InitFormationPositions()
    {
        if (formationRoot == null)
        {
            Debug.LogWarning($"{nameof(TurnBaseCombatManager)}: {nameof(formationRoot)} is null.");
            return;
        }

        if (heroRoot == null)
        {
            heroRoot = EnsureChildRoot(formationRoot, nameof(heroRoot));
        }

        if (enemyRoot == null)
        {
            enemyRoot = EnsureChildRoot(formationRoot, nameof(enemyRoot));
        }

        // Put the two teams apart (symmetrical around formation root).
        heroRoot.localPosition = Vector3.left * (twoTeamDistance * 0.5f);
        enemyRoot.localPosition = Vector3.right * (twoTeamDistance * 0.5f);

        RecreateChildren(heroRoot);
        RecreateChildren(enemyRoot);

        int count = Mathf.Max(0, maxMemberPerTeam);
        HeroPositions = new Transform[count];
        EnemyPositions = new Transform[count];

        // Center slots around Z=0 so formationRoot feels "in the middle".
        float zCenterOffset = (count - 1) * distancePerTeamUnit * 0.5f;

        for (int i = 0; i < count; i++)
        {
            // Slots are spaced on Z so X remains "team side".
            float z = (i * distancePerTeamUnit) - zCenterOffset;

            var heroSlot = new GameObject($"HeroPos_{i:00}").transform;
            heroSlot.SetParent(heroRoot, false);
            heroSlot.localPosition = new Vector3(0f, 0f, z);
            HeroPositions[i] = heroSlot;

            var enemySlot = new GameObject($"EnemyPos_{i:00}").transform;
            enemySlot.SetParent(enemyRoot, false);
            enemySlot.localPosition = new Vector3(0f, 0f, z);
            EnemyPositions[i] = enemySlot;
        }
    }

    static Transform EnsureChildRoot(Transform parent, string rootName)
    {
        var existing = parent.Find(rootName);
        if (existing != null)
            return existing;

        var go = new GameObject(rootName);
        var t = go.transform;
        t.SetParent(parent, false);
        return t;
    }

    static void RecreateChildren(Transform root)
    {
        if (root == null)
            return;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            var child = root.GetChild(i);

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    void OnDrawGizmos()
    {
        if (!drawFormationGizmos)
            return;

        if (formationRoot != null)
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.9f);
            Gizmos.DrawWireSphere(formationRoot.position, gizmoRootRadius);
        }

        DrawTeamGizmos(heroRoot, HeroPositions, new Color(0.2f, 0.8f, 1f, 0.9f)); // cyan-ish
        DrawTeamGizmos(enemyRoot, EnemyPositions, new Color(1f, 0.35f, 0.35f, 0.9f)); // red-ish
    }

    void DrawTeamGizmos(Transform teamRoot, Transform[] slots, Color color)
    {
        if (teamRoot == null)
            return;

        Gizmos.color = color;
        Gizmos.DrawWireSphere(teamRoot.position, gizmoRootRadius);

        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            var t = slots[i];
            if (t == null)
                continue;

            Gizmos.DrawSphere(t.position, gizmoSlotRadius);
            Gizmos.DrawLine(teamRoot.position, t.position);
        }
    }

    [TitleGroup("Current Combatans")]
    [ReadOnly]
    [ShowInInspector]
    public List<CombatantBase> AllCombatants { get; set; }

    [TitleGroup("Current Combatans")]
    [ReadOnly]
    [ShowInInspector]
    public List<CombatantBase> HeroCombatants { get; set; }

    [TitleGroup("Current Combatans")]
    [ReadOnly]
    [ShowInInspector]
    public List<CombatantBase> EnemyCombatants { get; set; }

    [TitleGroup("Runtime")]
    [ReadOnly]
    [ShowInInspector]
    public Queue<CombatantBase> TurnQueues { get; set; }

    [TitleGroup("Debug Functions")]
    [Button]
    public void StartCombat(List<CombatantBase> heroCombatants, List<CombatantBase> enemyCombatants)
    {
        Debug.Log("Combat Started");
    }

    [TitleGroup("Debug Functions")]
    [Button]
    public void Attack(CombatantBase attacker, 
                        CombatantBase defender, 
                        DamageProfileWithAttribute damageProfile)
    {
        var attackReq = TurnBaseCombatHelper.BuildAttackRequest(attacker, defender, damageProfile);
        TurnBaseCombatHelper.SendAttack(attackReq, out AttackResult attackRes);
    }
}
