using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering; 

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

    // https://kidscancode.org/godot_recipes/3.x/ai/context_map/index.html
    public static void SetDirectionWeights(Transform mover, Vector3 target, List<float> interests) {
        Vector2 horizontalPlanePath = new(target.x - mover.position.x, target.z - mover.position.z);

        for (int i = 0; i < SteerDirections.Count; i++) {
            interests[i] = Mathf.Max(0, Vector2.Dot(SteerDirections[i], horizontalPlanePath));

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
                
                // check if the nearby space is walkable
                // TODO this doesn't operate as intended when there's a pit between the player and the pathscan point
                bool walkable = false;

                foreach (GameObject go in GameObject.FindGameObjectsWithTag("Terrain:Path")) {
                    Collider pathCollider = go.GetComponent<Collider>();
                    walkable |= pathCollider.bounds.Contains(
                        mover.position + Vector3.down*1.5f + currentDirection*PathScanDistance
                    );
                }

                if (!walkable) {
                    interests[i] = 0;
                    continue;
                }
            }
        }
    }

    public static Vector2 GetSteerDirection(Transform mover, Vector3 target, List<float> interests) {
        SetDirectionWeights(mover, target, interests);

        Vector2 maxDirection = Vector2.zero;
        float maxInterest = .1f;

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
    public static int GetAttackCastId(Transform mover, Transform target) {
        Vector2 horizontalPlanePath = new(target.position.x - mover.position.x, target.position.z - mover.position.z);

        if (horizontalPlanePath.magnitude<2f) {
            return Random.Range((int) CastId.Light1, (int) CastId.HeavyS);
        } else {
            return -1;
        }
    }
}

/// <summary>
/// Contains functions to be applied to CharacterFrameInput
/// </summary>
public static class ActionAtoms {
    public static void AimAtEnemy(CharacterFrameInput input, CharacterBehavior state) {
        Vector3 aimDirection = state.Enemy.transform.position-state.Character.transform.position;
        input.AimDirectionX = aimDirection.x;
        input.AimDirectionZ = aimDirection.z;
    }

    public static void Block(CharacterFrameInput input, CharacterBehavior state) {
        input.Blocking = true;
    }

    // TODO current implementation will run every frame - find a way to reassess this according to state stored in CharacterBehavior
    public static void MoveTowardsEnemy(CharacterFrameInput input, CharacterBehavior state) {
        Vector2 SteerDirection = ContextSteering.GetSteerDirection(
            state.Character.transform,
            state.Enemy.transform.position,
            state.SteerInterests
        );

        input.MoveDirectionX = SteerDirection.x;
        input.MoveDirectionZ = SteerDirection.y;
    }
}

public class CharacterAction {
    public List<(System.Action<CharacterFrameInput, CharacterBehavior>, bool)> Atoms;

    public CharacterAction(List<(System.Action<CharacterFrameInput, CharacterBehavior>, bool)> atoms) {
        Atoms = atoms;
    }

    public void ApplyTo(CharacterBehavior state, CharacterFrameInput input) {
        input.Reset();

        for (int i = Atoms.Count-1; i >= 0; i--) {
            System.Action<CharacterFrameInput, CharacterBehavior> action = Atoms[i].Item1;
            bool persist = Atoms[i].Item2;
            action(input, state);

            if (!persist) {
                Atoms.RemoveAt(i);
            }
        }
    }
}

public static class CharacterActionFactory {
    public static CharacterAction Approach() {
        return new CharacterAction(
            new List<(System.Action<CharacterFrameInput, CharacterBehavior>, bool)>(){
                (ActionAtoms.MoveTowardsEnemy, true)
            }
        );
    }
}