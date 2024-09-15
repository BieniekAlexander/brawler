using UnityEngine;

public enum ShieldLevel {
    None = 0,
    Normal = 1,
    Boosted = 2
}

public class Shield : MonoBehaviour {
    public ShieldLevel ShieldLevel { get; set; } = ShieldLevel.None;
    public Collider Collider { get; set; }
    public Material Material { get; set; }
    
    // public float - TODO should I tie the shield to a resource?

    private void Awake() {
        Collider = GetComponent<Collider>();
        Material = GetComponent<Renderer>().material;
    }

    private void Update() {
        HandleVisuals();
    }

    private void HandleVisuals() {
        /*
         * Bolt Indicator
         */
        // duration
        if (ShieldLevel == ShieldLevel.Boosted)
            Material.color=Color.blue;
        else if (ShieldLevel == ShieldLevel.Normal)
            Material.color=Color.cyan;
        else
            Material.color=Color.white;
    }
}
