using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatPhaseUI : MonoBehaviour
{
    [TitleGroup("Ref")]
    [SerializeField]
    Image phaseIconImg;

    [TitleGroup("Ref")]
    [SerializeField]
    TextMeshProUGUI phaseNameTmp;

    [TitleGroup("Ref")]
    [SerializeField]
    GameObject currentPhaseIndicator;

    public void ApplyState(Color color, bool isCurrent)
    {
        if (phaseIconImg != null)
            phaseIconImg.color = color;

        if (phaseNameTmp != null)
            phaseNameTmp.color = color;

        if (currentPhaseIndicator != null)
            currentPhaseIndicator.SetActive(isCurrent);
        
        if (isCurrent)
        {
            transform.SetAsLastSibling();
        }
    }
}
