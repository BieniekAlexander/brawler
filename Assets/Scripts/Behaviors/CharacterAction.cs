using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public static class ContextSteering {
    private static float PathScanDistance = 3f;
    public static List<Vector2> SteerDirections = new(){
        new Vector2(+0, +1).normalized,
        new Vector2(+0, -1).normalized,
        new Vector2(+1, +0).normalized,
        new Vector2(+1, +1).normalized,
        new Vector2(+1, -1).normalized,
        new Vector2(-1, +0).normalized,
        new Vector2(-1, +1).normalized,
        new Vector2(-1, -1).normalized
    };

    private static float _getInterest(Vector3 position, Vector3 target, Vector2 steerDirection, Range range) {
        if (range != null) {
            float rangeMean = range.Mean(); // NOTE: just looking at the mean for performance
            float distance = MathUtils.RayDistanceToCircle(
                new Vector2(position.x, position.z),
                steerDirection,
                new Vector2(target.x, target.z),
                rangeMean
            );

            return 1-Mathf.Abs(distance)/100; // TODO something without magic numbers :^) https://www.wolframalpha.com/input?i=1-abs%28x%29%2F100
        } else {
            Vector2 horizontalPlanePath = new(target.x - position.x, target.z - position.z);
            return Mathf.Max(0, Vector2.Dot(steerDirection, horizontalPlanePath));
        }
    }

    public static void SetDirectionWeights(Transform mover, Vector3 target, List<float> interests, Range range) {
        if (!NavMesh.SamplePosition(mover.position, out NavMeshHit navMeshHit, 1f, NavMesh.AllAreas)) {
            return;
        }

        // adapted from https://kidscancode.org/godot_recipes/3.x/ai/context_map/index.html
        for (int i = 0; i < SteerDirections.Count; i++) {
            Vector2 horizontalPlanePath = new(mover.transform.position.x - target.x, mover.transform.position.z - target.z);
            interests[i] = _getInterest(mover.position, target, SteerDirections[i], range);

            if (interests[i]>0) {
                // check for obstructions to collide with
                Vector3 currentDirection = new Vector3(SteerDirections[i].x, 0f, SteerDirections[i].y).normalized;
                Physics.Raycast(
                    mover.position,
                    currentDirection,
                    out RaycastHit hitInfo,
                    horizontalPlanePath.magnitude,
                    1<<LayerMask.NameToLayer("Terrain")
                );

                if (hitInfo.transform!=null) {
                    interests[i] = 0;
                    continue;
                } 
                
                // check if we hit the end of the NavMesh
                if (NavMesh.Raycast(navMeshHit.position, navMeshHit.position+5*currentDirection, out NavMeshHit _, NavMesh.AllAreas)) {
                    interests[i] = 0;
                    continue;
                }
            }
        }
    }

    public static Vector2 GetSteerDirection(Transform mover, Vector3 target, List<float> interests, Range range = null) {
        Vector2 maxDirection = Vector2.zero;
        float maxInterest = .1f;
        SetDirectionWeights(mover, target, interests, range);

        if (range!=null && interests.Max() < maxInterest) {
            // TODO hacky solution - if range is set such that the character can't find any path, it'll get stuck behind obstacles
            SetDirectionWeights(mover, target, interests, null);
        }

        for (int i = 1; i < interests.Count; i++) {
            if (interests[i]>maxInterest) {
                maxDirection = SteerDirections[i];
                maxInterest = interests[i];
            }
        }

        return maxDirection;
    }
}

public static class AttackBehaviorUtils { // super rough implementation to get some attack behavior going
    public static int GetAttackCastId() {
        return Random.Range((int) CastType.Light, (int) CastType.Heavy);
    }
}

public static class ActionAtoms {
    // Movement
    public static void MoveAtEnemy(CharacterBehavior state, CharacterFrameInput input) {
        Vector2 direction = MovementUtils.inXZ(state.Enemy.transform.position - state.Me.transform.position).normalized;
        Vector2 SteerDirection = EnumUtils<Vector2>.ArgMax(ContextSteering.SteerDirections, i => Vector2.Dot(direction, i));
        input.MoveDirectionX = SteerDirection.x;
        input.MoveDirectionZ = SteerDirection.y;
    }

    public static void MoveFromEnemy(CharacterBehavior state, CharacterFrameInput input) {
        Vector2 direction = MovementUtils.inXZ(state.Enemy.transform.position - state.Me.transform.position).normalized;
        Vector2 SteerDirection = EnumUtils<Vector2>.ArgMax(ContextSteering.SteerDirections, i => -Vector2.Dot(direction, i));
        input.MoveDirectionX = SteerDirection.x;
        input.MoveDirectionZ = SteerDirection.y;
    }

    public static void MoveTowardsEnemy(CharacterBehavior state, CharacterFrameInput input) {
        Vector2 steerDirection = ContextSteering.GetSteerDirection(
            state.Me.transform,
            state.Enemy.transform.position,
            state.SteerInterests,
            null
        );

        if (steerDirection==Vector2.zero) {
            Vector3 start = MovementUtils.inXZ(state.Me.transform.position);
            Vector3 end = MovementUtils.inXZ(state.Enemy.transform.position);

            if (
                    NavMesh.CalculatePath(
                    start,
                    end,
                    NavMesh.AllAreas,
                    state.NMPath
                    ) && state.NMPath.corners.Length>1
            ) {
                steerDirection = ContextSteering.GetSteerDirection(
                    state.Me.transform,
                    state.NMPath.corners[1],
                    state.SteerInterests,
                    null
                );
            }
        }

        input.MoveDirectionX = steerDirection.x;
        input.MoveDirectionZ = steerDirection.y;
    }

