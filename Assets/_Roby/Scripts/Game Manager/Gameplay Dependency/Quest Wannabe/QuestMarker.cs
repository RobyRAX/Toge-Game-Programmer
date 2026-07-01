using UnityEngine;
using UnityEngine.Rendering.Universal;

public class QuestMarker : MonoBehaviour
{
    public DecalProjector radiusDecal;

    public void SetMarker(Vector3 position, float radius)
    {
        transform.position = position;

        if (radiusDecal == null)
            return;

        var size = radiusDecal.size;
        var diameter = radius * 2f;
        radiusDecal.size = new Vector3(diameter, diameter, size.z);
        radiusDecal.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (radiusDecal != null)
            radiusDecal.gameObject.SetActive(false);
    }
}
