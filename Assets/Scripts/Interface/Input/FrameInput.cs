using System;
using UnityEngine;
using System.Diagnostics.Contracts;

[Serializable]
public class CharacterFrameInput {
    public CharacterFrameInput() {
        Reset();
    }

    public CharacterFrameInput(
        Vector3 aimDirection,
        Vector2 moveDirection,
        bool blocking,
        int castId) {
        MoveDirectionX = moveDirection.x;
        MoveDirectionZ = moveDirection.y;
        AimDirectionX = aimDirection.x;
        AimDirectionZ = aimDirection.z;
        Blocking = blocking;
        CastId = castId;
    }

    public void Reset() {
        MoveDirectionX = 0f;
        MoveDirectionZ = 0f;
        AimDirectionX = 0f;
        AimDirectionZ = 0f;
        Blocking = false;
        CastId = -1;    
    }

    public float MoveDirectionX;
    public float MoveDirectionZ;
    public float AimDirectionX;
    public float AimDirectionZ;
    public bool Blocking;
    public int CastId;
}