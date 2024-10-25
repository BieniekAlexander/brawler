using UnityEngine;

public class StageTerrain : MonoBehaviour, ICollidable {
    // calling it this for now to avoid the naming conflict with Unity's Terrain Component :(
    // TODO cool shit like breaking walls
    private Collider _collider;
    public Transform Transform { get { return transform; } }

    private void Start() {
        _collider = GetComponent<Collider>();
    }

    public bool OnCollideWith(ICollidable other, CollisionInfo info) => false;
    public void HandleCollisions() {}
    public Collider Collider { get { return _collider; } }
    public int ImmaterialStack {  get { return 0; } set {; } } // TODO this might be nice for shroud? LOS blockers?
}
