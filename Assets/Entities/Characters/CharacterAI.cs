using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Numerics;
using System.ComponentModel;
using System;
using UnityEngine.Rendering;

public class CharacterAI : Agent
{
    // tracking agent stats
    [SerializeField] Character enemy;
    [SerializeField] StageTerrain floor;
    private Character character;
    private int lastFrameHP = 0;
    private int maxEpisodeTimer = 300*60;
    private int episodeTimer;

    public UnityEngine.Vector2 getDistancesToEdges(Transform transform, StageTerrain floor) {
        // utility function to get X distance to edge and Z distance to edge
        // this is a super rough calculation, TODO improve
        float xLeft = (floor.transform.position.x - floor.transform.localScale.x/2)-transform.position.x;
        float xRight = (floor.transform.position.x + floor.transform.localScale.x/2)-transform.position.x;
        float xFore = (floor.transform.position.z - floor.transform.localScale.z/2)-transform.position.z;
        float xBack = (floor.transform.position.z + floor.transform.localScale.z/2)-transform.position.z;
        UnityEngine.Vector2 ret = new(
            Math.Abs(xLeft)<Math.Abs(xRight) ? xLeft : xRight,
            Math.Abs(xFore)<Math.Abs(xBack) ? xFore : xBack
        );

        return ret;
        
    }

    public void Awake() {
        character = GetComponent<Character>();
        lastFrameHP = character.HP;
    }

    // reference: https://www.youtube.com/watch?v=zPFU30tbyKs
    public override void CollectObservations(VectorSensor sensor) {
        // TODO maybe give it some read of the closest world edge position?
        sensor.AddObservation(character.isActiveAndEnabled); // +1
        sensor.AddObservation(getDistancesToEdges(character.transform, floor)); // +2 = 3
        sensor.AddObservation(enemy.transform.position-character.transform.position); // +3 = 6
        sensor.AddObservation(character.GetVelocity()+UnityEngine.Vector3.up*character.VerticalVelocity); // +3 = 9
    }

    public virtual void Heuristic(in ActionBuffers actionsOut) {
        // set Heuristic actions
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 1; // don't move
        discreteActionsOut[1] = 1; // don't move
        discreteActionsOut[1] = 0; // don't shoot

        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = 1;
        continuousActionsOut[1] = 0;

    }

    public override void OnActionReceived(ActionBuffers actions) {
        /* Set inputs of Character behavior */
        // movement
        character.InputMoveDirection = new UnityEngine.Vector3(
            actions.DiscreteActions[0]-1,   // [0,2] -> [-1,1]
            0,
            actions.DiscreteActions[1]-1    // [0,2] -> [-1,1]
        ).normalized;

        // cast
        character.CastIdBuffer = (actions.DiscreteActions[2]==0) ? -1 : (int)CastId.Medium1;

        // aim
        UnityEngine.Vector3 aim = new UnityEngine.Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);
        if (aim == UnityEngine.Vector3.zero) {
            aim = UnityEngine.Vector3.right;
        }
        character.LookDirection = aim.normalized;

        
    }

    private void FixedUpdate() {
        float frameRewardTotal = character.DamageDealt;

        if (lastFrameHP != character.HP) {
            frameRewardTotal += character.HP-lastFrameHP;
            lastFrameHP = character.HP;
        }

        AddReward(frameRewardTotal);

        if (episodeTimer--==0 || character == null || character.HP<=0 || enemy.HP==0) {
            Debug.Log($"cumulative reward: {GetCumulativeReward()}");
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin() {
        Debug.Log("restarting");
        episodeTimer = maxEpisodeTimer;
        character.HP = Character.HPMax;
        lastFrameHP = Character.HPMax;
        enemy.transform.position = new UnityEngine.Vector3(UnityEngine.Random.Range(-5f, 5f), 1.2f, UnityEngine.Random.Range(-5f, 5f)); // TODO I'm so lazy
    }
}
