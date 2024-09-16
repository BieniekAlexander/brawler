using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEngine.EventSystems;


[Serializable]
public enum Positioning {
    Directional,
    Absolute
}

[Serializable]
public struct HitEvent {
    public Hit hit;
    public int startFrame;
}

[Serializable]
public struct ProjectileEvent {
    public Projectile projectile;
    public int startFrame;
    public float range;
    public Positioning positioning;
    public Quaternion radialOffset;
}

public interface ICastMessage : IEventSystemHandler {
    // functions that can be called via the messaging system
    void FinishCast();
}

/// <summary>
/// Default object for evaluating cast behavior
/// Extend this object for more particular cast logic
/// </summary>
public class CastBase : MonoBehaviour, ICastMessage {
    // Start is called before the first frame update
    [SerializeField] public HitEvent[] hitEvents;
    [SerializeField] ProjectileEvent[] projectileEvents;
    [SerializeField] List<StatusEffectBase> statusEffectPefabs;
    [SerializeField] public CommandMovementBase commandMovementPrefab;

    private int frame = 0;
    public int Frame { get { return frame; } }

    private bool rotatingClockwise = true;
    [SerializeField] public int startupTime = 0;
    [SerializeField] public int duration;


    public Character caster;
    public Character Caster { get { return caster; } }
    public List<Hit> hits;
    public List<Projectile> projectiles;
    public List<StatusEffectBase> statusEffects;

    public void Initialize(Character _caster, bool _rotatingClockwise) {
        caster =_caster;
        rotatingClockwise = _rotatingClockwise;

        foreach (HitEvent hitEvent in hitEvents) {
            Assert.IsNotNull(hitEvent.hit);
        }

        foreach (StatusEffectBase sePrefab in statusEffectPefabs) {
            StatusEffectBase se = Instantiate(sePrefab);
            se.Initialize(caster);
            statusEffects.Add(se);
        }

        if (commandMovementPrefab != null) {
            CommandMovementBase commandMovement = Instantiate(commandMovementPrefab, transform);
            commandMovement.Initialize(caster, caster.GetCursorWorldPosition(), caster.transform.position);
            caster.SetCommandMovement(commandMovement);
        }
    }

    void FixedUpdate() {
        foreach (HitEvent hitEvent in hitEvents) {
            if (hitEvent.startFrame==frame) {
                Hit h = Instantiate(hitEvent.hit);
                h.Initialize(caster, caster.transform, !rotatingClockwise);
                hits.Add(h);
            }
        }

        foreach (ProjectileEvent projectileEvent in projectileEvents) {
            if (projectileEvent.startFrame==frame) {
                Vector3 projectilePosition;

                if (projectileEvent.positioning==Positioning.Directional) {
                    projectilePosition = caster.transform.position+(caster.transform.rotation*projectileEvent.radialOffset)*Vector3.forward*projectileEvent.range;
                } else {
                    Vector3 castVector = caster.GetCursorWorldPosition() - caster.transform.position;
                    castVector.y = 0;
                    float distance = Mathf.Min(projectileEvent.range, castVector.magnitude);
                    projectilePosition = caster.transform.position + distance*castVector.normalized;
                }

                Transform target = new GameObject("Projectile Target").transform;
                target.position = caster.GetCursorWorldPosition();
                Projectile p = Instantiate(projectileEvent.projectile, projectilePosition, caster.transform.rotation*projectileEvent.radialOffset);
                p.Initialize(caster, target);
                projectiles.Add(p);
            }
        }

        if (++frame>=duration) {
            Destroy(gameObject);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns>whether the cast was updated</returns>
    public virtual bool UpdateCast(Vector3 worldPosition) {
        return false; // nothing to update
    }

    public void FinishCast() {
        for (int i = 0; i < hits.Count; i++) {
            hits[i].duration = 0;
        }

        for (int i = 0; i < statusEffects.Count; i++) {
            statusEffects[i].duration = 0;
        }
    }
}
