using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ColliderPhysicsUtils {
    // https://github.com/justonia/UnityExtensions/blob/master/PhysicsExtensions.cs
    private static Vector3 AbsVec3(Vector3 v) {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    private static float MaxVec3(Vector3 v) {
        return Mathf.Max(v.x, Mathf.Max(v.y, v.z));
    }

    public static void ToWorldSpaceSphere(this SphereCollider sphere, out Vector3 center, out float radius) {
        center = sphere.transform.TransformPoint(sphere.center);
        radius = sphere.radius * MaxVec3(AbsVec3(sphere.transform.lossyScale));
    }

    public static void ToWorldSpaceBox(this BoxCollider box, out Vector3 center, out Vector3 halfExtents, out Quaternion orientation) {
        orientation = box.transform.rotation;
        center = box.transform.TransformPoint(box.center);
        var lossyScale = box.transform.lossyScale;
        var scale = AbsVec3(lossyScale);
        halfExtents = Vector3.Scale(scale, box.size) * 0.5f;
    }

    public static void ToWorldSpaceCapsule(this CapsuleCollider capsule, out Vector3 point0, out Vector3 point1, out float radius) {
        var center = capsule.transform.TransformPoint(capsule.center);
        radius = 0f;
        float height = 0f;
        Vector3 lossyScale = AbsVec3(capsule.transform.lossyScale);
        Vector3 dir = Vector3.zero;

        switch (capsule.direction) {
            case 0: // x
                radius = Mathf.Max(lossyScale.y, lossyScale.z) * capsule.radius;
                height = lossyScale.x * capsule.height;
                dir = capsule.transform.TransformDirection(Vector3.right);
                break;
            case 1: // y
                radius = Mathf.Max(lossyScale.x, lossyScale.z) * capsule.radius;
                height = lossyScale.y * capsule.height;
                dir = capsule.transform.TransformDirection(Vector3.up);
                break;
            case 2: // z
                radius = Mathf.Max(lossyScale.x, lossyScale.y) * capsule.radius;
                height = lossyScale.z * capsule.height;
                dir = capsule.transform.TransformDirection(Vector3.forward);
                break;
        }

        if (height < radius*2f) {
            dir = Vector3.zero;
        }

        point0 = center + dir * (height * 0.5f - radius);
        point1 = center - dir * (height * 0.5f - radius);
    }

    public static void ToWorldSpaceCharacterCapsule(this CharacterController capsule, out Vector3 point0, out Vector3 point1, out float radius) {
        // adapted from ToWorldSpaceCapsule - for a CharacterController, direction is always Vector3.up
        var center = capsule.transform.TransformPoint(capsule.center);
        radius = capsule.radius;
        float height = capsule.height;
        Vector3 dir = capsule.transform.TransformDirection(Vector3.up);

        if (height < radius*2f) {
            dir = Vector3.zero;
        }

        point0 = center + dir * (height * 0.5f - radius);
        point1 = center - dir * (height * 0.5f - radius);
    }
}

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
    public static bool CollisionLogPushUpdated(Dictionary<ICollidable, int> CollisionLog, ICollidable ICollidable, int retriggerDuration) {
        // TODO maybe this should go to Castable? 
        if (CollisionLog.ContainsKey(ICollidable) && CollisionLog[ICollidable]>0) {
            return false;
        } else {
            CollisionLog[ICollidable]=retriggerDuration;
            return true;
        }
    }
    public static IEnumerable<Collider> GetOverlappingColliders(Collider collider) {
        // TODO I suspect that the performance of this could be improved
        // - maybe check layer mask and ignore things in specified layers, but then:
        // - will there be clashes? then I need hitbox collisions to do something
        // - Grab techs? Then I'll need grabs to be in a different layer, or let them be hitboxes and collide
        if (collider is CapsuleCollider capsule) {
            ColliderPhysicsUtils.ToWorldSpaceCapsule(capsule, out Vector3 point0, out Vector3 point1, out float radius);

            return from c in Physics.OverlapCapsule(point0, point1, radius)
                   where (c!=collider)
                   select c;
        } else if (collider is CharacterController character) {
            ColliderPhysicsUtils.ToWorldSpaceCharacterCapsule(character, out Vector3 point0, out Vector3 point1, out float radius);

            return from c in Physics.OverlapCapsule(point0, point1, radius)
                   where (c!=collider)
                   select c;
        }
        else if (collider is SphereCollider sphere) {
            ColliderPhysicsUtils.ToWorldSpaceSphere(sphere, out Vector3 point, out float radius);

            return from c in Physics.OverlapSphere(point, radius)
                   where (c!=collider)
                   select c;
        } else if (collider is BoxCollider box) {
            ColliderPhysicsUtils.ToWorldSpaceBox(box, out Vector3 center, out Vector3 halfExtents, out Quaternion orientation);

            return from c in Physics.OverlapBox(center, halfExtents, orientation)
                   where (c!=collider)
                   select c;
        } else {
            Debug.Log($"Unhandled Collider type in {collider}");
            throw new NotImplementedException("unhandled collider type");
        }
    }

    /// <summary>
    /// Handle all collisions for <paramref name="ThisCollidable"/>, updating its <paramref name="CollisionLog"/> if it was supplied.
    /// </summary>
    /// <param name="ThisCollidable"></param>
    /// <param name="CollisionLog"></param>
    public static void HandleCollisions(ICollidable ThisCollidable, Dictionary<ICollidable, int> CollisionLog, int retriggerDuration) {
        if (ThisCollidable.Collider != null) {
            foreach (Collider otherCollider in GetOverlappingColliders(ThisCollidable.Collider)) {
                if (otherCollider.GetComponent<ICollidable>() is ICollidable OtherCollidable) {
                    if (CollisionLog==null || CollisionLogPushUpdated(CollisionLog, OtherCollidable, retriggerDuration)) {
                        Vector3 normal = GetDecollisionVector(ThisCollidable, OtherCollidable).normalized;
                        CollisionInfo info = new CollisionInfo(normal); // TOOD is this an effective way to find the normal?
                        ThisCollidable.OnCollideWith(OtherCollidable, info);
                    }
                }
            }
        }
    }

    public static void HandleCollisions(ICollidable thisCollidable) {
        HandleCollisions(thisCollidable, null, 0);
    }
    
    public static Vector3 GetDecollisionVector(ICollidable thisCollidable, ICollidable otherCollidable) {
        if (thisCollidable is IMoves Mover) {
            if (Physics.ComputePenetration(
                thisCollidable.Collider,
                thisCollidable.Transform.position,
                thisCollidable.Transform.rotation,
                otherCollidable.Collider,
                otherCollidable.Transform.position,
                otherCollidable.Transform.rotation,
                out Vector3 direction,
                out float distance
            )) {
                return direction * (Mover.BaseSpeed*.2f+.02f);
            } else {
                return Vector3.zero;
            }
        } else {
            return Vector3.zero;
        }
    }
}

    public class CollisionInfo {
    public Vector3 Normal;
    
    public CollisionInfo(Vector3 _normal) {
        Normal = _normal;
    }
    
}

/// <summary>
/// An interface that defines how a given GameObject should behave when it collides with another GameObject
/// </summary>
public interface ICollidable {
    public bool OnCollideWith(ICollidable other, CollisionInfo info);

    public void HandleCollisions();

    public Collider Collider { get; }
    public Transform Transform { get; }

    // status effects
    public int ImmaterialStack { get; set; }
}
