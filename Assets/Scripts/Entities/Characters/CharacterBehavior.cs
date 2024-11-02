using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class ContextSteering {
    private static float PathScanDistance = 7.5f*12f/60f;

    // https://kidscancode.org/godot_recipes/3.x/ai/context_map/index.html
    public static void SetDirectionWeights(Transform mover, Transform target, List<Vector2> directions, List<float> interests) {
        Vector2 horizontalPlanePath = new(target.position.x - mover.position.x, target.position.z - mover.position.z);

        for (int i = 0; i < directions.Count; i++) {
            interests[i] = Mathf.Max(0, Vector2.Dot(directions[i], horizontalPlanePath));

            if (interests[i]>0) {
                // check for obstructions to collide with
                Vector3 currentDirection = new Vector3(directions[i].x, 0f, directions[i].y).normalized;
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

    public static Vector2 GetSteerDirection(Transform mover, Transform target, List<Vector2> directions, List<float> interests) {
            SetDirectionWeights(mover, target, directions, interests);

            Vector2 maxDirection = Vector2.zero;
            float maxInterest = .1f;

            for (int i = 1; i < interests.Count; i++) {
                if (interests[i]>maxInterest) {
                    maxDirection = directions[i];
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

public class CharacterBehavior : MonoBehaviour
{
    // Context Based Steering
    public static readonly int NUM_STEER_RAYS = 8;
    [SerializeField] public Character enemy;
    
    private Character character;
    private int steerEvaluationRate = 5; private int steerEvaluationTimer = 0;
    private int attackEvaluationRate = 5; private int attackEvaluationTimer = 0;
    private List<float> steerInterests = Enumerable.Repeat(0f, steerDirections.Count).ToList();
    private static List<Vector2> steerDirections = new(){
        new Vector2(+0, +1).normalized,
        new Vector2(+0, -1).normalized,
        new Vector2(+1, +0).normalized,
        new Vector2(+1, +1).normalized,
        new Vector2(+1, -1).normalized,
        new Vector2(-1, +0).normalized,
        new Vector2(-1, +1).normalized,
        new Vector2(-1, -1).normalized
    };

    public void Awake() {
        character = GetComponent<Character>();
        //obstructions = SceneController.GetSceneTerrainObstructions().Map(i=>)
    }

    private void FixedUpdate() {
        // check or hitboxes and projectiles
        /*
        IEnumerable<Hit> hits = (
            from GameObject go in GameObject.FindGameObjectsWithTag("Hit")
            select go.GetComponent<Hit>()
        );
        
        foreach (Hit hit in hits) {
            if (hit.Caster!=null && hit.Caster.TeamBitMask > 0) {
                Debug.Log($"I see {hit.name}");
            }
        }*/

        // evaluate movement direction
        if (++steerEvaluationTimer==steerEvaluationRate) {
            steerEvaluationTimer = 0;
            character.InputMoveDirection = ContextSteering.GetSteerDirection(
                character.transform, enemy.transform, steerDirections, steerInterests
            );
        }

        character.InputAimDirection = AimAt(transform, enemy.transform);

        // evaluate hit
        if (++attackEvaluationTimer==attackEvaluationRate) {
            attackEvaluationTimer = 0;
            int nextCast = AttackBehaviorUtils.GetAttackCastId(transform, enemy.transform);
            character.InputCastId = nextCast;
        }
    }

    public static Vector3 AimAt(Transform me, Transform target) {
        return new(target.position.x - me.position.x, 0f, target.position.z - me.position.z);
    }
}
