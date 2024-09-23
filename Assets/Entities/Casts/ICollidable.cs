using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public static class CollisionUtils {
    /// <summary>
    /// Adds an <typeparamref name="ICollidable"/> to the <paramref name="CollisionLog"/>, returning whether the <typeparamref name="ICollidable"/> was not previously in the set.
    /// </summary>
    /// <remark>
    /// Struggling to think of a good naming for this :(
    /// </remark>
    /// <param name="CollisionLog"></param>
    /// <param name="ICollidable"></param>
    /// <returns></returns>
    public static bool CollisionLogPushUpdated(HashSet<ICollidable> CollisionLog, ICollidable ICollidable) {
        // TODO maybe this should go to Castable? 
        if (CollisionLog.Contains(ICollidable)) {
            return false;
        } else {
            CollisionLog.Add(ICollidable);
            return true;
        }
    }

    public static IEnumerable<Collider> GetOverlappingColliders(Collider collider) {
        // TODO I suspect that the performance of this could be improved
        // - maybe check layer mask and ignore things in specified layers, but then:
        // - will there be clashes? then I need hitbox collisions to do something
        // - Grab techs? Then I'll need grabs to be in a different layer, or let them be hitboxes and collide
        Collider[] colliders = Object.FindObjectsOfType<Collider>();

        return (
            from c in colliders
            where (c!=collider && collider.bounds.Intersects(c.bounds))
            select c
        );
    }

    /// <summary>
    /// Handle all collisions for <paramref name="ThisCollidable"/>, updating its <paramref name="CollisionLog"/> if it was supplied.
    /// </summary>
    /// <param name="ThisCollidable"></param>
    /// <param name="CollisionLog"></param>
    public static void HandleCollisions(ICollidable ThisCollidable, HashSet<ICollidable> CollisionLog) {
        if (ThisCollidable.GetCollider() is Collider collider && collider != null) {
            foreach (Collider otherCollider in GetOverlappingColliders(collider)) {
                if (otherCollider.GetComponent<ICollidable>() is ICollidable OtherCollidable) {
                    if (CollisionLog==null || CollisionLogPushUpdated(CollisionLog, OtherCollidable)) {
                        ThisCollidable.OnCollideWith(OtherCollidable);
                    }
                }
            }
        }
    }

    public static void HandleCollisions(ICollidable ThisCollidable) {
        HandleCollisions(ThisCollidable, null);
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
