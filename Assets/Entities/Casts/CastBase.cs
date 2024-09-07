using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;


[Serializable]
public enum Positioning {
    Directional,
    Absolute
}

[Serializable]
public struct HitEvent {
    public List<Hit> hits;
    public int startFrame;
}

[Serializable]
public struct ProjectileEvent {
    public List<Projectile> projectiles;
    public int startFrame;
    public float range;
    public Positioning positioning;
}

/// <summary>
/// Default object for evaluating cast behavior
/// Extend this object for more particular cast logic
/// </summary>
public class CastBase : MonoBehaviour {
    // Start is called before the first frame update
    [SerializeField] public HitEvent[] hitEvents;
    [SerializeField] public ProjectileEvent[] projectileEvents;

    private int frame = 0;
    private bool rotatingClockwise = true;
    [SerializeField] public int duration;

    private Character caster;
    public List<Hit> hits;
    public List<Projectile> projectiles;

    public void Initialize(Character _caster, bool _rotatingClockwise) {
        caster =_caster;
        rotatingClockwise = _rotatingClockwise;
        foreach (HitEvent hitEvent in hitEvents) {
            foreach (Hit hit in hitEvent.hits) {
                Assert.IsNotNull(hit);
            }
        }
    }

    void FixedUpdate() {
        foreach (HitEvent hitEvent in hitEvents) {
            if (hitEvent.startFrame==frame) {
                foreach (Hit hit in hitEvent.hits) {
                    Hit h = Instantiate(hit);
                    h.Initialize(caster, caster.transform, !rotatingClockwise);
                    hits.Add(h);
                }
            }
        }

        foreach (ProjectileEvent projectileEvent in projectileEvents) {
            if (projectileEvent.startFrame==frame) {
                foreach (Projectile projectile in projectileEvent.projectiles) {
                    Vector3 projectilePosition;
                    if (projectileEvent.positioning==Positioning.Directional) {
                        projectilePosition = caster.transform.position+caster.transform.rotation*Vector3.forward*projectileEvent.range;
                    } else {
                        Vector3 castVector = caster.getCursorWorldPosition() - caster.transform.position;
                        castVector.y = 0;
                        float distance = Mathf.Min(projectileEvent.range, castVector.magnitude);
                        projectilePosition = caster.transform.position + distance*castVector.normalized;
                    }

                    Transform target = new GameObject().transform;
                    target.position = caster.getCursorWorldPosition();
                    Projectile p = Instantiate(projectile, projectilePosition, caster.transform.rotation);
                    p.Initialize(caster, target);
                    projectiles.Add(p);
                }
            }
        }

        if (frame++>=duration)
            Destroy(gameObject);
    }

    public virtual void UpdateCast(Vector3 worldPosition) {

    }
}