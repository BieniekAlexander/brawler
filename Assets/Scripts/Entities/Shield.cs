using UnityEngine;

public enum ShieldTier {
    Exposed = 0,
    Blocking = 1,
    Shielding = 2
}

public class Shield : MonoBehaviour, ICollidable {
    [HideInInspector] public ShieldTier ShieldTier = ShieldTier.Exposed;
    public Material Material { get; set; }
    public Transform Transform { get { return transform; } }
    
    // public float - TODO should I tie the shield to a resource?

    private void Awake() {
        _collider = GetComponent<Collider>();
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
    private Collider _collider;

    public void OnCollideWith(ICollidable other, CollisionInfo info) {
        ;
        // TODO currently no-op
        // have this reflect projectiles on Parry, and maybe hitstun attackers on parry
    }

    public void HandleCollisions() {
        CollisionUtils.HandleCollisions(this, null);
    }

    public Collider Collider { get { return _collider; } }
    public int ImmaterialStack { get { return 0; } set {; } }
}
