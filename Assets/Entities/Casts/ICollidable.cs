using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CollisionUtils {
    public static IEnumerable<Collider> GetOverlappingColliders(Collider collider) {
        Collider[] colliders = Object.FindObjectsOfType<Collider>();

        return (
            from c in colliders
            where (c!=collider && collider.bounds.Intersects(c.bounds))
            select c
        );
    }

    public static void HandleCollisions(ICollidable ICollidable) {
        if (ICollidable.GetCollider() is Collider collider) {
            foreach (Collider otherCollider in GetOverlappingColliders(collider)) {
                GameObject go = otherCollider.gameObject;
                ICollidable target = go.GetComponent<ICollidable>();

                if (target != null) {
                    ICollidable.OnCollideWith(target);
                }
            }
        }
    }
}

/// <summary>
/// An interface that defines how a given GameObject should behave when it collides with another GameObject
/// </summary>
public interface ICollidable {
    public void OnCollideWith(ICollidable other);

    public void HandleCollisions();

    public Collider GetCollider();
}
