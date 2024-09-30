using UnityEngine;

public class StageTerrain : MonoBehaviour, ICollidable {
    // calling it this for now to avoid the naming conflict with Unity's Terrain Component :(
    // TODO cool shit like breaking walls
    private Collider _collider;

    private void Start() {
        _collider = GetComponent<Collider>();
    }

    public Collider GetCollider() {
        return _collider;
    }

    public void HandleCollisions(){}

    public void OnCollideWith(ICollidable other, CollisionInfo info){}
}
