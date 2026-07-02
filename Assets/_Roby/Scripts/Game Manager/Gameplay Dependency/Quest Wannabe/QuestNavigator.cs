using UnityEngine;

public class QuestNavigator : MonoBehaviour
{
    [SerializeField]
    bool useMainHeroPositionAsOrigin = true;

    QuestWannabe manager;
    Transform mainHero;

    const float MinDirectionSqrMagnitude = 0.0001f;

    public void Setup(QuestWannabe questManager, Transform mainHeroTransform)
    {
        manager = questManager;
        mainHero = mainHeroTransform;
    }

    void LateUpdate()
    {
        if (manager == null || mainHero == null)
            return;

        var targetPos = manager.GetResolvedMarkerPosition();

        var originPos = useMainHeroPositionAsOrigin ? mainHero.position : transform.position;
        var dir = targetPos - originPos;
        dir.y = 0f;

        if (dir.sqrMagnitude < MinDirectionSqrMagnitude)
            return;

        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
}
