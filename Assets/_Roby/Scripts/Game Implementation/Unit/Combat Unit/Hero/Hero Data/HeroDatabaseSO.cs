using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "HeroDatabaseSO", menuName = "RAXY/Unit/Hero/HeroDatabaseSO")]
public class HeroDatabaseSO : ScriptableObject
{
    public List<HeroDataSO> heroSOs;

    public HeroDataSO GetHeroData(string heroId)
    {
        return heroSOs.Find(x => x.ItemId == heroId);
    }

#if UNITY_EDITOR
    [Button]
    void FindAllHeroData()
    {
        heroSOs ??= new List<HeroDataSO>();
        heroSOs.Clear();

        foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(HeroDataSO)}"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.LoadAssetAtPath<HeroDataSO>(path) is HeroDataSO heroSO)
                heroSOs.Add(heroSO);
        }

        heroSOs = heroSOs
            .Distinct()
            .OrderBy(hero => hero.ItemId)
            .ToList();

        EditorUtility.SetDirty(this);
    }
#endif
}
