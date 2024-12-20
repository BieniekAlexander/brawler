using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Numerics;
using System.ComponentModel;
using System;
using UnityEngine.Rendering;

public class CharacterAgent : Agent
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
        sensor.AddObservation(enemy.transform.position-character.transform.position); // +3 = 668
        sensor.AddObservation(character.Velocity); // +3 = 9
        sensor.AddObservation(character.IsGrounded()); // +1 = 10
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        // set Heuristic actions
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 1; // don't move
        discreteActionsOut[1] = 1; // don't move
        discreteActionsOut[2] = 0; // don't attack

        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = 1;
        continuousActionsOut[1] = 0;

    }

    public override void OnActionReceived(ActionBuffers actions) {
        UnityEngine.Vector3 aim = new UnityEngine.Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);
        aim = (aim == UnityEngine.Vector3.zero) ? UnityEngine.Vector3.forward : aim;

        CharacterFrameInput i = new(
            aim, // TODO the aim will need to have some range because casts can lock to world position
            new UnityEngine.Vector2(
                actions.DiscreteActions[0]-1,   // [0,2] -> [-1,1]
                actions.DiscreteActions[1]-1    // [0,2] -> [-1,1]
            ).normalized,
            actions.DiscreteActions[2]==1, // blocking
            actions.DiscreteActions[3]-1 // CastId TODO expand set of values
        );
        
        character.LoadCharacterFrameInput(i);
    }

    private void FixedUpdate() {
        float frameRewardTotal = 2*character.DamageDealt;

        if (lastFrameHP != character.HP) {
            frameRewardTotal += character.HP-lastFrameHP;
            lastFrameHP = character.HP;
        }

        AddReward(frameRewardTotal);

        if (episodeTimer--==0 || character == null || character.HP<=0 || enemy.HP==0) {
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin() {
        episodeTimer = maxEpisodeTimer;
        character.OnRespawn();
        enemy.OnRespawn();
        lastFrameHP = character.HP;
    }
}
