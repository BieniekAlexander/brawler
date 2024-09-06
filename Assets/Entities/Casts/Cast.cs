using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Collections;
using TreeEditor;
using static UnityEngine.GraphicsBuffer;

[Serializable]
public struct HitEvent {
    public List<Hit> hits;
    public int startFrame;
}

[Serializable]
public struct ProjectileEvent {
    public List<Projectile> projectiles;
    public int startFrame;
    public float initialOffset;
}

public class Cast : MonoBehaviour {
    // Start is called before the first frame update
    [SerializeField] public HitEvent[] hitEvents;
    [SerializeField] public ProjectileEvent[] projectileEvents;
    
    private int frame = 0;
    private bool rotatingClockwise = true;
    [SerializeField] private int duration;

    private CharacterBehavior caster;
    private Vector3 initialPosition = new();
    private Quaternion initialRotation = new();

    public void Initialize(CharacterBehavior _caster, bool _rotatingClockwise) {
        caster =_caster;
        rotatingClockwise = _rotatingClockwise;
        foreach (HitEvent hitEvent in hitEvents) {
            foreach (Hit hit in hitEvent.hits) {
                Assert.IsNotNull(hit);
            }
        }
    }

    public void Initialize(Vector3 _initialPosition, Quaternion _initialRotation) {
        initialPosition=_initialPosition;
        initialRotation=_initialRotation;

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
                    Hit h = Instantiate(hit, initialPosition, initialRotation);
                    h.Initialize(caster, caster.transform, !rotatingClockwise);
                }
            }
        }

        foreach (ProjectileEvent projectileEvent in projectileEvents) {
            if (projectileEvent.startFrame==frame) {
                foreach (Projectile projectile in projectileEvent.projectiles) {
                    Vector3 projectilePosition = caster.transform.position+caster.transform.rotation*Vector3.forward*projectileEvent.initialOffset;
                    Transform target = new GameObject().transform;
                    target.position = caster.getCursorWorldPosition();
                    Instantiate(projectile, projectilePosition, caster.transform.rotation).Initialize(caster, null);
                }
            }
        }

        if (frame++>=duration)
            Destroy(gameObject);
    }
}