    public static void SpaceNearEnemy(CharacterBehavior state, CharacterFrameInput input) {
        // TODO currently gets stock behind objects if they obstruct the interection of steerDir with the enemy circle
        Vector2 steerDirection = ContextSteering.GetSteerDirection(
            state.Me.transform,
            state.Enemy.transform.position,
            state.SteerInterests,
            new Range(){Min=3, Max=4}
        );

        if (steerDirection==Vector2.zero) { // TODO wrap this somehow so that the code isn't copied smh
            Vector3 start = MovementUtils.inXZ(state.Me.transform.position);
            Vector3 end = MovementUtils.inXZ(state.Enemy.transform.position);

            if (NavMesh.CalculatePath(
                start,
                end,
                NavMesh.AllAreas,
                state.NMPath
            )) {
                steerDirection = ContextSteering.GetSteerDirection(
                    state.Me.transform,
                    state.NMPath.corners[1],
                    state.SteerInterests,
                    null
                );
            }
        }

        input.MoveDirectionX = steerDirection.x;
        input.MoveDirectionZ = steerDirection.y;
    }

    // Attack
    public static void Dash(CharacterBehavior state, CharacterFrameInput input) {
        input.CastId = (int) CastType.Rush;
    }

    // Aim
    public static void AimAtEnemy(CharacterBehavior state, CharacterFrameInput input) {
        Vector3 aimDirection = state.Enemy.transform.position-state.Me.transform.position;
        input.AimDirectionX = aimDirection.x;
        input.AimDirectionZ = aimDirection.z;
    }

    // Attack
    public static void StartAttack(CharacterBehavior state, CharacterFrameInput input) {
        input.CastId = AttackBehaviorUtils.GetAttackCastId();
    }

    // Block
    public static void Block(CharacterBehavior state, CharacterFrameInput input) {
        input.Blocking = true;
    }
}

public class CharacterAction {
    public List<(System.Action<CharacterBehavior, CharacterFrameInput>, bool)> Atoms { get; set; }
    public System.Func<CharacterBehavior, bool> EndCondition { get; set; }

    public void ApplyTo(CharacterBehavior state, CharacterFrameInput input) {
        input.Reset();

        for (int i = Atoms.Count-1; i >= 0; i--) {
            System.Action<CharacterBehavior, CharacterFrameInput> action = Atoms[i].Item1;
            bool persist = Atoms[i].Item2;
            action(state, input);

            if (!persist) {
                Atoms.RemoveAt(i);
            }
        }
    }

    public bool IsDone(CharacterBehavior state) {
        return EndCondition(state);
    }
}

public static class CharacterActionFactory {
    public static CharacterAction Approach() {
        return new CharacterAction(){
            Atoms = new List<(System.Action<CharacterBehavior, CharacterFrameInput>, bool)>() {
                (ActionAtoms.MoveTowardsEnemy, true),
                (ActionAtoms.AimAtEnemy, true)
            }, EndCondition = CharacterObservations.CloseToEnemy
        };
    }

    public static CharacterAction Attack() {
        return new CharacterAction(){
            Atoms = new List<(System.Action<CharacterBehavior, CharacterFrameInput>, bool)>() {
                (ActionAtoms.MoveTowardsEnemy, true),
                (ActionAtoms.AimAtEnemy, true),
                (ActionAtoms.StartAttack, false)
            }, EndCondition = CharacterObservations.CastBufferEmpty
        };
    }

    public static CharacterAction Space() {
        return new CharacterAction(){
            Atoms = new List<(System.Action<CharacterBehavior, CharacterFrameInput>, bool)>() {
                (ActionAtoms.SpaceNearEnemy, true),
                (ActionAtoms.AimAtEnemy, true)
            }, EndCondition = CharacterObservations.SpacedFromEnemy
        };
    }

    public static CharacterAction BackPedal() {
        return new CharacterAction(){
            Atoms = new List<(System.Action<CharacterBehavior, CharacterFrameInput>, bool)>() {
                (ActionAtoms.MoveFromEnemy, true),
                (ActionAtoms.AimAtEnemy, true)
            }, EndCondition = CharacterObservations.SpacedFromEnemy
        };
    }

    public static CharacterAction HastenGetUp() {
         return new CharacterAction(){
            Atoms = new List<(System.Action<CharacterBehavior, CharacterFrameInput>, bool)>() {
                (ActionAtoms.MoveAtEnemy, true),
                (ActionAtoms.AimAtEnemy, true)
            }, EndCondition = CharacterObservations.SpacedFromEnemy
        };
    }

    public static CharacterAction Idle() {
         return new CharacterAction(){
            Atoms = new List<(System.Action<CharacterBehavior, CharacterFrameInput>, bool)>() {
                (ActionAtoms.AimAtEnemy, true)
            }, EndCondition = CharacterObservations.FarFromEnemy
        };
    }

    public static CharacterAction DashIn() {
         return new CharacterAction(){
            Atoms = new List<(System.Action<CharacterBehavior, CharacterFrameInput>, bool)>() {
                (ActionAtoms.MoveAtEnemy, true),
                (ActionAtoms.AimAtEnemy, true),
                (ActionAtoms.Dash, false)
            }, EndCondition = CharacterObservations.FarFromEnemy
        };
    }
}