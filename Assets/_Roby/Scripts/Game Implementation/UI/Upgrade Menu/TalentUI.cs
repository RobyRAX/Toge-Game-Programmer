using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalentUI : MonoBehaviour
{
    [TitleGroup("UI Ref")]
    [SerializeField]
    Button button;

    [TitleGroup("UI Ref")]
    [SerializeField]
    Image talentIconImg;

    // [TitleGroup("UI Ref")]
    // [SerializeField]
    // TextMeshProUGUI talentNameTmp;

    [TitleGroup("UI Ref")]
    [SerializeField]
    TextMeshProUGUI talentLevelTmp;

    public void Setup(Talent talentData, int level)
    {
        if (talentIconImg != null)
        {
            talentIconImg.sprite = talentData?.talentIcon;
            talentIconImg.enabled = talentIconImg.sprite != null;
        }

        // if (talentNameTmp != null)
        //     talentNameTmp.text = displayName ?? string.Empty;

        if (talentLevelTmp != null)
            talentLevelTmp.text = $"Lv. {level}";

        if (button != null)
            button.interactable = false;
    }
}
