using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;
using System.Text;

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

/// <summary>
/// Default object for evaluating cast behavior
/// Extend this object for more particular cast logic
/// </summary>
public class Cast : MonoBehaviour, ICastMessage {
    // Start is called before the first frame update
    [SerializeField] List<Effect> effectPefabs;
    [SerializeField] public CommandMovement commandMovementPrefab;
    [SerializeField] public FrameCastablesDictionary FrameCastablesMap = new();
    [SerializeField]
    public ConditionCastablesDictionary ConditionCastablesMap = (
        Enum.GetValues(typeof(CastableCondition))
            .Cast<CastableCondition>()
            .ToDictionary(t => t, t => new Castable[0])
        as ConditionCastablesDictionary
    );

    [HideInInspector] public int Frame = 0;
    [SerializeField] public int startupTime = 0;
    [SerializeField] public bool Encumbering = false;
    [SerializeField] public TargetingMethod TargetingMethod = TargetingMethod.ContinuousTargeting;
    [SerializeField] public int duration;
    private bool rotatingClockwise = true;
    public Transform Target { get; private set; }

    [HideInInspector] private ICasts Caster;
    [HideInInspector] public Transform Origin;
    [HideInInspector] public List<Castable> ActiveCastables { get; set; } = new();
    [HideInInspector] public List<Effect> statusEffects;

    public static Cast Initiate(Cast _cast, ICasts _caster, Transform _origin, Transform _castAimPositionTransform, bool _rotatingClockwise) {
        Cast cast = Instantiate(_cast);
        cast.Initialize(_caster, _origin, _castAimPositionTransform, _rotatingClockwise);
        return cast;
    }

    private void Initialize(ICasts _caster, Transform _origin, Transform _castAimPositionTransform, bool _rotatingClockwise) {
        Caster = _caster;
        Origin = _origin;
        rotatingClockwise = _rotatingClockwise;

        if (TargetingMethod == TargetingMethod.ContinuousTargeting) {
            Target = _castAimPositionTransform;
        } else if (TargetingMethod == TargetingMethod.DiscreteTargeting) {
            GameObject go = new("Cast Target");
            go.transform.parent = transform;
            go.transform.position = _castAimPositionTransform.position;
            Target = go.transform;
        }
        

        foreach (KeyValuePair<CastableCondition, Castable[]> CastableEvents in ConditionCastablesMap) {
            foreach (Castable Castable in CastableEvents.Value) {
                Assert.IsNotNull(Castable);
            }
        }
    }

    void FixedUpdate() {
        if (FrameCastablesMap.ContainsKey(Frame)) {
            foreach (Castable Castable in FrameCastablesMap[Frame]) {
                ActiveCastables.Add(
                    Castable.CreateCast(
                        Castable,
                        Caster,
                        Origin,
                        Target,
                        !Caster.IsRotatingClockwise()
                    )
                );
            }
        }

        if (Frame==startupTime) {
            if (Caster != null) {
                foreach (Effect sePrefab in effectPefabs) { // activate effects
                    Effect se = Instantiate(sePrefab);
                    se.Initialize(Caster as MonoBehaviour);
                    statusEffects.Add(se);
                }

                if (commandMovementPrefab != null) { // set command movement
                    if ((Caster as MonoBehaviour) is IMoves Mover) {
                        CommandMovement commandMovement = Instantiate(commandMovementPrefab, transform);
                        commandMovement.Initialize(Mover, Target);
                        Mover.CommandMovement = commandMovement;
                    }
                }
            }

            if (ConditionCastablesMap.ContainsKey(CastableCondition.OnFinishStartup)) { // create any casts that required startup
                foreach (Castable Castable in ConditionCastablesMap[CastableCondition.OnFinishStartup]) {
                    ActiveCastables.Add(
                        Castable.CreateCast(
                            Castable,
                            Caster,
                            Caster.GetOriginTransform(),
                            Target,
                            !Caster.IsRotatingClockwise()
                        )
                    );
                }
            }
        }

        if (++Frame>=duration) {
            Destroy(gameObject);
        }
    }


    /// <summary/>
    /// <param name="worldPosition"></param>
    public bool Recast(Transform worldPosition) {
        bool recasted = false;
        
        foreach (Castable Castable in ActiveCastables) {
            recasted |= Castable.Recast(worldPosition);
        }

        return recasted;
    }

    public void FinishCast() {
        // TODO this was written for that Dash Special that I wrote a long time ago
        // I think, if the ability reached the desired target before max range,
        // it would call the cast to say it was done
        // maybe this can get reimplemented without events, maybe not, I'm not sure,
        // but I'll leave this here for now
        for (int i = 0; i < ActiveCastables.Count; i++) {
            ActiveCastables[i].Duration = 0;
        }
    }

    public void OnDestroy() {

    }
}
