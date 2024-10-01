using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;

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
    [SerializeField] public int duration;
    private bool rotatingClockwise = true;
    public Transform CastAimPositionTransform { get; private set; }

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
        CastAimPositionTransform = _castAimPositionTransform;
        rotatingClockwise = _rotatingClockwise;

        foreach (KeyValuePair<CastableCondition, Castable[]> CastableEvents in ConditionCastablesMap) {
            foreach (Castable Castable in CastableEvents.Value) {
                Assert.IsNotNull(Castable);
            }
        }

        if (Caster != null) {
            foreach (Effect sePrefab in effectPefabs) {
                Effect se = Instantiate(sePrefab);
                se.Initialize(Caster as MonoBehaviour);
                statusEffects.Add(se);
            }

            if (commandMovementPrefab != null) {
                if ((Caster as MonoBehaviour) is IMoves Mover) {
                    CommandMovement commandMovement = Instantiate(commandMovementPrefab, Mover.GetTransform());
                    commandMovement.Initialize(Mover, Caster.GetTargetTransform());
                    Mover.SetCommandMovement(commandMovement);
                }
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
                        Caster.GetTargetTransform(),
                        !Caster.IsRotatingClockwise()
                    )
                );
            }
        }

        if (Frame==startupTime) {
            if (ConditionCastablesMap.ContainsKey(CastableCondition.OnFinishStartup)) {
                // I don't know that this is tecnically the right way to implement it, but it's okay for now
                // the idea is, if I start a cast (say it takes 120 frames to run), and I run the cast to completion,
                // create the casts
                foreach (Castable Castable in ConditionCastablesMap[CastableCondition.OnFinishStartup]) {
                    ActiveCastables.Add(
                        Castable.CreateCast(
                            Castable,
                            Caster,
                            Caster.GetOriginTransform(),
                            Caster.GetTargetTransform(),
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
    public virtual void Recast(Transform worldPosition) {
        foreach (Castable Castable in ActiveCastables) {
            Castable.Recast(worldPosition);
        }
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
