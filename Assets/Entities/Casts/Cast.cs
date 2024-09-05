using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Collections;
using TreeEditor;

[Serializable]
public struct HitEvent {
    public List<Hit> hits;
    public int startFrame;
}

public class Cast : MonoBehaviour {
    // Start is called before the first frame update
    [SerializeField] public HitEvent[] hitEvents;
    // [SerializeField] public MonoBehaviour CastBehavior;
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
                duration=Mathf.Max(duration, hitEvent.startFrame+hit.duration);
            }
        }
    }

    void FixedUpdate() {
        foreach (HitEvent hitEvent in hitEvents) {
            if (hitEvent.startFrame==frame) {
                foreach (Hit hit in hitEvent.hits) {
                    Hit h = Instantiate(hit, initialPosition, initialRotation);

                    if (caster!=null) {
                        h.Initialize(caster, caster.transform, !rotatingClockwise);
                    }

                    h.HandleMove();
                    h.HandleHitCollisions();
                }
            }
        }

        if (frame++>=duration)
            Destroy(gameObject);
    }
}