using UnityEngine;

public enum ShieldTier {
    Exposed = 0,
    Blocking = 1,
    Shielding = 2
}

public class Shield : MonoBehaviour {
    [HideInInspector] public ShieldTier ShieldTier = ShieldTier.Exposed;
    public Material Material { get; set; }
    public Transform Transform { get { return transform; } }
    public int ParryWindow { get; private set; }
    private int _parryWindowMax = 30;
    
    // public float - TODO should I tie the shield to a resource?

    private void Awake() {
        Material = GetComponent<Renderer>().material;
    }

    private void Update() {
        HandleVisuals();
    }

    private void FixedUpdate() {
        ParryWindow--;
    }

    private void HandleVisuals() {
        /*
         * Bolt Indicator
         */
        // duration
        if (ShieldTier == ShieldTier.Shielding)
            Material.color=Color.blue;
        else if (ShieldTier == ShieldTier.Blocking)
            Material.color=Color.cyan;
        else
            Material.color=Color.red;
    }

    private void OnEnable() {
        ParryWindow = _parryWindowMax;
    }
}
