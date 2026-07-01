using TMPro;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField]
    Collider listenCollider;

    public string portalId;
    public string sceneTargetName;
    public string portalTargetId;
    public string displayLabel;
    public GameObject worldCanvas;
    public TextMeshProUGUI displayLabelTmp;
    public Transform spawnPoint;

    void Awake()
    {
        var collider = listenCollider != null ? listenCollider : GetComponent<Collider>();
        if (collider == null)
            return;

        var listener = collider.gameObject.GetComponent<PortalColliderListener>();
        if (listener == null)
            listener = collider.gameObject.AddComponent<PortalColliderListener>();

        listener.Portal = this;
    }

    void Start()
    {
        if (displayLabelTmp != null && !string.IsNullOrEmpty(displayLabel))
            displayLabelTmp.text = displayLabel;
    }

    public void HandleHeroEntered(Collider other)
    {
        if (other == null)
            return;

        var hero = other.GetComponent<HeroController>();
        if (hero == null)
            return;

        if (GameplayManager.Instance == null ||
            hero != GameplayManager.Instance.MainHero ||
            GameplayManager.Instance.CurrentState != GameplayState.Explore)
            return;

        GameplayManager.Instance.RequestTeleportViaPortal(this);
    }
}
