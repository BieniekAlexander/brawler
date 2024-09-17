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

    [SerializeField] public int startupTime = 0;
    [SerializeField] public int duration;
    private bool rotatingClockwise = true;
    public Transform CastAimPositionTransform { get; private set; }


    public Transform Origin { get; private set; }
    public List<Hit> hits;
    public List<Projectile> projectiles;
    public List<StatusEffectBase> statusEffects;

    public static CastBase Initiate(CastBase _cast, Transform _origin, Transform _castAimPositionTransform, bool _rotatingClockwise) {
        CastBase cast = Instantiate(_cast);
        cast.Initialize(_origin, _castAimPositionTransform, _rotatingClockwise);
        return cast;
    }

    private void Initialize(Transform _origin, Transform _castAimPositionTransform, bool _rotatingClockwise) {
        Origin = _origin;
        CastAimPositionTransform = _castAimPositionTransform;
        rotatingClockwise = _rotatingClockwise;

        foreach (HitEvent hitEvent in hitEvents) {
            Assert.IsNotNull(hitEvent.hit);
        }

        if (Origin.CompareTag("Character")) {
            Character caster = Origin.GetComponent<Character>();
            foreach (StatusEffectBase sePrefab in statusEffectPefabs) {
                StatusEffectBase se = Instantiate(sePrefab);
                se.Initialize(caster);
                statusEffects.Add(se);
            }

            if (commandMovementPrefab != null) {
                CommandMovementBase commandMovement = Instantiate(commandMovementPrefab, transform);
                commandMovement.Initialize(caster, CastAimPositionTransform.position, caster.transform.position);
                caster.SetCommandMovement(commandMovement);
            }
        }

    }

    void FixedUpdate() {
        foreach (HitEvent hitEvent in hitEvents) {
            if (hitEvent.startFrame==frame) {
                hits.Add(
                    Hit.Initiate(
                        hitEvent.hit,
                        Origin,
                        !rotatingClockwise
                    )
                );
            }
        }

        foreach (ProjectileEvent projectileEvent in projectileEvents) {
            if (projectileEvent.startFrame==frame) {
                Vector3 projectilePosition;

                if (projectileEvent.positioning==Positioning.Directional) {
                    projectilePosition = Origin.position+(projectileEvent.radialOffset*Origin.rotation)*Vector3.forward*projectileEvent.range;
                } else {
                    Vector3 castVector = CastAimPositionTransform.position - Origin.position;
                    castVector.y = 0;
                    float distance = Mathf.Min(projectileEvent.range, castVector.magnitude);
                    projectilePosition = Origin.position + distance*castVector.normalized;
                }
                
                projectiles.Add(
                    Projectile.Initiate(
                        projectileEvent.projectile,
                        projectilePosition,
                        Origin.rotation*projectileEvent.radialOffset,
                        Origin,
                        CastAimPositionTransform
                    )
                );
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
    public virtual bool UpdateCast() {
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
