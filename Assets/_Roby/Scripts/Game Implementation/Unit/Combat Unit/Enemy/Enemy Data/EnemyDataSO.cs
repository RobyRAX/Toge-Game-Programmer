using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDataSO", menuName = "RAXY/Unit/Enemy/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [SerializeField] 
    string enemyId;

    [SerializeField]  
    string enemyName;

    [SerializeField]  
    Sprite enemyIcon;

    public GameObject enemyPrefab;
}
