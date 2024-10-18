using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;

[Serializable]
public enum TargetingMethod {
    ContinuousTargeting,
    DiscreteTargeting,
    SnapTarget,
    SelfTarget
}

public interface ICastMessage : IEventSystemHandler {
    // functions that can be called via the messaging system
    void FinishCast();
}

public static class CastUtils {
    public static Character GetSnapTarget(Vector3 worldPosition, bool hitsFriendlies, bool hitsEnemies, float maxDistance) {
        List<Character> characters = (from go in GameObject.FindGameObjectsWithTag("Character") select go.GetComponent<Character>()).ToList();
        List<float> characterDists = (from ch in characters select (worldPosition-ch.transform.position).magnitude).ToList();

        float minDist = Mathf.Infinity;
        int minIndex = -1;

        for (int i = 0; i < characterDists.Count; i++) {
            float dist = characterDists[i];
            if (dist < minDist && dist<maxDistance) {
                minDist = dist;
                minIndex = i;
            }
        }

        if (minIndex>=0) {
            return characters[minIndex];
        } else {
            return null;
        }
    }
}

/// <summary>
/// Default object for evaluating cast behavior
/// Extend this object for more particular cast logic
/// </summary>
public class Cast : MonoBehaviour/*, ICastMessage*/ {
    // Start is called before the first frame update
    [SerializeField] public FrameCastablesDictionary FrameCastablesMap = new();
    [SerializeField]
    public ConditionCastablesDictionary ConditionCastablesMap = (
        Enum.GetValues(typeof(CastableCondition))
            .Cast<CastableCondition>()
            .ToDictionary(t => t, t => new Castable[0])
        as ConditionCastablesDictionary
    );

    [HideInInspector] public int Frame = 0;
    [SerializeField] public float Range = 0;
    [SerializeField] public TargetingMethod TargetingMethod = TargetingMethod.ContinuousTargeting;
    [SerializeField] public int Duration;
    [SerializeField] public int CastTime;
    private bool rotatingClockwise = true;
    public Transform Target { get; private set; }

    private Castable RootCastable;
    private ICasts Caster;
    [HideInInspector] public Transform Origin;
    [HideInInspector] public List<Castable> ActiveCastables { get; set; } = new();

    public static Cast StartCast(Cast _cast, ICasts _caster, Transform _origin, Transform _castAimPositionTransform, bool _rotatingClockwise, Castable rootCastable) {
        Cast cast = Instantiate(_cast);
        cast.Initialize(_caster, _origin, _castAimPositionTransform, _rotatingClockwise);
        cast.RootCastable = rootCastable;
        return cast;
    }

    private void Initialize(ICasts _caster, Transform _origin, Transform _castAimPositionTransform, bool _rotatingClockwise) {
        Caster = _caster;
        Origin = _origin;
        rotatingClockwise = _rotatingClockwise;

        if (TargetingMethod == TargetingMethod.ContinuousTargeting) {
            Target = _castAimPositionTransform;
        } else if (TargetingMethod == TargetingMethod.DiscreteTargeting) {
            Target = null;
        }

        foreach (KeyValuePair<CastableCondition, Castable[]> CastableEvents in ConditionCastablesMap) {
            foreach (Castable Castable in CastableEvents.Value) {
                Assert.IsNotNull(Castable);
            }
        }
    }

    void FixedUpdate() {
        if (FrameCastablesMap.ContainsKey(Frame)) {
            foreach (Castable CastablePrefab in FrameCastablesMap[Frame]) {
                ActiveCastables.Add(
                    Castable.CreateCastable(
                        CastablePrefab,
                        Caster,
                        Origin,
                        Target,
                        !Caster.IsRotatingClockwise(),
                        RootCastable
                    )
                );
            }
        }

        if (++Frame>=Duration) {
            Destroy(gameObject);
        }
    }

    /*
     * public void FinishCast() {
        // TODO this was written for that Dash Special that I wrote a long time ago
        // I think, if the ability reached the desired target before max range,
        // it would call the cast to say it was done
        // maybe this can get reimplemented without events, maybe not, I'm not sure,/*
        // but I'll leave this here for now
        for (int i = 0; i < ActiveCastables.Count; i++) {
            ActiveCastables[i].Duration = 0;
        }
    }
    */

    public void OnDestroy() {}
}
