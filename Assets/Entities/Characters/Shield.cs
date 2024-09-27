using UnityEngine;

public enum ShieldTier {
    Exposed = 0,
    Blocking = 1,
    Shielding = 2
}

public class Shield : MonoBehaviour, ICollidable {
    [HideInInspector] public ShieldTier ShieldTier = ShieldTier.Exposed;
    [HideInInspector] Collider Collider;
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
        if (ShieldTier == ShieldTier.Shielding)
            Material.color=Color.blue;
        else if (ShieldTier == ShieldTier.Blocking)
            Material.color=Color.cyan;
        else
            Material.color=Color.white;
    }

    /* ICollidable Methods */
    public void OnCollideWith(ICollidable other) {
        ;
        // TODO currently no-op
        // have this reflect projectiles on Parry, and maybe hitstun attackers on parry
    }

    public void HandleCollisions() {
        CollisionUtils.HandleCollisions(this, null);
    }

    public Collider GetCollider() {
        return Collider;
    }
}
